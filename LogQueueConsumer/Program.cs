using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Messaging;
using Risen.LogQueueConsumer.Configuration;
using Risen.Shared.Msmq;
using StructureMap;

namespace Risen.LogQueueConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsumerRegistry.Configure();

            var consumer = ObjectFactory.GetInstance<IConsumer>();
            var messagesToWriteToDb = consumer.ConsumeLogMessages();
            var formatter = new XmlMessageFormatter(new[] {typeof (LogMessage)});

            using (var connection = new SqlConnection(ConfigurationManager.AppSettings["ConnectionString"]))
            {
                connection.Open();

                foreach (var messageToWrite in messagesToWriteToDb)
                {
                    messageToWrite.Formatter = formatter;
                    var message = (LogMessage) messageToWrite.Body;

                    using (var command = new SqlCommand("CreateLogMessage", connection) { CommandType = CommandType.StoredProcedure})
                    {
                        command.Parameters.AddWithValue("@Category", message.Category);
                        command.Parameters.AddWithValue("@Severity", message.Severity);
                        command.Parameters.AddWithValue("@Description", message.Description);
                        command.Parameters.AddWithValue("@CreatedDate", message.CreatedDate);
                        command.Parameters.AddWithValue("@CreatedBy", message.CreatedBy);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }

    public class Consumer : IConsumer
    {
        private readonly ILogMessageQueue _logMessageQueue;

        public Consumer(ILogMessageQueue logMessageQueue)
        {
            _logMessageQueue = logMessageQueue;
        }

        public IEnumerable<Message> ConsumeLogMessages()
        {
            return _logMessageQueue.ReceiveMessages();
        }
    }

    public interface IConsumer
    {
        IEnumerable<Message> ConsumeLogMessages();
    }
}
