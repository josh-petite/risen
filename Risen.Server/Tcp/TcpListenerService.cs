using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Risen.Server.Entities;
using Risen.Shared.Models;

namespace Risen.Server.Tcp
{
    public class TcpListenerService
    {
        private readonly IConnectionService _connectionService;

        public TcpListenerService(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        private TcpListener _tcpListener;

        public void Start()
        {
            _tcpListener = new TcpListener(IPAddress.Any, 4444);
            _tcpListener.Start();
            _tcpListener.AcceptTcpClientAsync().ContinueWith(TcpClientConnected);
        }

        public void Stop()
        {
            _tcpListener.Stop();
        }

        private void TcpClientConnected(Task<TcpClient> tcpClientTask)
        {
            HandshakeAndIdentify(tcpClientTask.Result);
            _tcpListener.AcceptTcpClientAsync().ContinueWith(TcpClientConnected);
        }

        private void HandshakeAndIdentify(TcpClient result)
        {
            var connectedUser = new ConnectedUser {TcpClient = result, Identifier = Guid.NewGuid()};
            AwaitLoginInformationFromNewConnection(connectedUser);
            _connectionService.AddConnection(connectedUser);
        }

        private void AwaitLoginInformationFromNewConnection(ConnectedUser connectedUser)
        {
            new TcpLoginService().Await(connectedUser);
        }
    }
    
    public class TcpLoginService // this will be refactored ... just hardcoding a login service to test json serialization/deserialization
    {
        public void Await(ConnectedUser connectedUser)
        {
            while (true)
            {
                var prefixBuffer = new byte[4];
                var messageTypeBuffer = new byte[1];

                if (!connectedUser.TcpClient.Connected)
                    continue;

                var stream = connectedUser.TcpClient.GetStream();
                if (!stream.CanRead || !stream.DataAvailable)
                    continue;

                stream.Read(prefixBuffer, 0, 4);
                stream.Read(messageTypeBuffer, 0, 1);

                var length = BitConverter.ToInt32(prefixBuffer, 0);
                var buffer = new byte[length];
                
                stream.Read(buffer, 0, length);
                
                var loginModel = JsonConvert.DeserializeObject<LoginModel>(Encoding.ASCII.GetString(buffer));

                if (loginModel != null)
                {
                    connectedUser.Player = new Player {User = new User {Username = loginModel.Username, Password = loginModel.Password}};
                    Console.WriteLine("User: {0} with password of {1} has logged in with the following identifier: {2}.", loginModel.Username, loginModel.Password,
                                      connectedUser.Identifier);
                    break;
                }
            }
        }
    }
}
