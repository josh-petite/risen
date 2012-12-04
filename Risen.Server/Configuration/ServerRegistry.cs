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
                                                            });

                                                 r.For<ILogger>()
                                                  .Use<Logger>()
                                                  .Ctor<bool>("shouldLogToConsole").EqualToAppSetting("ShouldLogToConsole")
                                                  .Ctor<bool>("isLoggerEnabled").EqualToAppSetting("IsLoggerEnabled");
                                             });
            }
        }
    }
}
