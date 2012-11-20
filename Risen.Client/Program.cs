using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var testmsg = "hi there server";

            var client = new Server.Tcp.Client(new TcpClient("127.0.0.1", 4000), Encoding.Default.GetBytes(testmsg));

        }
    }
}
