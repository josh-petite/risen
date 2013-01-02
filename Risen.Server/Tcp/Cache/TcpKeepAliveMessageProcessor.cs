using System;
using Risen.Shared.Enums;

namespace Risen.Server.Tcp.Cache
{
    public class TcpKeepAliveMessageProcessor : ITcpMessageProcessor
    {
        public bool AppliesTo(MessageType messageType)
        {
            return messageType == MessageType.KeepAlive;
        }

        public void Execute(ConnectedUser connectedUser, string jsonMessage)
        {
            Console.WriteLine("--------------- Keep-alive messasge received. ---------------");
        }
    }
}