using System.Text;
using System.Collections.Generic;


namespace QSharp.Scheme.Utility
{
    /// <summary>
    ///  simple LRU cache manager
    /// </summary>
    public class EasyLRUCache
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
            public int      nLen = 0;

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
            public int      Index = 0;

            /// <summary>
            ///  true if the cahce is dirty and requires a write-back
            /// </summary>
            public bool     Dirty = false;

            /// <summary>
            ///  node where the page sits in the linked list of cached pages in the 
            ///  cache manager
            /// </summary>
            public LinkedListNode<CachePage>  lruNode;

            #endregion

            #region Constructors

            /// <summary>
            ///  constructor of the page
            /// </summary>
            /// <param name="nIndex">index assigned to the page</param>
            public CachePage(int nIndex)
            {
                Index = nIndex;
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
        protected List<CachePage> myPagesByIndex = new List<CachePage>();

        /// <summary>
        ///  linked list of cached pages that are ordered in such way that
        ///  underpins LRU policy
        /// </summary>
        protected LinkedList<CachePage> myPagesLRU = new LinkedList<CachePage>();

        /// <summary>
        ///  backing field for max page num setting
        /// </summary>
        protected int myMaxPageNum = -1;

        /// <summary>
        ///  
        /// </summary>
        protected IndexComparer myIndexComp = new IndexComparer();

        #endregion

        #region Properties

        /// <summary>
        ///  configured max page number
        /// </summary>
        public int MaxPageNum
        {
            get
            {
                return myMaxPageNum;
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
                if (iByIndex < 0 || iByIndex >= myPagesByIndex.Count)
                {
                    return null;
                }
                return myPagesByIndex[iByIndex];
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  paramterless constructor that sets maximum page number to -1
        ///  which indicates that there is no limit on the page number
        /// </summary>
        public EasyLRUCache()
        {
            myMaxPageNum = -1;
        }

        /// <summary>
        ///  constructor that allows user to specify the maxium page number
        /// </summary>
        /// <param name="nMaxPageNum">the maximum page number to set</param>
        public EasyLRUCache(int nMaxPageNum)
        {
            myMaxPageNum = nMaxPageNum;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  clear the cache
        /// </summary>
        public void Clear()
        {
            myPagesByIndex.Clear();
            myPagesLRU.Clear();
        }


        /// <summary>
        ///  clear the cache and reconfigure it to specific maximum page number
        /// </summary>
        /// <param name="nMaxPageNum">maximum page number to set</param>
        public void Config(int nMaxPageNum)
        {
            Clear();
            myMaxPageNum = nMaxPageNum;
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
            iByIndex = myPagesByIndex.BinarySearch(target, myIndexComp);
            if (iByIndex < 0)
            {   // cache miss
                iByIndex = -(iByIndex + 1);
                lruNode = null;
                return false;
            }
            lruNode = myPagesByIndex[iByIndex].lruNode;
            return true;
        }

        /// <summary>
        ///  method that is called indicating a cache hit
        /// </summary>
        /// <param name="lruNode"></param>
        public void Hit(LinkedListNode<CachePage> lruNode)
        {
            myPagesLRU.Remove(lruNode);
            lruNode.Value.lruNode = lruNode;
            myPagesLRU.AddLast(lruNode);
        }

        /// <summary>
        ///  requests a write-back for a page with specific index
        /// </summary>
        /// <param name="iByIndex">index of the page in the cache manager</param>
        protected void WriteBack(int iByIndex)
        {
            if (WriteBackRequest != null && myPagesByIndex[iByIndex].Dirty)
            {
                WriteBackRequest(iByIndex);
            }
        }

        /// <summary>
        ///  this is called before a page load as the result of
        ///  a cache miss indicated by 'REtrieve'
        /// </summary>
        /// <param name="iBIIns">index in the cache list where the page is to be inserted</param>
        /// <param name="iPage">the id of the cached page to create and insert</param>
        /// <returns>the index where the page is inserted, normally the same as iBIIns</returns>
        public int Miss(int iBIIns, int iPage)
        {
            LinkedListNode<CachePage> lruNode;
            CachePage cp = new CachePage(iPage);
            if (myMaxPageNum > 0 && myPagesLRU.Count >= myMaxPageNum)
            {
                /* get one slot from LRU */
                lruNode = myPagesLRU.First;

                /* remove from PBI */
                int iByIndex = myPagesByIndex.BinarySearch(lruNode.Value, myIndexComp);

                // about to remove the cache page
                // write it back if necessary
                WriteBack(iByIndex);

                myPagesByIndex.RemoveAt(iByIndex);
                if (iByIndex < iBIIns)
                {
                    iBIIns--;
                }

                /* remove from LRU */
                myPagesLRU.Remove(lruNode);

                lruNode.Value = cp;
            }
            else
            {
                lruNode = new LinkedListNode<CachePage>(cp);
            }

            cp.lruNode = lruNode;
            myPagesLRU.AddLast(lruNode);
            myPagesByIndex.Insert(iBIIns, cp);

            return iBIIns;
        }

        /// <summary>
        ///  this is called after the cache was made dirty due to cache modification
        /// </summary>
        /// <param name="iByIndex">index of the page in cache</param>
        /// <returns>true if successfully modified</returns>
        public bool Modify(int iByIndex)
        {
            if (iByIndex < 0 || iByIndex >= myPagesByIndex.Count)
            {
                return false;
            }
            myPagesByIndex[iByIndex].Dirty = true;
            return true;
        }

        /// <summary>
        ///  returns a string that represents the cache
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("PBI List = ");
            foreach (CachePage cp in myPagesByIndex)
            {
                sb.Append(cp.ToString()).Append("; ");
            }
            sb.Append("\nLRU List = ");

            foreach (CachePage cp in myPagesLRU)
            {
                sb.Append(cp.ToString()).Append("; ");
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
