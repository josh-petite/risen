namespace Risen.Server.Tcp.EventArgs
{
    public class ProcessReceiveEventArgs : System.EventArgs
    {
        private readonly SocketAsyncEvent _socketAsyncEvent;

        public ProcessReceiveEventArgs(SocketAsyncEvent socketAsyncEvent)
        {
            _socketAsyncEvent = socketAsyncEvent;
        }

        public SocketAsyncEvent SocketAsyncEvent { get { return _socketAsyncEvent; } }
    }

    public class ProcessSendEventArgs : System.EventArgs
    {
        private readonly SocketAsyncEvent _socketAsyncEvent;

        public ProcessSendEventArgs(SocketAsyncEvent socketAsyncEvent)
        {
            _socketAsyncEvent = socketAsyncEvent;
        }

        public SocketAsyncEvent SocketAsyncEvent { get { return _socketAsyncEvent; } }
    }
}
