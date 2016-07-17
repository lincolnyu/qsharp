using System;
using System.Threading;

namespace QSharp.Scheme.CSharpRocks.Threading
{
    public class Future<T>
    {
        public class Promise
        {
            public Promise()
            {
                Future = new Future<T>();
            }
            public Future<T> Future
            {
                get;
            }
            public void Set(T t)
            {
                Future.Set(t);
            }
        }

        public enum States
        {
            NotReady,
            Ready
        }

        private T _value;
        private ManualResetEvent _readyEvent = new ManualResetEvent(false);

        public T Value => Get();

        public States State { get; private set; }

        private void Set(T t)
        {
            if (State == States.Ready)
            {
                throw new InvalidOperationException("Future already set");
            }
            _value = t;
            State = States.Ready;
            _readyEvent.Set();
        }

        public T Get(int timeoutMilliseconds = -1)
        {
            if (State == States.Ready)
            {
                return _value;
            }
            _readyEvent.WaitOne(timeoutMilliseconds);
            return _value;
        }

        public T Get(TimeSpan timeout)
        {
            if (State == States.Ready)
            {
                return _value;
            }
            _readyEvent.WaitOne(timeout);
            return _value;            
        }
    }
}
