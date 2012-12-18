using System;
using System.Net.Sockets;
using System.Text;
using Risen.Server.Msmq;
using Risen.Server.Tcp.Tokens;

namespace Risen.Server.Tcp
{
    public interface IPrefixHandler
    {
        int HandlePrefix(SocketAsyncEventArgs socketAsyncEventArgs, DataHoldingUserToken token, int remainingBytesToProcess);
    }

    public class PrefixHandler : IPrefixHandler
    {
        private readonly ILogger _logger;

        public PrefixHandler(ILogger logger)
        {
            _logger = logger;
        }

        public int HandlePrefix(SocketAsyncEventArgs socketAsyncEventArgs, DataHoldingUserToken token, Int32 remainingBytesToProcess)
        {
            if (token.ReceivedPrefixBytesDoneCount == 0)
            {
                _logger.QueueMessage(LogMessage.Create(LogCategory.TcpServer, LogSeverity.Debug, string.Format("Prefix Handler: Creating prefix array: {0}", token.TokenId)));
                token.ByteArrayForPrefix = new byte[token.ReceivePrefixLength];
            }

            if (remainingBytesToProcess >= token.ReceivePrefixLength - token.ReceivedPrefixBytesDoneCount)
            {
                _logger.QueueMessage(LogMessage.Create(LogCategory.TcpServer, LogSeverity.Debug,
                                                       string.Format("PrefixHandler, enough for prefix on Token Id: {0}. remainingBytesToProcess = {1}",
                                                                     token.TokenId,
                                                                     remainingBytesToProcess)));
                
                Buffer.BlockCopy(socketAsyncEventArgs.Buffer,
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
                _logger.QueueMessage(LogMessage.Create(LogCategory.TcpServer, LogSeverity.Warning,
                                                       string.Format("PrefixHandler, NOT all of prefix on Token Id: {0}. remainingBytesToProcess = {1}",
                                                                     token.TokenId,
                                                                     remainingBytesToProcess)));
                
                Buffer.BlockCopy(socketAsyncEventArgs.Buffer,
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
            sb.Append(string.Format("Token Id: {0}. {1} bytes in prefix:", receiveSendToken.TokenId, receiveSendToken.ReceivePrefixLength));

            foreach (byte theByte in receiveSendToken.ByteArrayForPrefix)
                sb.Append(" " + theByte);

            sb.Append(string.Format(". Message length: {0}", receiveSendToken.LengthOfCurrentIncomingMessage));
            _logger.QueueMessage(LogMessage.Create(LogCategory.TcpServer, LogSeverity.Debug, sb.ToString()));
        }
    }
}