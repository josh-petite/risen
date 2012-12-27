using System;
using System.Messaging;
using Risen.Server.Tcp;

namespace Risen.Server.Msmq
{
    public interface ILogger
    {
        void QueueMessage(LogCategory logCategory, LogSeverity logSeverity, string message);
        void Enable(bool isEnabled);
    }

    public class Logger : ILogger
    {
        private readonly object _mutex = new object();
        private readonly ILogMessageQueue _logMessageQueue;
        private bool _isEnabled;

        public Logger(ILogMessageQueue logMessageQueue, IServerConfiguration serverConfiguration)
        {
            _logMessageQueue = logMessageQueue;
            _isEnabled = serverConfiguration.IsLoggerEnabled;
        }

        public void QueueMessage(LogCategory logCategory, LogSeverity logSeverity, string message)
        {
            if (!_isEnabled)
                return;

            var logMessage = LogMessage.Create(logCategory, logSeverity, message);
            Console.WriteLine(logMessage.ToString());

            lock (_mutex)
                _logMessageQueue.Send(new Message {Body = logMessage, Label = "LogMessage", UseDeadLetterQueue = true});
        }

        public void Enable(bool isEnabled)
        {
            _isEnabled = isEnabled;
        }
    }
}