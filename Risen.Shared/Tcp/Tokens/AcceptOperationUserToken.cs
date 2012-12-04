namespace Risen.Shared.Tcp.Tokens
{
    public interface IAcceptOperationUserToken : IUserToken
    {
        int TokenId { get; set; }
        int SocketHandleNumber { get; set; }
    }

    public class AcceptOperationUserToken : IAcceptOperationUserToken
    {
        public int TokenId { get; set; }
        public int SocketHandleNumber { get; set; }
    }
}