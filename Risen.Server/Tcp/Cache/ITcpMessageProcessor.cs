using Risen.Shared.Enums;

namespace Risen.Server.Tcp.Cache
{
    public interface ITcpMessageProcessor
    {
        bool AppliesTo(MessageType messageType);
        void Execute(ConnectedUser connectedUser, string jsonMessage);
    }
}