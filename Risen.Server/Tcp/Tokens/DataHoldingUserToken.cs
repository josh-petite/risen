using System.Net.Sockets;
using System.Threading;
using Risen.Server.Tcp.Factories;
using Risen.Shared.Tcp;
using Risen.Shared.Tcp.Tokens;

namespace Risen.Server.Tcp.Tokens
{
    public class DataHoldingUserToken : IUserToken
    {
        private readonly IMediatorFactory _mediatorFactory;
        private readonly ISharedConfiguration _sharedConfiguration;

        public IMediator Mediator;
        public IDataHolder DataHolder { get; set; }
        public int BufferReceiveOffset { get; set; }
        public int PermanentReceiveMessageOffset;
        public int BufferOffsetSend;
        public int LengthOfCurrentIncomingMessage { get; set; }

        //receiveMessageOffset is used to mark the byte position where the message
        //begins in the receive buffer. This value can sometimes be out of
        //bounds for the data stream just received. But, if it is out of bounds, the
        //code will not access it.
        public int ReceiveMessageOffset { get; set; }
        public byte[] ByteArrayForPrefix { get; set; }
        public int ReceivePrefixLength { get; set; }
        public int ReceivedPrefixBytesDoneCount { get; set; }
        public int ReceivedMessageBytesDoneCount { get; set; }

        //This variable will be needed to calculate the value of the
        //receiveMessageOffset variable in one situation. Notice that the
        //name is similar but the usage is different from the variable
        //receiveSendToken.receivePrefixBytesDone.
        public int RecPrefixBytesDoneThisOperation { get; set; }

        public int SendBytesRemainingCount;
        public int SendPrefixLength;
        public byte[] DataToSend;
        public int BytesSentAlreadyCount;

        //The session ID correlates with all the data sent in a connected session.
        //It is different from the transmission ID in the DataHolder, which relates
        //to one TCP message. A connected session could have many messages, if you
        //set up your app to allow it.

        public DataHoldingUserToken(IMediatorFactory mediatorFactory, ISharedConfiguration sharedConfiguration)
        {
            _mediatorFactory = mediatorFactory;
            _sharedConfiguration = sharedConfiguration;
            ReceivedPrefixBytesDoneCount = 0;
            RecPrefixBytesDoneThisOperation = 0;
            ReceivedMessageBytesDoneCount = 0;
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
            BufferReceiveOffset = SocketAsyncEventArgs.Offset;
            BufferOffsetSend = SocketAsyncEventArgs.Offset + _sharedConfiguration.BufferSize;
            ReceivePrefixLength = _sharedConfiguration.ReceivePrefixLength;
            SendPrefixLength = _sharedConfiguration.SendPrefixLength;
            ReceiveMessageOffset = BufferReceiveOffset + ReceivePrefixLength;
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