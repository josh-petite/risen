using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Risen.Server.Extentions;

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

        private SocketAsyncEventArgsPool _poolOfAcceptEventArgs; // pool of reusable SocketAsyncEventArgs objects for accept operations
        private SocketAsyncEventArgsPool _poolOfRecSendEventArgs; // pool of reusable SocketAsyncEventArgs objects for receive and send socket operations

        // Create an uninitialized server instance.   
        // To start the server listening for connection requests 
        // call the Init method followed by Start method  
        public SocketListener(IListenerConfiguration listenerConfiguration, IBufferManager bufferManager)
        {
            _listenerConfiguration = listenerConfiguration;
            // allocate buffers such that the maximum number of sockets can have one outstanding read and write posted to the socket simultaneously  
            _bufferManager = bufferManager; // new BufferManager(_listenerConfiguration.GetTotalBytesRequiredForInitialBufferConfiguration(), _listenerConfiguration.ReceiveBufferSize);
            _maxNumberAcceptedClients = new Semaphore(listenerConfiguration.MaxNumberOfConnections, listenerConfiguration.MaxNumberOfConnections);

            Init();
            StartListen();
        }

        public static List<DataHolder> DataHolders { get; set; }
        public static int MainSessionId
        {
            get { return 1000000000; }
            set { throw new NotImplementedException(); }
        }

        // Initializes the server by preallocating reusable buffers and  
        // context objects.  These objects do not need to be preallocated  
        // or reused, but it is done this way to illustrate how the API can  
        // easily be used to create reusable objects to increase server performance. 
        // 
        public void Init()
        {
            InitializeBufferManager();
            InitializeAcceptEventArgsPool();
            InitializeSendReceiveEventArgsPool();
        }

        private void InitializeBufferManager()
        {
            // Allocates one large byte buffer which all I/O operations use a piece of.  
            // This guards against memory fragmentation
            _bufferManager.InitBuffer();
        }

        private void InitializeAcceptEventArgsPool()
        {
            _poolOfAcceptEventArgs = new SocketAsyncEventArgsPool(_listenerConfiguration.MaxSimultaneousAcceptOperations);

            // preallocate pool of SocketAsyncEventArgs objects
            for (int i = 0; i < _listenerConfiguration.MaxSimultaneousAcceptOperations; i++)
            {
                // add SocketAsyncEventArg to the pool
                _poolOfAcceptEventArgs.Push(CreateNewSaeaForAccept());
            }
        }

        private void InitializeSendReceiveEventArgsPool()
        {
            _poolOfRecSendEventArgs = new SocketAsyncEventArgsPool(_listenerConfiguration.NumberOfSaeaForRecSend);

            for (int i = 0; i < _listenerConfiguration.NumberOfSaeaForRecSend; i++)
            {
                var eventArgForPool = new SocketAsyncEventArgs();
                _bufferManager.SetBuffer(eventArgForPool);

                eventArgForPool.Completed += IoCompleted;
                var receiveSendUserToken = new DataHoldingUserToken(eventArgForPool, _listenerConfiguration, _poolOfRecSendEventArgs.AssignTokenId() + 1000000);
                receiveSendUserToken.CreateNewDataHolder();
                eventArgForPool.UserToken = receiveSendUserToken;
                _poolOfAcceptEventArgs.Push(eventArgForPool);
            }
        }

        private SocketAsyncEventArgs CreateNewSaeaForAccept()
        {
            var acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += AcceptEventArg_Completed;
            acceptEventArg.UserToken = new AcceptOperationUserToken(_poolOfAcceptEventArgs.AssignTokenId() + 10000);

            return acceptEventArg;
        }

        private void StartListen()
        {
            // Listens for incoming connections
            _listenSocket = new Socket(_listenerConfiguration.LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // Bind it to specified port (in configuration)
            _listenSocket.Bind(_listenerConfiguration.LocalEndPoint);
            // The backlog value represents the number of excess clients that can queue up to wait for an open connection.
            _listenSocket.Listen(_listenerConfiguration.Backlog);
            StartAccept();
        }

        public void StartAccept()
        {
            var acceptEventArg = _poolOfAcceptEventArgs.Any() ? _poolOfAcceptEventArgs.Pop() : CreateNewSaeaForAccept();

            // Used to control access to pool resources
            _maxNumberAcceptedClients.WaitOne();

            var willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);

            // AcceptAsync returns true if the I/O operation is pending, i.e. is working asynchronously.
            // When it completes it will call the acceptEventArg.Completed event (AcceptEventArg_Completed, as wired above).
            if (!willRaiseEvent)
                ProcessAccept(acceptEventArg);
        }

        // This method is the callback method associated with Socket.AcceptAsync  
        // operations and is invoked when an accept operation is complete 
        // 
        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs.SocketError != SocketError.Success)
            {
                StartAccept(); // Something failed, try again with a new SocketAsyncEventArgs
                var acceptOperationToken = (AcceptOperationUserToken) acceptEventArgs.UserToken; // wtf is this here for?
                HandleBadAccept(acceptEventArgs); // Kill socket as it might be in a bad state
                return;
            }

            // Things are good, lets start over
            StartAccept();
            var receiveSendEventArgs = _poolOfRecSendEventArgs.Pop();
            receiveSendEventArgs.DataHoldingUserToken().CreateSessionId();
            // A new socket was created by the AcceptAsync method. 
            // The SAEA that did the accept operation has that socket info in its AcceptSocket property.
            receiveSendEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;

            acceptEventArgs.AcceptSocket = null; // clear out accept and push it back on the queue for further accept requests
            _poolOfAcceptEventArgs.Push(acceptEventArgs);
            StartReceive(receiveSendEventArgs);
        }

        private void IoCompleted(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just
            // completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;

                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;

                default:
                    //This exception will occur if you code the Completed event of some operation to come to this method, by mistake.
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void HandleBadAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            var acceptOpToken = (acceptEventArgs.UserToken as AcceptOperationUserToken); // again, wtf?

            //This method closes the socket and releases all resources, both
            //managed and unmanaged. It internally calls Dispose.
            acceptEventArgs.AcceptSocket.Close();

            //Put the SAEA back in the pool.
            _poolOfAcceptEventArgs.Push(acceptEventArgs);
        }

        private void StartReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            receiveSendEventArgs.SetBuffer(((DataHoldingUserToken)receiveSendEventArgs.UserToken).BufferOffsetReceive, _listenerConfiguration.ReceiveBufferSize);

            var willRaiseEvent = receiveSendEventArgs.AcceptSocket.ReceiveAsync(receiveSendEventArgs);

            // willRaiseEvent will return as false if I/O operation completed synchronously
            if (!willRaiseEvent)
                ProcessReceive(receiveSendEventArgs);
        }

        // This method is invoked when an asynchronous receive operation completes.  
        // If the remote host closed the connection, then the socket is closed.   
        // If data was received then the data is echoed back to the client. 
        // 
        private void ProcessReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var dataHoldingUserToken = receiveSendEventArgs.DataHoldingUserToken();
            var remainingBytesToProcess = receiveSendEventArgs.BytesTransferred; //The BytesTransferred property tells us how many bytes we need to process.

            if (SocketIsInInvalidState(receiveSendEventArgs, dataHoldingUserToken)) 
                return;

            if (ClientIsFinishedSendingData(receiveSendEventArgs, dataHoldingUserToken)) 
                return;

            if (PrefixDataForCurrentMessageStillRemains(receiveSendEventArgs, dataHoldingUserToken, remainingBytesToProcess))
                return;

            // If we have processed the prefix, we can work on the message now.
            // We'll arrive here when we have received enough bytes to read
            // the first byte after the prefix.
            ProcessReceive(receiveSendEventArgs, dataHoldingUserToken, remainingBytesToProcess);
        }

        private void ProcessReceive(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken dataHoldingUserToken, int remainingBytesToProcess)
        {
            bool incomingTcpMessageIsReady = _messageHandler.HandleMessage(receiveSendEventArgs, dataHoldingUserToken, remainingBytesToProcess);

            if (incomingTcpMessageIsReady)
            {
                // Pass the DataHolder object to the Mediator here. The data in
                // this DataHolder can be used for all kinds of things that an
                // intelligent and creative person like you might think of.
                dataHoldingUserToken.Mediator.HandleData(dataHoldingUserToken.DataHolder);

                // Create a new DataHolder for next message.
                dataHoldingUserToken.CreateNewDataHolder();

                //Reset the variables in the UserToken, to be ready for the
                //next message that will be received on the socket in this SAEA object.
                dataHoldingUserToken.Reset();
                dataHoldingUserToken.Mediator.PrepareOutgoingData();
                StartSend(dataHoldingUserToken.Mediator.GiveBack());
            }
            else
            {
                // Since we have NOT gotten enough bytes for the whole message,
                // we need to do another receive op. Reset some variables first.

                // All of the data that we receive in the next receive op will be
                // message. None of it will be prefix. So, we need to move the
                // receiveSendToken.receiveMessageOffset to the beginning of the
                // receive buffer space for this SAEA.
                dataHoldingUserToken.ReceiveMessageOffset = dataHoldingUserToken.BufferOffsetReceive;

                // Do NOT reset receiveSendToken.receivedPrefixBytesDoneCount here.
                // Just reset recPrefixBytesDoneThisOp.
                dataHoldingUserToken.RecPrefixBytesDoneThisOperation = 0;

                // Since we have not gotten enough bytes for the whole message,
                // we need to do another receive op.
                StartReceive(receiveSendEventArgs);
            }
        }

        private void StartSend(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var dataHoldingUserToken = receiveSendEventArgs.DataHoldingUserToken();

            //The number of bytes to send depends on whether the message is larger than
            //the buffer or not. If it is larger than the buffer, then we will have
            //to post more than one send operation. If it is less than or equal to the
            //size of the send buffer, then we can accomplish it in one send op.
            if (dataHoldingUserToken.SendBytesRemainingCount <= _listenerConfiguration.ReceiveBufferSize)
            {
                receiveSendEventArgs.SetBuffer(dataHoldingUserToken.BufferOffsetSend, dataHoldingUserToken.SendBytesRemainingCount);
                CopyDataToBufferAssociatedWithSaeaObject(receiveSendEventArgs, dataHoldingUserToken);
            }
            else
            {
                //We cannot try to set the buffer any larger than its size.
                //So since receiveSendToken.sendBytesRemainingCount > BufferSize, we just
                //set it to the maximum size, to send the most data possible.
                receiveSendEventArgs.SetBuffer(dataHoldingUserToken.BufferOffsetSend, _listenerConfiguration.ReceiveBufferSize);
                CopyDataToBufferAssociatedWithSaeaObject(receiveSendEventArgs, dataHoldingUserToken);

                //We'll change the value of sendUserToken.sendBytesRemainingCount in the ProcessSend method.
            }

            //post asynchronous send operation
            bool willRaiseEvent = receiveSendEventArgs.AcceptSocket.SendAsync(receiveSendEventArgs);

            if (!willRaiseEvent)
                ProcessSend(receiveSendEventArgs);
        }

        private static void CopyDataToBufferAssociatedWithSaeaObject(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken dataHoldingUserToken)
        {
            Buffer.BlockCopy(dataHoldingUserToken.DataToSend,
                             dataHoldingUserToken.BytesSentAlreadyCount,
                             receiveSendEventArgs.Buffer,
                             dataHoldingUserToken.BufferOffsetSend,
                             dataHoldingUserToken.SendBytesRemainingCount);
        }

        private bool SocketIsInInvalidState(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken dataHoldingUserToken)
        {
            if (receiveSendEventArgs.SocketError != SocketError.Success)
            {
                dataHoldingUserToken.Reset();
                CloseClientSocket(receiveSendEventArgs);
                return true;
            }

            return false;
        }

        private bool ClientIsFinishedSendingData(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken dataHoldingUserToken)
        {
            // If no data was received, close the connection. This is a NORMAL
            // situation that shows when the client has finished sending data.
            if (receiveSendEventArgs.BytesTransferred == 0)
            {
                dataHoldingUserToken.Reset();
                CloseClientSocket(receiveSendEventArgs);
                return true;
            }

            return false;
        }

        private bool PrefixDataForCurrentMessageStillRemains(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken dataHoldingUserToken, int remainingBytesToProcess)
        {
            //If we have not got all of the prefix already, then we need to work on it here.
            if (dataHoldingUserToken.ReceivedPrefixBytesDoneCount < _listenerConfiguration.ReceivePrefixLength)
            {
                remainingBytesToProcess = _prefixHandler.HandlePrefix(receiveSendEventArgs, dataHoldingUserToken, remainingBytesToProcess);

                if (remainingBytesToProcess == 0)
                {
                    // We need to do another receive op, since we do not have the message yet, but remainingBytesToProcess == 0.
                    StartReceive(receiveSendEventArgs);
                    return true;
                }
            }

            return false;
        }

        private void ProcessSend(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var receiveSendToken = receiveSendEventArgs.DataHoldingUserToken();

            receiveSendToken.SendBytesRemainingCount = receiveSendToken.SendBytesRemainingCount - receiveSendEventArgs.BytesTransferred;
            receiveSendToken.BytesSentAlreadyCount += receiveSendEventArgs.BytesTransferred;

            if (receiveSendEventArgs.SocketError == SocketError.Success)
            {
                if (receiveSendToken.SendBytesRemainingCount == 0)
                    StartReceive(receiveSendEventArgs);
                else
                {
                    //If some of the bytes in the message have NOT been sent,
                    //then we will need to post another send operation. So let's loop back to StartSend().
                    StartSend(receiveSendEventArgs);
                }
            }
            else
            {
                //If we are in this else-statement, there was a socket error.
                //In this example we'll just close the socket if there was a socket error
                //when receiving data from the client.
                receiveSendToken.Reset();
                CloseClientSocket(receiveSendEventArgs);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            var receiveSendToken = (e.UserToken as DataHoldingUserToken);

            // do a shutdown before you close the socket
            try
            {
                e.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            // throws if socket was already closed
            catch (Exception)
            {
            }

            //This method closes the socket and releases all resources, both
            //managed and unmanaged. It internally calls Dispose.
            e.AcceptSocket.Close();

            //Make sure the new DataHolder has been created for the next connection.
            //If it has, then dataMessageReceived should be null.
            if (receiveSendToken != null && receiveSendToken.DataHolder.DataMessageReceived != null)
            {
                receiveSendToken.CreateNewDataHolder();
            }

            // Put the SocketAsyncEventArg back into the pool,
            // to be used by another client. This
            _poolOfRecSendEventArgs.Push(e);

            // decrement the counter keeping track of the total number of clients
            //connected to the server, for testing
            var numberOfAcceptedSockets = NumberOfAcceptedSockets;
            Interlocked.Decrement(ref numberOfAcceptedSockets);

            //Release Semaphore so that its connection counter will be decremented.
            //This must be done AFTER putting the SocketAsyncEventArg back into the pool,
            //or you can run into problems.
            _maxNumberAcceptedClients.Release();
        }

        protected int NumberOfAcceptedSockets { get; set; }
    }

    public class AcceptOperationUserToken
    {
        private readonly int _tokenId;

        public AcceptOperationUserToken(int tokenId)
        {
            _tokenId = tokenId;
        }
    }
}