namespace QSharp.Scheme.Classical.Hash
{
    /// <summary>
    ///  Split order hash with dynamic bucket table implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SoHashDynamic<T> : SoHashBase<T>
    {
        #region Nested classes

        /// <summary>
        ///  Bucket table implementation
        /// </summary>
        private class BucketTable : IBucketTable
        {
            #region Fields

            /// <summary>
            ///  The size of the first level segment table
            /// </summary>
            private const int Level1SegmentSize = 1024;

            /// <summary>
            ///  The size of the second level segment table 
            /// </summary>
            private const int Level2SegmentSize = 1024;

            /// <summary>
            ///  The size of the third (last) level segment table
            /// </summary>
            private const int Level3SegmentSize = 1024;

            /// <summary>
            ///  The table as jagged arrays
            /// </summary>
            private readonly BaseNode[][][] _innerTable = new BaseNode[Level1SegmentSize][][];

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
                    var temp = index;
                    var indexLevel1 = temp / (Level2SegmentSize * Level3SegmentSize);
                    var entryLevel1 = _innerTable[indexLevel1];
                    if (entryLevel1 == null)
                    {
                        return null;
                    }

                    temp %= Level2SegmentSize * Level3SegmentSize;
                    var indexLevel2 = temp / Level3SegmentSize;
                    var entryLevel2 = entryLevel1[indexLevel2];
                    if (entryLevel2 == null)
                    {
                        return null;
                    }

                    var indexLevel3 = temp % Level3SegmentSize;
                    return entryLevel2[indexLevel3];
                }

                set
                {
                    var temp = index;
                    var indexLevel1 = temp / (Level2SegmentSize * Level3SegmentSize);

                    var entryLevel1 = _innerTable[indexLevel1] ??
                                      (_innerTable[indexLevel1] = new BaseNode[Level2SegmentSize][]);

                    temp %= Level2SegmentSize * Level3SegmentSize;
                    var indexLevel2 = temp / Level3SegmentSize;
                    var entryLevel2 = entryLevel1[indexLevel2] ??
                                      (entryLevel1[indexLevel2] = new BaseNode[Level3SegmentSize]);

                    var indexLevel3 = temp % Level3SegmentSize;
                    entryLevel2[indexLevel3] = value;
                }
            }

            #endregion

            #endregion

            #region Methods

            public void Reset()
            {
                for (var i = 0; i < Level1SegmentSize; i++)
                {
                    _innerTable[i] = null;
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        ///  The instance of the bucket table that keeps all the hash data
        /// </summary>
        private readonly BucketTable _bucketTable = new BucketTable();

        /// <summary>
        ///  The field that keeps the current size of the bucket table
        /// </summary>
        private int _tableSize;

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a split ordered hash table with specified max-load parameter
        /// </summary>
        /// <param name="maxLoad">The max-load parameter</param>
        public SoHashDynamic(float maxLoad = 1.5f)
        {
            MaxLoad = maxLoad;
            _tableSize = 2;
        }

        #endregion

        #region Properties

        #region SoHashBase<T> members

        /// <summary>
        ///  The current size of the bucket table
        /// </summary>
        protected override int TableSize { get { return _tableSize; } }

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
            Double();
            _tableSize *= 2;
        }

        /// <summary>
        ///  Adds a node to the specified bucket as part of the CAS expanding process;
        ///  The adding is always starting from the previous table end and performed in order 
        /// </summary>
        /// <param name="indexBucket">The location of the bucket (in some implementation might be ignored_</param>
        /// <param name="node">The node to add</param>
        protected override void AddBucket(int indexBucket, BaseNode node)
        {
            _bucketTable[indexBucket] = node;
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
