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

            var service = new TcpListenerService();
            service.Start();
            new ConnectedUsersMonitor().Start(service.ConnectedUsers);

            Process.GetCurrentProcess().WaitForExit();
        }
    }
}
