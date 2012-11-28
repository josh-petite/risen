using System.Net;
using System.Net.Sockets;
using System.Threading;
using Risen.Server.Extentions;

namespace Risen.Server.Tcp
{
    public class IncomingDataPreparer
    {
        private static readonly object Mutex = new object();
        private DataHolder _dataHolder;
        private readonly SocketAsyncEventArgs _socketAsyncEventArgs;
        private readonly IListenerConfiguration _listenerConfiguration;
        private readonly ILogger _logger;

        public IncomingDataPreparer(SocketAsyncEventArgs socketAsyncEventArgs, IListenerConfiguration listenerConfiguration, ILogger logger)
        {
            _socketAsyncEventArgs = socketAsyncEventArgs;
            _listenerConfiguration = listenerConfiguration;
            _logger = logger;
        }

        private int ReceivedTransmissionIdGetter()
        {
            int mainTransmissionId = _listenerConfiguration.MainTransmissionId;
            int receivedTransmissionId = Interlocked.Increment(ref mainTransmissionId);
            return receivedTransmissionId;
        }

        private EndPoint GetRemoteEndpoint()
        {
            return _socketAsyncEventArgs.AcceptSocket.RemoteEndPoint;
        }

        internal DataHolder HandleReceivedData(DataHolder incomingDataHolder, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            var receiveToken = socketAsyncEventArgs.DataHoldingUserToken();

            _logger.WriteLine(LogCategory.Info, string.Format("IncomingDataPreparer, HandleReceiveData() - Token Id: {0}", receiveToken.TokenId));

            _dataHolder = incomingDataHolder;
            _dataHolder.SessionId = receiveToken.SessionId;
            _dataHolder.ReceivedTransmissionId = ReceivedTransmissionIdGetter();
            _dataHolder.RemoteEndpoint = GetRemoteEndpoint();
            AddDataHolder();

            return _dataHolder;
        }

        private void AddDataHolder()
        {
            lock (Mutex)
                SocketListener.DataHolders.Add(_dataHolder);
        }
    }
}