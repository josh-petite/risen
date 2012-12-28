namespace Risen.Server.Tcp
{
    public class Mediator
    {
        private readonly IOutgoingDataPreparer _outgoingDataPreparer;
        private DataHolder _dataHolder;
        
        public Mediator(IOutgoingDataPreparer outgoingDataPreparer)
        {
            _outgoingDataPreparer = outgoingDataPreparer;
        }

        public IIncomingDataPreparer IncomingDataPreparer { get; set; }
        public SocketAsyncEvent SocketAsyncEvent { get; set; }

        public void HandleData(DataHolder incomingDataHolder)
        {
            _dataHolder = IncomingDataPreparer.HandleReceivedData(incomingDataHolder, SocketAsyncEvent);
        }

        public void PrepareOutgoingData()
        {
            _outgoingDataPreparer.PrepareOutgoingData(SocketAsyncEvent, _dataHolder);
        }

        public SocketAsyncEvent GiveBack()
        {
            return SocketAsyncEvent;
        }
    }
}