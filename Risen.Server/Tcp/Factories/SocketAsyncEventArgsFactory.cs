using System;
using System.Net.Sockets;

namespace Risen.Server.Tcp.Factories
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
            var args = new SocketAsyncEventArgs {UserToken = _acceptOperationUserTokenFactory.GenerateAcceptOperationUserToken(tokenId)};
            args.Completed += completedAction;

            return args;
        }

        public SocketAsyncEventArgs GenerateReceiveSendSocketAsyncEventArgs(EventHandler<SocketAsyncEventArgs> completedAction)
        {
            var args = new SocketAsyncEventArgs();
            args.Completed += completedAction;
            _bufferManager.SetBuffer(args);

            return args;
        }
    }
}