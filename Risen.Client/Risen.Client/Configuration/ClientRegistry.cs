using Risen.Client.Tcp;
using Risen.Shared.Tcp;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace Risen.Client.Configuration
{
    public class ClientRegistry : Registry
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

                        r.For<ILogger>()
                         .Singleton()
                         .Use<Logger>()
                         .Ctor<bool>("shouldLogToConsole").EqualToAppSetting("ShouldLogToConsole")
                         .Ctor<bool>("isLoggerEnabled").EqualToAppSetting("IsLoggerEnabled");

                        r.For<IBufferManager>()
                         .Singleton()
                         .Use<BufferManager>()
                         .Ctor<IConfiguration>().Is<ClientConfiguration>();
                    });
            }
        }
    }
}
