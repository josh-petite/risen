using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Risen.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TcpClientWrapper();
        }
    }

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
            _tcpClient.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000));

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

    public class SocketClient
    {
        private Socket _socket;

        public SocketClient()
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
