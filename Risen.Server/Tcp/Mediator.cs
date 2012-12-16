using System;
using System.Net.Sockets;
using System.Text;

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
        public SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }

        public void HandleData(DataHolder incomingDataHolder)
        {
            _dataHolder = IncomingDataPreparer.HandleReceivedData(incomingDataHolder, SocketAsyncEventArgs);
            Console.WriteLine("Data Received On Server: {0}", Encoding.Default.GetString(_dataHolder.DataMessageReceived));
        }

        public void PrepareOutgoingData()
        {
            _outgoingDataPreparer.PrepareOutgoingData(SocketAsyncEventArgs, _dataHolder);
        }

        public SocketAsyncEventArgs GiveBack()
        {
            return SocketAsyncEventArgs;
        }
    }
}