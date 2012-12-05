using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Risen.Client.Tcp.Extensions;
using Risen.Client.Tcp.Factories;
using Risen.Client.Tcp.Tokens;
using Risen.Shared.Tcp;
using Risen.Shared.Tcp.Factories;

namespace Risen.Client.Tcp
{
    public interface ISocketClient
    {
        void CleanUpOnExit();
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
        private readonly IClientDataUserTokenFactory _clientDataUserTokenFactory;
        private readonly Semaphore _maxConnectionsEnforcer;
        
        private ISocketAsyncEventArgsPool _poolOfConnectEventArgs; // pool of reusable SocketAsyncEventArgs objects for connect operations
        private ISocketAsyncEventArgsPool _poolOfRecSendEventArgs; // pool of reusable SocketAsyncEventArgs objects for receive and send socket operations
        private int _totalNumberOfConnectionRetries;
        private BlockingStack<OutgoingMessageHolder> _outgoingMessages;

        public SocketClient(IClientConfiguration clientConfiguration, IPrefixHandler prefixHandler, IMessageHandler messageHandler, IMessagePreparer messagePreparer, IBufferManager bufferManager,
            ISocketAsyncEventArgsFactory socketAsyncEventArgsFactory, ISocketAsyncEventArgsPoolFactory socketAsyncEventArgsPoolFactory, IClientDataUserTokenFactory clientDataUserTokenFactory)
        {
            _clientConfiguration = clientConfiguration;
            _bufferManager = bufferManager;
            _socketAsyncEventArgsFactory = socketAsyncEventArgsFactory;
            _socketAsyncEventArgsPoolFactory = socketAsyncEventArgsPoolFactory;
            _clientDataUserTokenFactory = clientDataUserTokenFactory;
            _prefixHandler = prefixHandler;
            _messageHandler = messageHandler;
            _messagePreparer = messagePreparer;

            _maxConnectionsEnforcer = new Semaphore(clientConfiguration.MaxNumberOfConnections, clientConfiguration.MaxNumberOfConnections);
            Init();
        }

        public const int PrefixLength = 4; // number of bytes in both send/receive prefix length

        private void Init()
        {
            InitializeBuffer();
            InitializePoolOfConnectEventArgs();
            InitializePoolOfRecSendEventArgs();
        }

        private void InitializeBuffer()
        {
            _bufferManager.InitBuffer();
        }

        private void InitializePoolOfConnectEventArgs()
        {
            _poolOfConnectEventArgs = _socketAsyncEventArgsPoolFactory.GenerateSocketAsyncEventArgsPool(_clientConfiguration.MaxConnectOperations);

            for (int i = 0; i < _clientConfiguration.MaxConnectOperations; i++)
                _poolOfConnectEventArgs.Push(CreateNewSaeaForAccept());
        }

        private void InitializePoolOfRecSendEventArgs()
        {
            _poolOfRecSendEventArgs = _socketAsyncEventArgsPoolFactory.GenerateSocketAsyncEventArgsPool(_clientConfiguration.NumberOfSaeaForRecSend);

            for (int i = 0; i < _clientConfiguration.NumberOfSaeaForRecSend; i++)
            {
                var eventArgForPool = _socketAsyncEventArgsFactory.GenerateReceiveSendSocketAsyncEventArgs(SocketIoCompleted);
                _bufferManager.SetBuffer(eventArgForPool);
                var token = _clientDataUserTokenFactory.GenerateClientDataUserToken(eventArgForPool, _clientConfiguration.BufferSize, _poolOfRecSendEventArgs.AssignTokenId());
                token.CreateNewDataHolder();
                eventArgForPool.UserToken = token;
                _poolOfRecSendEventArgs.Push(eventArgForPool);
            }
        }
        
        private SocketAsyncEventArgs CreateNewSaeaForAccept()
        {
            return _socketAsyncEventArgsFactory.GenerateAcceptSocketAsyncEventArgs(SocketIoCompleted, _poolOfConnectEventArgs.AssignTokenId());
        }

        private void SocketIoCompleted(object sender, SocketAsyncEventArgs e)
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
                        var receiveSendToken = (IClientDataUserToken) socketAsyncEventArgs.UserToken;
                        throw new ArgumentException("\r\nError in I/O Completed, id = " + receiveSendToken.TokenId);
                    }
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs connectEventArgs)
        {
            var connectOperationUserToken = (ConnectOperationUserToken)connectEventArgs.UserToken;

            if (connectEventArgs.SocketError == SocketError.Success)
            {
                var receiveSendEventArgs = _poolOfRecSendEventArgs.Pop();
                receiveSendEventArgs.AcceptSocket = connectEventArgs.AcceptSocket;

                //Earlier, in the UserToken of connectEventArgs we put an array 
                //of messages to send. Now we move that array to the DataHolder in
                //the UserToken of receiveSendEventArgs.
                var receiveSendToken = (IClientDataUserToken)receiveSendEventArgs.UserToken;
                receiveSendToken.DataHolder.AsClientDataHolder().SetMessagesToSend(connectOperationUserToken.OutgoingMessageHolder.ArrayOfMessages);

                _messagePreparer.GetDataToSend(receiveSendEventArgs);
                StartSend(receiveSendEventArgs);

                //release connectEventArgs object back to the pool.
                connectEventArgs.AcceptSocket = null;
                _poolOfConnectEventArgs.Push(connectEventArgs);
            }

                //This else statement is when there was a socket error
            else
            {
                ProcessConnectionError(connectEventArgs);
            }
        }

        private void StartSend(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var receiveSendToken = (ClientDataUserToken)receiveSendEventArgs.UserToken;

            if (receiveSendToken.SendBytesRemaining <= _clientConfiguration.BufferSize)
            {
                receiveSendEventArgs.SetBuffer(receiveSendToken.BufferSendOffset, receiveSendToken.SendBytesRemaining);

                //Copy the bytes to the buffer associated with this SAEA object.
                Buffer.BlockCopy(receiveSendToken.DataToSend, receiveSendToken.BytesSentAlready, receiveSendEventArgs.Buffer, receiveSendToken.BufferSendOffset, receiveSendToken.SendBytesRemaining);
            }
            else
            {
                //We cannot try to set the buffer any larger than its size.
                //So since receiveSendToken.sendBytesRemaining > its size, we just
                //set it to the maximum size, to send the most data possible.
                receiveSendEventArgs.SetBuffer(receiveSendToken.BufferSendOffset, _clientConfiguration.BufferSize);
                //Copy the bytes to the buffer associated with this SAEA object.
                Buffer.BlockCopy(receiveSendToken.DataToSend, receiveSendToken.BytesSentAlready, receiveSendEventArgs.Buffer, receiveSendToken.BufferSendOffset, _clientConfiguration.BufferSize);

                //We'll change the value of sendUserToken.sendBytesRemaining
                //in the ProcessSend method.
            }

            //post the send
            bool willRaiseEvent = receiveSendEventArgs.AcceptSocket.SendAsync(receiveSendEventArgs);

            if (!willRaiseEvent)
            {
                Debug.WriteLine(" StartSend in if (!willRaiseEvent), id = " + receiveSendToken.TokenId);
                ProcessSend(receiveSendEventArgs);
            }
        }

        private void ProcessConnectionError(SocketAsyncEventArgs connectEventArgs)
        {
            var theConnectingToken = (ConnectOperationUserToken) connectEventArgs.UserToken;
            Interlocked.Increment(ref _totalNumberOfConnectionRetries);

            if (connectEventArgs.SocketError != SocketError.ConnectionRefused
                && connectEventArgs.SocketError != SocketError.TimedOut
                && connectEventArgs.SocketError != SocketError.HostUnreachable)
            {
                CloseSocket(connectEventArgs.AcceptSocket);
            }

            if (_clientConfiguration.ContinuallyRetryConnectIfSocketError)
            {
                // Since we did not send the messages, let's put them back in the stack.
                // We cannot leave them in the SAEA for connect ops, because the SAEA 
                // could get pushed down in the stack and not reached.
                _outgoingMessages.Push(theConnectingToken.OutgoingMessageHolder);
                _poolOfConnectEventArgs.Push(connectEventArgs);

            }
            else
            {
                //it is time to release connectEventArgs object back to the pool.
                _poolOfConnectEventArgs.Push(connectEventArgs);
            }

            _maxConnectionsEnforcer.Release();
        }

        private void CloseSocket(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }

            socket.Close();
        }

        private void ProcessReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var receiveSendToken = (IClientDataUserToken)receiveSendEventArgs.UserToken;

            // If there was a socket error, close the connection.
            if (ReceiveResultedInError(receiveSendEventArgs, receiveSendToken))
                return;

            //If no data was received, close the connection.
            if (NoDataWasReceived(receiveSendEventArgs, receiveSendToken))
                return;

            var remainingBytesToProcess = receiveSendEventArgs.BytesTransferred;

            // If we have not got all of the prefix then we need to work on it. 
            // receivedPrefixBytesDoneCount tells us how many prefix bytes were
            // processed during previous receive ops which contained data for 
            // this message. (In normal use, usually there will NOT have been any 
            // previous receive ops here. So receivedPrefixBytesDoneCount would be 0.)
            if (ReceivedPrefixBytesIsLessThanPrefixLength(receiveSendEventArgs, receiveSendToken, ref remainingBytesToProcess))
                return;

            // If we have processed the prefix, we can work on the message now.
            // We'll arrive here when we have received enough bytes to read
            // the first byte after the prefix.
            bool incomingTcpMessageIsReady = _messageHandler.HandleMessage(receiveSendEventArgs, receiveSendToken, remainingBytesToProcess);

            if (incomingTcpMessageIsReady)
            {
                //In the design of our SocketClient used for testing the
                //DataHolder can contain data for multiple messages. That is 
                //different from the server design, where we have one DataHolder
                //for one message.

                //If we have set runLongTest to true, then we will assume that
                //we cannot put the data in memory, because there would be too much
                //data. So we'll just skip writing that data, in that case. We
                //write it when runLongTest == false.
                
                //Write to DataHolder.
                receiveSendToken.DataHolder.AsClientDataHolder().MessagesReceived.Add(receiveSendToken.DataHolder.DataMessageReceived);

                // ******
                // ******
                // need to do something with the data here
                // ******
                // ******


                //null out the byte array, for the next message
                receiveSendToken.DataHolder.DataMessageReceived = null;

                //Reset the variables in the UserToken, to be ready for the
                //next message that will be received on the socket in this
                //SAEA object.
                receiveSendToken.Reset();

                //If we have not sent all the messages, get the next message, and
                //loop back to StartSend.
                if (receiveSendToken.DataHolder.AsClientDataHolder().NumberOfMessagesSent < _clientConfiguration.NumberOfMessagesPerConnection)
                {
                    //No need to reset the buffer for send here.
                    //It is reset in the StartSend method.
                    _messagePreparer.GetDataToSend(receiveSendEventArgs);
                    StartSend(receiveSendEventArgs);
                }
                else
                {
                    //Since we have sent all the messages that we planned to send,
                    //time to disconnect.                    
                    StartDisconnect(receiveSendEventArgs);
                }
            }
            else
            {
                // Since we have NOT gotten enough bytes for the whole message,
                // we need to do another receive op. Reset some variables first.

                // All of the data that we receive in the next receive op will be
                // message. None of it will be prefix. So, we need to move the 
                // receiveSendToken.receiveMessageOffset to the beginning of the 
                // buffer space for this SAEA.
                receiveSendToken.ReceiveMessageOffset = receiveSendToken.BufferReceiveOffset;

                // Do NOT reset receiveSendToken.receivedPrefixBytesDoneCount here.
                // Just reset recPrefixBytesDoneThisOp.
                receiveSendToken.ReceivePrefixBytesDoneThisOperation = 0;

                StartReceive(receiveSendEventArgs);
            }
        }

        private bool ReceivedPrefixBytesIsLessThanPrefixLength(SocketAsyncEventArgs receiveSendEventArgs, IClientDataUserToken receiveSendToken, ref int remainingBytesToProcess)
        {
            if (receiveSendToken.ReceivedPrefixBytesDoneCount < _clientConfiguration.ReceivePrefixLength)
            {
                remainingBytesToProcess = _prefixHandler.HandlePrefix(receiveSendEventArgs, receiveSendToken, remainingBytesToProcess);

                if (remainingBytesToProcess == 0)
                {
                    // We need to do another receive op, since we do not have
                    // the message yet.
                    StartReceive(receiveSendEventArgs);

                    //Jump out of the method, since there is no more data.
                    return true;
                }
            }
            return false;
        }

        private bool NoDataWasReceived(SocketAsyncEventArgs receiveSendEventArgs, IClientDataUserToken receiveSendToken)
        {
            if (receiveSendEventArgs.BytesTransferred == 0)
            {
                receiveSendToken.Reset();
                StartDisconnect(receiveSendEventArgs);
                return true;
            }
            return false;
        }

        private bool ReceiveResultedInError(SocketAsyncEventArgs receiveSendEventArgs, IClientDataUserToken receiveSendToken)
        {
            if (receiveSendEventArgs.SocketError != SocketError.Success)
            {
                receiveSendToken.Reset();
                StartDisconnect(receiveSendEventArgs);
                return true;
            }
            return false;
        }

        private void ProcessSend(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var receiveSendToken = (IClientDataUserToken)receiveSendEventArgs.UserToken;

            if (receiveSendEventArgs.SocketError == SocketError.Success)
            {
                receiveSendToken.SendBytesRemaining = receiveSendToken.SendBytesRemaining - receiveSendEventArgs.BytesTransferred;
                // If this if statement is true, then we have sent all of the
                // bytes in the message. Otherwise, at least one more send
                // operation will be required to send the data.
                if (receiveSendToken.SendBytesRemaining == 0)
                {
                    
                    //incrementing count of messages sent on this connection                
                    receiveSendToken.DataHolder.AsClientDataHolder().NumberOfMessagesSent++;
                    StartReceive(receiveSendEventArgs);
                }
                else
                {
                    // So since (receiveSendToken.sendBytesRemaining == 0) is false,
                    // we have more bytes to send for this message. So we need to 
                    // call StartSend, so we can post another send message.
                    receiveSendToken.BytesSentAlready += receiveSendEventArgs.BytesTransferred;
                    StartSend(receiveSendEventArgs);
                }
            }
            else
            {
                // We'll just close the socket if there was a
                // socket error when receiving data from the client.
                receiveSendToken.Reset();
                StartDisconnect(receiveSendEventArgs);
            }  
        }

        private void StartReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var receiveSendToken = (IClientDataUserToken)receiveSendEventArgs.UserToken;
            
            //Set buffer for receive.          
            receiveSendEventArgs.SetBuffer(receiveSendToken.BufferReceiveOffset, _clientConfiguration.BufferSize);

            var willRaiseEvent = receiveSendEventArgs.AcceptSocket.ReceiveAsync(receiveSendEventArgs);

            if (!willRaiseEvent)
                ProcessReceive(receiveSendEventArgs);
        }

        private void StartDisconnect(SocketAsyncEventArgs receiveSendEventArgs)
        {
            receiveSendEventArgs.AcceptSocket.Shutdown(SocketShutdown.Both);
            var willRaiseEvent = receiveSendEventArgs.AcceptSocket.DisconnectAsync(receiveSendEventArgs);
            
            if (!willRaiseEvent)
                ProcessDisconnectAndCloseSocket(receiveSendEventArgs);
        }

        private void ProcessDisconnectAndCloseSocket(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var receiveSendToken = (IClientDataUserToken)receiveSendEventArgs.UserToken;

            //This method closes the socket and releases all resources, both
            //managed and unmanaged. It internally calls Dispose.
            receiveSendEventArgs.AcceptSocket.Close();

            //create an object that we can write data to.
            receiveSendToken.CreateNewDataHolder();

            // It is time to release this SAEA object.
            _poolOfRecSendEventArgs.Push(receiveSendEventArgs);

            
            _maxConnectionsEnforcer.Release();
        }

        public void CleanUpOnExit()
        {
            DisposeAllSaeaObjects();
        }

        private void DisposeAllSaeaObjects()
        {
            SocketAsyncEventArgs eventArgs;
            
            while (_poolOfConnectEventArgs.Count > 0)
            {
                eventArgs = _poolOfConnectEventArgs.Pop();
                eventArgs.Dispose();
            }

            while (_poolOfRecSendEventArgs.Count > 0)
            {
                eventArgs = _poolOfRecSendEventArgs.Pop();
                eventArgs.Dispose();
            }
        }
    }
    
}
