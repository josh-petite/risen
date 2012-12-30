using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Risen.Shared.Enums;

namespace Risen.Client.Tcp
{
    public interface ISocketClient
    {
        void Send(MessageType messageType, string message);
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
                var preparedMessage = PrepareMessage(MessageType.Unknown, i.ToString());
                stream.Write(preparedMessage, 0, preparedMessage.Length);
            }

        }

        public void Send(MessageType messageType, string message)
        {
            var preparedMessage = PrepareMessage(messageType, message);
            var stream = _tcpClient.GetStream();
            stream.Write(preparedMessage, 0, preparedMessage.Length);
        }

        private byte[] PrepareMessage(MessageType messageType, string message)
        {
            var messageInBytes = Encoding.Default.GetBytes(message);
            var messageTypeInBytes = new[] {(byte) messageType};
            var prefix = BitConverter.GetBytes(messageInBytes.Length);
            var result = new byte[prefix.Length + messageTypeInBytes.Length + messageInBytes.Length];

            Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
            Buffer.BlockCopy(messageTypeInBytes, 0, result, prefix.Length, messageTypeInBytes.Length);
            Buffer.BlockCopy(messageInBytes, 0, result, prefix.Length + messageTypeInBytes.Length, messageInBytes.Length);
            
            return result;
        }
    }
}
