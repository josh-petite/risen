using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Sockets;
using System.Threading;

namespace Risen.Server.Tcp
{
    public interface ISocketAsyncEventArgsPool
    {
        void Push(SocketAsyncEventArgs item);
        int AssignTokenId();
    }

    public class SocketAsyncEventArgsPool : ISocketAsyncEventArgsPool
    {
        private int _nextTokenId = 0;
        private Stack<SocketAsyncEventArgs> _pool;

        /// <summary>
        /// Initializes the pool to the specified size.
        /// </summary>
        /// <param name="capacity">Maximum number of <see cref="System.Net.Sockets.SocketAsyncEventArgs"/>
        /// objects the pool can hold.</param>
        public void Init(int capacity)
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
            Contract.Requires<ArgumentNullException>(item != null, "item must not be null.");

            lock (_pool)
            {
                _pool.Push(item);
            }
        }

        public int AssignTokenId()
        {
            Int32 tokenId = Interlocked.Increment(ref nextTokenId);
            return tokenId;
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