using System;
using System.Messaging;
using Risen.Shared.Tcp;

namespace Risen.Shared.Msmq
{
    public interface ILogger
    {
        void QueueMessage(LogMessage logMessage);
    }

    public class Logger : ILogger
    {
        private readonly object _mutex = new object();
        private readonly ILogMessageQueue _logMessageQueue;
        private readonly ISharedConfiguration _sharedConfiguration;

        public Logger(ILogMessageQueue logMessageQueue, ISharedConfiguration sharedConfiguration)
        {
            _sharedConfiguration = sharedConfiguration;
            _logMessageQueue = logMessageQueue;
        }

        public bool IsEnabled { get { return _sharedConfiguration.IsLoggerEnabled; } }

        public void QueueMessage(LogMessage logMessage)
        {
            if (!IsEnabled)
                return;

            Console.WriteLine(logMessage.ToString());

            lock (_mutex)
                _logMessageQueue.Send(new Message {Body = logMessage, Label = "LogMessage", UseDeadLetterQueue = true});
        }
    }
}