using System.Net.Sockets;
using StructureMap;

namespace Risen.Shared.Tcp.Factories
{
    public interface IMediatorFactory
    {
        IMediator GenerateMediator(SocketAsyncEventArgs eventArgs);
    }

    public class MediatorFactory : IMediatorFactory
    {
        private readonly IIncomingDataPreparerFactory _incomingDataPreparerFactory;

        public MediatorFactory(IIncomingDataPreparerFactory incomingDataPreparerFactory)
        {
            _incomingDataPreparerFactory = incomingDataPreparerFactory;
        }

        public IMediator GenerateMediator(SocketAsyncEventArgs eventArgs)
        {
            var incomingDataPreparer = _incomingDataPreparerFactory.GenerateIncomingDataPreparer(eventArgs);

            var mediator = ObjectFactory.GetInstance<IMediator>();
            mediator.SocketAsyncEventArgs = eventArgs;
            mediator.IncomingDataPreparer = incomingDataPreparer;

            return mediator;
        }
    }
}