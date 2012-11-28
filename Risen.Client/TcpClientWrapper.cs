using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Risen.Client
{
    public class TcpClientWrapper
    {
        private TcpClient _tcpClient;

        public TcpClientWrapper()
        {
            Init();
        }

        private void Init()
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444));

            while (true)
            {
                Console.Write("Enter Text: ");
                var text = Console.ReadLine();

                if (string.IsNullOrEmpty(text))
                    break;

                var data = Encoding.Default.GetBytes(text);
                var stream = _tcpClient.GetStream();

                stream.Write(data, 0, data.Length);
            }

            _tcpClient.Close();

        }
    }
}