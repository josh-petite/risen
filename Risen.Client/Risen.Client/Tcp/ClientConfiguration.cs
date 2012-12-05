using System.Net;
using Risen.Shared.Tcp;

namespace Risen.Client.Tcp
{
    public interface IClientConfiguration : IConfiguration
    {
        int MaxNumberOfConnections { get; }
        int MaxSimultaneousAcceptOperations { get; }
        int NumberOfSaeaForRecSend { get; }
    }

    public class ClientConfiguration : IClientConfiguration
    {
        public ClientConfiguration()
        {
            Init();
        }

        public int MaxNumberOfConnections { get; private set; }
        public int Port { get; private set; }
        public int ReceiveBufferSize { get; private set; }
        public int MaxSimultaneousAcceptOperations { get; set; }
        public int Backlog { get; private set; }
        public int OperationsToPreallocate { get; private set; }
        public int ExcessSaeaObjectsInPool { get; private set; }
        public int ReceivePrefixLength { get; private set; }
        public int SendPrefixLength { get; private set; }
        public int MainTransmissionId { get; private set; }
        public int StartingId { get; private set; }
        public int NumberOfSaeaForRecSend { get; private set; }
        public IPEndPoint LocalEndPoint { get; set; }

        private void Init()
        {
            MaxNumberOfConnections = 5;
            Port = 4444;
            ReceiveBufferSize = 100;
            MaxSimultaneousAcceptOperations = 5;
            Backlog = 10;
            OperationsToPreallocate = 2;
            ExcessSaeaObjectsInPool = 1;
            ReceivePrefixLength = 4;
            SendPrefixLength = 4;
            MainTransmissionId = 100;
            StartingId = 0;
            
            NumberOfSaeaForRecSend = MaxNumberOfConnections + ExcessSaeaObjectsInPool;
            LocalEndPoint = new IPEndPoint(IPAddress.Any, Port);
        }

        public int GetTotalBytesRequiredForInitialBufferConfiguration()
        {
            return ReceiveBufferSize * NumberOfSaeaForRecSend * OperationsToPreallocate;
        }

        public int GetBufferSize()
        {
            return ReceiveBufferSize * OperationsToPreallocate;
        }
    }
}