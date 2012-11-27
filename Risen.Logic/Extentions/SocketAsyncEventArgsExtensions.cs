using System.Net.Sockets;
using Risen.Server.Tcp;

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