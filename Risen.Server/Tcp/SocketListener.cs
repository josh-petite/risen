using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Risen.Server.Extentions;
using Risen.Server.Msmq;
using Risen.Server.Tcp.Factories;
using Risen.Server.Tcp.Tokens;

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
        private static readonly object Mutex = new object();

        private int _numberOfAcceptedSockets;
        private readonly IBufferManager _bufferManager;
        private readonly IServerConfiguration _serverConfiguration;
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
        public const long PacketSizeThreshhold = 5000;

        public SocketListener(IServerConfiguration serverConfiguration, IBufferManager bufferManager, IPrefixHandler prefixHandler,
                              IMessageHandler messageHandler, ILogger logger, IDataHoldingUserTokenFactory dataHoldingUserTokenFactory,
                              ISocketAsyncEventArgsFactory socketAsyncEventArgsFactory, ISocketAsyncEventArgsPoolFactory socketAsyncEventArgsPoolFactory)
        {
            _serverConfiguration = serverConfiguration;
            _prefixHandler = prefixHandler;
            _messageHandler = messageHandler;
            _logger = logger;
            _dataHoldingUserTokenFactory = dataHoldingUserTokenFactory;
            _socketAsyncEventArgsFactory = socketAsyncEventArgsFactory;
            _socketAsyncEventArgsPoolFactory = socketAsyncEventArgsPoolFactory;
            _bufferManager = bufferManager;

            _maxConnectionsEnforcer = new Semaphore(serverConfiguration.MaxNumberOfConnections, serverConfiguration.MaxNumberOfConnections);
            InitialTransmissionId = serverConfiguration.MainTransmissionId;
        }

        public static int InitialTransmissionId { get; set; }

        public void Init()
        {
            InitializeBufferManager();
            InitializeAcceptEventArgsPool();
            InitializeSendReceiveEventArgsPool();
        }

        private void InitializeBufferManager()
        {
            _bufferManager.InitBuffer();
        }

        private void InitializeAcceptEventArgsPool()
        {
            _poolOfAcceptEventArgs = _socketAsyncEventArgsPoolFactory.GenerateSocketAsyncEventArgsPool(_serverConfiguration.MaxAcceptOperations);

            for (int i = 0; i < _serverConfiguration.MaxAcceptOperations; i++)
                _poolOfAcceptEventArgs.Push(CreateNewSaeaForAccept());
        }

        private void InitializeSendReceiveEventArgsPool()
        {
            _poolOfRecSendEventArgs = _socketAsyncEventArgsPoolFactory.GenerateSocketAsyncEventArgsPool(_serverConfiguration.NumberOfSaeaForRecSend);

            for (int i = 0; i < _serverConfiguration.NumberOfSaeaForRecSend; i++)
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
            _listenSocket = new Socket(_serverConfiguration.LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(_serverConfiguration.LocalEndPoint);
            _listenSocket.Listen(_serverConfiguration.Backlog);
            Console.WriteLine("****** Server is listening. ******");
            StartAccept();
        }

        public void StartAccept()
        {
            SocketAsyncEventArgs acceptEventArg;

            if (_poolOfAcceptEventArgs.Count > 1)
            {
                try
                {
                    acceptEventArg = _poolOfAcceptEventArgs.Pop();
                }
                catch
                {
                    acceptEventArg = CreateNewSaeaForAccept();
                }
            }
            else
                acceptEventArg = CreateNewSaeaForAccept();

            // Used to control access to pool resources
            _maxConnectionsEnforcer.WaitOne();
            var willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);

            // AcceptAsync returns true if the I/O operation is pending, i.socketAsyncEventArgs. is working asynchronously.
            // When it completes it will call the acceptEventArg.Completed event (AcceptEventArg_Completed, as wired above).
            if (!willRaiseEvent)
            {
                var acceptOperationUserToken = (AcceptOperationUserToken)acceptEventArg.UserToken;
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
                                     string.Format("StartAccept: In if (!willRaiseEvent), AcceptOp Token Id: {0}", acceptOperationUserToken.TokenId));

                ProcessAccept(acceptEventArg);
            }
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
            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
                                 string.Format("ProcessAccept: Accept Id: {0}, RecSend Id: {1}, Remote Endpoint: {2}:{3} *** Client(s) connected = {4}",
                                               acceptOperationUserToken.TokenId,
                                               ((DataHoldingUserToken) receiveSendEventArgs.UserToken).TokenId,
                                               IPAddress.Parse(((IPEndPoint) receiveSendEventArgs.AcceptSocket.RemoteEndPoint).Address.ToString()),
                                               ((IPEndPoint) receiveSendEventArgs.AcceptSocket.RemoteEndPoint).Port,
                                               _numberOfAcceptedSockets));

            acceptEventArgs.ClearAcceptSocket();
            _poolOfAcceptEventArgs.Push(acceptEventArgs);
            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
                                 string.Format("ProcessAccept: Accept Id: {0} goes back to pool.", ((AcceptOperationUserToken) acceptEventArgs.UserToken).TokenId));

            StartReceive(receiveSendEventArgs);
        }

        private void TrackMaxNumberOfAcceptedSockets(SocketAsyncEventArgs acceptEventArgs)
        {
            var maxSimultaneousAcceptOperations = MaxSimultaneousClientsThatWereConnected;
            var numberOfConnectedSockets = Interlocked.Increment(ref _numberOfAcceptedSockets);

            if (numberOfConnectedSockets > maxSimultaneousAcceptOperations)
                Interlocked.Increment(ref MaxSimultaneousClientsThatWereConnected);

            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
                                 string.Format("TrackMaxNumberOfAcceptedSockets: ProcessAccept, Accept Id: {0}",
                                               ((AcceptOperationUserToken) acceptEventArgs.UserToken).TokenId));
        }

        private bool EventArgsAreInInvalidState(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs.SocketError != SocketError.Success)
            {
                StartAccept(); // Something failed, try again with a new SocketAsyncEventArgs
                var acceptOperationUserToken = (AcceptOperationUserToken) acceptEventArgs.UserToken;
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Error,
                                                       string.Format("EventArgsAreInInvalidState: *** SocketError *** Accept Id: {0}", acceptOperationUserToken.TokenId));
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
                    //lock (Mutex)
                    //{
                        Console.WriteLine("*** SendReceiveCompleted: Entering Lock...");
                        EvaluateIncomingMessage(socketAsyncEventArgs);
                        Console.WriteLine("*** SendReceiveCompleted: Exiting Lock...");
                    //}
                    break;

                case SocketAsyncOperation.Send:
                    ProcessSend(socketAsyncEventArgs);
                    break;

                default:
                    //This exception will occur if you code the Completed event of some operation to come to this method, by mistake.
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void EvaluateIncomingMessage(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            var dataHoldingUserToken = (DataHoldingUserToken) socketAsyncEventArgs.UserToken;
            var incomingMessage = new byte[socketAsyncEventArgs.BytesTransferred];

            for (int i = 0; i < socketAsyncEventArgs.BytesTransferred; i++)
                incomingMessage[i] = socketAsyncEventArgs.Buffer[dataHoldingUserToken.BufferOffsetReceive + i];

            var prefix = incomingMessage.Take(_serverConfiguration.ReceivePrefixLength).ToArray();
            var messageLength = BitConverter.ToInt32(prefix, 0);
            var messageByteLength = prefix.Length + messageLength;

            if (messageByteLength < incomingMessage.Length)
            {
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Error, "*** Multiple messages detected!! Parsing... ***");
                LogEventArgsBuffer("EvaluateIncomingMessage", socketAsyncEventArgs, dataHoldingUserToken);
                var currentMessageOffset = 0;

                while (incomingMessage.Length > _serverConfiguration.ReceivePrefixLength)
                {
                    Console.WriteLine("* Looping *");
                    //ClearCurrentBufferMessageBlock(socketAsyncEventArgs, incomingMessage.Length);
                    //StageMessage(socketAsyncEventArgs, messageByteLength, incomingMessage);
                    socketAsyncEventArgs.SetBuffer(dataHoldingUserToken.BufferOffsetReceive + currentMessageOffset, _serverConfiguration.BufferSize - messageByteLength);
                    ProcessReceive(socketAsyncEventArgs);
                    incomingMessage = incomingMessage.Skip(messageByteLength).ToArray();
                    currentMessageOffset += messageByteLength;
                }
            }
            else
            {
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug, "Regular ProcessReceive");
                ProcessReceive(socketAsyncEventArgs);
            }
        }

        private void StageMessage(SocketAsyncEventArgs socketAsyncEventArgs, int messageByteLength, byte[] incomingMessage)
        {
            var currentBuffer = socketAsyncEventArgs.Buffer;

            for (int i = 0; i < messageByteLength; i++)
                currentBuffer[socketAsyncEventArgs.Offset + i] = incomingMessage[i];
        }

        private void ClearCurrentBufferMessageBlock(SocketAsyncEventArgs socketAsyncEventArgs, int length)
        {
            var currentBuffer = socketAsyncEventArgs.Buffer;

            for (int i = 0; i < length; i++)
                currentBuffer[socketAsyncEventArgs.Offset + i] = 0;
        }

        private void HandleBadAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Error,
                                                   string.Format("HandleBadAccept: Closing socket of Accept Id: {0}", ((AcceptOperationUserToken)acceptEventArgs.UserToken).TokenId));

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
            
            while (_poolOfAcceptEventArgs.Any())
            {
                eventArgs = _poolOfAcceptEventArgs.Pop();
                eventArgs.Dispose();
            }

            while (_poolOfRecSendEventArgs.Any())
            {
                eventArgs = _poolOfRecSendEventArgs.Pop();
                eventArgs.Dispose();
            }
        }

        private void StartReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var dataHoldingUserToken = (DataHoldingUserToken) receiveSendEventArgs.UserToken;
            receiveSendEventArgs.SetBuffer(dataHoldingUserToken.BufferOffsetReceive, _serverConfiguration.BufferSize);
            
            var willRaiseEvent = receiveSendEventArgs.AcceptSocket.ReceiveAsync(receiveSendEventArgs);

            // willRaiseEvent will return as false if I/O operation completed synchronously
            if (!willRaiseEvent)
            {
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Error, string.Format("StartReceive: In willRaiseEvent if block."));
                ProcessReceive(receiveSendEventArgs);
            }
        }

        // This method is invoked when an asynchronous receive operation completes.  
        // If the remote host closed the connection, then the socket is closed.   
        // If data was received then the data is echoed back to the client. 
        private void ProcessReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var dataHoldingUserToken = receiveSendEventArgs.GetDataHoldingUserToken();
            LogEventArgsBuffer("****** ProcessReceive", receiveSendEventArgs, dataHoldingUserToken);

            if (SocketIsInInvalidState(receiveSendEventArgs, dataHoldingUserToken))
                return;

            if (ClientIsFinishedSendingData(receiveSendEventArgs, dataHoldingUserToken))
                return;

            var remainingBytesToProcess = GetRemainingBytesToProcess(receiveSendEventArgs);
            
            if (remainingBytesToProcess > PacketSizeThreshhold)
                return;

            if (PrefixDataForCurrentMessageStillRemains(receiveSendEventArgs, dataHoldingUserToken, ref remainingBytesToProcess))
                return;

            // If we have processed the prefix, we can work on the message now.
            // We'll arrive here when we have received enough bytes to read
            // the first byte after the prefix.
            ProcessReceivedMessage(receiveSendEventArgs, dataHoldingUserToken, remainingBytesToProcess);
        }

        private int GetRemainingBytesToProcess(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var prefix = new ArraySegment<byte>(receiveSendEventArgs.Buffer, receiveSendEventArgs.Offset, _serverConfiguration.ReceivePrefixLength).ToArray();
            var messageLength = BitConverter.ToInt32(prefix, 0);
            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
                                 string.Format("GetRemainingBytesToProcess: Prefix Length:{0} MessageLength:{1}", prefix.Length, messageLength));
            return prefix.Length + messageLength;
        }

        private void ProcessReceivedMessage(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken dataToken, int remainingBytesToProcess)
        {
            bool incomingTcpMessageIsReady = _messageHandler.HandleMessage(receiveSendEventArgs, dataToken, remainingBytesToProcess);

            if (incomingTcpMessageIsReady)
            {
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
                                     string.Format("ProcessReceivedMessage: Message in DataHolder: {0}", Encoding.ASCII.GetString(dataToken.DataHolder.DataMessageReceived)));

                dataToken.Mediator.HandleData(dataToken.DataHolder); // at this point, use the data
                dataToken.CreateNewDataHolder();
                dataToken.Reset();
                dataToken.Mediator.PrepareOutgoingData();
                StartSend(dataToken.Mediator.GiveBack());
            }
            else
            {
                dataToken.ReceiveMessageOffset = dataToken.BufferOffsetReceive;
                dataToken.RecPrefixBytesDoneThisOperation = 0;
                StartReceive(receiveSendEventArgs);
            }
        }

        private void LogEventArgsBuffer(string callingFunction, SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken dataHoldingUserToken)
        {
            var output = new byte[receiveSendEventArgs.BytesTransferred];

            for (int i = 0; i < receiveSendEventArgs.BytesTransferred; i++)
                output[i] = receiveSendEventArgs.Buffer[dataHoldingUserToken.BufferOffsetReceive + i];

            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Warning, string.Format("{0}: Bytes Received: {1}", callingFunction, BitConverter.ToString(output)));
        }

        private void StartSend(SocketAsyncEventArgs receiveSendEventArgs)
        {
            //while (Monitor.TryEnter(receiveSendEventArgs, 0) == false)
            //{
            //    Console.WriteLine("Sleeping at: {0}", DateTime.Now.TimeOfDay);
            //    Thread.Sleep(25);
            //}
            
            //Console.WriteLine("*** Passed monitor try enter for start send! ***");
            var dataHoldingUserToken = receiveSendEventArgs.GetDataHoldingUserToken();

            if (dataHoldingUserToken.SendBytesRemainingCount <= _serverConfiguration.BufferSize)
            {
                receiveSendEventArgs.SetBuffer(dataHoldingUserToken.BufferOffsetSend, dataHoldingUserToken.SendBytesRemainingCount);
                Buffer.BlockCopy(dataHoldingUserToken.DataToSend,
                             dataHoldingUserToken.BytesSentAlreadyCount,
                             receiveSendEventArgs.Buffer,
                             dataHoldingUserToken.BufferOffsetSend,
                             dataHoldingUserToken.SendBytesRemainingCount);
            }
            else
            {
                receiveSendEventArgs.SetBuffer(dataHoldingUserToken.BufferOffsetSend, _serverConfiguration.BufferSize);
                Buffer.BlockCopy(dataHoldingUserToken.DataToSend,
                                 dataHoldingUserToken.BytesSentAlreadyCount,
                                 receiveSendEventArgs.Buffer,
                                 dataHoldingUserToken.BufferOffsetSend,
                                 _serverConfiguration.BufferSize);

                //We'll change the value of sendUserToken.sendBytesRemainingCount in the ProcessSend method.
            }

            //post asynchronous send operation
            var willRaiseEvent = receiveSendEventArgs.AcceptSocket.SendAsync(receiveSendEventArgs);

            if (!willRaiseEvent)
                ProcessSend(receiveSendEventArgs);
        }
        
        private bool SocketIsInInvalidState(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken dataHoldingUserToken)
        {
            if (receiveSendEventArgs.SocketError != SocketError.Success)
            {
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Error,
                                     string.Format("SocketIsInInvalidState: ReceiveSendToken Id: {0}", dataHoldingUserToken.TokenId));
                dataHoldingUserToken.Reset();
                CloseClientSocket(receiveSendEventArgs);
                return true;
            }

            return false;
        }

        private bool ClientIsFinishedSendingData(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken dataHoldingUserToken)
        {
            if (receiveSendEventArgs.BytesTransferred == 0)
            {
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
                                     string.Format("ClientIsFinishedSendingData: NO DATA on Token Id: {0}", dataHoldingUserToken.TokenId));

                dataHoldingUserToken.Reset();
                CloseClientSocket(receiveSendEventArgs);
                return true;
            }

            return false;
        }

        private bool PrefixDataForCurrentMessageStillRemains(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken dataHoldingUserToken, ref int remainingBytesToProcess)
        {
            //If we have not got all of the prefix already, then we need to work on it here.
            if (dataHoldingUserToken.ReceivedPrefixBytesDoneCount < _serverConfiguration.ReceivePrefixLength)
            {
                remainingBytesToProcess = _prefixHandler.HandlePrefix(receiveSendEventArgs, dataHoldingUserToken, remainingBytesToProcess);
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
                                     string.Format("PrefixDataForCurrentMessageStillRemains: after prefix work on token Id: {0}. RemainingBytesToProcess = {1}",
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

            if (receiveSendEventArgs.SocketError == SocketError.Success)
            {
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug, string.Format("ProcessSend: Data sent to client."));
                receiveSendToken.SendBytesRemainingCount = receiveSendToken.SendBytesRemainingCount - receiveSendEventArgs.BytesTransferred;

                if (receiveSendToken.SendBytesRemainingCount == 0)
                    StartReceive(receiveSendEventArgs);
                else
                {
                    receiveSendToken.BytesSentAlreadyCount += receiveSendEventArgs.BytesTransferred;
                    StartSend(receiveSendEventArgs);
                }
            }
            else
            {
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
                    _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Error,
                                         string.Format("Close client socket attempt failed on Id: {0}", receiveSendToken.TokenId));
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
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
                                     string.Format("Id: {0} disconnected. {1} client(s) connected.", receiveSendToken.TokenId, _numberOfAcceptedSockets));

            // Release Semaphore so that its connection counter will be decremented.
            // This must be done AFTER putting the SocketAsyncEventArg back into the pool, 
            // or you can run into problems.
            _maxConnectionsEnforcer.Release();
        }
    }
}