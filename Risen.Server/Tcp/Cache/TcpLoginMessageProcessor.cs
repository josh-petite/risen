using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Risen.Server.Entities;
using Risen.Shared.Enums;
using Risen.Shared.Models;

namespace Risen.Server.Tcp.Cache
{
    public class TcpLoginMessageProcessor : ITcpMessageProcessor
    {
        public bool AppliesTo(MessageType messageType)
        {
            return messageType == MessageType.Login;
        }

        public void Execute(ConnectedUser connectedUser, string jsonMessage)
        {
            var loginModel = JsonConvert.DeserializeObject<LoginModel>(jsonMessage);

            if (loginModel != null)
            {
                connectedUser.Player = new Player { User = new User { Username = loginModel.Username, Password = loginModel.Password } };
                EstablishKeepAliveFor(connectedUser);
                Console.WriteLine("User: {0} with password of {1} has logged in with the following identifier: {2}.", loginModel.Username, loginModel.Password,
                                  connectedUser.Identifier);
            }
        }

        private void EstablishKeepAliveFor(ConnectedUser connectedUser)
        {
            var client = connectedUser.TcpClient.Client;

            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            const uint time = 1000;
            const uint interval = 2000;
            BuildKeepAliveValues(client, true, time, interval);
        }

        private void BuildKeepAliveValues(Socket client, bool on, uint time, uint interval)
        {
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);

            /* the native structure
            struct tcp_keepalive {
            ULONG onoff;
            ULONG keepalivetime;
            ULONG keepaliveinterval;
            };
            */

            // marshal the equivalent of the native structure into a byte array
            var sizeOfUint = Marshal.SizeOf((uint)0);
            var optionInValue = new byte[sizeOfUint * 3];

            BitConverter.GetBytes(on ? (uint)1 : 0).CopyTo(optionInValue, 0);
            BitConverter.GetBytes(time).CopyTo(optionInValue, sizeOfUint);
            BitConverter.GetBytes(interval).CopyTo(optionInValue, sizeOfUint * 2);

            // call WSAIoctl via IOControl
            client.IOControl(IOControlCode.KeepAliveValues, optionInValue, null);
        }
    }
}