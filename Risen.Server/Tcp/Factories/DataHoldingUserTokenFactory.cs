using System.Net.Sockets;
using Risen.Shared.Tcp.Tokens;
using StructureMap;

namespace Risen.Server.Tcp.Factories
{
    public interface IDataHoldingUserTokenFactory
    {
        IDataHoldingUserToken GenerateDataHoldingUserToken(SocketAsyncEventArgs eventArgs, int tokenId);
    }

    public class DataHoldingUserTokenFactory : IDataHoldingUserTokenFactory
    {
        public IDataHoldingUserToken GenerateDataHoldingUserToken(SocketAsyncEventArgs eventArgs, int tokenId)
        {
            var token = ObjectFactory.GetInstance<IDataHoldingUserToken>();
            token.SocketAsyncEventArgs = eventArgs;
            token.TokenId = tokenId;
            token.Init();
            token.CreateNewDataHolder();

            return token;
        }
    }
}