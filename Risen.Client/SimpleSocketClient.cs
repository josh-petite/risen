using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Risen.Client
{
    public class SimpleSocketClient
    {
        private Socket _socket;

        public SimpleSocketClient()
        {
            Init();
        }

        private void Init()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var localEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000);

            try
            {
                _socket.Connect(localEndpoint);
            }
            catch (Exception)
            {
                Console.Write("Unable to connect to remote endpoint.\r\n");
            }

            while (true)
            {
                Console.Write("Enter Text: ");
                var text = Console.ReadLine();

                if (string.IsNullOrEmpty(text))
                    break;

                var data = Encoding.Default.GetBytes(text);
                _socket.Send(data, 0, data.Length, 0);
            }

            _socket.Close();
        }
    }
}