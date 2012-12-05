using System.Net.Sockets;
using Risen.Server.Tcp.Tokens;

namespace Risen.Server.Extentions
{
    public static class SocketAsyncEventArgsExtensions
    {
        public static DataHoldingUserToken GetDataHoldingUserToken(this SocketAsyncEventArgs socketAsyncEventArgs)
        {
            return (DataHoldingUserToken) socketAsyncEventArgs.UserToken;
        }

        public static void ClearAcceptSocket(this SocketAsyncEventArgs socketAsyncEventArgs)
        {
            socketAsyncEventArgs.AcceptSocket = null;
        }
    }
}