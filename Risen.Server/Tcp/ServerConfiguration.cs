using System;
using System.Configuration;
using System.Net;

namespace Risen.Server.Tcp
{
    public interface IServerConfiguration
    {
        int MaxAcceptOperations { get; set; }
        int MaxNumberOfConnections { get; }
        IPEndPoint LocalEndPoint { get; set; }
        int Backlog { get; }
        int NumberOfSaeaForRecSend { get; }
        int ReceivePrefixLength { get; }
        int SendPrefixLength { get; }
        string LogQueue { get; }
        bool IsLoggerEnabled { get; }
    }

    public class ServerConfiguration : IServerConfiguration
    {
        public ServerConfiguration()
        {
            MaxNumberOfConnections = Convert.ToInt32(ConfigurationManager.AppSettings["MaxNumberOfConnections"]);
            Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            MaxAcceptOperations = Convert.ToInt32(ConfigurationManager.AppSettings["MaxAcceptOperations"]);
            Backlog = Convert.ToInt32(ConfigurationManager.AppSettings["Backlog"]);
            ReceivePrefixLength = Convert.ToInt32(ConfigurationManager.AppSettings["ReceivePrefixLength"]);
            SendPrefixLength = Convert.ToInt32(ConfigurationManager.AppSettings["SendPrefixLength"]);
            MaxSimultaneousClientsThatWereConnected = Convert.ToInt32(ConfigurationManager.AppSettings["MaxSimultaneousClientsThatWereConnected"]);
            IsLoggerEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["IsLoggerEnabled"]);
            LogQueue = ConfigurationManager.AppSettings["LogQueue"];

            LocalEndPoint = new IPEndPoint(IPAddress.Any, Port);
        }

        public int MaxNumberOfConnections { get; private set; }
        public int Port { get; private set; }
        public int MaxAcceptOperations { get; set; }
        public int Backlog { get; private set; }
        public int ReceivePrefixLength { get; private set; }
        public int SendPrefixLength { get; private set; }
        public int MaxSimultaneousClientsThatWereConnected { get; private set; }
        public int NumberOfSaeaForRecSend { get; private set; }
        public IPEndPoint LocalEndPoint { get; set; }
        public bool IsLoggerEnabled { get; private set; }
        public string LogQueue { get; private set; }
    }
}