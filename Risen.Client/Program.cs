using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new SocketClient();
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

                Console.Write("Data sent!\r\n");

                //var buffer = new byte[8192];
                //var bytesReceived = _socket.Receive(buffer, 0, buffer.Length, 0);
                //Array.Resize(ref buffer, bytesReceived);

                //Console.WriteLine("Received: {0}", Encoding.Default.GetString(buffer));
            }

            _socket.Close();
        }
    }
}
