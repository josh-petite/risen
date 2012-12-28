using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Risen.Server.Msmq;
using Risen.Server.Tcp.EventArgs;
using Risen.Server.Tcp.Tokens;

namespace Risen.Server.Tcp
{
    public delegate void ProcessReceiveEvent(object sender, ProcessReceiveEventArgs args);
    public delegate void ProcessSendEvent(object sender, ProcessSendEventArgs args);

    public interface IOutboundQueue
    {
        void EnqueueReceive(SocketAsyncEventArgs socketAsyncEventArgs);
        void EnqueueSend(SocketAsyncEventArgs socketAsyncEventArgs);
    }
    
    public class OutboundQueue : IOutboundQueue
    {
        private readonly Queue<SocketAsyncEvent> _socketAsyncEvents;
        private Thread _processThread;

        public event ProcessReceiveEvent ProcessReceiveEvent;
        public event ProcessSendEvent ProcessSendEvent;

        protected virtual void OnProcessSendEvent(ProcessSendEventArgs args)
        {
            var handler = ProcessSendEvent;
            if (handler != null) handler(this, args);
        }

        protected virtual void OnProcessReceiveEvent(ProcessReceiveEventArgs args)
        {
            var handler = ProcessReceiveEvent;
            if (handler != null) handler(this, args);
        }

        public OutboundQueue()
        {
            _socketAsyncEvents = new Queue<SocketAsyncEvent>();
        }

        public void EnqueueReceive(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            _socketAsyncEvents.Enqueue(new SocketAsyncEvent
                                           {
                                               Type = SocketAsyncEventType.Receive,
                                               Token = socketAsyncEventArgs.UserToken
                                           });
        }

        public void EnqueueSend(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            _socketAsyncEvents.Enqueue(new SocketAsyncEvent
                                           {
                                               Type = SocketAsyncEventType.Send
                                           });
        }

        public void StartQueueWatcher()
        {
            _processThread = new Thread(Process);
            _processThread.Start();
        }

        private void Process()
        {
            foreach (var socketAsyncEvent in _socketAsyncEvents)
            {
                switch (socketAsyncEvent.Type)
                {
                    case SocketAsyncEventType.Receive:
                        ProcessReceiveEvent(null, new ProcessReceiveEventArgs(socketAsyncEvent));
                        break;
                    case SocketAsyncEventType.Send:
                        ProcessSendEvent(null, new ProcessSendEventArgs(socketAsyncEvent));
                        break;
                }
            }
        }

        
    }

    public class SocketAsyncEvent
    {
        public SocketAsyncEventType Type { get; set; }
        public object Token { get; set; }
        public int BytesTransferred { get; set; }
        public SocketError SocketError { get; set; }
        public Socket AcceptSocket { get; set; }
        public byte[] Buffer { get; set; }
        public int Offset { get; set; }
        public SocketAsyncEventArgs EventArgs { get; set; }

        public void SetBuffer(int offset, int count)
        {
            
        }
    }

    public enum SocketAsyncEventType
    {
        Receive,
        Send
    }
}
