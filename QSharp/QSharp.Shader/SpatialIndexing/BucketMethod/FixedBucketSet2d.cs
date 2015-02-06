using System;
using System.Collections.Generic;

namespace QSharp.Shader.SpatialIndexing.BucketMethod
{
    public abstract class FixedBucketSet2D
    {
        #region Fields

        private readonly Dictionary<int, Dictionary<int, IBucket>> _buckets;

        #endregion

        #region Properties

        public IBucket this[int row, int col]
        {
            get
            {
                var bucket = GetBucket(row, col);
                return bucket;
            }
        }

        /// <summary>
        ///  minimum x component of the bucket set
        /// </summary>
        public double XMin { get; private set; }

        /// <summary>
        ///  minimum y component of the bucket set
        /// </summary>
        public double YMin { get; private set; }

        /// <summary>
        ///  width of each bucket
        /// </summary>
        public double XSize { get; private set; }

        /// <summary>
        ///  height of each bucket
        /// </summary>
        public double YSize { get; private set; }

        /// <summary>
        ///  Number of buckets in X direction
        /// </summary>
        public int XBucketCount { get; private set; }

        /// <summary>
        ///  Number of buckets in Y direction
        /// </summary>
        public int YBucketCount { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a fixed bucket set with specified details
        /// </summary>
        /// <param name="nx">number of columns</param>
        /// <param name="ny">number of rows</param>
        /// <param name="xmin"></param>
        /// <param name="ymin"></param>
        /// <param name="xsize"></param>
        /// <param name="ysize"></param>
        public FixedBucketSet2D(int nx, int ny, double xmin, double ymin, double xsize, double ysize)
        {
            _buckets = new Dictionary<int, Dictionary<int, IBucket>>();
            XMin = xmin;
            YMin = ymin;
            XSize = xsize;
            YSize = ysize;
            XBucketCount = nx;
            YBucketCount = ny;
        }

        #endregion

        #region Methods
        
        /// <summary>
        ///  returns the bucket that contains the specified point
        /// </summary>
        /// <param name="x">x component of the point the bucket that contains which is to be returned</param>
        /// <param name="y">y component of the point the bucket that contains which is to be returned</param>
        /// <param name="row">row of the bucket</param>
        /// <param name="col">column of the bucket</param>
        /// <returns>true if a valid bucket is found</returns>
        public bool TryGetBucket(double x, double y, out int row, out int col)
        {
            col = (int)Math.Floor((x - XMin)/XSize);
            row = (int)Math.Floor((y - YMin)/YSize);

            if (col < 0 || row < 0 || col >= XBucketCount || row >= YBucketCount)
            {
                if (col < 0) col = 0;
                if (row < 0) row = 0;
                if (col >= XBucketCount) col = XBucketCount - 1;
                if (row >= YBucketCount) row = YBucketCount - 1;

                return false;
            }

            return true;
        }

        /// <summary>
        ///  Gets the bucket at the specified location, returning null if no bucket exists at that location
        ///  or throwing an ArgumentException if the location is out of valid region
        /// </summary>
        /// <param name="x">The x coordinate of the location to get the bucket at</param>
        /// <param name="y">The y coordinate of the location to get the bucket at</param>
        /// <returns>The bucket if available or null if not</returns>
        public IBucket GetBucket(double x, double y)
        {
            int row, col;
            if (!TryGetBucket(x, y, out row, out col))
            {
                throw new ArgumentException("Input position out of boundary");
            }
            return GetBucket(row, col);
        }

        /// <summary>
        ///  Gets the bucket at the specified bucket 2D index, returning null if no bucket exists at that location
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        ///<returns>The bucket if available or null if not</returns>
        public IBucket GetBucket(int row, int col)
        {
            Dictionary<int, IBucket> dict;
            if (!_buckets.TryGetValue(row, out dict))
            {
                return null;
            }
            IBucket bucket;
            if (!dict.TryGetValue(col, out bucket))
            {
                return null;
            }
            return bucket;
        }

        /// <summary>
        ///  Gets the bucket at the specified location or creates one if it doesn't yet exist
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        /// <returns>The bucket obtained or created or null if the location is invalid</returns>
        public IBucket GetOrCreateBucket(double x, double y)
        {
            int row, col;
            if (!TryGetBucket(x, y, out row, out col))
            {
                return null;
            }

            return GetOrCreateBucket(row, col);
        }

        /// <summary>
        ///  Gets the bucket at the specifed 2D index or creates one if it doesn't exist
        /// </summary>
        /// <param name="row">The row of the cell</param>
        /// <param name="col">The column of the cell</param>
        /// <returns>The bucket obtained or created</returns>
        public IBucket GetOrCreateBucket(int row, int col)
        {
            Dictionary<int, IBucket> dict;
            if (!_buckets.TryGetValue(row, out dict))
            {
                dict = new Dictionary<int, IBucket>();
                _buckets[row] = dict;
            }
            IBucket bucket;
            if (!dict.TryGetValue(col, out bucket))
            {
                bucket = CreateBucket();
            }
            return bucket;
        }

        /// <summary>
        ///  Methods that instantiates a bucket
        /// </summary>
        /// <returns>The bucket created</returns>
        protected abstract IBucket CreateBucket();

        /// <summary>
        ///  returns positions of the four sides of the bucket
        /// </summary>
        /// <param name="row">row of the bucket</param>
        /// <param name="col">column of the bucket</param>
        /// <param name="xmin">lowest x</param>
        /// <param name="ymin">lowest y</param>
        /// <param name="xmax">highest x</param>
        /// <param name="ymax">highest y</param>
        public void GetBucketSize(int row, int col, out double xmin, out double ymin, out double xmax, out double ymax)
        {
            xmin = XMin + col*XSize;
            ymin = YMin + row*XSize;
            xmax = XMin + (col+1) * XSize;
            ymax = YMin + (row + 1)*YSize;
        }

        /// <summary>
        ///  Clears out the set
        /// </summary>
        public virtual void Clear()
        {
            _buckets.Clear();
        }

        #endregion
    }
}
