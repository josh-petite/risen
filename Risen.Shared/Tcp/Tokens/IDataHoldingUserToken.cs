using System.Net.Sockets;

namespace Risen.Shared.Tcp.Tokens
{
    public interface IDataHoldingUserToken
    {
        SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }
        int TokenId { get; set; }
        int ReceivedMessageBytesDoneCount { get; }
        int ReceivedPrefixBytesDoneCount { get; set; }
        byte[] ByteArrayForPrefix { get; set; }
        int ReceivePrefixLength { get; set; }
        int ReceiveMessageOffset { get; set; }
        int RecPrefixBytesDoneThisOperation { get; set; }
        int LengthOfCurrentIncomingMessage { get; set; }
        void Init();
        void CreateNewDataHolder();
    }
}
