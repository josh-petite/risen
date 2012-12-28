using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Server.Tcp
{
    public class TcpListenerServer
    {
        private readonly IServerConfiguration _serverConfiguration;
        private TcpListener _listener;

        public TcpListenerServer(IServerConfiguration serverConfiguration)
        {
            _serverConfiguration = serverConfiguration;
        }

        public void Start()
        {
            _listener = new TcpListener(_serverConfiguration.LocalEndPoint);
            _listener.Start(_serverConfiguration.Backlog);
            Console.WriteLine("*** Server Listening ***");

            StartAccept();
        }

        public void StartAccept()
        {
            var tcpClientAsync = GetAcceptTcpClientAsync();
            ProcessAccept(tcpClientAsync.Result);
        }

        private void ProcessAccept(TcpClient tcpClient)
        {
            //var saea = new SocketAsyncEventArgs();
            //saea.Completed += (sender, args) => ProcessCompleted(args);
            
            StartReceive(tcpClient);
        }

        private void StartReceive(TcpClient tcpClient)
        {
            NetworkStream stream;
            do
            {
                stream = tcpClient.GetStream();
                var prefixBuffer = new byte[_serverConfiguration.ReceivePrefixLength];
                var bytesRead = stream.Read(prefixBuffer, 0, _serverConfiguration.ReceivePrefixLength);

                if (bytesRead < _serverConfiguration.ReceivePrefixLength)
                {
                    Console.WriteLine("Yeah, not enough bytes in message to satify the prefix.");
                    continue;
                }

                var messageLength = BitConverter.ToInt32(prefixBuffer, 0);
                var data = new byte[messageLength];
                var dataBytesRead = stream.Read(data, 0, messageLength);

                if (messageLength != dataBytesRead)
                {
                    Console.WriteLine("Wtf...");
                    break;
                }

                Console.WriteLine("Data Received: {0}", Encoding.Default.GetString(data));
            } while (true);
        }

        private void ProcessCompleted(SocketAsyncEventArgs args)
        {
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    Console.WriteLine("Receive event fired.");
                    break;
                case SocketAsyncOperation.Send:
                    Console.WriteLine("Send event fired.");
                    break;
            }
        }

        public async Task<TcpClient> GetAcceptTcpClientAsync()
        {
            var tcpClient = await _listener.AcceptTcpClientAsync();
            return tcpClient;
        }
    }
}
