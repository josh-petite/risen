using System;
using System.Net.Sockets;
using Risen.Server.Tcp.Tokens;

namespace Risen.Server.Tcp
{
    public interface IMessageHandler
    {
        bool HandleMessage(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken dataHoldingUserToken, int remainingBytesToProcess);
    }

    public class MessageHandler : IMessageHandler
    {
        private readonly ILogger _logger;

        public MessageHandler(ILogger logger)
        {
            _logger = logger;
        }

        public bool HandleMessage(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken receiveSendToken, int remainingBytesToProcess)
        {
            var incomingTcpMessageIsReady = false;

            //Create the array where we'll store the complete message,
            //if it has not been created on a previous receive op.
            if (receiveSendToken.ReceivedMessageBytesDoneCount == 0)
            {
                _logger.WriteLine(LogCategory.Info, string.Format("Message Handler: Creating Receive Array on Id: {0}", receiveSendToken.TokenId));
                receiveSendToken.DataHolder.DataMessageReceived = new Byte[receiveSendToken.LengthOfCurrentIncomingMessage];
            }

            // Remember there is a receiveSendToken.receivedPrefixBytesDoneCount
            // variable, which allowed us to handle the prefix even when it
            // requires multiple receive ops. In the same way, we have a
            // receiveSendToken.ReceivedMessageBytesDoneCount variable, which
            // helps us handle message data, whether it requires one receive operation or many.
            if (remainingBytesToProcess + receiveSendToken.ReceivedMessageBytesDoneCount == receiveSendToken.LengthOfCurrentIncomingMessage)
            {
                // If we are inside this if-statement, then we got
                // the end of the message. In other words,
                // the total number of bytes we received for this message matched the
                // message length value that we got from the prefix.

                // Write/append the bytes received to the byte array in the
                // DataHolder object that we are using to store our data.
                Buffer.BlockCopy(receiveSendEventArgs.Buffer,
                                 receiveSendToken.ReceiveMessageOffset,
                                 receiveSendToken.DataHolder.DataMessageReceived,
                                 receiveSendToken.ReceivedMessageBytesDoneCount,
                                 remainingBytesToProcess);

                incomingTcpMessageIsReady = true;
            }
            else
            {
                // If we are inside this else-statement, then that means that we
                // need another receive op. We still haven't got the whole message,
                // even though we have examined all the data that was received.
                // Not a problem. In SocketListener.ProcessReceive we will just call
                // StartReceive to do another receive op to receive more data.

                Buffer.BlockCopy(receiveSendEventArgs.Buffer,
                                 receiveSendToken.ReceiveMessageOffset,
                                 receiveSendToken.DataHolder.DataMessageReceived,
                                 receiveSendToken.ReceivedMessageBytesDoneCount,
                                 remainingBytesToProcess);

                receiveSendToken.ReceiveMessageOffset = receiveSendToken.ReceiveMessageOffset - receiveSendToken.RecPrefixBytesDoneThisOperation;
                receiveSendToken.ReceivedMessageBytesDoneCount += remainingBytesToProcess;
            }

            return incomingTcpMessageIsReady;
        }
    }
}