using Risen.Server.Tcp.Tokens;
using StructureMap;

namespace Risen.Server.Tcp.Factories
{
    public interface IAcceptOperationUserTokenFactory
    {
        AcceptOperationUserToken GenerateAcceptOperationUserToken(int tokenId);
    }

    public class AcceptOperationUserTokenFactory : IAcceptOperationUserTokenFactory
    {
        public AcceptOperationUserToken GenerateAcceptOperationUserToken(int tokenId)
        {
            var token = ObjectFactory.GetInstance<AcceptOperationUserToken>();
            token.TokenId = tokenId;

            return token;
        }
    }
}