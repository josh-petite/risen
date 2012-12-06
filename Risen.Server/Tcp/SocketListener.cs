using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Risen.Server.Extentions;
using Risen.Server.Tcp.Factories;
using Risen.Server.Tcp.Tokens;
using Risen.Shared.Tcp;
using Risen.Shared.Tcp.Factories;
using Risen.Shared.Tcp.Tokens;

namespace Risen.Server.Tcp
{
    public interface ISocketListener
    {
        void Init();
        void StartListen();
        void CleanUpOnExit();
    }

    public class SocketListener : ISocketListener
    {
        private int _numberOfAcceptedSockets;
        private readonly IBufferManager _bufferManager;
        private readonly IListenerConfiguration _listenerConfiguration;
        private readonly IPrefixHandler _prefixHandler;
        private readonly IMessageHandler _messageHandler;
        private readonly ILogger _logger;
        private readonly IDataHoldingUserTokenFactory _dataHoldingUserTokenFactory;
        private readonly ISocketAsyncEventArgsFactory _socketAsyncEventArgsFactory;
        private readonly ISocketAsyncEventArgsPoolFactory _socketAsyncEventArgsPoolFactory;
        private readonly Semaphore _maxConnectionsEnforcer;
        
        private Socket _listenSocket;
        private ISocketAsyncEventArgsPool _poolOfAcceptEventArgs;
        private ISocketAsyncEventArgsPool _poolOfRecSendEventArgs;

        public static int MaxSimultaneousClientsThatWereConnected = 0;
        public long MainSessionId = 1000;
        public const long PacketSizeThreshhold = 50000;

        public SocketListener(IListenerConfiguration listenerConfiguration, IBufferManager bufferManager, IPrefixHandler prefixHandler,
                              IMessageHandler messageHandler, ILogger logger, IDataHoldingUserTokenFactory dataHoldingUserTokenFactory,
                              ISocketAsyncEventArgsFactory socketAsyncEventArgsFactory, ISocketAsyncEventArgsPoolFactory socketAsyncEventArgsPoolFactory)
        {
            logger.WriteLine(LogCategory.Info, "-- Starting SocketListener Constructor --");

            _listenerConfiguration = listenerConfiguration;
            _prefixHandler = prefixHandler;
            _messageHandler = messageHandler;
            _logger = logger;
            _dataHoldingUserTokenFactory = dataHoldingUserTokenFactory;
            _socketAsyncEventArgsFactory = socketAsyncEventArgsFactory;
            _socketAsyncEventArgsPoolFactory = socketAsyncEventArgsPoolFactory;
            _bufferManager = bufferManager;

            _maxConnectionsEnforcer = new Semaphore(listenerConfiguration.MaxNumberOfConnections, listenerConfiguration.MaxNumberOfConnections);
            InitialTransmissionId = listenerConfiguration.MainTransmissionId;

            logger.WriteLine(LogCategory.Info, "-- SocketListener Constructor Complete --");
        }

        public static int InitialTransmissionId { get; set; }

        private void Log(Action action)
        {
            _logger.WriteLine(LogCategory.Info, string.Format("Method: {0} executed", action.Method));
            action.Invoke();
        }

        // Initializes the server by preallocating reusable buffers and  
        // context objects.  These objects do not need to be preallocated  
        // or reused, but it is done this way to illustrate how the API can  
        // easily be used to create reusable objects to increase server performance. 
        public void Init()
        {
            Log(InitializeBufferManager);
            Log(InitializeAcceptEventArgsPool);
            Log(InitializeSendReceiveEventArgsPool);
        }

        private void InitializeBufferManager()
        {
            // Allocates one large byte buffer which all I/O operations use a piece of.  
            // This guards against memory fragmentation
            Log(_bufferManager.InitBuffer);
        }

        private void InitializeAcceptEventArgsPool()
        {
            _poolOfAcceptEventArgs = _socketAsyncEventArgsPoolFactory.GenerateSocketAsyncEventArgsPool(_listenerConfiguration.MaxSimultaneousAcceptOperations);

            for (int i = 0; i < _listenerConfiguration.MaxSimultaneousAcceptOperations; i++)
                _poolOfAcceptEventArgs.Push(CreateNewSaeaForAccept());
        }

        private void InitializeSendReceiveEventArgsPool()
        {
            _poolOfRecSendEventArgs = _socketAsyncEventArgsPoolFactory.GenerateSocketAsyncEventArgsPool(_listenerConfiguration.NumberOfSaeaForRecSend);

            for (int i = 0; i < _listenerConfiguration.NumberOfSaeaForRecSend; i++)
            {
                var eventArgForPool = _socketAsyncEventArgsFactory.GenerateReceiveSendSocketAsyncEventArgs(SendReceiveCompleted);
                eventArgForPool.UserToken = _dataHoldingUserTokenFactory.GenerateDataHoldingUserToken(eventArgForPool, _poolOfRecSendEventArgs.AssignTokenId());
                _poolOfRecSendEventArgs.Push(eventArgForPool);
            }
        }

        private SocketAsyncEventArgs CreateNewSaeaForAccept()
        {
            return _socketAsyncEventArgsFactory.GenerateAcceptSocketAsyncEventArgs(AcceptCompleted, _poolOfAcceptEventArgs.AssignTokenId());
        }

        public void StartListen()
        {
            _listenSocket = new Socket(_listenerConfiguration.LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(_listenerConfiguration.LocalEndPoint);
            _listenSocket.Listen(_listenerConfiguration.Backlog);

            StartAccept();
        }

        public void StartAccept()
        {
            var acceptEventArg = _poolOfAcceptEventArgs.Any() ? _poolOfAcceptEventArgs.Pop() : CreateNewSaeaForAccept();

            // Used to control access to pool resources
            _maxConnectionsEnforcer.WaitOne();

            var willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);

            // AcceptAsync returns true if the I/O operation is pending, i.socketAsyncEventArgs. is working asynchronously.
            // When it completes it will call the acceptEventArg.Completed event (AcceptEventArg_Completed, as wired above).
            if (!willRaiseEvent)
                ProcessAccept(acceptEventArg);
        }

        // This method is the callback method associated with Socket.AcceptAsync  
        // operations and is invoked when an accept operation is complete 
        private void AcceptCompleted(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            ProcessAccept(socketAsyncEventArgs);
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (EventArgsAreInInvalidState(acceptEventArgs)) 
                return;

            TrackMaxNumberOfAcceptedSockets(acceptEventArgs);

            // Things are good, lets start over
            StartAccept();

            var receiveSendEventArgs = _poolOfRecSendEventArgs.Pop();
            receiveSendEventArgs.GetDataHoldingUserToken().CreateSessionId(ref MainSessionId);

            // A new socket was created by the AcceptAsync method. 
            // The SAEA that did the accept operation has that socket info in its AcceptSocket property.
            receiveSendEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;

            var acceptOperationUserToken = (AcceptOperationUserToken)acceptEventArgs.UserToken;
            _logger.WriteLine(LogCategory.Info,
                              string.Format("Accept Id: {0}, RecSend Id: {1}, Remote Endpoint: {2}:{3} *** Client(s) connected = {4}",
                                            acceptOperationUserToken.TokenId,
                                            ((DataHoldingUserToken) receiveSendEventArgs.UserToken).TokenId,
                                            IPAddress.Parse(((IPEndPoint) receiveSendEventArgs.AcceptSocket.RemoteEndPoint).Address.ToString()),
                                            ((IPEndPoint) receiveSendEventArgs.AcceptSocket.RemoteEndPoint).Port,
                                            _numberOfAcceptedSockets));

            acceptEventArgs.ClearAcceptSocket();
            _poolOfAcceptEventArgs.Push(acceptEventArgs);
            _logger.WriteLine(LogCategory.Info, string.Format("Accept Id: {0} goes back to pool.", ((AcceptOperationUserToken)acceptEventArgs.UserToken).TokenId));

            StartReceive(receiveSendEventArgs);
        }

        private void TrackMaxNumberOfAcceptedSockets(SocketAsyncEventArgs acceptEventArgs)
        {
            var maxSimultaneousAcceptOperations = MaxSimultaneousClientsThatWereConnected;
            var numberOfConnectedSockets = Interlocked.Increment(ref _numberOfAcceptedSockets);

            if (numberOfConnectedSockets > maxSimultaneousAcceptOperations)
                Interlocked.Increment(ref MaxSimultaneousClientsThatWereConnected);

            _logger.WriteLine(LogCategory.Info, string.Format("ProcessAccept, Accept Id: {0}", ((AcceptOperationUserToken)acceptEventArgs.UserToken).TokenId));
        }

        private bool EventArgsAreInInvalidState(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs.SocketError != SocketError.Success)
            {
                StartAccept(); // Something failed, try again with a new SocketAsyncEventArgs
                var acceptOperationUserToken = (AcceptOperationUserToken) acceptEventArgs.UserToken;
                _logger.WriteLine(LogCategory.Error, string.Format("*** SocketError *** Accept Id: {0}", acceptOperationUserToken.TokenId));
                HandleBadAccept(acceptEventArgs); // Kill socket as it might be in a bad state
                return true;
            }

            return false;
        }

        private void SendReceiveCompleted(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            // determine which type of operation just completed and call the associated handler
            switch (socketAsyncEventArgs.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(socketAsyncEventArgs);
                    break;

                case SocketAsyncOperation.Send:
                    ProcessSend(socketAsyncEventArgs);
                    break;

                default:
                    //This exception will occur if you code the Completed event of some operation to come to this method, by mistake.
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void HandleBadAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            _logger.WriteLine(LogCategory.Error, string.Format("Closing socket of Accept Id: {0}", ((AcceptOperationUserToken) acceptEventArgs.UserToken).TokenId));

            //This method closes the socket and releases all resources, both
            //managed and unmanaged. It internally calls Dispose.
            acceptEventArgs.AcceptSocket.Close();

            //Put the SAEA back in the pool.
            _poolOfAcceptEventArgs.Push(acceptEventArgs);
        }

        public void CleanUpOnExit()
        {
            DisposeAllSaeaObjects();
        }

        private void DisposeAllSaeaObjects()
        {
            SocketAsyncEventArgs eventArgs;
            
            while (_poolOfAcceptEventArgs.Count > 0)
            {
                eventArgs = _poolOfAcceptEventArgs.Pop();
                eventArgs.Dispose();
            }

            while (_poolOfRecSendEventArgs.Count > 0)
            {
                eventArgs = _poolOfRecSendEventArgs.Pop();
                eventArgs.Dispose();
            }
        }

        private void StartReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            receiveSendEventArgs.SetBuffer(((DataHoldingUserToken)receiveSendEventArgs.UserToken).BufferReceiveOffset, _listenerConfiguration.BufferSize);

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
            var dataHoldingUserToken = receiveSendEventArgs.GetDataHoldingUserToken();
            
            if (SocketIsInInvalidState(receiveSendEventArgs, dataHoldingUserToken)) 
                return;

            if (ClientIsFinishedSendingData(receiveSendEventArgs, dataHoldingUserToken)) 
                return;

            var remainingBytesToProcess = receiveSendEventArgs.BytesTransferred;

            if (remainingBytesToProcess > PacketSizeThreshhold)
                return;

            if (PrefixDataForCurrentMessageStillRemains(receiveSendEventArgs, dataHoldingUserToken, ref remainingBytesToProcess))
                return;

            // If we have processed the prefix, we can work on the message now.
            // We'll arrive here when we have received enough bytes to read
            // the first byte after the prefix.
            ProcessReceivedMessage(receiveSendEventArgs, dataHoldingUserToken, remainingBytesToProcess);
        }

        private void ProcessReceivedMessage(SocketAsyncEventArgs receiveSendEventArgs, IUserToken userToken, int remainingBytesToProcess)
        {
            bool incomingTcpMessageIsReady = _messageHandler.HandleMessage(receiveSendEventArgs, userToken, remainingBytesToProcess);

            if (incomingTcpMessageIsReady)
            {
                var dataToken = userToken.AsDataHoldingUserToken();
                dataToken.Mediator.HandleData(userToken.DataHolder);

                // at this point, use the data
                var receivedData = Encoding.Default.GetString(dataToken.Mediator.IncomingDataPreparer.);

                // Create a new DataHolder for next message.
                userToken.CreateNewDataHolder();

                //Reset the variables in the UserToken, to be ready for the
                //next message that will be received on the socket in this SAEA object.
                userToken.Reset();
                userToken.AsDataHoldingUserToken().Mediator.PrepareOutgoingData();
                StartSend(userToken.AsDataHoldingUserToken().Mediator.GiveBack());
            }
            else
            {
                // Since we have NOT gotten enough bytes for the whole message,
                // we need to do another receive op. Reset some variables first.

                // All of the data that we receive in the next receive op will be
                // message. None of it will be prefix. So, we need to move the
                // receiveSendToken.receiveMessageOffset to the beginning of the
                // receive buffer space for this SAEA.
                userToken.ReceiveMessageOffset = userToken.BufferReceiveOffset;

                // Do NOT reset receiveSendToken.receivedPrefixBytesDoneCount here.
                // Just reset recPrefixBytesDoneThisOp.
                userToken.RecPrefixBytesDoneThisOperation = 0;

                // Since we have not gotten enough bytes for the whole message,
                // we need to do another receive op.
                StartReceive(receiveSendEventArgs);
            }
        }

        private void StartSend(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var dataHoldingUserToken = receiveSendEventArgs.GetDataHoldingUserToken();

            //The number of bytes to send depends on whether the message is larger than
            //the buffer or not. If it is larger than the buffer, then we will have
            //to post more than one send operation. If it is less than or equal to the
            //size of the send buffer, then we can accomplish it in one send op.
            if (dataHoldingUserToken.SendBytesRemainingCount <= _listenerConfiguration.BufferSize)
            {
                receiveSendEventArgs.SetBuffer(dataHoldingUserToken.BufferOffsetSend, dataHoldingUserToken.SendBytesRemainingCount);
                CopyDataToBufferAssociatedWithSaeaObject(receiveSendEventArgs, dataHoldingUserToken);
            }
            else
            {
                //We cannot try to set the buffer any larger than its size.
                //So since receiveSendToken.sendBytesRemainingCount > BufferSize, we just
                //set it to the maximum size, to send the most data possible.
                receiveSendEventArgs.SetBuffer(dataHoldingUserToken.BufferOffsetSend, _listenerConfiguration.BufferSize);
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
                _logger.WriteLine(LogCategory.Error, string.Format("ProcessReceive, ReceiveSendToken Id: {0}", dataHoldingUserToken.TokenId));
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
                _logger.WriteLine(LogCategory.Info, string.Format("ProcessReceive, NO DATA on Token Id: {0}", dataHoldingUserToken.TokenId));
                dataHoldingUserToken.Reset();
                CloseClientSocket(receiveSendEventArgs);
                return true;
            }

            return false;
        }

        private bool PrefixDataForCurrentMessageStillRemains(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken dataHoldingUserToken, ref int remainingBytesToProcess)
        {
            //If we have not got all of the prefix already, then we need to work on it here.
            if (dataHoldingUserToken.ReceivedPrefixBytesDoneCount < _listenerConfiguration.ReceivePrefixLength)
            {
                remainingBytesToProcess = _prefixHandler.HandlePrefix(receiveSendEventArgs, dataHoldingUserToken, remainingBytesToProcess);
                _logger.WriteLine(LogCategory.Info, string.Format("ProcessReceive, after prefix work token Id: {0}. RemainingBytesToProcess = {1}",
                                                                  dataHoldingUserToken.TokenId,
                                                                  remainingBytesToProcess));
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
            var receiveSendToken = receiveSendEventArgs.GetDataHoldingUserToken();

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

            try // do a shutdown before you close the socket
            {
                e.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception) // throws if socket was already closed
            {
                if (receiveSendToken != null) 
                    _logger.WriteLine(LogCategory.Error, string.Format("Close client socket attempt failed on Id: {0}", receiveSendToken.TokenId));
            }

            // This method closes the socket and releases all resources, both
            // managed and unmanaged. It internally calls Dispose.
            e.AcceptSocket.Close();

            // Make sure the new DataHolder has been created for the next connection.
            // If it has, then dataMessageReceived should be null.
            if (receiveSendToken != null && receiveSendToken.DataHolder.DataMessageReceived != null)
                receiveSendToken.CreateNewDataHolder();

            // Put the SocketAsyncEventArg back into the pool, to be used by another client.
            _poolOfRecSendEventArgs.Push(e);

            Interlocked.Decrement(ref _numberOfAcceptedSockets);

            if (receiveSendToken != null)
                _logger.WriteLine(LogCategory.Info, string.Format("Id: {0} disconnected. {1} client(s) connected.", receiveSendToken.TokenId, _numberOfAcceptedSockets));

            // Release Semaphore so that its connection counter will be decremented.
            // This must be done AFTER putting the SocketAsyncEventArg back into the pool, 
            // or you can run into problems.
            _maxConnectionsEnforcer.Release();
        }
    }
}