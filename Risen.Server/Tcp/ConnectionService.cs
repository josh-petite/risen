using System.Collections.Generic;
using System.Threading;
using Risen.Server.Extentions;
using Risen.Server.Tcp.Cache;

namespace Risen.Server.Tcp
{
    public interface IConnectionService
    {
        void AddConnection(ConnectedUser user);
        void RemoveConnection(ConnectedUser user);
        ConnectedUser[] GetConnections();
        void PollConnectedUsersForUpdates();
    }

    public class ConnectionService : IConnectionService
    {
        private readonly ITcpMessageProcessorCache _tcpMessageProcessorCache;
        private readonly object _connectionMutex = new object();
        private readonly List<ConnectedUser> _connections = new List<ConnectedUser>();

        public ConnectionService(ITcpMessageProcessorCache tcpMessageProcessorCache)
        {
            _tcpMessageProcessorCache = tcpMessageProcessorCache;
        }

        public void AddConnection(ConnectedUser user)
        {
            lock (_connectionMutex)
                _connections.Add(user);
        }

        public void RemoveConnection(ConnectedUser user)
        {
            lock (_connectionMutex)
                _connections.Remove(user);
        }

        public ConnectedUser[] GetConnections()
        {
            lock (_connectionMutex)
                return _connections.ToArray();
        }

        public void PollConnectedUsersForUpdates()
        {
            while (true)
            {
                Thread.Sleep(10);

                lock (_connectionMutex)
                {
                    foreach (var connection in _connections)
                        connection.PollSocket(_tcpMessageProcessorCache);
                }
            }
        }
    }
}
