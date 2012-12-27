using System;
using System.Messaging;

namespace Risen.Server.Msmq
{
    public interface ILogger
    {
        void QueueMessage(LogCategory logCategory, LogSeverity logSeverity, string message);
    }

    public class Logger : ILogger
    {
        private readonly object _mutex = new object();
        private readonly ILogMessageQueue _logMessageQueue;

        public Logger(ILogMessageQueue logMessageQueue)
        {
            _logMessageQueue = logMessageQueue;
        }

        public void QueueMessage(LogCategory logCategory, LogSeverity logSeverity, string message)
        {
            var logMessage = LogMessage.Create(logCategory, logSeverity, message);
            Console.WriteLine(logMessage.ToString());

            lock (_mutex)
                _logMessageQueue.Send(new Message {Body = logMessage, Label = "LogMessage", UseDeadLetterQueue = true});
        }
    }
}