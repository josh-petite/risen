using System.Net.Sockets;
using Risen.Shared.Tcp.Tokens;

namespace Risen.Shared.Extensions
{
    public static class SocketAsyncEventArgsExtensions
    {
        public static DataHoldingUserToken GetDataHoldingUserToken(this SocketAsyncEventArgs socketAsyncEventArgs)
        {
            return (DataHoldingUserToken) socketAsyncEventArgs.UserToken;
        }
    }
}