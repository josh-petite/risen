using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Risen.Server.Tcp
{
    public interface ISocketAsyncEventArgsPool
    {
        void Init(int capacity);
        void Push(SocketAsyncEventArgs socketAsyncEventArgs);
        int AssignTokenId();
        bool Any();
        SocketAsyncEventArgs Pop();
        int Count { get; }
    }

    public class SocketAsyncEventArgsPool : ISocketAsyncEventArgsPool
    {
        private int _nextTokenId;
        private Stack<SocketAsyncEventArgs> _pool;
        
        public void Init(int capacity)
        {
            _pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        public void Push(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            if (socketAsyncEventArgs == null)
                throw new ArgumentNullException("socketAsyncEventArgs");
            

            lock (_pool)
            {
                _pool.Push(socketAsyncEventArgs);
            }
        }

        public int AssignTokenId()
        {
            var tokenId = Interlocked.Increment(ref _nextTokenId);
            return tokenId;
        }

        public bool Any()
        {
            return _pool.Count > 0;
        }

        public SocketAsyncEventArgs Pop()
        {
            lock (_pool)
            {
                return _pool.Pop();
            }
        }

        public int Count
        {
            get { return _pool.Count; }
        }
    }
}