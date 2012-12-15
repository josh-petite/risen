using System;
using System.Net.Sockets;
using Risen.Server.Extentions;

namespace Risen.Server.Tcp
{
    public interface IOutgoingDataPreparer
    {
        void PrepareOutgoingData(SocketAsyncEventArgs e, IDataHolder handledDataHolder);
    }

    public class OutgoingDataPreparer : IOutgoingDataPreparer
    {
        private IDataHolder _dataHolder;

        public void PrepareOutgoingData(SocketAsyncEventArgs e, IDataHolder handledDataHolder)
        {
            var userToken = e.GetDataHoldingUserToken();
            _dataHolder = handledDataHolder;

            //In this example code, we will send back the receivedTransMissionId,
            // followed by the
            //message that the client sent to the server. And we must
            //prefix it with the length of the message. So we put 3
            //things into the array.
            // 1) prefix,
            // 2) receivedTransMissionId,
            // 3) the message that we received from the client, which
            // we stored in our DataHolder until we needed it.
            //That is our communication protocol. The client must know the protocol.

            //Convert the receivedTransMissionId to byte array.
            var idByteArray = BitConverter.GetBytes(_dataHolder.ReceivedTransmissionId);

            //Determine the length of all the data that we will send back.
            int lengthOfCurrentOutgoingMessage = idByteArray.Length + _dataHolder.DataMessageReceived.Length;

            //So, now we convert the length integer into a byte array.
            var prefixInBytes = BitConverter.GetBytes(lengthOfCurrentOutgoingMessage);

            //Create the byte array to send.
            userToken.DataToSend = new Byte[userToken.SendPrefixLength + lengthOfCurrentOutgoingMessage];

            //Now copy the 3 things to the theUserToken.dataToSend.
            Buffer.BlockCopy(prefixInBytes,
                             0,
                             userToken.DataToSend,
                             0,
                             userToken.SendPrefixLength);

            Buffer.BlockCopy(idByteArray,
                             0,
                             userToken.DataToSend,
                             userToken.SendPrefixLength,
                             idByteArray.Length);

            //The message that the client sent is already in a byte array, in DataHolder.
            Buffer.BlockCopy(_dataHolder.DataMessageReceived,
                             0,
                             userToken.DataToSend,
                             userToken.SendPrefixLength + idByteArray.Length,
                             _dataHolder.DataMessageReceived.Length);

            userToken.SendBytesRemainingCount = userToken.SendPrefixLength + lengthOfCurrentOutgoingMessage;
            userToken.BytesSentAlreadyCount = 0;
        }
    }
}