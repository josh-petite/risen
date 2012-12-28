using StructureMap;

namespace Risen.Server.Tcp.Factories
{
    public interface IIncomingDataPreparerFactory
    {
        IIncomingDataPreparer GenerateIncomingDataPreparer(SocketAsyncEvent socketAsyncEvent);
    }

    public class IncomingDataPreparerFactory : IIncomingDataPreparerFactory
    {
        public IIncomingDataPreparer GenerateIncomingDataPreparer(SocketAsyncEvent socketAsyncEvent)
        {
            var dataPreparer = ObjectFactory.GetInstance<IIncomingDataPreparer>();
            dataPreparer.SocketAsyncEvent = socketAsyncEvent;

            return dataPreparer;
        }
    }
}