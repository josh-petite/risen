using System.Net.Sockets;
using System.Text;

namespace Risen.Client.Tcp
{
    public interface ISocketClient
    {
        void Send(string message);
        void Connect();
    }

    public class SocketClient : ISocketClient
    {
        private TcpClient _tcpClient;

        public void Connect()
        {
            _tcpClient = new TcpClient("127.0.0.1", 4444);
        }

        public void Send(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            var stream = _tcpClient.GetStream();
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
