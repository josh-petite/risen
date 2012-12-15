using System;
using System.Configuration;
using System.Net;

namespace Risen.Server.Tcp
{
    public interface IServerConfiguration
    {
        int BufferSize { get; }
        int MaxSimultaneousAcceptOperations { get; set; }
        int MaxNumberOfConnections { get; }
        IPEndPoint LocalEndPoint { get; set; }
        int Backlog { get; }
        int NumberOfSaeaForRecSend { get; }
        int ReceivePrefixLength { get; }
        int SendPrefixLength { get; }
        int MainTransmissionId { get; }
        string LogQueue { get; }

        int GetTotalBytesRequiredForInitialBufferConfiguration();
        int GetTotalBufferSize();
    }

    public class ServerConfiguration : IServerConfiguration
    {
        public ServerConfiguration()
        {
            Init();
        }

        public int MaxNumberOfConnections { get; private set; }
        public int Port { get; private set; }
        public int BufferSize { get; private set; }
        public int MaxSimultaneousAcceptOperations { get; set; }
        public int Backlog { get; private set; }
        public int OperationsToPreallocate { get; private set; }
        public int ExcessSaeaObjectsInPool { get; private set; }
        public int ReceivePrefixLength { get; private set; }
        public int SendPrefixLength { get; private set; }
        public int MainTransmissionId { get; private set; }
        public int StartingId { get; private set; }
        public int MaxSimultaneousClientsThatWereConnected { get; private set; }
        public int NumberOfSaeaForRecSend { get; private set; }
        public IPEndPoint LocalEndPoint { get; set; }
        public bool IsLoggerEnabled { get; private set; }
        public string LogQueue { get; private set; }

        private void Init()
        {
            MaxNumberOfConnections = Convert.ToInt32(ConfigurationManager.AppSettings["MaxNumberOfConnections"]);
            Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            BufferSize = Convert.ToInt32(ConfigurationManager.AppSettings["BufferSize"]);
            MaxSimultaneousAcceptOperations = Convert.ToInt32(ConfigurationManager.AppSettings["MaxSimultaneousAcceptOperations"]);
            Backlog = Convert.ToInt32(ConfigurationManager.AppSettings["Backlog"]);
            OperationsToPreallocate = Convert.ToInt32(ConfigurationManager.AppSettings["OperationsToPreallocate"]);
            ExcessSaeaObjectsInPool = Convert.ToInt32(ConfigurationManager.AppSettings["ExcessSaeaObjectsInPool"]);
            ReceivePrefixLength = Convert.ToInt32(ConfigurationManager.AppSettings["ReceivePrefixLength"]);
            SendPrefixLength = Convert.ToInt32(ConfigurationManager.AppSettings["SendPrefixLength"]);
            MainTransmissionId = Convert.ToInt32(ConfigurationManager.AppSettings["MainTransmissionId"]);
            StartingId = Convert.ToInt32(ConfigurationManager.AppSettings["StartingId"]);
            MaxSimultaneousClientsThatWereConnected = Convert.ToInt32(ConfigurationManager.AppSettings["MaxSimultaneousClientsThatWereConnected"]);
            IsLoggerEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["IsLoggerEnabled"]);
            LogQueue = ConfigurationManager.AppSettings["LogQueue"];

            NumberOfSaeaForRecSend = MaxNumberOfConnections + ExcessSaeaObjectsInPool;
            LocalEndPoint = new IPEndPoint(IPAddress.Any, Port);
        }

        public int GetTotalBytesRequiredForInitialBufferConfiguration()
        {
            return BufferSize*NumberOfSaeaForRecSend*OperationsToPreallocate;
        }

        public int GetTotalBufferSize()
        {
            return BufferSize*OperationsToPreallocate;
        }
    }
}