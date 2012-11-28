using System.Net.Sockets;
using System.Threading;

namespace Risen.Server.Tcp
{
    public class DataHoldingUserToken
    {
        private readonly int _id;

        public Mediator Mediator;
        public DataHolder DataHolder;
        public readonly int BufferOffsetReceive;
        public readonly int PermanentReceiveMessageOffset;
        public readonly int BufferOffsetSend;
        public int LengthOfCurrentIncomingMessage;

        //receiveMessageOffset is used to mark the byte position where the message
        //begins in the receive buffer. This value can sometimes be out of
        //bounds for the data stream just received. But, if it is out of bounds, the
        //code will not access it.
        public int ReceiveMessageOffset;
        public byte[] ByteArrayForPrefix;
        public readonly int ReceivePrefixLength;
        public int ReceivedPrefixBytesDoneCount = 0;
        public int ReceivedMessageBytesDoneCount = 0;

        //This variable will be needed to calculate the value of the
        //receiveMessageOffset variable in one situation. Notice that the
        //name is similar but the usage is different from the variable
        //receiveSendToken.receivePrefixBytesDone.
        public int RecPrefixBytesDoneThisOperation = 0;

        public int SendBytesRemainingCount;
        public readonly int SendPrefixLength;
        public byte[] DataToSend;
        public int BytesSentAlreadyCount;

        //The session ID correlates with all the data sent in a connected session.
        //It is different from the transmission ID in the DataHolder, which relates
        //to one TCP message. A connected session could have many messages, if you
        //set up your app to allow it.
        private int _sessionId;

        public DataHoldingUserToken(SocketAsyncEventArgs e, IListenerConfiguration listenerConfiguration, ILogger logger, int tokenId)
        {
            _id = tokenId;

            Mediator = new Mediator(e, listenerConfiguration, logger);
            BufferOffsetReceive = e.Offset;
            BufferOffsetSend = e.Offset + listenerConfiguration.ReceiveBufferSize;
            ReceivePrefixLength = listenerConfiguration.ReceivePrefixLength;
            SendPrefixLength = listenerConfiguration.SendPrefixLength;
            ReceiveMessageOffset = BufferOffsetReceive + ReceivePrefixLength;
            PermanentReceiveMessageOffset = ReceiveMessageOffset;
        }

        //Let's use an ID for this object during testing, just so we can see what
        //is happening better if we want to.
        public int TokenId
        {
            get
            {
                return _id;
            }
        }

        internal void CreateNewDataHolder()
        {
            DataHolder = new DataHolder();
        }

        //Used to create sessionId variable in DataHoldingUserToken.
        //Called in ProcessAccept().
        internal void CreateSessionId()
        {
            int mainSessionId = SocketListener.MainSessionId;
            _sessionId = Interlocked.Increment(ref mainSessionId);
        }

        public int SessionId
        {
            get
            {
                return _sessionId;
            }
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