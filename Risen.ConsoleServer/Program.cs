using System.Diagnostics;
using System.Net;
using Risen.Server.Tcp;

namespace Risen.ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Simple Async Tcp Server
            //var server = new AsyncTcpServer(new IPAddress(0), 4000);
            //server.Start();

            // SocketAsyncEventArgsPool server
            var listenerConfiguration = new ListenerConfiguration();
            var bufferManager = new BufferManager(listenerConfiguration.GetTotalBytesRequiredForInitialBufferConfiguration(), listenerConfiguration.GetBufferSize());
            var server = new SocketListener(listenerConfiguration, bufferManager, new PrefixHandler(), new MessageHandler(), new Logger(true));

            Process.GetCurrentProcess().WaitForExit();
        }
    }
}
