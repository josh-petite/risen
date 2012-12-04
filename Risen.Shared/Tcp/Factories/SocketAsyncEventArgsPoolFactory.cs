using StructureMap;

namespace Risen.Shared.Tcp.Factories
{
    public interface ISocketAsyncEventArgsPoolFactory
    {
        ISocketAsyncEventArgsPool GenerateSocketAsyncEventArgsPool(int capacity);
    }

    public class SocketAsyncEventArgsPoolFactory : ISocketAsyncEventArgsPoolFactory
    {
        public ISocketAsyncEventArgsPool GenerateSocketAsyncEventArgsPool(int capacity)
        {
            var pool = ObjectFactory.GetInstance<ISocketAsyncEventArgsPool>();
            pool.Init(capacity);

            return pool;
        }
    }
}