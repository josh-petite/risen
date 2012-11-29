using System.Diagnostics;
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

            // Simple Async Tcp Server
            //var server = new AsyncTcpServer(new IPAddress(0), 4000);
            //server.Start();

            // SocketAsyncEventArgsPool server
            var server = ObjectFactory.GetInstance<ISocketListener>();
            
            Process.GetCurrentProcess().WaitForExit();

            server.CleanUpOnExit();
        }
    }
}
