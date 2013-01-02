using System;
using Risen.Shared.Enums;

namespace Risen.Server.Tcp.Cache
{
    public class TcpUnknownMessageProcessor : ITcpMessageProcessor
    {
        public bool AppliesTo(MessageType messageType)
        {
            return messageType == MessageType.Unknown;
        }

        public void Execute(ConnectedUser connectedUser, string jsonMessage)
        {
            Console.WriteLine("Unknown Data Received: {0}", jsonMessage);
        }
    }
}