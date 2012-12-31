using System.Diagnostics;
using System.Linq;
using Risen.ConsoleServer.Configuration;
using Risen.Server.Tcp;
using StructureMap;

namespace Risen.ConsoleServer
{
    class Program
    {
        private static void Main(string[] args)
        {
            ConsoleServerRegistry.Configure();

            var connectionService = ObjectFactory.GetInstance<IConnectionService>();
            ObjectFactory.GetInstance<ITcpListenerService>();
            var connectedUsersMonitor = ObjectFactory.GetInstance<IConnectedUsersMonitor>();
            connectedUsersMonitor.Start(connectionService);

            Process.GetCurrentProcess().WaitForExit();
        }
    }
}
