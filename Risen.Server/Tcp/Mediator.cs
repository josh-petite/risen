﻿using System.Net.Sockets;
using Risen.Shared.Tcp;

namespace Risen.Server.Tcp
{
    public interface IMediator
    {
        SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }
        IIncomingDataPreparer IncomingDataPreparer { get; set; }
        void HandleData(DataHolder dataHolder);
        void PrepareOutgoingData();
        SocketAsyncEventArgs GiveBack();
    }

    public class Mediator : IMediator
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