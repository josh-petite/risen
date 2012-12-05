using System;
using System.Net.Sockets;

namespace Risen.Client.Tcp.Factories
{
    public interface ISocketAsyncEventArgsFactory
    {
        SocketAsyncEventArgs GenerateReceiveSendSocketAsyncEventArgs(EventHandler<SocketAsyncEventArgs> sendReceiveCompleted);
        SocketAsyncEventArgs GenerateAcceptSocketAsyncEventArgs(EventHandler<SocketAsyncEventArgs> acceptCompleted, int tokenId);
    }

    public class SocketAsyncEventArgsFactory : ISocketAsyncEventArgsFactory
    {
        private readonly IConnectOperationUserTokenFactory _connectOperationUserTokenFactory;

        public SocketAsyncEventArgsFactory(IConnectOperationUserTokenFactory connectOperationUserTokenFactory)
        {
            _connectOperationUserTokenFactory = connectOperationUserTokenFactory;
        }

        public SocketAsyncEventArgs GenerateReceiveSendSocketAsyncEventArgs(EventHandler<SocketAsyncEventArgs> sendReceiveCompleted)
        {
            var socketAsyncEventArgs = new SocketAsyncEventArgs();
            socketAsyncEventArgs.Completed += sendReceiveCompleted;
            
            return socketAsyncEventArgs;
        }

        public SocketAsyncEventArgs GenerateAcceptSocketAsyncEventArgs(EventHandler<SocketAsyncEventArgs> acceptCompleted, int tokenId)
        {
            var socketAsyncEventArgs = new SocketAsyncEventArgs();
            socketAsyncEventArgs.Completed += acceptCompleted;
            socketAsyncEventArgs.UserToken = _connectOperationUserTokenFactory.GenerateConnectOperationUserToken(tokenId);

            return socketAsyncEventArgs;
        }
    }
}
