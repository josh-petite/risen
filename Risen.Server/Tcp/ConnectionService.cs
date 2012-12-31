using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Server.Tcp
{
    public class ConnectionService : IConnectionService
    {
        private readonly object _connectionMutex = new object();
        private readonly List<ConnectedUser> _connections = new List<ConnectedUser>();

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
    }
}
