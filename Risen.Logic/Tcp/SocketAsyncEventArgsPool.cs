using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Sockets;
using System.Threading;

namespace Risen.Server.Tcp
{
    public class SocketAsyncEventArgsPool
    {
        private int _nextTokenId;
        private readonly Stack<SocketAsyncEventArgs> _pool;

        public SocketAsyncEventArgsPool(int capacity)
        {
            _pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        /// <summary>
        /// Adds a <see cref="System.Net.Sockets.SocketAsyncEventArgs"/> instance to the pool.
        /// </summary>
        /// <param name="item">The <see cref="System.Net.Sockets.SocketAsyncEventArgs"/> instance
        /// to add to the pool.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="item"/> is null.</exception>
        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            

            lock (_pool)
            {
                _pool.Push(item);
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