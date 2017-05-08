using QSharp.Scheme.Classical.Trees;
using System.Collections.Generic;

namespace QSharp.Scheme.Utility
{
    public class LruCachePolicy<TKey, TValue>
    {
        class KeyVisit
        {
            public TKey Key;
            public ulong VisitCount;
        }

        private Dictionary<TKey, KeyVisit> _visitDict = new Dictionary<TKey, KeyVisit>();

        private AvlTree<KeyVisit> _visitTree = new AvlTree<KeyVisit>((a,b)=>a.VisitCount.CompareTo(b.VisitCount));

        public ulong _nextCount = 0;

        public LruCachePolicy(uint cacheSize)
        {
            CacheSize = cacheSize;
        }

        public uint CacheSize { get; }
        
        public void UpdateCache(IDictionary<TKey, TValue> dict, TKey key, TValue val)
        {
            var visit = new KeyVisit { Key = key, VisitCount = _nextCount };
            _visitDict[key] = visit;
            _visitTree.Insert(visit);
            dict[key] = val;
            while (_visitDict.Count > CacheSize)
            {
                var oldest = GetOldestVisit();
                var keyToRemove = oldest.Entry.Key;
                _visitDict.Remove(key);
                dict.Remove(key);
                _visitTree.Remove(oldest);
            }
        }

        private AvlTreeWorker.INode<KeyVisit> GetOldestVisit()
        {
            var v = _visitTree.Root;
            while (true)
            {
                if (v.Left == null)
                {
                    return (AvlTreeWorker.INode<KeyVisit>)v;
                }
                v = v.Left;
            }
        }
    }
}
