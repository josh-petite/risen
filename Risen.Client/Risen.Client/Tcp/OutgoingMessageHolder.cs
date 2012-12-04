using System;

namespace Risen.Client.Tcp
{
    public class OutgoingMessageHolder
    {
        internal string[] ArrayOfMessages;
        internal Int32 CountOfConnectionsRetries = 0;

        public OutgoingMessageHolder(string[] theArrayOfMessages)
        {
            ArrayOfMessages = theArrayOfMessages;
        }
    }
}