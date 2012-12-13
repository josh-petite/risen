using System;
using System.Messaging;
using Risen.Shared.Tcp;

namespace Risen.Shared.Msmq
{
    public interface ILogMessageQueue
    {
        void Send(Message message);
    }

    public class LogMessageQueue : MessageQueue, ILogMessageQueue
    {
        public LogMessageQueue(ISharedConfiguration configuration) : base(configuration.LogQueue)
        {
        }

        public void Send(Message message)
        {
            if (Transactional)
            {
                using (var transaction = new MessageQueueTransaction())
                {
                    transaction.Begin();

                    try
                    {
                        Send(message, MessageQueueTransactionType.Single);
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Abort();
                    }
                }
            }
            else
                base.Send(message);
        }
    }
}
