using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Risen.Client.Tcp
{
    public interface ISocketClient
    {
        void Send(string message);
        void Connect();
        void Hammer();
    }

    public class SocketClient : ISocketClient
    {
        private TcpClient _tcpClient;

        public void Connect()
        {
            _tcpClient = new TcpClient("127.0.0.1", 4444);
        }

        public void Hammer()
        {
            var stream = _tcpClient.GetStream();

            for (int i = 0; i < 500000; i++)
            {
                var preparedMessage = PrepareMessage(i.ToString());
                stream.Write(preparedMessage, 0, preparedMessage.Length);
            }

        }

        public void Send(string message)
        {
            var preparedMessage = PrepareMessage(message);
            var stream = _tcpClient.GetStream();

            Program.TraceListener.WriteLine(string.Format("Message: {0} - Bytes: {1} - Prepared Message: {2}",
                                                          message,
                                                          preparedMessage.Aggregate(string.Empty, (current, t) => current + t),
                                                          BitConverter.ToString(preparedMessage)));

            stream.Write(preparedMessage, 0, preparedMessage.Length);

            Program.TraceListener.Flush();
        }
        
        private byte[] PrepareMessage(string message)
        {
            var messageInBytes = Encoding.Default.GetBytes(message);
            var prefix = BitConverter.GetBytes(messageInBytes.Length);
            var result = new byte[prefix.Length + messageInBytes.Length];

            Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
            Buffer.BlockCopy(messageInBytes, 0, result, prefix.Length, messageInBytes.Length);
            
            return result;
        }
    }
}
