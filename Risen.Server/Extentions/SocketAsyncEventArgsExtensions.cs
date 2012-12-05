using System.Net.Sockets;
using Risen.Shared.Tcp.Tokens;

namespace Risen.Server.Extentions
{
    public static class SocketAsyncEventArgsExtensions
    {
        public static IDataHoldingUserToken GetDataHoldingUserToken(this SocketAsyncEventArgs socketAsyncEventArgs)
        {
            return (IDataHoldingUserToken) socketAsyncEventArgs.UserToken;
        }
    }
}