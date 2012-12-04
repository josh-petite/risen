using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Risen.Shared.Extensions;
using Risen.Shared.Tcp;
using Risen.Shared.Tcp.Factories;
using Risen.Shared.Tcp.Tokens;

namespace Risen.Client.Tcp
{
    public interface ISocketClient
    {
    }

    public class SocketClient : ISocketClient
    {
        private readonly object _mutex = new object();

        private readonly IClientConfiguration _clientConfiguration;
        private readonly IBufferManager _bufferManager;
        private readonly IPrefixHandler _prefixHandler;
        private readonly IMessageHandler _messageHandler;
        private readonly IMessagePreparer _messagePreparer;
        private readonly ISocketAsyncEventArgsFactory _socketAsyncEventArgsFactory;
        private readonly ISocketAsyncEventArgsPoolFactory _socketAsyncEventArgsPoolFactory;
        private Semaphore _maxConnectionsEnforcer;
        private ISocketAsyncEventArgsPool _poolOfAcceptEventArgs; // pool of reusable SocketAsyncEventArgs objects for accept operations
        private ISocketAsyncEventArgsPool _poolOfRecSendEventArgs; // pool of reusable SocketAsyncEventArgs objects for receive and send socket operations

        private const int PrefixLength = 4; // number of bytes in both send/receive prefix length

        public SocketClient(IClientConfiguration clientConfiguration, IPrefixHandler prefixHandler, IMessageHandler messageHandler, IMessagePreparer messagePreparer, IBufferManager bufferManager,
            ISocketAsyncEventArgsFactory socketAsyncEventArgsFactory, ISocketAsyncEventArgsPoolFactory socketAsyncEventArgsPoolFactory)
        {
            _clientConfiguration = clientConfiguration;
            _bufferManager = bufferManager;
            _socketAsyncEventArgsFactory = socketAsyncEventArgsFactory;
            _socketAsyncEventArgsPoolFactory = socketAsyncEventArgsPoolFactory;
            _prefixHandler = prefixHandler;
            _messageHandler = messageHandler;
            _messagePreparer = messagePreparer;

            _maxConnectionsEnforcer = new Semaphore(clientConfiguration.MaxNumberOfConnections, clientConfiguration.MaxNumberOfConnections);
            Init();
        }

        private void Init()
        {
            InitializeBuffer();
            InitializePoolOfAcceptEventArgs();
            InitializePoolOfRecSendEventArgs();
        }

        private void InitializeBuffer()
        {
            _bufferManager.InitBuffer();
        }

        private void InitializePoolOfAcceptEventArgs()
        {
            _poolOfAcceptEventArgs = _socketAsyncEventArgsPoolFactory.GenerateSocketAsyncEventArgsPool(_clientConfiguration.MaxSimultaneousAcceptOperations);

            for (int i = 0; i < _clientConfiguration.MaxSimultaneousAcceptOperations; i++)
                _poolOfAcceptEventArgs.Push(CreateNewSaeaForAccept());
        }

        private void InitializePoolOfRecSendEventArgs()
        {

        }

        private SocketAsyncEventArgs CreateNewSaeaForAccept()
        {
            return _socketAsyncEventArgsFactory.GenerateAcceptSocketAsyncEventArgs(AcceptCompleted, _poolOfAcceptEventArgs.AssignTokenId());
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessLastOperation(e);
        }

        private void ProcessLastOperation(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            switch (socketAsyncEventArgs.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(socketAsyncEventArgs);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(socketAsyncEventArgs);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(socketAsyncEventArgs);
                    break;
                case SocketAsyncOperation.Disconnect:
                    ProcessDisconnectAndCloseSocket(socketAsyncEventArgs);
                    break;
                default:
                    {
                        var receiveSendToken = (DataHoldingUserToken) socketAsyncEventArgs.UserToken;
                        throw new ArgumentException("\r\nError in I/O Completed, id = " + receiveSendToken.TokenId);
                    }
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs connectEventArgs)
        {
            var theConnectingToken = (ConnectOpUserToken)connectEventArgs.UserToken;

            if (connectEventArgs.SocketError == SocketError.Success)
            {
                var receiveSendEventArgs = _poolOfRecSendEventArgs.Pop();
                receiveSendEventArgs.AcceptSocket = connectEventArgs.AcceptSocket;

                //Earlier, in the UserToken of connectEventArgs we put an array 
                //of messages to send. Now we move that array to the DataHolder in
                //the UserToken of receiveSendEventArgs.
                var receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
                receiveSendToken.DataHolder.PutMessagesToSend(theConnectingToken.OutgoingMessageHolder.ArrayOfMessages);

                if (Program.showConnectAndDisconnect == true)
                {
                    Program.testWriter.WriteLine("ProcessConnect connect id " + theConnectingToken.TokenId + " socket info now passing to\r\n   sendReceive id " + receiveSendToken.TokenId + ", local endpoint = " + IPAddress.Parse(((IPEndPoint)connectEventArgs.AcceptSocket.LocalEndPoint).Address.ToString()) + ": " + ((IPEndPoint)connectEventArgs.AcceptSocket.LocalEndPoint).Port.ToString() + ". Clients connected to server from this machine = " + this.clientsNowConnectedCount);
                }

                messagePreparer.GetDataToSend(receiveSendEventArgs);
                StartSend(receiveSendEventArgs);

                //release connectEventArgs object back to the pool.
                connectEventArgs.AcceptSocket = null;
                this.poolOfConnectEventArgs.Push(connectEventArgs);

                if (Program.watchProgramFlow == true)   //for testing
                {
                    Program.testWriter.WriteLine("back to pool for connection object " + theConnectingToken.TokenId);
                }
            }

                //This else statement is when there was a socket error
            else
            {
                ProcessConnectionError(connectEventArgs);
            }
        }

        private void ProcessConnectionError(SocketAsyncEventArgs connectEventArgs)
        {

            var theConnectingToken = (ConnectOpUserToken)connectEventArgs.UserToken;

            if (Program.watchProgramFlow == true)   //for testing
            {
                Program.testWriter.WriteLine("ProcessConnectionError() id = " + theConnectingToken.TokenId + ". ERROR: " + connectEventArgs.SocketError.ToString());
            }
            else if (Program.writeErrorsToLog == true)
            {
                Program.testWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " ProcessConnectionError() id = " + theConnectingToken.TokenId + ". ERROR: " + connectEventArgs.SocketError.ToString());
            }

            Interlocked.Increment(ref this.totalNumberOfConnectionRetries);

            // If connection was refused by server or timed out or not reachable, then we'll keep this socket.
            // If not, then we'll destroy it.
            if ((connectEventArgs.SocketError != SocketError.ConnectionRefused) && (connectEventArgs.SocketError != SocketError.TimedOut) && (connectEventArgs.SocketError != SocketError.HostUnreachable))
            {
                CloseSocket(connectEventArgs.AcceptSocket);
            }

            if (Program.continuallyRetryConnectIfSocketError == true)
            {
                // Since we did not send the messages, let's put them back in the stack.
                // We cannot leave them in the SAEA for connect ops, because the SAEA 
                // could get pushed down in the stack and not reached.

                // If runLongTest == true, we reuse the same array of messages. So in that case
                // we do NOT need to put the array back in the BlockingStack.
                // But if runLongTest == false, we need to put the array of messages back in 
                // the blocking stack, so that it will be taken out and sent.
                if (Program.runLongTest == true)
                {
                    this.counterForLongTest.Release();
                }
                else
                {
                    this.stackOfOutgoingMessages.Push(theConnectingToken.OutgoingMessageHolder);
                    this.poolOfConnectEventArgs.Push(connectEventArgs);
                }
            }
            else
            {
                //it is time to release connectEventArgs object back to the pool.
                this.poolOfConnectEventArgs.Push(connectEventArgs);

                if (Program.watchProgramFlow == true)   //for testing
                {
                    Program.testWriter.WriteLine("back to pool for socket and SAEA " + theConnectingToken.TokenId);
                }

                Interlocked.Increment(ref this.totalNumberOfConnectionsFinished);
                //If we are not retrying the failed connections, then we need to
                //account for them here, when deciding whether we have finished
                //the test.
                if (this.totalNumberOfConnectionsFinished == this.socketClientSettings.ConnectionsToRun)
                {

                    FinishTest();
                }
            }

            _maxConnectionsEnforcer.Release();
        }

        private void ProcessReceive(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            throw new NotImplementedException();
        }

        private void ProcessSend(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            throw new NotImplementedException();
        }

        private void ProcessDisconnectAndCloseSocket(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            throw new NotImplementedException();
        }
    }
}
