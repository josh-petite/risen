using System;
using System.Messaging;
using Risen.Shared.Tcp;

namespace Risen.Shared.Msmq
{
    public interface ILogger
    {
        void QueueLogItem(LogCategory logCategory, string line, params object[] formatArguments);
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

        public void QueueLogItem(LogCategory logCategory, string format, params object[] formatArguments)
        {
            if (!IsEnabled)
                return;

            var formattedLine = string.Format("{0}: {1}", logCategory, string.Format(format, formatArguments));

            Console.WriteLine(formattedLine);

            lock (_mutex)
                _logMessageQueue.Send(new Message {Body = formattedLine, Label = "Log", UseDeadLetterQueue = true});
        }
    }

    public enum LogCategory
    {
        Info,
        Warning,
        Error
    }
}