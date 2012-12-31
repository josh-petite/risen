using Risen.Server.Msmq;
using Risen.Server.Tcp;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace Risen.ConsoleServer.Configuration
{
    public class ConsoleServerRegistry : Registry
    {
        private static bool _isConfigured;

        public static void Configure()
        {
            if (_isConfigured) 
                return;

            _isConfigured = true;

            ObjectFactory.Initialize(r =>
                                         {
                                             r.Scan(x =>
                                                        {
                                                            x.TheCallingAssembly();
                                                            x.AssemblyContainingType<ILogger>();
                                                            x.WithDefaultConventions();
                                                        });

                                             r.For<ILogger>().Singleton();
                                             r.For<ILogMessageQueue>().Singleton();
                                             r.For<ITcpListenerService>().Singleton().OnCreationForAll(o => o.Start());
                                             r.For<IConnectionService>().Singleton();
                                         });
        }
    }
}
