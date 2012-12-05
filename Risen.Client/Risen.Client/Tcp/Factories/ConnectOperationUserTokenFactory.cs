using Risen.Client.Tcp.Tokens;

namespace Risen.Client.Tcp.Factories
{
    public interface IConnectOperationUserTokenFactory
    {
        ConnectOperationUserToken GenerateConnectOperationUserToken(int tokenId);
    }

    public class ConnectOperationUserTokenFactory : IConnectOperationUserTokenFactory
    {
        public ConnectOperationUserToken GenerateConnectOperationUserToken(int tokenId)
        {
            return new ConnectOperationUserToken(tokenId);
        }
    }
}
