using System.Text;
using System.Collections.Generic;


namespace QSharp.Scheme.Utility
{
    /// <summary>
    ///  simple LRU cache manager
    /// </summary>
    public class EasyLruCache
    {
        #region Nested types

        /// <summary>
        ///  base class for all cache page classes
        /// </summary>
        public class CachePageBase
        {
            #region Fields

            /// <summary>
            ///  length of the cache
            /// </summary>
            public int      Len = 0;

            /// <summary>
            ///  the backing field for cached data
            /// </summary>
            public byte[]   Buf = null;

            #endregion
        }

        /// <summary>
        ///  a cache page class that supports LRU features
        /// </summary>
        public class CachePage : CachePageBase
        {
            #region Fields

            /// <summary>
            ///  index of the page
            /// </summary>
            public int      Index;

            /// <summary>
            ///  true if the cahce is dirty and requires a write-back
            /// </summary>
            public bool     Dirty;

            /// <summary>
            ///  node where the page sits in the linked list of cached pages in the 
            ///  cache manager
            /// </summary>
            public LinkedListNode<CachePage>  LruNode;

            #endregion

            #region Constructors

            /// <summary>
            ///  constructor of the page
            /// </summary>
            /// <param name="nIndex">index assigned to the page</param>
            public CachePage(int nIndex)
            {
                Index = nIndex;
                Dirty = false;
            }

            #endregion

            #region Methods

            /// <summary>
            ///  overriding ToString() method that returns the string representation
            ///  of the cache page
            /// </summary>
            /// <returns>string that represents the cache page</returns>
            public override string ToString()
            {
                return new StringBuilder('(')
                    .Append(Index.ToString())
                    .Append(',')
                    .Append(Dirty.ToString())
                    .Append(')').ToString();
            }
            #endregion
        }

        /// <summary>
        ///  an funcitonal class that compares two cache pages based on their indices
        /// </summary>
        protected class IndexComparer : IComparer<CachePage>
        {
            #region Methods

            /// <summary>
            ///  the method that compares two cache pages
            /// </summary>
            /// <param name="x">first cache page</param>
            /// <param name="y">second cache page</param>
            /// <returns>the result of the comparison</returns>
            public int Compare(CachePage x, CachePage y)
            {
                if (x.Index > y.Index)
                {
                    return 1;
                }
                if (x.Index < y.Index)
                {
                    return -1;
                }
                return 0;
            }

            #endregion
        }

        #endregion

        #region Delegates and events

        /// <summary>
        ///  a delegate type that a write-back event should be defined as
        /// </summary>
        /// <param name="iByIndex">index to write back on</param>
        public delegate void WriteBackHandler(int iByIndex);

        /// <summary>
        ///  an event raised when a write back is requested
        /// </summary>
        public event WriteBackHandler  WriteBackRequest;

        #endregion

        #region Fields

        /// <summary>
        ///  list of cached page accessed by index
        /// </summary>
        private readonly List<CachePage> _pagesByIndex = new List<CachePage>();

        /// <summary>
        ///  linked list of cached pages that are ordered in such way that
        ///  underpins LRU policy
        /// </summary>
        private readonly LinkedList<CachePage> _pagesLru = new LinkedList<CachePage>();

        /// <summary>
        ///  backing field for max page num setting
        /// </summary>
        private int _maxPageNum;

        /// <summary>
        ///  
        /// </summary>
        private readonly IndexComparer _indexComp = new IndexComparer();

        #endregion

        #region Properties

        /// <summary>
        ///  configured max page number
        /// </summary>
        public int MaxPageNum
        {
            get
            {
                return _maxPageNum;
            }
        }

        /// <summary>
        ///  returns the cached page by given index
        /// </summary>
        /// <param name="iByIndex">the index of the cached page to get</param>
        /// <returns>the cache page</returns>
        public CachePageBase this[int iByIndex]
        {
            get
            {
                if (iByIndex < 0 || iByIndex >= _pagesByIndex.Count)
                {
                    return null;
                }
                return _pagesByIndex[iByIndex];
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  paramterless constructor that sets maximum page number to -1
        ///  which indicates that there is no limit on the page number
        /// </summary>
        public EasyLruCache()
        {
            _maxPageNum = -1;
        }

        /// <summary>
        ///  constructor that allows user to specify the maxium page number
        /// </summary>
        /// <param name="nMaxPageNum">the maximum page number to set</param>
        public EasyLruCache(int nMaxPageNum)
        {
            _maxPageNum = nMaxPageNum;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  clear the cache
        /// </summary>
        public void Clear()
        {
            _pagesByIndex.Clear();
            _pagesLru.Clear();
        }


        /// <summary>
        ///  clear the cache and reconfigure it to specific maximum page number
        /// </summary>
        /// <param name="nMaxPageNum">maximum page number to set</param>
        public void Config(int nMaxPageNum)
        {
            Clear();
            _maxPageNum = nMaxPageNum;
        }

        /// <summary>
        ///  retrieve a page by given page number from the cache
        /// </summary>
        /// <param name="iByIndex">index of the page in the cache</param>
        /// <param name="lruNode">the node where the cached page is stored</param>
        /// <param name="iPage">id of the page</param>
        /// <returns>true if the page is successfully retrieved</returns>
        public bool Retrieve(out int iByIndex,
            out LinkedListNode<CachePage> lruNode, int iPage)
        {
            CachePage target = new CachePage(iPage);
            iByIndex = _pagesByIndex.BinarySearch(target, _indexComp);
            if (iByIndex < 0)
            {   // cache miss
                iByIndex = -(iByIndex + 1);
                lruNode = null;
                return false;
            }
            lruNode = _pagesByIndex[iByIndex].LruNode;
            return true;
        }

        /// <summary>
        ///  method that is called indicating a cache hit
        /// </summary>
        /// <param name="lruNode"></param>
        public void Hit(LinkedListNode<CachePage> lruNode)
        {
            _pagesLru.Remove(lruNode);
            lruNode.Value.LruNode = lruNode;
            _pagesLru.AddLast(lruNode);
        }

        /// <summary>
        ///  requests a write-back for a page with specific index
        /// </summary>
        /// <param name="iByIndex">index of the page in the cache manager</param>
        protected void WriteBack(int iByIndex)
        {
            if (WriteBackRequest != null && _pagesByIndex[iByIndex].Dirty)
            {
                WriteBackRequest(iByIndex);
            }
        }

        /// <summary>
        ///  this is called before a page load as the result of
        ///  a cache miss indicated by 'REtrieve'
        /// </summary>
        /// <param name="iBiIns">index in the cache list where the page is to be inserted</param>
        /// <param name="iPage">the id of the cached page to create and insert</param>
        /// <returns>the index where the page is inserted, normally the same as iBIIns</returns>
        public int Miss(int iBiIns, int iPage)
        {
            LinkedListNode<CachePage> lruNode;
            CachePage cp = new CachePage(iPage);
            if (_maxPageNum > 0 && _pagesLru.Count >= _maxPageNum)
            {
                /* get one slot from LRU */
                lruNode = _pagesLru.First;

                /* remove from PBI */
                int iByIndex = _pagesByIndex.BinarySearch(lruNode.Value, _indexComp);

                // about to remove the cache page
                // write it back if necessary
                WriteBack(iByIndex);

                _pagesByIndex.RemoveAt(iByIndex);
                if (iByIndex < iBiIns)
                {
                    iBiIns--;
                }

                /* remove from LRU */
                _pagesLru.Remove(lruNode);

                lruNode.Value = cp;
            }
            else
            {
                lruNode = new LinkedListNode<CachePage>(cp);
            }

            cp.LruNode = lruNode;
            _pagesLru.AddLast(lruNode);
            _pagesByIndex.Insert(iBiIns, cp);

            return iBiIns;
        }

        /// <summary>
        ///  this is called after the cache was made dirty due to cache modification
        /// </summary>
        /// <param name="iByIndex">index of the page in cache</param>
        /// <returns>true if successfully modified</returns>
        public bool Modify(int iByIndex)
        {
            if (iByIndex < 0 || iByIndex >= _pagesByIndex.Count)
            {
                return false;
            }
            _pagesByIndex[iByIndex].Dirty = true;
            return true;
        }

        /// <summary>
        ///  returns a string that represents the cache
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("PBI List = ");
            foreach (CachePage cp in _pagesByIndex)
            {
                sb.Append(cp).Append("; ");
            }
            sb.Append("\nLRU List = ");

            foreach (CachePage cp in _pagesLru)
            {
                sb.Append(cp).Append("; ");
            }
            sb.Append("\treesize");

            return sb.ToString();
        }

        #endregion
    }

#if TEST_Scheme_Utility_EasyLRUCache
    // TODO this test unit should be placed somewhere else appropriate
    /// <summary>
    ///  test this LRU cache unit
    /// </summary>
    public static class EasyLRUCache_Test
    {
        /// <summary>
        ///  this class assists write back test
        /// </summary>
        class WriteBackTest
        {
            public int Id = 0;

            public WriteBackTest(int id)
            {
                Id = id;
            }

            public void WriteBack(int iByIndex)
            {
                System.Console.WriteLine(": Writing back on {0} at cache index {1}", Id, iByIndex);
            }
        }

        /// <summary>
        ///  entry method for test
        /// </summary>
        public static void TestEntry()
        {
            EasyLRUCache lru = new EasyLRUCache(4);
            WriteBackTest wbtest = new WriteBackTest(123);
            lru.WriteBackRequest += wbtest.WriteBack;

            while (true)
            {
                string userCmd = System.Console.ReadLine();
                if (userCmd == "Q" || userCmd == "q")
                {
                    break;
                }

                char c0 = userCmd[0];
                string arg = userCmd.Substring(1);
                int iPage = 0;
                int iByIndex;
                LinkedListNode<EasyLRUCache.CachePage>   lruNode;
                if (c0 == 'R' || c0 == 'r')
                {
                    iPage = System.Convert.ToInt32(arg);
                    if (lru.Retrieve(out iByIndex, out lruNode, iPage))
                    {
                        lru.Hit(lruNode);
                        System.Console.WriteLine(": Cache Hit.");
                    }
                    else
                    {
                        iByIndex = lru.Miss(iByIndex, iPage);
                        System.Console.WriteLine(": Cache Miss.");
                    }
                }
                else if (c0 == 'W' || c0 == 'w')
                {
                    iPage = System.Convert.ToInt32(arg);
                    if (lru.Retrieve(out iByIndex, out lruNode, iPage))
                    {
                        lru.Hit(lruNode);
                        lru.Modify(iByIndex);
                        System.Console.WriteLine(": Cache Hit and Modified.");
                    }
                    else
                    {
                        iByIndex = lru.Miss(iByIndex, iPage);
                        lru.Modify(iByIndex);
                        System.Console.WriteLine(": Cache Miss, Loaded and Modified.");
                    }
                }
                else if (userCmd == "P" || userCmd == "p")
                {
                    System.Console.Write(lru.ToString());
                }
                else
                {
                    System.Console.WriteLine("! Bad command.");
                }
            }
        }
    }
#endif  // if TEST_Scheme_Utility_EasyLRUCache
}
