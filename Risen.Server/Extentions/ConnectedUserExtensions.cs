using System;
using System.Linq;
using System.Text;
using Risen.Server.Tcp;
using Risen.Server.Tcp.Cache;
using Risen.Shared.Enums;

namespace Risen.Server.Extentions
{
    public static class ConnectedUserExtensions
    {
        public static void PollSocket(this ConnectedUser connectedUser, ITcpMessageProcessorCache tcpMessageProcessorCache)
        {
            if (!connectedUser.TcpClient.Connected)
                return;

            var stream = connectedUser.TcpClient.GetStream();
            if (!stream.CanRead || !stream.DataAvailable)
                return;

            var prefixBuffer = new byte[4];
            var messageTypeBuffer = new byte[1];

            stream.Read(prefixBuffer, 0, 4);
            stream.Read(messageTypeBuffer, 0, 1);

            var length = BitConverter.ToInt32(prefixBuffer, 0);
            var buffer = new byte[length];

            stream.Read(buffer, 0, length);

            var processor = tcpMessageProcessorCache.GetApplicableProcessor((MessageType)messageTypeBuffer.First());
            processor.Execute(connectedUser, Encoding.ASCII.GetString(buffer));
        }
    }
}
