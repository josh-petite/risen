using System;
using System.Text;
using Risen.Server.Msmq;
using Risen.Server.Tcp.Tokens;

namespace Risen.Server.Tcp
{
    public interface IPrefixHandler
    {
        int HandlePrefix(SocketAsyncEvent socketAsyncEvent, DataHoldingUserToken token, int remainingBytesToProcess);
    }

    public class PrefixHandler : IPrefixHandler
    {
        private readonly ILogger _logger;

        public PrefixHandler(ILogger logger)
        {
            _logger = logger;
        }

        public int HandlePrefix(SocketAsyncEvent socketAsyncEvent, DataHoldingUserToken token, int remainingBytesToProcess)
        {
            if (token.ReceivedPrefixBytesDoneCount == 0)
            {
                token.ByteArrayForPrefix = new byte[token.ReceivePrefixLength];
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
                                     string.Format("PrefixHandler: Creating prefix array: {0} with byte array length of: {1}", token.TokenId,
                                                   token.ReceivePrefixLength));
            }

            if (remainingBytesToProcess >= token.ReceivePrefixLength - token.ReceivedPrefixBytesDoneCount)
            {
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug,
                                     string.Format("PrefixHandler: Enough for prefix on Token Id: {0}. remainingBytesToProcess = {1}",
                                                   token.TokenId,
                                                   remainingBytesToProcess));

                Buffer.BlockCopy(socketAsyncEvent.Buffer,
                                 token.ReceiveMessageOffset - token.ReceivePrefixLength + token.ReceivedPrefixBytesDoneCount,
                                 token.ByteArrayForPrefix,
                                 token.ReceivedPrefixBytesDoneCount,
                                 token.ReceivePrefixLength - token.ReceivedPrefixBytesDoneCount);

                remainingBytesToProcess -= token.ReceivePrefixLength + token.ReceivedPrefixBytesDoneCount;
                token.RecPrefixBytesDoneThisOperation = token.ReceivePrefixLength - token.ReceivedPrefixBytesDoneCount;
                token.ReceivedPrefixBytesDoneCount = token.ReceivePrefixLength;
                token.LengthOfCurrentIncomingMessage = BitConverter.ToInt32(token.ByteArrayForPrefix, 0);
                LogPrefixDetails(token);
            }
            else
            {
                _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Warning,
                                                       string.Format("PrefixHandler: NOT all of prefix on Token Id: {0}. remainingBytesToProcess = {1}",
                                                                     token.TokenId,
                                                                     remainingBytesToProcess));
                
                Buffer.BlockCopy(socketAsyncEvent.Buffer,
                                 token.ReceiveMessageOffset - token.ReceivePrefixLength + token.ReceivedPrefixBytesDoneCount,
                                 token.ByteArrayForPrefix,
                                 token.ReceivedPrefixBytesDoneCount,
                                 remainingBytesToProcess);

                token.RecPrefixBytesDoneThisOperation = remainingBytesToProcess;
                token.ReceivedPrefixBytesDoneCount += remainingBytesToProcess;
                remainingBytesToProcess = 0;
            }

            if (remainingBytesToProcess == 0)
            {
                token.ReceiveMessageOffset = token.ReceiveMessageOffset - token.RecPrefixBytesDoneThisOperation;
                token.RecPrefixBytesDoneThisOperation = 0;
            }

            return remainingBytesToProcess;
        }

        private void LogPrefixDetails(DataHoldingUserToken receiveSendToken)
        {
            var sb = new StringBuilder(receiveSendToken.ByteArrayForPrefix.Length);
            sb.Append(string.Format("PrefixHandler: Token Id: {0}. {1} bytes in prefix:", receiveSendToken.TokenId, receiveSendToken.ReceivePrefixLength));

            foreach (byte theByte in receiveSendToken.ByteArrayForPrefix)
                sb.Append(" " + theByte);

            sb.Append(string.Format(". Message length: {0}", receiveSendToken.LengthOfCurrentIncomingMessage));
            _logger.QueueMessage(LogCategory.TcpServer, LogSeverity.Debug, sb.ToString());
        }
    }
}