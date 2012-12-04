using System;
using System.Net.Sockets;
using StructureMap;

namespace Risen.Shared.Tcp.Factories
{
    public interface ISocketAsyncEventArgsFactory
    {
        SocketAsyncEventArgs GenerateAcceptSocketAsyncEventArgs(EventHandler<SocketAsyncEventArgs> completedAction, int tokenId);
        SocketAsyncEventArgs GenerateReceiveSendSocketAsyncEventArgs(EventHandler<SocketAsyncEventArgs> completedAction);
    }

    public class SocketAsyncEventArgsFactory : ISocketAsyncEventArgsFactory
    {
        private readonly IBufferManager _bufferManager;
        private readonly IAcceptOperationUserTokenFactory _acceptOperationUserTokenFactory;

        public SocketAsyncEventArgsFactory(IBufferManager bufferManager, IAcceptOperationUserTokenFactory acceptOperationUserTokenFactory)
        {
            _bufferManager = bufferManager;
            _acceptOperationUserTokenFactory = acceptOperationUserTokenFactory;
        }

        public SocketAsyncEventArgs GenerateAcceptSocketAsyncEventArgs(EventHandler<SocketAsyncEventArgs> completedAction, int tokenId)
        {
            var args = ObjectFactory.GetInstance<SocketAsyncEventArgs>();
            args.Completed += completedAction;
            args.UserToken = _acceptOperationUserTokenFactory.GenerateAcceptOperationUserToken(tokenId);

            return args;
        }

        public SocketAsyncEventArgs GenerateReceiveSendSocketAsyncEventArgs(EventHandler<SocketAsyncEventArgs> completedAction)
        {
            var args = ObjectFactory.GetInstance<SocketAsyncEventArgs>();

            _bufferManager.SetBuffer(args);
            args.Completed += completedAction;

            return args;
        }
    }
}