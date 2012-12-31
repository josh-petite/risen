using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Risen.Server.Tcp
{
    public class ConnectedUsersMonitor
    {
        public void Start(IConnectionService connectionService)
        {
            while (true)
            {
                Thread.Sleep(5000);

                var connectedUsers = connectionService.GetConnections();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Number of players connected: {0}.", connectedUsers.Count());
                Console.ForegroundColor = ConsoleColor.White;

                for (int i = 0; i < connectedUsers.Length; i++)
                {
                    var connectedUser = connectedUsers[i];
                    if (connectedUser.TcpClient.Connected)
                        Console.WriteLine("Player {0} is connected.", connectedUser.Player.User.Username);
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0} has disconnected!");
                        Console.ForegroundColor = ConsoleColor.White;
                        connectionService.RemoveConnection(connectedUser);
                    }
                }
            }
        }
    }
}