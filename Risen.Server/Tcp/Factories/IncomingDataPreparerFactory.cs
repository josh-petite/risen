using System.Net.Sockets;
using StructureMap;

namespace Risen.Server.Tcp.Factories
{
    public interface IIncomingDataPreparerFactory
    {
        IIncomingDataPreparer GenerateIncomingDataPreparer(SocketAsyncEventArgs eventArgs);
    }

    public class IncomingDataPreparerFactory : IIncomingDataPreparerFactory
    {
        public IIncomingDataPreparer GenerateIncomingDataPreparer(SocketAsyncEventArgs eventArgs)
        {
            var dataPreparer = ObjectFactory.GetInstance<IIncomingDataPreparer>();
            dataPreparer.SocketAsyncEventArgs = eventArgs;

            return dataPreparer;
        }
    }
}