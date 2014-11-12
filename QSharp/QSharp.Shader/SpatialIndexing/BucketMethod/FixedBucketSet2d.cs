using System;
using System.Collections.Generic;

namespace QSharp.Shader.SpatialIndexing.BucketMethod
{
    public class FixedBucketSet2D
    {
        #region Fields

        private readonly Dictionary<int, Dictionary<int, IBucket>> _buckets;

        #endregion

        #region Properties

        public IBucket this[int row, int col]
        {
            get
            {
                IBucket bucket;
                if (!TryGetBucket(row, col, out bucket))
                {
                    bucket = null;
                }
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

            if (col < 0 || row < 0 || col > XBucketCount || row > YBucketCount)
            {
                return false;
            }

            return true;
        }

        public bool TryGetBucket(double x, double y, out IBucket bucket)
        {
            int row, col;
            if (!TryGetBucket(x, y, out row, out col))
            {
                bucket = null;
                return false;
            }
            return TryGetBucket(row, col, out bucket);
        }

        public bool TryGetBucket(int row, int col, out IBucket bucket)
        {
             Dictionary<int, IBucket> dict;
            if (!_buckets.TryGetValue(row, out dict))
            {
                bucket = null;
                return false;
            }
            if (!dict.TryGetValue(col, out bucket))
            {
                return false;
            }
            return true;
        }

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

        #endregion


    }
}
