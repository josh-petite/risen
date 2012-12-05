using System.Net.Sockets;
using Risen.Shared.Tcp;
using Risen.Shared.Tcp.Tokens;

namespace Risen.Client.Tcp.Tokens
{
    public interface IClientDataUserToken : IUserToken
    {
        byte[] DataToSend { get; set; }
        int PrefixLength { get; }
        int SendBytesRemaining { get; set; }
        int BytesSentAlready { get; set; }
        int ReceivePrefixBytesDoneThisOperation { get; set; }
    }

    public class ClientDataUserToken : IClientDataUserToken
    {
        private readonly int _receiveMessageOffsetPlaceholder;

        public ClientDataUserToken(int receiveOffset, int sendOffset, int tokenId)
        {
            PrefixLength = SocketClient.PrefixLength;
            TokenId = tokenId;
            BufferReceiveOffset = receiveOffset;
            BufferSendOffset = sendOffset;
            ReceiveMessageOffset = BufferReceiveOffset + PrefixLength;
            _receiveMessageOffsetPlaceholder = ReceiveMessageOffset;
        }

        
        public SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }
        public IDataHolder DataHolder { get; set; }
        public byte[] DataToSend { get; set; }
        public int PrefixLength { get; private set; }
        public int SendBytesRemaining { get; set; }
        public int BytesSentAlready { get; set; }
        public int ReceivedMessageBytesDoneCount { get; set; }
        public int ReceivePrefixLength { get; set; }
        public int TokenId { get; set; }
        public int BufferReceiveOffset { get; set; }
        public int BufferSendOffset { get; private set; }
        public int ReceiveMessageOffset { get; set; }
        public int RecPrefixBytesDoneThisOperation { get; set; }
        public int LengthOfCurrentIncomingMessage { get; set; }
        public int ReceivePrefixBytesDoneThisOperation { get; set; }
        public int ReceivedPrefixBytesDoneCount { get; set; }
        public byte[] ByteArrayForPrefix { get; set; }

        public void CreateNewDataHolder()
        {
            DataHolder = new ClientDataHolder();
        }

        public void Reset()
        {
            ReceivedPrefixBytesDoneCount = 0;
            ReceivedMessageBytesDoneCount = 0;
            ReceivePrefixBytesDoneThisOperation = 0;
            ReceiveMessageOffset = _receiveMessageOffsetPlaceholder;
        }
    }
}