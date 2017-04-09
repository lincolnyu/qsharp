using System.Collections.Generic;

namespace QSharp.Scheme.Utility
{
    public abstract class Cache<TKey, TValue>
    {
        public delegate TValue GetValueCb(TKey key);
        public delegate void UpdateCacheCb(IDictionary<TKey, TValue> dict, TKey key, TValue val);

        private GetValueCb _getValueCb;
        private UpdateCacheCb _updateCacheCb;
        private Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();

        public Cache(GetValueCb getValueCb, UpdateCacheCb updateCb)
        {
            _getValueCb = getValueCb;
            _updateCacheCb = updateCb;
        }

        public TValue Get(TKey key)
        {
            TValue val;
            if (!_dict.TryGetValue(key, out val))
            {
                val = _getValueCb(key);
                _updateCacheCb(_dict, key, val);
            }
            return val;
        }

    }
}
