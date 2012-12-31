using System.Diagnostics;
using System.Linq;
using Risen.ConsoleServer.Configuration;
using Risen.Server.Tcp;

namespace Risen.ConsoleServer
{
    class Program
    {
        private static void Main(string[] args)
        {
            ConsoleServerRegistry.Configure();

            var connectionService = new ConnectionService();

            var service = new TcpListenerService(connectionService);
            service.Start();
            new ConnectedUsersMonitor().Start(connectionService);

            Process.GetCurrentProcess().WaitForExit();
        }
    }
}
