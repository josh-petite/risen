using System;
using System.Net.Sockets;

namespace Risen.Server.Tcp
{
    public class PrefixHandler
    {
        public int HandlePrefix(SocketAsyncEventArgs e, DataHoldingUserToken receiveSendToken, Int32 remainingBytesToProcess)
        {
            //ReceivedPrefixBytesDoneCount tells us how many prefix bytes were
            //processed during previous receive ops which contained data for
            //this message. Usually there will NOT have been any previous
            //receive ops here. So in that case,
            //receiveSendToken.ReceivedPrefixBytesDoneCount would equal 0.
            //Create a byte array to put the new prefix in, if we have not
            //already done it in a previous loop.
            if (receiveSendToken.ReceivedPrefixBytesDoneCount == 0)
            {
                receiveSendToken.ByteArrayForPrefix = new byte[receiveSendToken.ReceivePrefixLength];
            }

            //If this next if-statement is true, then we have received at
            //least enough bytes to have the prefix. So we can determine the
            //length of the message that we are working on.
            if (remainingBytesToProcess >= receiveSendToken.ReceivePrefixLength - receiveSendToken.ReceivedPrefixBytesDoneCount)
            {
                //Now copy that many bytes to ByteArrayForPrefix.
                //We can use the variable receiveMessageOffset as our main
                //index to show which index to get data from in the TCP
                //buffer.
                Buffer.BlockCopy(e.Buffer, receiveSendToken.ReceiveMessageOffset
                                           - receiveSendToken.ReceivePrefixLength
                                           + receiveSendToken.ReceivedPrefixBytesDoneCount,
                                 receiveSendToken.ByteArrayForPrefix,
                                 receiveSendToken.ReceivedPrefixBytesDoneCount,
                                 receiveSendToken.ReceivePrefixLength
                                 - receiveSendToken.ReceivedPrefixBytesDoneCount);

                remainingBytesToProcess = remainingBytesToProcess
                                          - receiveSendToken.ReceivePrefixLength
                                          + receiveSendToken.ReceivedPrefixBytesDoneCount;

                receiveSendToken.RecPrefixBytesDoneThisOperation =
                    receiveSendToken.ReceivePrefixLength
                    - receiveSendToken.ReceivedPrefixBytesDoneCount;

                receiveSendToken.ReceivedPrefixBytesDoneCount =
                    receiveSendToken.ReceivePrefixLength;

                receiveSendToken.LengthOfCurrentIncomingMessage =
                    BitConverter.ToInt32(receiveSendToken.ByteArrayForPrefix, 0);

                return remainingBytesToProcess;
            }

            //This next else-statement deals with the situation
            //where we have some bytes
            //of this prefix in this receive operation, but not all.
            
            //Write the bytes to the array where we are putting the
            //prefix data, to save for the next loop.
            Buffer.BlockCopy(e.Buffer, receiveSendToken.ReceiveMessageOffset
                                       - receiveSendToken.ReceivePrefixLength
                                       + receiveSendToken.ReceivedPrefixBytesDoneCount,
                             receiveSendToken.ByteArrayForPrefix,
                             receiveSendToken.ReceivedPrefixBytesDoneCount,
                             remainingBytesToProcess);

            receiveSendToken.RecPrefixBytesDoneThisOperation = remainingBytesToProcess;
            receiveSendToken.ReceivedPrefixBytesDoneCount += remainingBytesToProcess;
            remainingBytesToProcess = 0;

            // This section is needed when we have received
            // an amount of data exactly equal to the amount needed for the prefix,
            // but no more. And also needed with the situation where we have received
            // less than the amount of data needed for prefix.
            if (remainingBytesToProcess == 0)
            {
                receiveSendToken.ReceiveMessageOffset = receiveSendToken.ReceiveMessageOffset - receiveSendToken.RecPrefixBytesDoneThisOperation;
                receiveSendToken.RecPrefixBytesDoneThisOperation = 0;
            }
            return remainingBytesToProcess;
        }
    }
}