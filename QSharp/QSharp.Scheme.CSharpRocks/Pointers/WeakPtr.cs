namespace QSharp.Scheme.CSharpRocks.Pointers
{
    public class WeakPtr<T> where T : class
    {
        private ISharedPtrCore<T> _ptr;

        public WeakPtr(SharedPtr<T> other)
        {
            Assign(other);
        }
       
        public void Assign<T2>(SharedPtr<T2> other) where T2 : class, T
        {
            _ptr = other.Ptr;
        }

        public IWeakLock<T> Lock() => _ptr?.RefCount > 0 ? _ptr.WeakLock() : null;

        internal ISharedPtrCore<T> Ptr => _ptr;

        #region For debug only

        public uint TargetRefCount => _ptr?.RefCount ?? 0;

        public uint WeakLockCount => _ptr?.WeakLockCount ?? 0;

        #endregion
    }

    public class WeakAtomicPtr<T> : WeakPtr<Wrapper<T>>
    {
        public WeakAtomicPtr(SharedPtr<Wrapper<T>> other) : base(other)
        {
        }
    }
}
