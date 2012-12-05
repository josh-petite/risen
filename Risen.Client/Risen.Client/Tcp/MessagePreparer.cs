using System;
using System.Net.Sockets;
using System.Text;
using Risen.Shared.Tcp;
using Risen.Shared.Tcp.Tokens;

namespace Risen.Client.Tcp
{
    public interface IMessagePreparer
    {
    }

    public class MessagePreparer : IMessagePreparer
    {
        internal void GetDataToSend(SocketAsyncEventArgs e)
        {
            var userToken = (IDataHoldingUserToken)e.UserToken;
            DataHolder dataHolder = userToken.DataHolder;

            //In this example code, we will  
            //prefix the message with the length of the message. So we put 2 
            //things into the array.
            // 1) prefix,
            // 2) the message.

            //Determine the length of the message that we will send.
            Int32 lengthOfCurrentOutgoingMessage = dataHolder.arrayOfMessagesToSend[dataHolder.NumberOfMessagesSent].Length;

            //convert the message to byte array
            Byte[] arrayOfBytesInMessage = Encoding.ASCII.GetBytes(dataHolder.arrayOfMessagesToSend[dataHolder.NumberOfMessagesSent]);

            //So, now we convert the length integer into a byte array.
            //Aren't byte arrays wonderful? Maybe you'll dream about byte arrays tonight!
            Byte[] arrayOfBytesInPrefix = BitConverter.GetBytes(lengthOfCurrentOutgoingMessage);

            //Create the byte array to send.
            userToken.dataToSend = new Byte[userToken.sendPrefixLength + lengthOfCurrentOutgoingMessage];

            //Now copy the 2 things to the theUserToken.dataToSend.
            Buffer.BlockCopy(arrayOfBytesInPrefix, 0, userToken.dataToSend, 0, userToken.sendPrefixLength);
            Buffer.BlockCopy(arrayOfBytesInMessage, 0, userToken.dataToSend, userToken.sendPrefixLength, lengthOfCurrentOutgoingMessage);

            userToken.sendBytesRemaining = userToken.sendPrefixLength + lengthOfCurrentOutgoingMessage;
            userToken.bytesSentAlready = 0;
        }
    }
}
