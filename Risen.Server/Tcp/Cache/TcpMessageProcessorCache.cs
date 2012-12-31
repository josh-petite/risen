using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Risen.Shared.Enums;

namespace Risen.Server.Tcp.Cache
{
    public interface ITcpMessageProcessorCache
    {
        ITcpMessageProcessor GetApplicableProcessor(MessageType messageType);
    }

    public class TcpMessageProcessorCache : ITcpMessageProcessorCache
    {
        private readonly Dictionary<Type, ITcpMessageProcessor> _messageProcessors;

        public TcpMessageProcessorCache()
        {
            _messageProcessors = new Dictionary<Type, ITcpMessageProcessor>();
            Initialize();
        }

        private void Initialize()
        {
            var processorTypes = Assembly.GetAssembly(GetType())
                .GetTypes()
                .Where(o => o.GetInterfaces().Contains(typeof(ITcpMessageProcessor)) && !o.IsInterface && o.IsClass)
                .ToList();

            foreach (var processorType in processorTypes)
                _messageProcessors.Add(processorType, (ITcpMessageProcessor) Activator.CreateInstance(processorType));
        }

        public ITcpMessageProcessor GetApplicableProcessor(MessageType messageType)
        {
            return _messageProcessors.Single(o => o.Value.AppliesTo(messageType)).Value;
        }
    }
}