using System;
using System.Net.Sockets;
using Risen.Server.Entities;

namespace Risen.Server.Tcp
{
    public class ConnectedUser
    {
        public TcpClient TcpClient { get; set; }
        public Guid Identifier { get; set; }
        public Player Player { get; set; }
    }
}