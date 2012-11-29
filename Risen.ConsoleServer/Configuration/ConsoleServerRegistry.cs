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
                                                                x.WithDefaultConventions();
                                                                x.AssemblyContainingType<ISocketListener>();
                                                            });

                                                 r.For<ILogger>()
                                                  .Singleton()
                                                  .Use<Logger>()
                                                  .Ctor<bool>("shouldLogToConsole").EqualToAppSetting("ShouldLogToConsole")
                                                  .Ctor<bool>("isLoggerEnabled").EqualToAppSetting("IsLoggerEnabled");

                                                 r.For<ISocketListener>().Singleton().OnCreationForAll(o =>
                                                                                                           {
                                                                                                               o.Init();
                                                                                                               o.StartListen();
                                                                                                           });
                                             });
            }
        }
    }
}
