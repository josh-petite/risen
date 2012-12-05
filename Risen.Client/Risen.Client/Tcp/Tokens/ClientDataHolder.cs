using System.Collections.Generic;
using System.Net;
using Risen.Shared.Tcp;

namespace Risen.Client.Tcp.Tokens
{
    public class ClientDataHolder : IDataHolder
    {
        public string[] MessagesToSend;
        public byte[] DataMessageReceived { get; set; }
        public long SessionId { get; set; }
        public int ReceivedTransmissionId { get; set; }
        public EndPoint RemoteEndpoint { get; set; }
        public List<byte[]> MessagesReceived = new List<byte[]>();
        public int NumberOfMessagesSent { get; set; }

        public void SetMessagesToSend(string[] messagesToSend)
        {
            MessagesToSend = messagesToSend;
        }
    }
}