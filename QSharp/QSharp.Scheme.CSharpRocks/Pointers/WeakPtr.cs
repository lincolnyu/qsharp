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
            _ptr = other.GetPtr();
        }

        public IWeakLock Lock() => _ptr.RefCount > 0 ? _ptr.WeakLock() : null;

        public T Data => _ptr.GetData();
    }
}
