using Risen.Logic.Tcp;

namespace Risen.ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SocketServer();
            server.Execute();
        }
    }
}
