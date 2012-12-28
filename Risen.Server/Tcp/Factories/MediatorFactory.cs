using System.Net.Sockets;
using StructureMap;

namespace Risen.Server.Tcp.Factories
{
    public interface IMediatorFactory
    {
        Mediator GenerateMediator(SocketAsyncEvent eventArgs);
    }

    public class MediatorFactory : IMediatorFactory
    {
        private readonly IIncomingDataPreparerFactory _incomingDataPreparerFactory;

        public MediatorFactory(IIncomingDataPreparerFactory incomingDataPreparerFactory)
        {
            _incomingDataPreparerFactory = incomingDataPreparerFactory;
        }

        public Mediator GenerateMediator(SocketAsyncEvent eventArgs)
        {
            var incomingDataPreparer = _incomingDataPreparerFactory.GenerateIncomingDataPreparer(eventArgs);

            var mediator = ObjectFactory.GetInstance<Mediator>();
            mediator.SocketAsyncEvent = eventArgs;
            mediator.IncomingDataPreparer = incomingDataPreparer;

            return mediator;
        }
    }
}