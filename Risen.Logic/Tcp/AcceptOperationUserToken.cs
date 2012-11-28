namespace Risen.Server.Tcp
{
    public class AcceptOperationUserToken
    {
        public AcceptOperationUserToken(int tokenId)
        {
            TokenId = tokenId;
        }

        public int TokenId { get; set; }
        public int SocketHandleNumber { get; set; }
    }
}