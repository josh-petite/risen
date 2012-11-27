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

        public IncomingDataPreparer(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            _socketAsyncEventArgs = socketAsyncEventArgs;
        }

        private int ReceivedTransMissionIdGetter()
        {
            int mainSessionId = SocketListener.MainSessionId;
            int receivedTransMissionId = Interlocked.Increment(ref mainSessionId);
            return receivedTransMissionId;
        }

        private EndPoint GetRemoteEndpoint()
        {
            return _socketAsyncEventArgs.AcceptSocket.RemoteEndPoint;
        }

        internal DataHolder HandleReceivedData(DataHolder incomingDataHolder, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            var receiveToken = socketAsyncEventArgs.DataHoldingUserToken();
            _dataHolder = incomingDataHolder;
            _dataHolder.SessionId = receiveToken.SessionId;
            _dataHolder.ReceivedTransmissionId = ReceivedTransMissionIdGetter();
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