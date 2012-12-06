using System;
using System.Net.Sockets;
using System.Text;
using Risen.Client.Tcp.Extensions;
using Risen.Client.Tcp.Tokens;
using Risen.Shared.Tcp;

namespace Risen.Client.Tcp
{
    public interface IMessagePreparer
    {
        void GetDataToSend(SocketAsyncEventArgs receiveSendEventArgs);
    }

    public class MessagePreparer : IMessagePreparer
    {
        public void GetDataToSend(SocketAsyncEventArgs e)
        {
            var userToken = (IClientDataUserToken)e.UserToken;
            var dataHolder = userToken.DataHolder.AsClientDataHolder();

            //In this example code, we will  
            //prefix the message with the length of the message. So we put 2 
            //things into the array.
            // 1) prefix,
            // 2) the message.

            //Determine the length of the message that we will send.
            Int32 lengthOfCurrentOutgoingMessage = dataHolder.MessagesToSend[dataHolder.NumberOfMessagesSent].Length;

            //convert the message to byte array
            Byte[] arrayOfBytesInMessage = Encoding.ASCII.GetBytes(dataHolder.MessagesToSend[dataHolder.NumberOfMessagesSent]);

            //So, now we convert the length integer into a byte array.
            //Aren't byte arrays wonderful? Maybe you'll dream about byte arrays tonight!
            Byte[] arrayOfBytesInPrefix = BitConverter.GetBytes(lengthOfCurrentOutgoingMessage);

            //Create the byte array to send.
            userToken.DataToSend = new Byte[userToken.ReceivePrefixLength + lengthOfCurrentOutgoingMessage];

            //Now copy the 2 things to the theUserToken.dataToSend.
            Buffer.BlockCopy(arrayOfBytesInPrefix, 0, userToken.DataToSend, 0, userToken.ReceivePrefixLength);
            Buffer.BlockCopy(arrayOfBytesInMessage, 0, userToken.DataToSend, userToken.ReceivePrefixLength, lengthOfCurrentOutgoingMessage);

            userToken.SendBytesRemaining = userToken.ReceivePrefixLength + lengthOfCurrentOutgoingMessage;
            userToken.BytesSentAlready = 0;
        }
    }
}
