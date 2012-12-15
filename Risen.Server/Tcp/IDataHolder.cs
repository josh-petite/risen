using System.Net;

namespace Risen.Server.Tcp
{
    public interface IDataHolder
    {
        byte[] DataMessageReceived { get; set; }
        long SessionId { get; set; }
        int ReceivedTransmissionId { get; set; }
        EndPoint RemoteEndpoint { get; set; }
    }
}
