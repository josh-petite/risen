using System.Net;
using System.Net.Sockets;
using System.Threading;
using Risen.Server.Extentions;
using Risen.Server.Msmq;

namespace Risen.Server.Tcp
{
    public interface IIncomingDataPreparer
    {
        SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }
        DataHolder HandleReceivedData(DataHolder incomingDataHolder, SocketAsyncEventArgs socketAsyncEventArgs);
    }

    public class IncomingDataPreparer : IIncomingDataPreparer
    {
        private DataHolder _dataHolder;
        private readonly IServerConfiguration _serverConfiguration;
        private readonly ILogger _logger;

        public IncomingDataPreparer(IServerConfiguration serverConfiguration, ILogger logger)
        {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
        }

        public SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }

        private int ReceivedTransmissionIdGetter()
        {
            int mainTransmissionId = _serverConfiguration.MainTransmissionId;
            int receivedTransmissionId = Interlocked.Increment(ref mainTransmissionId);
            return receivedTransmissionId;
        }

        private EndPoint GetRemoteEndpoint()
        {
            return SocketAsyncEventArgs.AcceptSocket.RemoteEndPoint;
        }

        public DataHolder HandleReceivedData(DataHolder incomingDataHolder, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            var receiveToken = socketAsyncEventArgs.GetDataHoldingUserToken();

            _logger.QueueMessage(LogMessage.Create(LogCategory.TcpServer, LogSeverity.Debug,
                                                   string.Format("IncomingDataPreparer, HandleReceiveData() - Token Id: {0}", receiveToken.TokenId)));

            _dataHolder = incomingDataHolder;
            _dataHolder.SessionId = receiveToken.SessionId;
            _dataHolder.ReceivedTransmissionId = ReceivedTransmissionIdGetter();
            _dataHolder.RemoteEndpoint = GetRemoteEndpoint();

            return _dataHolder;
        }
    }
}