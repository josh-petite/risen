using System.Net.Sockets;
using Risen.Client.Tcp.Tokens;

namespace Risen.Client.Tcp.Factories
{
    public interface IClientDataUserTokenFactory
    {
        ClientDataUserToken GenerateClientDataUserToken(SocketAsyncEventArgs socketAsyncEventArgs, int bufferSize, int tokenId);
    }

    public class ClientDataUserTokenFactory : IClientDataUserTokenFactory
    {
        public ClientDataUserToken GenerateClientDataUserToken(SocketAsyncEventArgs socketAsyncEventArgs, int bufferSize, int tokenId)
        {
            return new ClientDataUserToken(socketAsyncEventArgs.Offset, socketAsyncEventArgs.Offset + bufferSize, tokenId)
                       {
                           SocketAsyncEventArgs = socketAsyncEventArgs
                       };
        }
    }
}
