using System;

namespace Risen.Client.Tcp
{
    public class OutgoingMessageHolder
    {
        internal string[] Messages;
        internal Int32 CountOfConnectionsRetries = 0;

        public OutgoingMessageHolder(string[] messages)
        {
            Messages = messages;
        }
    }
}