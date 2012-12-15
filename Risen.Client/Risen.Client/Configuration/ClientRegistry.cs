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
                                x.WithDefaultConventions();
                            });
                    });
            }
        }
    }
}
