using System.Linq;
using System.Net;
using System.Threading;
using Risen.Server.Tcp;

namespace Risen.ConsoleServer
{
    class Program
    {
        // http://robjdavey.wordpress.com/2011/02/12/asynchronous-tcp-server-example/
        static void Main(string[] args)
        {
            var server = new AsyncTcpServer(new IPAddress(0), 4000);
            server.Start();

            while (server.TcpClients.Any())
            {
            }
        }
    }
}
