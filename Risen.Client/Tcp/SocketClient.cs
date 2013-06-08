using System;
using System.Net.Sockets;
using System.Text;
using Risen.Shared.Enums;


namespace Risen.Client.Tcp
{
    public interface ISocketClient : IDisposable
    {
        void Send(MessageType messageType, string message);
        void Connect();
        void Hammer();
        void Update();
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

        public void Update()
        {
            var stream = _tcpClient.GetStream();

            if (!stream.CanRead || !stream.DataAvailable)
                return;

            var prefixBuffer = new byte[4];
            var messageTypeBuffer = new byte[1];

            stream.Read(prefixBuffer, 0, 4);
            stream.Read(messageTypeBuffer, 4, 1);

            var length = BitConverter.ToInt32(prefixBuffer, 0);
            var messageBuffer = new byte[length];

            stream.Read(messageBuffer, 0, length);

            Receive((MessageType)BitConverter.ToInt32(messageTypeBuffer, 0), Encoding.Default.GetString(messageBuffer));
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
            var messageTypeInBytes = new[] { (byte)messageType };
            var prefix = BitConverter.GetBytes(messageInBytes.Length);
            var result = new byte[prefix.Length + messageTypeInBytes.Length + messageInBytes.Length];

            Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
            Buffer.BlockCopy(messageTypeInBytes, 0, result, prefix.Length, messageTypeInBytes.Length);
            Buffer.BlockCopy(messageInBytes, 0, result, prefix.Length + messageTypeInBytes.Length, messageInBytes.Length);

            return result;
        }

        private void Receive(MessageType messageType, string message)
        {
            switch (messageType)
            {
                case MessageType.KeepAlive:
                    //GameMain.KeepAlivesReceived++;
                    break;
                case MessageType.Unknown:
                    //GameMain.MessageReceived = message;
                    break;
            }
        }

        public void Dispose()
        {
            _tcpClient.Close();
        }
    }
}
