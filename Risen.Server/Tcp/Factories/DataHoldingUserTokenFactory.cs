using Risen.Server.Tcp.Tokens;
using StructureMap;

namespace Risen.Server.Tcp.Factories
{
    public interface IDataHoldingUserTokenFactory
    {
        DataHoldingUserToken GenerateDataHoldingUserToken(SocketAsyncEvent socketAsyncEvent, int tokenId);
    }

    public class DataHoldingUserTokenFactory : IDataHoldingUserTokenFactory
    {
        public DataHoldingUserToken GenerateDataHoldingUserToken(SocketAsyncEvent socketAsyncEvent, int tokenId)
        {
            var token = ObjectFactory.GetInstance<DataHoldingUserToken>();
            token.SocketAsyncEvent = socketAsyncEvent;
            token.TokenId = tokenId;
            token.Init();
            token.CreateNewDataHolder();

            return token;
        }
    }
}