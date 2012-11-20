using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Risen.Server.Tcp
{
    public class AsyncTcpServer
    {
        private readonly TcpListener _tcpListener;
        private readonly List<Client> _clients;

        public AsyncTcpServer(IPAddress address, int port) : this()
        {
            _tcpListener = new TcpListener(address, port);
        }

        public AsyncTcpServer(IPEndPoint endPoint) : this()
        {
            _tcpListener = new TcpListener(endPoint);
        }

        public AsyncTcpServer()
        {
            Encoding = Encoding.Default;
            _clients = new List<Client>();
        }

        public Encoding Encoding { get; set; }

        public IEnumerable<TcpClient> TcpClients
        {
            get { return _clients.Select(client => client.TcpClient); }
        }

        public void Start()
        {
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
        }

        public void Stop()
        {
            _tcpListener.Stop();

            lock (_clients)
            {
                foreach (var client in _clients)
                    client.TcpClient.Client.Disconnect(false);

                _clients.Clear();
            }
        }

        public void Write(byte[] bytes)
        {
            foreach (var client in _clients)
                Write(client.TcpClient, bytes);
        }

        public void Write(TcpClient tcpClient, byte[] bytes)
        {
            var networkStream = tcpClient.GetStream();
            networkStream.BeginWrite(bytes, 0, bytes.Length, WriteCallBack, tcpClient);
        }

        private void WriteCallBack(IAsyncResult ar)
        {
            var tcpClient = ar.AsyncState as TcpClient;
            
            if (tcpClient != null)
            {
                var networkStream = tcpClient.GetStream();
                networkStream.EndWrite(ar);
            }
        }

        private void AcceptTcpClientCallback(IAsyncResult ar)
        {
            var tcpClient = _tcpListener.EndAcceptTcpClient(ar);
            var buffer = new byte[tcpClient.ReceiveBufferSize];
            var client = new Client(tcpClient, buffer);

            lock (_clients)
            {
                _clients.Add(client);
            }

            var networkStream = client.NetworkStream;
            networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            var client = ar.AsyncState as Client;
            if (client == null) return;

            var networkStream = client.NetworkStream;
            int read;

            try { read = networkStream.EndRead(ar); }
            catch { read = 0; }

            if (read == 0)
            {
                lock (_clients)
                {
                    _clients.Remove(client);
                    return;
                }
            }

            var data = Encoding.GetString(client.Buffer, 0, read);

            // need to use the data here
            Console.WriteLine(data);

            networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);
        }
    }

    internal class Client
    {
        public Client(TcpClient tcpClient, byte[] buffer)
        {
            if (tcpClient == null) throw new ArgumentNullException("tcpClient");
            if (buffer == null) throw new ArgumentNullException("buffer");

            TcpClient = tcpClient;
            Buffer = buffer;
        }

        public TcpClient TcpClient { get; private set; }
        public byte[] Buffer { get; private set; }
        public NetworkStream NetworkStream { get { return TcpClient.GetStream(); } }
    }
}