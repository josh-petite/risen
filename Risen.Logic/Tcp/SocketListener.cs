using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Risen.Server.Tcp
{
    // sample walkthrough at: http://www.codeproject.com/Articles/83102/C-SocketAsyncEventArgs-High-Performance-Socket-Cod
    // Implements the connection logic for the socket server.   
    // After accepting a connection, all data read from the client  
    // is sent back to the client. The read and echo back to the client pattern  
    // is continued until the client disconnects.
    public class SocketListener
    {
        private readonly IBufferManager _bufferManager; // represents a large reusable set of buffers for all socket operations 
        private Socket _listenSocket; // the socket used to listen for incoming connection requests 
        private readonly IListenerConfiguration _listenerConfiguration;
        private readonly Semaphore _maxNumberAcceptedClients;
        private PrefixHandler _prefixHandler;
        private MessageHandler _messageHandler;

        // pool of reusable SocketAsyncEventArgs objects for accept operations
        ISocketAsyncEventArgsPool _poolOfAcceptEventArgs;
        // pool of reusable SocketAsyncEventArgs objects for receive and send socket operations
        ISocketAsyncEventArgsPool _poolOfRecSendEventArgs;

        // Create an uninitialized server instance.   
        // To start the server listening for connection requests 
        // call the Init method followed by Start method  
        // 
        public SocketListener(IListenerConfiguration listenerConfiguration, IBufferManager bufferManager, ISocketAsyncEventArgsPool poolOfRecSendEventArgs, ISocketAsyncEventArgsPool poolOfAcceptEventArgs)
        {
            _listenerConfiguration = listenerConfiguration;
            // allocate buffers such that the maximum number of sockets can have one outstanding read and write posted to the socket simultaneously  
            _bufferManager = bufferManager; // new BufferManager(_listenerConfiguration.GetTotalBytesRequiredForInitialBufferConfiguration(), _listenerConfiguration.ReceiveBufferSize);
            _poolOfRecSendEventArgs = poolOfRecSendEventArgs;
            _poolOfAcceptEventArgs = poolOfAcceptEventArgs;
            _maxNumberAcceptedClients = new Semaphore(listenerConfiguration.MaxNumberOfConnections, listenerConfiguration.MaxNumberOfConnections);

            Init();
            Start();
        }

        // Initializes the server by preallocating reusable buffers and  
        // context objects.  These objects do not need to be preallocated  
        // or reused, but it is done this way to illustrate how the API can  
        // easily be used to create reusable objects to increase server performance. 
        // 
        public void Init()
        {
            // Allocates one large byte buffer which all I/O operations use a piece of.  This guards  
            // against memory fragmentation
            _bufferManager.InitBuffer();

            // preallocate pool of SocketAsyncEventArgs objects
            for (int i = 0; i < _listenerConfiguration.MaxSimultaneousAcceptOperations; i++)
            {
                // add SocketAsyncEventArg to the pool
                _poolOfAcceptEventArgs.Push(CreateNewSaeaForAccept());
            }
        }

        private SocketAsyncEventArgs CreateNewSaeaForAccept()
        {
            var acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += AcceptEventArg_Completed;
            var acceptOperationToken = new AcceptOperationUserToken(_poolOfAcceptEventArgs.AssignTokenId() + 10000);
            acceptEventArg.UserToken = acceptOperationToken;

            return acceptEventArg;
        }

        // Starts the server such that it is listening for  
        // incoming connection requests.     
        // 
        // <param name="localEndPoint">The endpoint which the server will listening 
        // for connection requests on</param> 
        public void Start(IPEndPoint localEndPoint)
        {
            // create the socket which listens for incoming connections
            _listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(localEndPoint);
            // start the server with a listen backlog of 100 connections
            _listenSocket.Listen(100);

            // post accepts on the listening socket
            StartAccept(null);

            //Console.WriteLine("{0} connected sockets with one outstanding receive posted to each....press any key", _outstandingReadCount);
            Console.WriteLine("Press any key to terminate the server process....");
            Console.ReadKey();
        }


        // Begins an operation to accept a connection request from the client  
        // 
        // <param name="acceptEventArg">The context object to use when issuing 
        // the accept operation on the server's listening socket</param> 
        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += AcceptEventArg_Completed;
            }
            else
            {
                // socket must be cleared since the context object is being reused
                acceptEventArg.AcceptSocket = null;
            }

            _maxNumberAcceptedClients.WaitOne();
            bool willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
            }
        }

        // This method is the callback method associated with Socket.AcceptAsync  
        // operations and is invoked when an accept operation is complete 
        // 
        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Interlocked.Increment(ref _numConnectedSockets);
            Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
                              _numConnectedSockets);

            // Get the socket for the accepted client connection and put it into the  
            //ReadEventArg object user token
            SocketAsyncEventArgs readEventArgs = _readWritePool.Pop();
            ((AsyncUserToken) readEventArgs.UserToken).Socket = e.AcceptSocket;

            // As soon as the client is connected, post a receive to the connection 
            bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(readEventArgs);
            }

            // Accept the next connection request
            StartAccept(e);
        }

        // This method is called whenever a receive or send operation is completed on a socket  
        // 
        // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler 
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

        }

        // This method is invoked when an asynchronous receive operation completes.  
        // If the remote host closed the connection, then the socket is closed.   
        // If data was received then the data is echoed back to the client. 
        // 
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            AsyncUserToken token = (AsyncUserToken) e.UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                //increment the count of the total bytes receive by the server
                Interlocked.Add(ref _totalBytesRead, e.BytesTransferred);
                Console.WriteLine("The server has read a total of {0} bytes", _totalBytesRead);

                //echo the data received back to the client
                e.SetBuffer(e.Offset, e.BytesTransferred);
                bool willRaiseEvent = token.Socket.SendAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessSend(e);
                }

            }
            else
            {
                CloseClientSocket(e);
            }
        }

        // This method is invoked when an asynchronous send operation completes.   
        // The method issues another receive on the socket to read any additional  
        // data sent from the client 
        // 
        // <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // done echoing data back to the client
                AsyncUserToken token = (AsyncUserToken) e.UserToken;
                // read the next block of data send from the client 
                bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken as AsyncUserToken;

            // close the socket associated with the client 
            try
            {
                token.Socket.Shutdown(SocketShutdown.Send);
            }
                // throws if client process has already closed 
            catch (Exception)
            {
            }
            token.Socket.Close();

            // decrement the counter keeping track of the total number of clients connected to the server
            Interlocked.Decrement(ref _numConnectedSockets);
            _maxNumberAcceptedClients.Release();
            Console.WriteLine(
                "A client has been disconnected from the server. There are {0} clients connected to the server",
                _numConnectedSockets);

            // Free the SocketAsyncEventArg so they can be reused by another client
            _readWritePool.Push(e);
        }
    }
}