namespace Risen.Server.Tcp.Tokens
{
    public interface IUserToken 
    {
        int ReceivedPrefixBytesDoneCount { get; set; }
        byte[] ByteArrayForPrefix { get; set; }
        int ReceivePrefixLength { get; set; }
        int TokenId { get; set; }
        int ReceiveMessageOffset { get; set; }
        int RecPrefixBytesDoneThisOperation { get; set; }
        int LengthOfCurrentIncomingMessage { get; set; }
        int ReceivedMessageBytesDoneCount { get; set; }
        IDataHolder DataHolder { get; set; }
        int BufferReceiveOffset { get; set; }
        void CreateNewDataHolder();
        void Reset();
    }
}