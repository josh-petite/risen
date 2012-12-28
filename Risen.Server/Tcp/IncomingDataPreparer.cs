using System.Net;
using System.Net.Sockets;
using System.Threading;
using Risen.Server.Msmq;
using Risen.Server.Tcp.Tokens;

namespace Risen.Server.Tcp
{
    public interface IIncomingDataPreparer
    {
        SocketAsyncEvent SocketAsyncEvent { get; set; }
        DataHolder HandleReceivedData(DataHolder incomingDataHolder, SocketAsyncEvent socketAsyncEventArgs);
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

        public SocketAsyncEvent SocketAsyncEvent { get; set; }

        private int GetReceivedTransmissionId()
        {
            int mainTransmissionId = _serverConfiguration.MainTransmissionId;
            int receivedTransmissionId = Interlocked.Increment(ref mainTransmissionId);
            return receivedTransmissionId;
        }

        private EndPoint GetRemoteEndpoint()
        {
            return SocketAsyncEvent.AcceptSocket.RemoteEndPoint;
        }

        public DataHolder HandleReceivedData(DataHolder incomingDataHolder, SocketAsyncEvent socketAsyncEventArgs)
        {
            var receiveToken = (DataHoldingUserToken)socketAsyncEventArgs.Token;

            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
                                 string.Format("IncomingDataPreparer: HandleReceiveData() - Token Id: {0}", receiveToken.TokenId));

            _dataHolder = incomingDataHolder;
            _dataHolder.SessionId = receiveToken.SessionId;
            _dataHolder.ReceivedTransmissionId = GetReceivedTransmissionId();
            _dataHolder.RemoteEndpoint = GetRemoteEndpoint();

            return _dataHolder;
        }
    }
}