using System;
using System.Diagnostics;

namespace QSharp.Scheme.CSharpRocks.Pointers
{
    public interface IWeakLock<out T> : IDisposable
    {
        T Data { get; }
    }

    public interface ISharedPtrCore<out T> : IDisposable
    {
        uint RefCount { get; }

        uint WeakLockCount { get; set; }

        T GetData();

        void AddRef();

        void Release();

        IWeakLock<T> WeakLock();
    }

    public class SharedPtr<T> : IDisposable where T : class
    {
        private class WeakLock : IWeakLock<T>
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

            public T Data => _ptr.GetData();

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
                        Debug.Assert(_ptr.WeakLockCount > 0);
                        if (--_ptr.WeakLockCount == 0 && _ptr.RefCount == 0)
                        {
                            _ptr.Dispose();
                        }
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
                lock (this)
                {
                    RefCount++;
                }
            }

            public void Release()
            {
                lock (this)
                {
                    Debug.Assert(RefCount > 0);
                    if (--RefCount == 0 && WeakLockCount == 0)
                    {
                        Dispose();
                    }
                }
            }

            public IWeakLock<T> WeakLock()
            {
                lock (this)
                {
                    WeakLockCount++;
                    return new WeakLock(this);
                }
            }

            #endregion
        }

        private ISharedPtrCore<T> _ptr;

        public SharedPtr(T val)
        {
            Reset(val);
        }

        public SharedPtr(SharedPtr<T> other)
        {
            Assign(other);
        }

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

        internal protected ISharedPtrCore<T> Ptr => _ptr;

        public T Data => _ptr.GetData();

        public bool IsNull => _ptr == null;

        public void Reset<T2>(T2 val) where T2 : T
        {
            Release();

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

        public uint TargetRefCount => _ptr?.RefCount?? 0;

        public uint WeakLockCount => _ptr?.WeakLockCount ?? 0;

        #endregion
    }

    public class Wrapper<T>
    {
        public T Value;
    }

    public class SharedAtomicPtr<T> : SharedPtr<Wrapper<T>>
    {
        public SharedAtomicPtr(T t = default(T)) : base(new Wrapper<T> { Value = t })
        {
        }

        public SharedAtomicPtr(SharedPtr<Wrapper<T>> p) : base(p)
        {
        }

        public T Value => Data.Value;

        public void Reset(T t = default(T)) => Reset(new Wrapper<T> { Value = t });

        public void ChangeValue(T t)
        {
            if (Ptr != null)
            {
                Ptr.GetData().Value = t;
            } 
            else
            {
                Reset(t);
            }
        }
    }
}
