﻿using System;
using System.Configuration;

namespace Risen.Server.Tcp
{
    public interface IListenerConfiguration
    {
        int GetTotalBytesRequiredForInitialBufferConfiguration();
        int ReceiveBufferSize { get; }
        int MaxSimultaneousAcceptOperations { get; }
        int MaxNumberOfConnections { get; }
    }

    public class ListenerConfiguration : IListenerConfiguration
    {
        public int MaxNumberOfConnections { get; private set; }
        public int Port { get; private set; }
        public int ReceiveBufferSize { get; private set; }
        public int MaxSimultaneousAcceptOperations { get; private set; }
        public int Backlog { get; private set; }
        public int OperationsToPreallocate { get; private set; }
        public int ExcessSaeaObjectsInPool { get; private set; }
        public int ReceivePrefixLength { get; private set; }
        public int SendPrefixLength { get; private set; }
        public int MainTransmissionId { get; private set; }
        public int StartingId { get; private set; }
        public int MainSessionId { get; private set; }
        public int MaxSimultaneousClientsThatWereConnected { get; private set; }
        public int NumberOfSaeaForRecSend { get; private set; }

        public void Init()
        {
            MaxNumberOfConnections = Convert.ToInt32(ConfigurationManager.AppSettings["MaxNumberOfConnections"]);
            Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            ReceiveBufferSize = Convert.ToInt32(ConfigurationManager.AppSettings["ReceiveBufferSize"]);
            MaxSimultaneousAcceptOperations = Convert.ToInt32(ConfigurationManager.AppSettings["MaxSimultaneousAcceptOperations"]);
            Backlog = Convert.ToInt32(ConfigurationManager.AppSettings["Backlog"]);
            OperationsToPreallocate = Convert.ToInt32(ConfigurationManager.AppSettings["OperationsToPreallocate"]);
            ExcessSaeaObjectsInPool = Convert.ToInt32(ConfigurationManager.AppSettings["ExcessSaeaObjectsInPool"]);
            ReceivePrefixLength = Convert.ToInt32(ConfigurationManager.AppSettings["ReceivePrefixLength"]);
            SendPrefixLength = Convert.ToInt32(ConfigurationManager.AppSettings["SendPrefixLength"]);
            MainTransmissionId = Convert.ToInt32(ConfigurationManager.AppSettings["MainTransmissionId"]);
            StartingId = Convert.ToInt32(ConfigurationManager.AppSettings["StartingId"]);
            MainSessionId = Convert.ToInt32(ConfigurationManager.AppSettings["MainSessionId"]);
            MaxSimultaneousClientsThatWereConnected = Convert.ToInt32(ConfigurationManager.AppSettings["MaxSimultaneousClientsThatWereConnected"]);

            NumberOfSaeaForRecSend = MaxNumberOfConnections + ExcessSaeaObjectsInPool;
        }

        public int GetTotalBytesRequiredForInitialBufferConfiguration()
        {
            return ReceiveBufferSize*NumberOfSaeaForRecSend*OperationsToPreallocate;
        }

    }
}