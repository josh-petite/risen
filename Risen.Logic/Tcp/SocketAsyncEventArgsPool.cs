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

        /// <summary>
        /// Adds a <see cref="System.Net.Sockets.SocketAsyncEventArgs"/> instance to the pool.
        /// </summary>
        /// <param name="socketAsyncEventArgs">The <see cref="System.Net.Sockets.SocketAsyncEventArgs"/> instance
        /// to add to the pool.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="socketAsyncEventArgs"/> is null.</exception>
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

        /// <summary>
        /// Removes and returns a <see cref="System.Net.Sockets.SocketAsyncEventArgs"/> instance
        /// from the pool.
        /// </summary>
        /// <returns>An available <see cref="System.Net.Sockets.SocketAsyncEventArgs"/> instance
        /// in the pool.</returns>
        public SocketAsyncEventArgs Pop()
        {
            lock (_pool)
            {
                return _pool.Pop();
            }
        }

        /// <summary>
        /// The number of <see cref="System.Net.Sockets.SocketAsyncEventArgs"/> instances
        /// available in the pool.
        /// </summary>
        public int Count
        {
            get { return _pool.Count; }
        }
    }
}