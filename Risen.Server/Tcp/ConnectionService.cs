using System.Collections.Generic;

namespace Risen.Server.Tcp
{
    public interface IConnectionService
    {
        void AddConnection(ConnectedUser user);
        void RemoveConnection(ConnectedUser user);
        ConnectedUser[] GetConnections();
    }

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
