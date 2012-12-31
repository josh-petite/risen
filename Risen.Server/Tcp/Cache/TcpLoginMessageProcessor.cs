using System;
using System.Text;
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
                Console.WriteLine("User: {0} with password of {1} has logged in with the following identifier: {2}.", loginModel.Username, loginModel.Password,
                                  connectedUser.Identifier);
            }
        }
    }
}