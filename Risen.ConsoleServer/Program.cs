using System.Diagnostics;
using System.Net;
using Risen.Server.Tcp;

namespace Risen.ConsoleServer
{
    class Program
    {
        private static void Main(string[] args)
        {
            // Simple Async Tcp Server
            //var server = new AsyncTcpServer(new IPAddress(0), 4000);
            //server.Start();

            // SocketAsyncEventArgsPool server
            var listenerConfiguration = new ListenerConfiguration();
            var bufferManager = new BufferManager(listenerConfiguration.GetTotalBytesRequiredForInitialBufferConfiguration(), listenerConfiguration.GetBufferSize());
            var logger = new Logger(true) {IsEnabled = true};
            var server = new SocketListener(listenerConfiguration, bufferManager, new PrefixHandler(logger), new MessageHandler(logger), logger);
            
            Process.GetCurrentProcess().WaitForExit();

            server.CleanUpOnExit();
            logger.WriteData(SocketListener.DataHolders, listenerConfiguration);
        }
    }
}
