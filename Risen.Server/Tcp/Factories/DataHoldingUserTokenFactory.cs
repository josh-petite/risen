using System.Net.Sockets;
using Risen.Server.Tcp.Tokens;
using StructureMap;

namespace Risen.Server.Tcp.Factories
{
    public interface IDataHoldingUserTokenFactory
    {
        DataHoldingUserToken GenerateDataHoldingUserToken(SocketAsyncEventArgs eventArgs, int tokenId);
    }

    public class DataHoldingUserTokenFactory : IDataHoldingUserTokenFactory
    {
        public DataHoldingUserToken GenerateDataHoldingUserToken(SocketAsyncEventArgs eventArgs, int tokenId)
        {
            var token = ObjectFactory.GetInstance<DataHoldingUserToken>();
            token.SocketAsyncEventArgs = eventArgs;
            token.TokenId = tokenId;
            token.Init();
            token.CreateNewDataHolder();

            return token;
        }
    }
}