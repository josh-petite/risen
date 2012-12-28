using System.Threading;
using Risen.Server.Tcp.Factories;
using StructureMap;

namespace Risen.Server.Tcp.Tokens
{
    public class DataHoldingUserToken
    {
        private readonly IMediatorFactory _mediatorFactory;
        private readonly IServerConfiguration _serverConfiguration;

        public DataHoldingUserToken(IMediatorFactory mediatorFactory, IServerConfiguration serverConfiguration)
        {
            _mediatorFactory = mediatorFactory;
            _serverConfiguration = serverConfiguration;
            ReceivedPrefixBytesDoneCount = 0;
            RecPrefixBytesDoneThisOperation = 0;
            ReceivedMessageBytesDoneCount = 0;
        }

        public Mediator Mediator { get; set; }
        public DataHolder DataHolder { get; set; }
        public int BufferOffsetReceive { get; set; }
        public int CachedReceiveMessageOffset { get; set; }
        public int BufferOffsetSend { get; set; }
        public int LengthOfCurrentIncomingMessage { get; set; }
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
        public int SendBytesRemainingCount { get; set; }
        public int SendPrefixLength { get; set; }
        public byte[] DataToSend { get; set; }
        public int BytesSentAlreadyCount { get; set; }

        //The session ID correlates with all the data sent in a connected session.
        //It is different from the transmission ID in the DataHolder, which relates
        //to one TCP message. A connected session could have many messages, if you
        //set up your app to allow it.
        public long SessionId { get; private set; }
        public int TokenId { get; set; }
        public SocketAsyncEvent SocketAsyncEvent { get; set; }

        public void CreateNewDataHolder()
        {
            DataHolder = ObjectFactory.GetInstance<DataHolder>();
        }

        public void Init()
        {
            Mediator = _mediatorFactory.GenerateMediator(SocketAsyncEvent);
            BufferOffsetReceive = SocketAsyncEvent.Offset;
            BufferOffsetSend = SocketAsyncEvent.Offset + _serverConfiguration.BufferSize;
            ReceivePrefixLength = _serverConfiguration.ReceivePrefixLength;
            SendPrefixLength = _serverConfiguration.SendPrefixLength;
            ReceiveMessageOffset = BufferOffsetReceive + ReceivePrefixLength;
            CachedReceiveMessageOffset = ReceiveMessageOffset;
        }

        public void CreateSessionId(ref long sessionId)
        {
            SessionId = Interlocked.Increment(ref sessionId);
        }

        public void Reset()
        {
            ReceivedPrefixBytesDoneCount = 0;
            ReceivedMessageBytesDoneCount = 0;
            RecPrefixBytesDoneThisOperation = 0;
            ReceiveMessageOffset = CachedReceiveMessageOffset;
        }
    }
}