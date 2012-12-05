using System.Collections.Generic;
using System.Threading;

namespace Risen.Client.Tcp
{
    public class BlockingStack<T>
    {
        private readonly Stack<T> _stack;

        public BlockingStack(Stack<T> stack)
        {
            _stack = stack;
        }

        public void Push(T item)
        {
            lock (_stack)
            {
                _stack.Push(item);

                if (_stack.Count == 1) //This means we have gone from empty stack to stack with 1 item. So, wake Pop().
                    Monitor.PulseAll(_stack);
            }
        }

        public T Pop()
        {
            lock (_stack)
            {
                if (_stack.Count == 0) //Stack is empty. Wait until Pulse is received from Push().
                    Monitor.Wait(_stack);

                var item = _stack.Pop();

                return item;
            }
        }
    }
}