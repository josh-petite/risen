using System;
using System.Net.Sockets;
using System.Text;
using Risen.Shared.Tcp;

namespace Risen.Server.Tcp
{
    public interface IMediator
    {
        SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }
        IIncomingDataPreparer IncomingDataPreparer { get; set; }
        void HandleData(IDataHolder dataHolder);
        void PrepareOutgoingData();
        SocketAsyncEventArgs GiveBack();
    }

    public class Mediator : IMediator
    {
        private readonly IOutgoingDataPreparer _outgoingDataPreparer;
        private IDataHolder _dataHolder;
        
        public Mediator(IOutgoingDataPreparer outgoingDataPreparer)
        {
            _outgoingDataPreparer = outgoingDataPreparer;
        }

        public IIncomingDataPreparer IncomingDataPreparer { get; set; }

        public SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }

        public void HandleData(IDataHolder incomingDataHolder)
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