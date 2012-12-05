using System;
using System.Net.Sockets;
using System.Text;
using Risen.Shared.Tcp.Tokens;

namespace Risen.Shared.Tcp
{
    public interface IPrefixHandler
    {
        int HandlePrefix(SocketAsyncEventArgs socketAsyncEventArgs, IDataHoldingUserToken dataHoldingUserToken, int remainingBytesToProcess);
    }

    public class PrefixHandler : IPrefixHandler
    {
        private readonly ILogger _logger;

        public PrefixHandler(ILogger logger)
        {
            _logger = logger;
        }

        public int HandlePrefix(SocketAsyncEventArgs socketAsyncEventArgs, IDataHoldingUserToken receiveSendToken, Int32 remainingBytesToProcess)
        {
            //ReceivedPrefixBytesDoneCount tells us how many prefix bytes were
            //processed during previous receive ops which contained data for
            //this message. Usually there will NOT have been any previous
            //receive ops here. So in that case,
            //receiveSendToken.ReceivedPrefixBytesDoneCount would equal 0.
            //Create a byte array to put the new prefix in, if we have not
            //already done it in a previous loop.
            if (receiveSendToken.ReceivedPrefixBytesDoneCount == 0)
            {
                _logger.WriteLine(LogCategory.Info, string.Format("Prefix Handler: Creating prefix array: {0}", receiveSendToken.TokenId));
                receiveSendToken.ByteArrayForPrefix = new byte[receiveSendToken.ReceivePrefixLength];
            }

            //If this next if-statement is true, then we have received at
            //least enough bytes to have the prefix. So we can determine the
            //length of the message that we are working on.
            if (remainingBytesToProcess >= receiveSendToken.ReceivePrefixLength - receiveSendToken.ReceivedPrefixBytesDoneCount)
            {
                _logger.WriteLine(LogCategory.Info, string.Format("PrefixHandler, enough for prefix on Token Id: {0}. remainingBytesToProcess = {1}",
                                                                  receiveSendToken.TokenId,
                                                                  remainingBytesToProcess));
                //Now copy that many bytes to ByteArrayForPrefix.
                //We can use the variable receiveMessageOffset as our main
                //index to show which index to get data from in the TCP buffer.
                Buffer.BlockCopy(socketAsyncEventArgs.Buffer,
                                 receiveSendToken.ReceiveMessageOffset - receiveSendToken.ReceivePrefixLength + receiveSendToken.ReceivedPrefixBytesDoneCount,
                                 receiveSendToken.ByteArrayForPrefix,
                                 receiveSendToken.ReceivedPrefixBytesDoneCount,
                                 receiveSendToken.ReceivePrefixLength - receiveSendToken.ReceivedPrefixBytesDoneCount);

                remainingBytesToProcess = remainingBytesToProcess - receiveSendToken.ReceivePrefixLength + receiveSendToken.ReceivedPrefixBytesDoneCount;
                receiveSendToken.RecPrefixBytesDoneThisOperation = receiveSendToken.ReceivePrefixLength - receiveSendToken.ReceivedPrefixBytesDoneCount;
                receiveSendToken.ReceivedPrefixBytesDoneCount = receiveSendToken.ReceivePrefixLength;
                receiveSendToken.LengthOfCurrentIncomingMessage = BitConverter.ToInt32(receiveSendToken.ByteArrayForPrefix, 0);
                LogPrefixDetails(receiveSendToken);
            }
                //This next else-statement deals with the situation
                //where we have some bytes of this prefix in this receive operation, but not all.
            else
            {
                _logger.WriteLine(LogCategory.Warning, string.Format("PrefixHandler, NOT all of prefix on Token Id: {0}. remainingBytesToProcess = {1}",
                                                                     receiveSendToken.TokenId,
                                                                     remainingBytesToProcess));
                //Write the bytes to the array where we are putting the
                //prefix data, to save for the next loop.
                Buffer.BlockCopy(socketAsyncEventArgs.Buffer,
                                 receiveSendToken.ReceiveMessageOffset - receiveSendToken.ReceivePrefixLength + receiveSendToken.ReceivedPrefixBytesDoneCount,
                                 receiveSendToken.ByteArrayForPrefix,
                                 receiveSendToken.ReceivedPrefixBytesDoneCount,
                                 remainingBytesToProcess);

                receiveSendToken.RecPrefixBytesDoneThisOperation = remainingBytesToProcess;
                receiveSendToken.ReceivedPrefixBytesDoneCount += remainingBytesToProcess;
                remainingBytesToProcess = 0;
            }

            // This section is needed when we have received
            // an amount of data exactly equal to the amount needed for the prefix,
            // but no more. And also needed with the situation where we have received
            // less than the amount of data needed for prefix.
            if (remainingBytesToProcess == 0)
            {
                receiveSendToken.ReceiveMessageOffset = receiveSendToken.ReceiveMessageOffset - receiveSendToken.RecPrefixBytesDoneThisOperation;
                receiveSendToken.RecPrefixBytesDoneThisOperation = 0;
            }

            return remainingBytesToProcess;
        }

        private void LogPrefixDetails(IDataHoldingUserToken receiveSendToken)
        {
            //Now see what integer the prefix bytes represent, for the length.
            var sb = new StringBuilder(receiveSendToken.ByteArrayForPrefix.Length);
            sb.Append(string.Format("Token Id: {0}. {1} bytes in prefix:", receiveSendToken.TokenId, receiveSendToken.ReceivePrefixLength));

            foreach (byte theByte in receiveSendToken.ByteArrayForPrefix)
                sb.Append(" " + theByte);

            sb.Append(string.Format(". Message length: {0}", receiveSendToken.LengthOfCurrentIncomingMessage));
            _logger.WriteLine(LogCategory.Info, sb.ToString());
        }
    }
}