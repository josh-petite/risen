using Risen.Shared.Msmq;
using Risen.Shared.Tcp;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace Risen.Server.Configuration
{
    public class ServerRegistry : Registry
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
                                                                x.WithDefaultConventions();
                                                                x.TheCallingAssembly();
                                                                x.AssemblyContainingType<ILogger>();
                                                            });

                                                 r.For<ILogger>().Singleton().Use<Logger>();
                                                 r.For<IBufferManager>().Singleton().Use<BufferManager>().Ctor<IConfiguration>().Is<SharedConfiguration>();
                                             });
            }
        }
    }
}
