using System.Net;
using System.Net.Sockets;
using System.Threading;
using Risen.Server.Extentions;
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
        private readonly IListenerConfiguration _listenerConfiguration;
        private readonly ILogger _logger;

        public IncomingDataPreparer(IListenerConfiguration listenerConfiguration, ILogger logger)
        {
            _listenerConfiguration = listenerConfiguration;
            _logger = logger;
        }

        public SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }

        private int ReceivedTransmissionIdGetter()
        {
            int mainTransmissionId = _listenerConfiguration.MainTransmissionId;
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

            _logger.WriteLine(LogCategory.Info, string.Format("IncomingDataPreparer, HandleReceiveData() - Token Id: {0}", receiveToken.TokenId));

            _dataHolder = incomingDataHolder;
            _dataHolder.SessionId = receiveToken.SessionId;
            _dataHolder.ReceivedTransmissionId = ReceivedTransmissionIdGetter();
            _dataHolder.RemoteEndpoint = GetRemoteEndpoint();

            return _dataHolder;
        }
    }
}