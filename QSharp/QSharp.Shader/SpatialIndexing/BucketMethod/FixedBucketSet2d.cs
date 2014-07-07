using System;

namespace QSharp.Shader.SpatialIndexing.BucketMethod
{
    public class FixedBucketSet2d
    {
        #region Fields

        /// <summary>
        ///  minimum x component of the bucket set
        /// </summary>
        protected double _x0;

        /// <summary>
        ///  minimum y component of the bucket set
        /// </summary>
        protected double _y0;

        /// <summary>
        ///  width of each bucket
        /// </summary>
        protected double _xdimension;

        /// <summary>
        ///  height of each bucket
        /// </summary>
        protected double _ydimension;

        private IBucket[,] _buckets;

        #endregion

        #region Properties

        public IBucket this[int row, int col]
        {
            get { return _buckets[row, col]; }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a fixed bucket set with specified details
        /// </summary>
        /// <param name="nx">number of columns</param>
        /// <param name="ny">number of rows</param>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="xdimension"></param>
        /// <param name="ydimension"></param>
        public FixedBucketSet2d(int nx, int ny, double x0, double y0, double xdimension, double ydimension)
        {
            _buckets = new IBucket[ny, nx];
            _x0 = x0;
            _y0 = y0;
            _xdimension = xdimension;
            _ydimension = ydimension;
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
        public bool GetContainingBucket(double x, double y, out int row, out int col)
        {
            col = (int)Math.Floor((x - _x0)/_xdimension);
            row = (int)Math.Floor((y - _y0)/_ydimension);

            int ny = _buckets.GetLength(0);
            int nx = _buckets.GetLength(1);
            if (col < 0 || row < 0 || col > nx || row > ny)
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
        public void GetBucketDimensions(int row, int col, out double xmin, out double ymin, out double xmax, out double ymax)
        {
            xmin = _x0 + col*_xdimension;
            ymin = _y0 + row*_xdimension;
            xmax = _x0 + (col+1) * _xdimension;
            ymax = _y0 + (row + 1)*_ydimension;
        }

        #endregion


    }
}
