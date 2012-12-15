using System;
using System.Collections.Generic;
using System.Messaging;
using Risen.Server.Tcp;

namespace Risen.Server.Msmq
{
    public interface ILogMessageQueue
    {
        void Send(Message message);
        IEnumerable<Message> ReceiveMessages();
    }

    public class LogMessageQueue : MessageQueue, ILogMessageQueue
    {
        public LogMessageQueue(IServerConfiguration configuration) : base(configuration.LogQueue)
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

        public IEnumerable<Message> ReceiveMessages()
        {
            return new Message[0];

            // maybe add something using parallel processing here to grab messages quick?
            //while ()
            //var message = Receive();
        }
    }
}
