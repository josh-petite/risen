using System.Net.Sockets;

namespace Risen.Server.Tcp
{
    public class Mediator
    {
        private readonly IncomingDataPreparer _incomingDataPreparer;
        private readonly OutgoingDataPreparer _outgoingDataPreparer;
        private DataHolder _dataHolder;
        private readonly SocketAsyncEventArgs _saeaObject;

        public Mediator(SocketAsyncEventArgs e, IListenerConfiguration listenerConfiguration, ILogger logger)
        {
            _saeaObject = e;
            _incomingDataPreparer = new IncomingDataPreparer(_saeaObject, listenerConfiguration, logger);
            _outgoingDataPreparer = new OutgoingDataPreparer();
        }

        internal void HandleData(DataHolder incomingDataHolder)
        {
            _dataHolder = _incomingDataPreparer.HandleReceivedData(incomingDataHolder, _saeaObject);
        }

        internal void PrepareOutgoingData()
        {
            _outgoingDataPreparer.PrepareOutgoingData(_saeaObject, _dataHolder);
        }

        internal SocketAsyncEventArgs GiveBack()
        {
            return _saeaObject;
        }
    }
}