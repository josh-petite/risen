//using System;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using Risen.Server.Extentions;
//using Risen.Server.Msmq;
//using Risen.Server.Tcp.Factories;
//using Risen.Server.Tcp.Tokens;

//namespace Risen.Server.Tcp
//{
//    public interface ISocketListener
//    {
//        void Init();
//        void StartListen();
//        void CleanUpOnExit();
//    }

//    public class SocketListener : ISocketListener
//    {
//        private const long PacketSizeThreshhold = 5000;

//        private int _numberOfAcceptedSockets;
//        private readonly IBufferManager _bufferManager;
//        private readonly IServerConfiguration _serverConfiguration;
//        private readonly ILogger _logger;
//        private readonly IPrefixHandler _prefixHandler;
//        private readonly IDataHoldingUserTokenFactory _dataHoldingUserTokenFactory;
//        private readonly ISocketAsyncEventArgsFactory _socketAsyncEventArgsFactory;
//        private readonly ISocketAsyncEventArgsPoolFactory _socketAsyncEventArgsPoolFactory;
//        private readonly IOutboundQueue _outboundQueue;
//        private readonly IMessageHandler _messageHandler;
//        private readonly Semaphore _maxConnectionsEnforcer;
        
//        private Socket _listenSocket;
//        private ISocketAsyncEventArgsPool _poolOfAcceptEventArgs;
//        private ISocketAsyncEventArgsPool _poolOfRecSendEventArgs;

//        public static int MaxSimultaneousClientsThatWereConnected = 0;
//        public long MainSessionId = 1000;

//        public SocketListener(IServerConfiguration serverConfiguration, IBufferManager bufferManager, ILogger logger, IPrefixHandler prefixHandler,
//                              IDataHoldingUserTokenFactory dataHoldingUserTokenFactory, ISocketAsyncEventArgsFactory socketAsyncEventArgsFactory,
//                              ISocketAsyncEventArgsPoolFactory socketAsyncEventArgsPoolFactory, IOutboundQueue outboundQueue, IMessageHandler messageHandler)
//        {
//            _serverConfiguration = serverConfiguration;
//            _logger = logger;
//            _prefixHandler = prefixHandler;
//            _dataHoldingUserTokenFactory = dataHoldingUserTokenFactory;
//            _socketAsyncEventArgsFactory = socketAsyncEventArgsFactory;
//            _socketAsyncEventArgsPoolFactory = socketAsyncEventArgsPoolFactory;
//            _outboundQueue = outboundQueue;
//            _messageHandler = messageHandler;
//            _bufferManager = bufferManager;

//            _maxConnectionsEnforcer = new Semaphore(serverConfiguration.MaxNumberOfConnections, serverConfiguration.MaxNumberOfConnections);
//            InitialTransmissionId = serverConfiguration.MainTransmissionId;
//        }

//        public static int InitialTransmissionId { get; set; }

//        public void Init()
//        {
//            InitializeBufferManager();
//            InitializeAcceptEventArgsPool();
//            InitializeSendReceiveEventArgsPool();
//        }

//        private void InitializeBufferManager()
//        {
//            _bufferManager.InitBuffer();
//        }

//        private void InitializeAcceptEventArgsPool()
//        {
//            _poolOfAcceptEventArgs = _socketAsyncEventArgsPoolFactory.GenerateSocketAsyncEventArgsPool(_serverConfiguration.MaxAcceptOperations);

//            for (int i = 0; i < _serverConfiguration.MaxAcceptOperations; i++)
//                _poolOfAcceptEventArgs.Push(CreateNewSaeaForAccept());
//        }

//        private void InitializeSendReceiveEventArgsPool()
//        {
//            _poolOfRecSendEventArgs = _socketAsyncEventArgsPoolFactory.GenerateSocketAsyncEventArgsPool(_serverConfiguration.NumberOfSaeaForRecSend);

//            for (int i = 0; i < _serverConfiguration.NumberOfSaeaForRecSend; i++)
//            {
//                var eventArgForPool = _socketAsyncEventArgsFactory.GenerateReceiveSendSocketAsyncEventArgs(SendReceiveCompleted);
//                eventArgForPool.UserToken = _dataHoldingUserTokenFactory.GenerateDataHoldingUserToken(eventArgForPool, _poolOfRecSendEventArgs.AssignTokenId());
//                _poolOfRecSendEventArgs.Push(eventArgForPool);
//            }
//        }

//        private SocketAsyncEventArgs CreateNewSaeaForAccept()
//        {
//            return _socketAsyncEventArgsFactory.GenerateAcceptSocketAsyncEventArgs(AcceptCompleted, _poolOfAcceptEventArgs.AssignTokenId());
//        }

//        public void StartListen()
//        {
//            _listenSocket = new Socket(_serverConfiguration.LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
//            _listenSocket.Bind(_serverConfiguration.LocalEndPoint);
//            _listenSocket.Listen(_serverConfiguration.Backlog);
//            Console.WriteLine("****** Server is listening. ******");
//            StartAccept();
//        }

//        public void StartAccept()
//        {
//            SocketAsyncEventArgs acceptEventArg;

//            if (_poolOfAcceptEventArgs.Count > 1)
//            {
//                try
//                {
//                    acceptEventArg = _poolOfAcceptEventArgs.Pop();
//                }
//                catch
//                {
//                    acceptEventArg = CreateNewSaeaForAccept();
//                }
//            }
//            else
//                acceptEventArg = CreateNewSaeaForAccept();

//            // Used to control access to pool resources
//            _maxConnectionsEnforcer.WaitOne();
//            var willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);

//            // AcceptAsync returns true if the I/O operation is pending, i.socketAsyncEventArgs. is working asynchronously.
//            // When it completes it will call the acceptEventArg.Completed event (AcceptEventArg_Completed, as wired above).
//            if (!willRaiseEvent)
//            {
//                var acceptOperationUserToken = (AcceptOperationUserToken)acceptEventArg.UserToken;
//                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
//                                     string.Format("StartAccept: In if (!willRaiseEvent), AcceptOp Token Id: {0}", acceptOperationUserToken.TokenId));

//                ProcessAccept(acceptEventArg);
//            }
//        }

//        // This method is the callback method associated with Socket.AcceptAsync  
//        // operations and is invoked when an accept operation is complete 
//        private void AcceptCompleted(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
//        {
//            ProcessAccept(socketAsyncEventArgs);
//        }

//        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
//        {
//            if (EventArgsAreInInvalidState(acceptEventArgs)) 
//                return;

//            TrackMaxNumberOfAcceptedSockets(acceptEventArgs);

//            // Things are good, lets start over
//            StartAccept();

//            var receiveSendEventArgs = _poolOfRecSendEventArgs.Pop();
//            receiveSendEventArgs.GetDataHoldingUserToken().CreateSessionId(ref MainSessionId);

//            // A new socket was created by the AcceptAsync method. 
//            // The SAEA that did the accept operation has that socket info in its AcceptSocket property.
//            receiveSendEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;

//            var acceptOperationUserToken = (AcceptOperationUserToken)acceptEventArgs.UserToken;
//            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
//                                 string.Format("ProcessAccept: Accept Id: {0}, RecSend Id: {1}, Remote Endpoint: {2}:{3} *** Client(s) connected = {4}",
//                                               acceptOperationUserToken.TokenId,
//                                               ((DataHoldingUserToken) receiveSendEventArgs.UserToken).TokenId,
//                                               IPAddress.Parse(((IPEndPoint) receiveSendEventArgs.AcceptSocket.RemoteEndPoint).Address.ToString()),
//                                               ((IPEndPoint) receiveSendEventArgs.AcceptSocket.RemoteEndPoint).Port,
//                                               _numberOfAcceptedSockets));

//            acceptEventArgs.ClearAcceptSocket();
//            _poolOfAcceptEventArgs.Push(acceptEventArgs);
//            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
//                                 string.Format("ProcessAccept: Accept Id: {0} goes back to pool.", ((AcceptOperationUserToken) acceptEventArgs.UserToken).TokenId));

//            StartReceive(receiveSendEventArgs);
//        }

//        public void StartReceive(SocketAsyncEventArgs receiveSendEventArgs)
//        {
//            var dataHoldingUserToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
//            receiveSendEventArgs.SetBuffer(dataHoldingUserToken.BufferOffsetReceive, _serverConfiguration.BufferSize);

//            var willRaiseEvent = receiveSendEventArgs.AcceptSocket.ReceiveAsync(receiveSendEventArgs);

//            // willRaiseEvent will return as false if I/O operation completed synchronously
//            if (!willRaiseEvent)
//            {
//                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Error, string.Format("StartReceive: In willRaiseEvent if block."));
//                _outboundQueue.EnqueueReceive(receiveSendEventArgs);
//            }
//        }

//        private void TrackMaxNumberOfAcceptedSockets(SocketAsyncEventArgs acceptEventArgs)
//        {
//            var maxSimultaneousAcceptOperations = MaxSimultaneousClientsThatWereConnected;
//            var numberOfConnectedSockets = Interlocked.Increment(ref _numberOfAcceptedSockets);

//            if (numberOfConnectedSockets > maxSimultaneousAcceptOperations)
//                Interlocked.Increment(ref MaxSimultaneousClientsThatWereConnected);

//            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
//                                 string.Format("TrackMaxNumberOfAcceptedSockets: ProcessAccept, Accept Id: {0}",
//                                               ((AcceptOperationUserToken) acceptEventArgs.UserToken).TokenId));
//        }

//        private bool EventArgsAreInInvalidState(SocketAsyncEventArgs acceptEventArgs)
//        {
//            if (acceptEventArgs.SocketError != SocketError.Success)
//            {
//                StartAccept(); // Something failed, try again with a new SocketAsyncEvent
//                var acceptOperationUserToken = (AcceptOperationUserToken) acceptEventArgs.UserToken;
//                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Error,
//                                                       string.Format("EventArgsAreInInvalidState: *** SocketError *** Accept Id: {0}", acceptOperationUserToken.TokenId));
//                HandleBadAccept(acceptEventArgs); // Kill socket as it might be in a bad state
//                return true;
//            }

//            return false;
//        }

//        private void SendReceiveCompleted(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
//        {
//            // determine which type of operation just completed and call the associated handler
//            switch (socketAsyncEventArgs.LastOperation)
//            {
//                case SocketAsyncOperation.Receive:
//                    _outboundQueue.EnqueueReceive(socketAsyncEventArgs);
//                    break;

//                case SocketAsyncOperation.Send:
//                    _outboundQueue.EnqueueSend(socketAsyncEventArgs);
//                    break;

//                default:
//                    //This exception will occur if you code the Completed event of some operation to come to this method, by mistake.
//                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
//            }
//        }

//        private void HandleBadAccept(SocketAsyncEventArgs acceptEventArgs)
//        {
//            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Error,
//                                                   string.Format("HandleBadAccept: Closing socket of Accept Id: {0}", ((AcceptOperationUserToken)acceptEventArgs.UserToken).TokenId));

//            //This method closes the socket and releases all resources, both
//            //managed and unmanaged. It internally calls Dispose.
//            acceptEventArgs.AcceptSocket.Close();

//            //Put the SAEA back in the pool.
//            _poolOfAcceptEventArgs.Push(acceptEventArgs);
//        }

//        // This method is invoked when an asynchronous receive operation completes.  
//        // If the remote host closed the connection, then the socket is closed.   
//        // If data was received then the data is echoed back to the client. 
//        private void ProcessReceive(SocketAsyncEvent socketAsyncEvent)
//        {
//            var dataHoldingUserToken = (DataHoldingUserToken)socketAsyncEvent.Token;
//            LogEventArgsBuffer("****** ProcessReceive", socketAsyncEvent, dataHoldingUserToken);

//            if (SocketIsInInvalidState(socketAsyncEvent, dataHoldingUserToken))
//                return;

//            if (ClientIsFinishedSendingData(socketAsyncEvent, dataHoldingUserToken))
//                return;

//            // fix this
//            EvaluateIncomingMessage(socketAsyncEvent);

//            var remainingBytesToProcess = GetRemainingBytesToProcess(socketAsyncEvent);

//            if (remainingBytesToProcess > PacketSizeThreshhold)
//                return;

//            if (PrefixDataForCurrentMessageStillRemains(socketAsyncEvent, dataHoldingUserToken, ref remainingBytesToProcess))
//                return;

//            // If we have processed the prefix, we can work on the message now.
//            // We'll arrive here when we have received enough bytes to read
//            // the first byte after the prefix.
//            ProcessReceivedMessage(socketAsyncEvent, dataHoldingUserToken, remainingBytesToProcess);
//        }

//        private void EvaluateIncomingMessage(SocketAsyncEvent socketAsyncEvent)
//        {
//            var dataHoldingUserToken = (DataHoldingUserToken)socketAsyncEvent.Token;
//            var incomingMessage = new byte[socketAsyncEvent.BytesTransferred];

//            for (int i = 0; i < socketAsyncEvent.BytesTransferred; i++)
//                incomingMessage[i] = socketAsyncEvent.Buffer[dataHoldingUserToken.BufferOffsetReceive + i];

//            var prefix = incomingMessage.Take(_serverConfiguration.ReceivePrefixLength).ToArray();
//            var messageLength = BitConverter.ToInt32(prefix, 0);
//            var messageByteLength = prefix.Length + messageLength;

//            if (messageByteLength < incomingMessage.Length)
//            {
//                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Error, "*** Multiple messages detected!! Parsing... ***");
//                LogEventArgsBuffer("EvaluateIncomingMessage", socketAsyncEvent, dataHoldingUserToken);
//                var currentMessageOffset = 0;

//                while (incomingMessage.Length > _serverConfiguration.ReceivePrefixLength)
//                {
//                    Console.WriteLine("* Looping *");
//                    //ClearCurrentBufferMessageBlock(socketAsyncEvent, incomingMessage.Length);
//                    //StageMessage(socketAsyncEvent, messageByteLength, incomingMessage);
//                    socketAsyncEvent.SetBuffer(dataHoldingUserToken.BufferOffsetReceive + currentMessageOffset, _serverConfiguration.BufferSize - messageByteLength);
//                    ProcessReceive(socketAsyncEvent);
//                    incomingMessage = incomingMessage.Skip(messageByteLength).ToArray();
//                    currentMessageOffset += messageByteLength;
//                }
//            }
//            else
//            {
//                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug, "Regular ProcessReceive");
//                ProcessReceive(socketAsyncEvent);
//            }
//        }

//        private void StageMessage(SocketAsyncEventArgs socketAsyncEventArgs, int messageByteLength, byte[] incomingMessage)
//        {
//            var currentBuffer = socketAsyncEventArgs.Buffer;

//            for (int i = 0; i < messageByteLength; i++)
//                currentBuffer[socketAsyncEventArgs.Offset + i] = incomingMessage[i];
//        }

//        private void ClearCurrentBufferMessageBlock(SocketAsyncEventArgs socketAsyncEventArgs, int length)
//        {
//            var currentBuffer = socketAsyncEventArgs.Buffer;

//            for (int i = 0; i < length; i++)
//                currentBuffer[socketAsyncEventArgs.Offset + i] = 0;
//        }

//        private int GetRemainingBytesToProcess(SocketAsyncEvent socketAsyncEvent)
//        {
//            var prefix = new ArraySegment<byte>(socketAsyncEvent.Buffer, socketAsyncEvent.Offset, _serverConfiguration.ReceivePrefixLength).ToArray();
//            var messageLength = BitConverter.ToInt32(prefix, 0);
//            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
//                                 String.Format("GetRemainingBytesToProcess: Prefix Length:{0} MessageLength:{1}", prefix.Length, messageLength));
//            return prefix.Length + messageLength;
//        }

//        private void ProcessReceivedMessage(SocketAsyncEvent socketAsyncEvent, DataHoldingUserToken dataToken, int remainingBytesToProcess)
//        {
//            bool incomingTcpMessageIsReady = _messageHandler.HandleMessage(socketAsyncEvent, dataToken, remainingBytesToProcess);

//            if (incomingTcpMessageIsReady)
//            {
//                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
//                                     String.Format("ProcessReceivedMessage: Message in DataHolder: {0}", Encoding.ASCII.GetString(dataToken.DataHolder.DataMessageReceived)));

//                dataToken.Mediator.HandleData(dataToken.DataHolder); // at this point, use the data
//                dataToken.CreateNewDataHolder();
//                dataToken.Reset();
//                dataToken.Mediator.PrepareOutgoingData();
//                StartSend(dataToken.Mediator.GiveBack());
//            }
//            else
//            {
//                dataToken.ReceiveMessageOffset = dataToken.BufferOffsetReceive;
//                dataToken.RecPrefixBytesDoneThisOperation = 0;
//                StartReceive(socketAsyncEvent);
//            }
//        }

//        private void LogEventArgsBuffer(string callingFunction, SocketAsyncEvent socketAsyncEvent, DataHoldingUserToken dataHoldingUserToken)
//        {
//            var output = new byte[socketAsyncEvent.BytesTransferred];

//            for (int i = 0; i < socketAsyncEvent.BytesTransferred; i++)
//                output[i] = socketAsyncEvent.Buffer[dataHoldingUserToken.BufferOffsetReceive + i];

//            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Warning, String.Format("{0}: Bytes Received: {1}", callingFunction, BitConverter.ToString(output)));
//        }

//        private void StartSend(SocketAsyncEvent socketAsyncEvent)
//        {
//            var dataHoldingUserToken = (DataHoldingUserToken)socketAsyncEvent.Token;

//            if (dataHoldingUserToken.SendBytesRemainingCount <= _serverConfiguration.BufferSize)
//            {
//                socketAsyncEvent.SetBuffer(dataHoldingUserToken.BufferOffsetSend, dataHoldingUserToken.SendBytesRemainingCount);
//                Buffer.BlockCopy(dataHoldingUserToken.DataToSend,
//                             dataHoldingUserToken.BytesSentAlreadyCount,
//                             socketAsyncEvent.Buffer,
//                             dataHoldingUserToken.BufferOffsetSend,
//                             dataHoldingUserToken.SendBytesRemainingCount);
//            }
//            else
//            {
//                socketAsyncEvent.SetBuffer(dataHoldingUserToken.BufferOffsetSend, _serverConfiguration.BufferSize);
//                Buffer.BlockCopy(dataHoldingUserToken.DataToSend,
//                                 dataHoldingUserToken.BytesSentAlreadyCount,
//                                 socketAsyncEvent.Buffer,
//                                 dataHoldingUserToken.BufferOffsetSend,
//                                 _serverConfiguration.BufferSize);

//                //We'll change the value of sendUserToken.sendBytesRemainingCount in the ProcessSend method.
//            }

//            //post asynchronous send operation
//            var willRaiseEvent = socketAsyncEvent.AcceptSocket.SendAsync(socketAsyncEvent.EventArgs);

//            if (!willRaiseEvent)
//                ProcessSend(socketAsyncEvent);
//        }

//        private bool SocketIsInInvalidState(SocketAsyncEvent socketAsyncEvent, DataHoldingUserToken dataHoldingUserToken)
//        {
//            if (socketAsyncEvent.SocketError != SocketError.Success)
//            {
//                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Error,
//                                     String.Format("SocketIsInInvalidState: ReceiveSendToken Id: {0}", dataHoldingUserToken.TokenId));
//                dataHoldingUserToken.Reset();
//                CloseClientSocket(socketAsyncEvent);
//                return true;
//            }

//            return false;
//        }

//        private bool ClientIsFinishedSendingData(SocketAsyncEvent socketAsyncEvent, DataHoldingUserToken dataHoldingUserToken)
//        {
//            if (socketAsyncEvent.BytesTransferred == 0)
//            {
//                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
//                                     String.Format("ClientIsFinishedSendingData: NO DATA on Token Id: {0}", dataHoldingUserToken.TokenId));

//                dataHoldingUserToken.Reset();
//                CloseClientSocket(socketAsyncEvent);
//                return true;
//            }

//            return false;
//        }

//        private bool PrefixDataForCurrentMessageStillRemains(SocketAsyncEvent socketAsyncEvent, DataHoldingUserToken dataHoldingUserToken, ref int remainingBytesToProcess)
//        {
//            //If we have not got all of the prefix already, then we need to work on it here.
//            if (dataHoldingUserToken.ReceivedPrefixBytesDoneCount < _serverConfiguration.ReceivePrefixLength)
//            {
//                remainingBytesToProcess = _prefixHandler.HandlePrefix(socketAsyncEvent, dataHoldingUserToken, remainingBytesToProcess);
//                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
//                                     String.Format("PrefixDataForCurrentMessageStillRemains: after prefix work on token Id: {0}. RemainingBytesToProcess = {1}",
//                                                   dataHoldingUserToken.TokenId,
//                                                   remainingBytesToProcess));
//                if (remainingBytesToProcess == 0)
//                {
//                    // We need to do another receive op, since we do not have the message yet, but remainingBytesToProcess == 0.
//                    StartReceive(socketAsyncEvent);
//                    return true;
//                }
//            }

//            return false;
//        }

//        private void ProcessSend(SocketAsyncEvent socketAsyncEvent)
//        {
//            var receiveSendToken = (DataHoldingUserToken)socketAsyncEvent.Token;

//            if (socketAsyncEvent.SocketError == SocketError.Success)
//            {
//                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug, String.Format("ProcessSend: Data sent to client."));
//                receiveSendToken.SendBytesRemainingCount = receiveSendToken.SendBytesRemainingCount - socketAsyncEvent.BytesTransferred;

//                if (receiveSendToken.SendBytesRemainingCount == 0)
//                    StartReceive(socketAsyncEvent.EventArgs);
//                else
//                {
//                    receiveSendToken.BytesSentAlreadyCount += socketAsyncEvent.BytesTransferred;
//                    StartSend(socketAsyncEvent);
//                }
//            }
//            else
//            {
//                receiveSendToken.Reset();
//                CloseClientSocket(socketAsyncEvent);
//            }
//        }

//        private void CloseClientSocket(SocketAsyncEvent e)
//        {
//            var receiveSendToken = (DataHoldingUserToken)e.Token;

//            try // do a shutdown before you close the socket
//            {
//                e.AcceptSocket.Shutdown(SocketShutdown.Both);
//            }
//            catch (Exception) // throws if socket was already closed
//            {
//                if (receiveSendToken != null)
//                    _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Error,
//                                         String.Format("Close client socket attempt failed on Id: {0}", receiveSendToken.TokenId));
//            }

//            // This method closes the socket and releases all resources, both
//            // managed and unmanaged. It internally calls Dispose.
//            e.AcceptSocket.Close();

//            // Make sure the new DataHolder has been created for the next connection.
//            // If it has, then dataMessageReceived should be null.
//            if (receiveSendToken != null && receiveSendToken.DataHolder.DataMessageReceived != null)
//                receiveSendToken.CreateNewDataHolder();

//            // Put the SocketAsyncEventArg back into the pool, to be used by another client.
//            _poolOfRecSendEventArgs.Push(e);

//            Interlocked.Decrement(ref _numberOfAcceptedSockets);

//            if (receiveSendToken != null)
//                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
//                                     String.Format("Id: {0} disconnected. {1} client(s) connected.", receiveSendToken.TokenId, _numberOfAcceptedSockets));

//            // Release Semaphore so that its connection counter will be decremented.
//            // This must be done AFTER putting the SocketAsyncEventArg back into the pool, 
//            // or you can run into problems.
//            _maxConnectionsEnforcer.Release();
//        }

//        public void CleanUpOnExit()
//        {
//            DisposeAllSaeaObjects();
//        }

//        private void DisposeAllSaeaObjects()
//        {
//            SocketAsyncEventArgs eventArgs;
            
//            while (_poolOfAcceptEventArgs.Any())
//            {
//                eventArgs = _poolOfAcceptEventArgs.Pop();
//                eventArgs.Dispose();
//            }

//            while (_poolOfRecSendEventArgs.Any())
//            {
//                eventArgs = _poolOfRecSendEventArgs.Pop();
//                eventArgs.Dispose();
//            }
//        }
//    }
//}