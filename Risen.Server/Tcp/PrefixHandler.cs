using System;
using System.Net.Sockets;
using System.Text;
using Risen.Server.Msmq;
using Risen.Server.Tcp.Tokens;

namespace Risen.Server.Tcp
{
    public interface IPrefixHandler
    {
        int HandlePrefix(SocketAsyncEventArgs socketAsyncEventArgs, DataHoldingUserToken userToken, int remainingBytesToProcess);
    }

    public class PrefixHandler : IPrefixHandler
    {
        private readonly ILogger _logger;

        public PrefixHandler(ILogger logger)
        {
            _logger = logger;
        }

        public int HandlePrefix(SocketAsyncEventArgs socketAsyncEventArgs, DataHoldingUserToken userToken, Int32 remainingBytesToProcess)
        {
            //ReceivedPrefixBytesDoneCount tells us how many prefix bytes were
            //processed during previous receive ops which contained data for
            //this message. Usually there will NOT have been any previous
            //receive ops here. So in that case,
            //userToken.ReceivedPrefixBytesDoneCount would equal 0.
            //Create a byte array to put the new prefix in, if we have not
            //already done it in a previous loop.
            if (userToken.ReceivedPrefixBytesDoneCount == 0)
            {
                _logger.QueueMessage(LogMessage.Create(LogCategory.TcpServer, LogSeverity.Debug, string.Format("Prefix Handler: Creating prefix array: {0}", userToken.TokenId)));
                userToken.ByteArrayForPrefix = new byte[userToken.ReceivePrefixLength];
            }

            //If this next if-statement is true, then we have received at
            //least enough bytes to have the prefix. So we can determine the
            //length of the message that we are working on.
            if (remainingBytesToProcess >= userToken.ReceivePrefixLength - userToken.ReceivedPrefixBytesDoneCount)
            {
                _logger.QueueMessage(LogMessage.Create(LogCategory.TcpServer, LogSeverity.Debug,
                                                       string.Format("PrefixHandler, enough for prefix on Token Id: {0}. remainingBytesToProcess = {1}",
                                                                     userToken.TokenId,
                                                                     remainingBytesToProcess)));
                //Now copy that many bytes to ByteArrayForPrefix.
                //We can use the variable receiveMessageOffset as our main
                //index to show which index to get data from in the TCP buffer.
                Buffer.BlockCopy(socketAsyncEventArgs.Buffer,
                                 userToken.ReceiveMessageOffset - userToken.ReceivePrefixLength + userToken.ReceivedPrefixBytesDoneCount,
                                 userToken.ByteArrayForPrefix,
                                 userToken.ReceivedPrefixBytesDoneCount,
                                 userToken.ReceivePrefixLength - userToken.ReceivedPrefixBytesDoneCount);

                remainingBytesToProcess = remainingBytesToProcess - userToken.ReceivePrefixLength + userToken.ReceivedPrefixBytesDoneCount;
                userToken.RecPrefixBytesDoneThisOperation = userToken.ReceivePrefixLength - userToken.ReceivedPrefixBytesDoneCount;
                userToken.ReceivedPrefixBytesDoneCount = userToken.ReceivePrefixLength;
                userToken.LengthOfCurrentIncomingMessage = BitConverter.ToInt32(userToken.ByteArrayForPrefix, 0);
                LogPrefixDetails(userToken);
            }
                //This next else-statement deals with the situation
                //where we have some bytes of this prefix in this receive operation, but not all.
            else
            {
                _logger.QueueMessage(LogMessage.Create(LogCategory.TcpServer, LogSeverity.Warning,
                                                       string.Format("PrefixHandler, NOT all of prefix on Token Id: {0}. remainingBytesToProcess = {1}",
                                                                     userToken.TokenId,
                                                                     remainingBytesToProcess)));
                //Write the bytes to the array where we are putting the
                //prefix data, to save for the next loop.
                Buffer.BlockCopy(socketAsyncEventArgs.Buffer,
                                 userToken.ReceiveMessageOffset - userToken.ReceivePrefixLength + userToken.ReceivedPrefixBytesDoneCount,
                                 userToken.ByteArrayForPrefix,
                                 userToken.ReceivedPrefixBytesDoneCount,
                                 remainingBytesToProcess);

                userToken.RecPrefixBytesDoneThisOperation = remainingBytesToProcess;
                userToken.ReceivedPrefixBytesDoneCount += remainingBytesToProcess;
                remainingBytesToProcess = 0;
            }

            // This section is needed when we have received
            // an amount of data exactly equal to the amount needed for the prefix,
            // but no more. And also needed with the situation where we have received
            // less than the amount of data needed for prefix.
            if (remainingBytesToProcess == 0)
            {
                userToken.ReceiveMessageOffset = userToken.ReceiveMessageOffset - userToken.RecPrefixBytesDoneThisOperation;
                userToken.RecPrefixBytesDoneThisOperation = 0;
            }

            return remainingBytesToProcess;
        }

        private void LogPrefixDetails(DataHoldingUserToken receiveSendToken)
        {
            //Now see what integer the prefix bytes represent, for the length.
            var sb = new StringBuilder(receiveSendToken.ByteArrayForPrefix.Length);
            sb.Append(string.Format("Token Id: {0}. {1} bytes in prefix:", receiveSendToken.TokenId, receiveSendToken.ReceivePrefixLength));

            foreach (byte theByte in receiveSendToken.ByteArrayForPrefix)
                sb.Append(" " + theByte);

            sb.Append(string.Format(". Message length: {0}", receiveSendToken.LengthOfCurrentIncomingMessage));
            _logger.QueueMessage(LogMessage.Create(LogCategory.TcpServer, LogSeverity.Debug, sb.ToString()));
        }
    }
}