using System.Collections.Generic;

namespace QSharp.Scheme.Classical.Hash
{
    /// <summary>
    ///  Split order hash with linear bucket table implementation
    /// </summary>
    /// <typeparam name="T">The type of the data kept in the hash</typeparam>
    public class SoHashLinear<T> : SoHashBase<T>
    {
        #region Nested types

        /// <summary>
        ///  Bucket table implementation
        /// </summary>
        private class BucketTable : IBucketTable
        {
            #region Fields

            /// <summary>
            ///  The bucket data structure as a list of bucket nodes
            /// </summary>
            private readonly List<BaseNode> _buckets = new List<BaseNode> {null, null};

            #endregion

            #region Properties

            #region IBucketTable members

            /// <summary>
            ///  Returns the node at the specified index to the table
            /// </summary>
            /// <param name="index">The index</param>
            /// <returns>The node; null if not initialized</returns>
            public BaseNode this[int index]
            {
                get
                {
                    return _buckets[index];
                }
                set
                {
                    while (_buckets.Count <= index)
                    {
                        _buckets.Add(null);
                    }
                    _buckets[index] = value;
                }
            }

            #endregion

            /// <summary>
            ///  Exposes the inner list
            /// </summary>
            public List<BaseNode> InnerList
            {
                get
                {
                    return _buckets;
                }
            }

            #endregion

            #region Methods

            /// <summary>
            ///  Resets the bucket table
            /// </summary>
            public void Reset()
            {
                _buckets.Clear();
                _buckets.Capacity = 2;
                _buckets.Add(null);
                _buckets.Add(null);
            }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        ///  The instance of the bucket table that keeps all the hash data
        /// </summary>
        private readonly BucketTable _bucketTable = new BucketTable();

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a split ordered hash table with specified max-load parameter
        /// </summary>
        /// <param name="maxLoad">The max-load parameter</param>
        public SoHashLinear(float maxLoad = 1.5f)
        {
            MaxLoad = maxLoad;
        }

        #endregion

        #region Properties

        #region SoHashBase<T> members

        /// <summary>
        ///  The current size of the bucket table
        /// </summary>
        protected override int TableSize
        {
            get { return _bucketTable.InnerList.Count; }
        }

        /// <summary>
        ///  The bucket table for the hash algorithm to access
        /// </summary>
        protected override IBucketTable Buckets
        {
            get { return _bucketTable; }
        }

        #endregion

        /// <summary>
        ///  The threshold of the ratio of total number of items to table size over which the table needs to be expanded
        /// </summary>
        public float MaxLoad { get; protected set; }

        #endregion

        #region Methods

        #region SoHashBase<T> members

        /// <summary>
        ///  Determines if an expansion is needed and performs it if so. 
        ///  The implementation must internally call Double() to perform a CAS based expansion if the
        ///  expansion is decided to be performed and at the end of a TableSize is by design doubled
        /// </summary>
        protected override void ExpandIfNeeded()
        {
            if (Count <= MaxLoad * TableSize)
            {
                return;
            }
            // NOTE this pre-allocates memory which is essential and doesn't increase the TableSize
            _bucketTable.InnerList.Capacity *= 2;

            // Table size is doubled by the population of the buckets, so we don't need to do anything about TableSize
            Double();
        }

        /// <summary>
        ///  Adds a node to the specified bucket as part of the CAS expanding process;
        ///  The adding is always starting from the previous table end and performed in order 
        /// </summary>
        /// <param name="indexBucket">The location of the bucket (in some implementation might be ignored_</param>
        /// <param name="node">The node to add</param>
        protected override void AddBucket(int indexBucket, BaseNode node)
        {
            _bucketTable.InnerList.Add(node);
        }

        /// <summary>
        ///  Sets buckets to initial state after clear up the contents of the hash
        /// </summary>
        protected override void ResetBuckets()
        {
            _bucketTable.Reset();
        }

        #endregion

        #endregion
    }
}
