namespace Risen.Server.Tcp
{
    public interface IConnectionService
    {
        void AddConnection(ConnectedUser user);
        void RemoveConnection(ConnectedUser user);
        ConnectedUser[] GetConnections();
    }
}