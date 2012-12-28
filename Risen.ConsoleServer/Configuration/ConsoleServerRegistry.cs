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
            if (!_isConfigured)
            {
                _isConfigured = true;

                ObjectFactory.Initialize(r =>
                                             {
                                                 r.Scan(x =>
                                                            {
                                                                x.TheCallingAssembly();
                                                                //x.AssemblyContainingType<ISocketListener>();
                                                                x.AssemblyContainingType<ILogger>();
                                                                x.WithDefaultConventions();
                                                            });

                                                 r.For<ILogger>().Singleton().Use<Logger>();
                                                 r.For<IBufferManager>().Singleton().Use<BufferManager>();
                                                 r.For<ILogMessageQueue>().Singleton().Use<LogMessageQueue>();

                                                 //r.For<ISocketListener>().Singleton().OnCreationForAll(o =>
                                                 //    {
                                                 //        o.Init();
                                                 //        o.StartListen();
                                                 //    });
                                             });
            }
        }
    }
}
