using System;
using System.Diagnostics;

namespace QSharp.Scheme.CSharpRocks.Pointers
{
    public interface IWeakLock : IDisposable { }

    internal interface ISharedPtrCore<out T> : IDisposable
    {
        uint RefCount { get; }

        uint WeakLockCount { get; set; }

        T GetData();

        void AddRef();

        void Release();

        IWeakLock WeakLock();
    }

    public class SharedPtr<T> : IDisposable where T : class
    {
        private class WeakLock : IWeakLock
        {
            private ISharedPtrCore<T> _ptr;

            public WeakLock(ISharedPtrCore<T> ptr)
            {
                _ptr = ptr;
            }

            ~WeakLock()
            {
                Unlock();
            }

            public void Dispose()
            {
                Unlock();
                GC.SuppressFinalize(this);
            }

            private void Unlock()
            {
                if (_ptr != null)
                {
                    lock (_ptr)
                    {
                        _ptr.WeakLockCount--;
                    }
                    _ptr = null;
                }
            }
        }

        private class SharedPtrCore<T2> : ISharedPtrCore<T> where T2 : T
        {
            public T2 Data { get; set; }

            #region ISharedPtrCore members

            public uint RefCount { get; private set; }

            public uint WeakLockCount { get; set; }

            #region IDisposable members

            public void Dispose()
            {
                var disp = Data as IDisposable;
                if (disp != null)
                {
                    disp.Dispose();
                }
                Data = default(T2);
            }

            #endregion

            public T GetData() => Data;

            public void AddRef()
            {
                lock(this)
                {
                    RefCount++;
                }
            }

            public void Release()
            {
                lock(this)
                {
                    Debug.Assert(RefCount > 0);
                    if (--RefCount == 0 && WeakLockCount == 0)
                    {
                        Dispose();
                    }
                }
            }

            public IWeakLock WeakLock()
            {
                lock(this)
                {
                    WeakLockCount++;
                    return new WeakLock(this);
                }
            }

            #endregion
        }

        private ISharedPtrCore<T> _ptr;

        #region IDisposable members

        public void Dispose()
        {
            Release();
            GC.SuppressFinalize(this);
        }

        #endregion

        ~SharedPtr()
        {
            Release();
        }

        public T Data => _ptr.GetData();

        public void CreateNew<T2>(T2 val) where T2 : T
        {
            _ptr = new SharedPtrCore<T2>()
            {
                Data = val,
            };
            _ptr.AddRef();
        }

        public void Assign<T2>(SharedPtr<T2> other) where T2 : class, T
        {
            Release();

            if (other == null)
            {
                return;
            }

            _ptr = other._ptr;
            _ptr.AddRef();
        }

        public void Release()
        {
            if (_ptr != null)
            {
                _ptr.Release();
                _ptr = null;
            }
        }

        #region For debug only

        public uint TargetRefCount() => _ptr.RefCount;

        internal ISharedPtrCore<T> GetPtr() => _ptr;

        #endregion
    }
}
