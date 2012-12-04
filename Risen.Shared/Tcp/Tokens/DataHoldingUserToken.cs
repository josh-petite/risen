using System.Net.Sockets;
using System.Threading;
using Risen.Shared.Tcp.Factories;

namespace Risen.Shared.Tcp.Tokens
{
    public interface IDataHoldingUserToken
    {
        SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }
        int TokenId { get; set; }
        void Init();
        void CreateNewDataHolder();
    }

    public class DataHoldingUserToken : IDataHoldingUserToken
    {
        private readonly IMediatorFactory _mediatorFactory;
        private readonly IListenerConfiguration _listenerConfiguration;

        public IMediator Mediator;
        public DataHolder DataHolder;
        public int BufferOffsetReceive;
        public int PermanentReceiveMessageOffset;
        public int BufferOffsetSend;
        public int LengthOfCurrentIncomingMessage;

        //receiveMessageOffset is used to mark the byte position where the message
        //begins in the receive buffer. This value can sometimes be out of
        //bounds for the data stream just received. But, if it is out of bounds, the
        //code will not access it.
        public int ReceiveMessageOffset;
        public byte[] ByteArrayForPrefix;
        public int ReceivePrefixLength;
        public int ReceivedPrefixBytesDoneCount = 0;
        public int ReceivedMessageBytesDoneCount = 0;

        //This variable will be needed to calculate the value of the
        //receiveMessageOffset variable in one situation. Notice that the
        //name is similar but the usage is different from the variable
        //receiveSendToken.receivePrefixBytesDone.
        public int RecPrefixBytesDoneThisOperation = 0;

        public int SendBytesRemainingCount;
        public int SendPrefixLength;
        public byte[] DataToSend;
        public int BytesSentAlreadyCount;

        //The session ID correlates with all the data sent in a connected session.
        //It is different from the transmission ID in the DataHolder, which relates
        //to one TCP message. A connected session could have many messages, if you
        //set up your app to allow it.

        public DataHoldingUserToken(IMediatorFactory mediatorFactory, IListenerConfiguration listenerConfiguration)
        {
            _mediatorFactory = mediatorFactory;
            _listenerConfiguration = listenerConfiguration;
        }

        public long SessionId { get; private set; }
        public int TokenId { get; set; }
        public SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }

        public void CreateNewDataHolder()
        {
            DataHolder = new DataHolder();
        }

        public void Init()
        {
            Mediator = _mediatorFactory.GenerateMediator(SocketAsyncEventArgs);
            BufferOffsetReceive = SocketAsyncEventArgs.Offset;
            BufferOffsetSend = SocketAsyncEventArgs.Offset + _listenerConfiguration.ReceiveBufferSize;
            ReceivePrefixLength = _listenerConfiguration.ReceivePrefixLength;
            SendPrefixLength = _listenerConfiguration.SendPrefixLength;
            ReceiveMessageOffset = BufferOffsetReceive + ReceivePrefixLength;
            PermanentReceiveMessageOffset = ReceiveMessageOffset;
        }

        // Used to create sessionId variable in GetDataHoldingUserToken. Called in ProcessAccept().
        public void CreateSessionId(ref long sessionId)
        {
            SessionId = Interlocked.Increment(ref sessionId);
        }

        public void Reset()
        {
            ReceivedPrefixBytesDoneCount = 0;
            ReceivedMessageBytesDoneCount = 0;
            RecPrefixBytesDoneThisOperation = 0;
            ReceiveMessageOffset = PermanentReceiveMessageOffset;
        }
    }
}