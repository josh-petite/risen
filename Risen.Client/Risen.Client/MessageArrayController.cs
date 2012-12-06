using System.Collections.Generic;
using System.Linq;
using Risen.Client.Tcp;

namespace Risen.Client
{
    public class MessageArrayController
    {
        private readonly Stack<OutgoingMessageHolder> _outgoingMessages;

        public MessageArrayController()
        {
            _outgoingMessages = new Stack<OutgoingMessageHolder>();
        }

        internal Stack<OutgoingMessageHolder> CreateMessageStack(IEnumerable<string> messages)
        {
            _outgoingMessages.Push(new OutgoingMessageHolder(messages.ToArray()));
            return _outgoingMessages;
        }
    }
}