using System.Net;
using System.Net.Sockets;
using System.Threading;
using Risen.Server.Extentions;
using Risen.Shared.Msmq;
using Risen.Shared.Tcp;

namespace Risen.Server.Tcp
{
    public interface IIncomingDataPreparer
    {
        SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }
        IDataHolder HandleReceivedData(IDataHolder incomingDataHolder, SocketAsyncEventArgs socketAsyncEventArgs);
    }

    public class IncomingDataPreparer : IIncomingDataPreparer
    {
        private IDataHolder _dataHolder;
        private readonly ISharedConfiguration _sharedConfiguration;
        private readonly ILogger _logger;

        public IncomingDataPreparer(ISharedConfiguration sharedConfiguration, ILogger logger)
        {
            _sharedConfiguration = sharedConfiguration;
            _logger = logger;
        }

        public SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }

        private int ReceivedTransmissionIdGetter()
        {
            int mainTransmissionId = _sharedConfiguration.MainTransmissionId;
            int receivedTransmissionId = Interlocked.Increment(ref mainTransmissionId);
            return receivedTransmissionId;
        }

        private EndPoint GetRemoteEndpoint()
        {
            return SocketAsyncEventArgs.AcceptSocket.RemoteEndPoint;
        }

        public IDataHolder HandleReceivedData(IDataHolder incomingDataHolder, SocketAsyncEventArgs socketAsyncEventArgs)
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