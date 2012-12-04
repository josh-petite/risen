using Risen.Shared.Tcp;


namespace Risen.Client.Tcp
{
    public interface ISocketClient
    {
    }

    public class SocketClient : ISocketClient
    {
        private readonly IPrefixHandler _prefixHandler;
        private readonly IMessageHandler _messageHandler;

        public SocketClient(IClientConfiguration clientConfiguration, IPrefixHandler prefixHandler, IMessageHandler messageHandler)
        {
            _prefixHandler = prefixHandler;
            _messageHandler = messageHandler;
        }

        private const int PrefixLength = 4; // number of bytes in both send/receive prefix length

    }
}
