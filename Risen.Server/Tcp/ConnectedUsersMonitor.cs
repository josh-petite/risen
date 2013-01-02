using System;
using System.Linq;
using System.Threading;

namespace Risen.Server.Tcp
{
    public interface IConnectedUsersMonitor
    {
        void Start(IConnectionService connectionService);
    }

    public class ConnectedUsersMonitor : IConnectedUsersMonitor
    {
        public void Start(IConnectionService connectionService)
        {
            while (true)
            {
                Thread.Sleep(5000);

                var connectedUsers = connectionService.GetConnections();
                ColorText(ConsoleColor.Green, "Number of players connected: {0}.", connectedUsers.Count());
                

                foreach (var connectedUser in connectedUsers)
                {
                    if (connectedUser.TcpClient.Connected)
                        Console.WriteLine("Player {0} is connected.", connectedUser.Player.User.Username);
                    else
                    {
                        ColorText(ConsoleColor.Red, "{0} has disconnected!", connectedUser.Player.User.Username);
                        connectionService.RemoveConnection(connectedUser);
                    }
                }
            }
        }

        private void ColorText(ConsoleColor consoleColor, string mask, params object[] extras)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(mask, extras);
            Console.ResetColor();
        }
    }
}