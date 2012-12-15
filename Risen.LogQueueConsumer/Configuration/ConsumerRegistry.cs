using Risen.Server.Msmq;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace Risen.LogQueueConsumer.Configuration
{
    public class ConsumerRegistry : Registry
    {
        private static bool _isConfigured;

        public static void Configure()
        {
            if (!_isConfigured)
            {
                _isConfigured = true;

                ObjectFactory.Initialize(r =>
                                             {
                                                 r.Scan(x =>
                                                            {
                                                                x.TheCallingAssembly();
                                                                x.AssemblyContainingType<ILogger>();
                                                                x.WithDefaultConventions();
                                                            });

                                                 r.For<ILogger>().Singleton().Use<Logger>();
                                                 r.For<ILogMessageQueue>().Singleton().Use<LogMessageQueue>();
                                             });
            }
        }
    }
}
