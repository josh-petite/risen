using System.Net.Sockets;
using Risen.Server.Tcp;
using Risen.Server.Tcp.Tokens;

namespace Risen.Server.Extentions
{
    public static class SocketAsyncEventArgsExtensions
    {
        public static DataHoldingUserToken DataHoldingUserToken(this SocketAsyncEventArgs socketAsyncEventArgs)
        {
            return (DataHoldingUserToken) socketAsyncEventArgs.UserToken;
        }
    }
}