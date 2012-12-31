using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Risen.Server.Tcp.Cache;
using Risen.Shared.Enums;

namespace Risen.Server.Tcp
{
    public interface ITcpListenerService
    {
        void Start();
    }

    public class TcpListenerService : ITcpListenerService
    {
        private readonly IConnectionService _connectionService;
        private readonly ITcpMessageProcessorCache _tcpMessageProcessorCache;

        public TcpListenerService(IConnectionService connectionService, ITcpMessageProcessorCache tcpMessageProcessorCache)
        {
            _connectionService = connectionService;
            _tcpMessageProcessorCache = tcpMessageProcessorCache;
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
            try
            {
                HandshakeAndIdentify(tcpClientTask.Result);
            }
            catch (AggregateException ex)
            {
                //TODO: Logging?
            }
            catch (Exception ex)
            {
                //TODO: Logging?
            }
            finally
            {
                _tcpListener.AcceptTcpClientAsync().ContinueWith(TcpClientConnected);
            }            
        }

        private void HandshakeAndIdentify(TcpClient result)
        {
            var connectedUser = new ConnectedUser {TcpClient = result, Identifier = Guid.NewGuid()};
            AwaitLoginInformationFromNewConnection(connectedUser);
            _connectionService.AddConnection(connectedUser);
        }

        private void AwaitLoginInformationFromNewConnection(ConnectedUser connectedUser)
        {
            while (true)
            {
                if (!connectedUser.TcpClient.Connected)
                    continue;

                var stream = connectedUser.TcpClient.GetStream();
                if (!stream.CanRead || !stream.DataAvailable)
                    continue;

                var prefixBuffer = new byte[4];
                var messageTypeBuffer = new byte[1];

                stream.Read(prefixBuffer, 0, 4);
                stream.Read(messageTypeBuffer, 0, 1);

                var length = BitConverter.ToInt32(prefixBuffer, 0);
                var buffer = new byte[length];
                
                stream.Read(buffer, 0, length);

                var processor = _tcpMessageProcessorCache.GetApplicableProcessor((MessageType) messageTypeBuffer.First());
                processor.Execute(connectedUser, Encoding.ASCII.GetString(buffer));

                break;
            }
        }
    }
}
