using System;
using QSharp.Shader.Geometry.Common2d;
using QSharp.Shader.SpatialIndexing.BucketMethod;
using QSharp.Shared;

namespace QSharp.Shader.Geometry.Triangulation.Passive
{
    /// <summary>
    ///  a bucket set for triangles for triangularisation manager to work with to maintain the triangles
    ///  collected and provide information to triangulariser to decide on how to triangularise
    /// </summary>
    public class TriangleBucketSet : FixedBucketSet2d
    {
        #region Fields

        /// <summary>
        ///  rate of epsilon over representative dimension
        /// </summary>
        private const double EpsilonRate = 0.01;

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a triangle bucket set with specified details
        /// </summary>
        /// <param name="nx">number of columns </param>
        /// <param name="ny">number of rows</param>
        /// <param name="x0">x component of the base (lowest) point</param>
        /// <param name="y0">y component of the base (lowest) point</param>
        /// <param name="xdimension">width of each bucket</param>
        /// <param name="ydimension">height of each bucket</param>
        public TriangleBucketSet(int nx, int ny, double x0, double y0, double xdimension, double ydimension)
            : base(nx, ny, x0, y0, xdimension, ydimension)
        {
        }

        #endregion


        /// <summary>
        ///  adds a triangle by attaching it to the appropriate buckets in the set
        /// </summary>
        /// <param name="triangle">the triangle to add</param>
        public void AddTriangle(IndexedTriangle triangle)
        {
            double xmin = Math.Min(triangle.Vertex1.X, Math.Min(triangle.Vertex2.X, triangle.Vertex3.X));
            double ymin = Math.Min(triangle.Vertex1.Y, Math.Min(triangle.Vertex2.Y, triangle.Vertex3.Y));
            double xmax = Math.Max(triangle.Vertex1.X, Math.Max(triangle.Vertex2.X, triangle.Vertex3.X));
            double ymax = Math.Max(triangle.Vertex1.Y, Math.Max(triangle.Vertex2.Y, triangle.Vertex3.Y));

            int rowmin, colmin, rowmax, colmax;
            if (!GetContainingBucket(xmin, ymin, out rowmin, out colmin)
                || !GetContainingBucket(xmax, ymax, out rowmax, out colmax))
            {
                throw new QException("Triangle out of bucket set boundary.");
            }

            bool added = false;
            double epsilon = Math.Min(_xdimension, _ydimension) * EpsilonRate;
            for (int row = rowmin; row <= rowmax; row++)
            {
                for (int col = colmin; col <= colmax; col++)
                {
                    double x0, y0, x1, y1;
                    GetBucketDimensions(row, col, out x0, out y0, out x1, out y1);
                    var v00 = new Vertex2d(x0, y0);
                    var v01 = new Vertex2d(x0, y1);
                    var v10 = new Vertex2d(x1, y0);
                    var v11 = new Vertex2d(x1, y1);
                    if (triangle.Intersect(v00, v01, v10, epsilon) != TriangleHelper.TriangleRelation.Separate 
                        || triangle.Intersect(v00, v11, v10, epsilon) != TriangleHelper.TriangleRelation.Separate)
                    {
                        var bucket = base[row, col] as TriangleBucket ?? new TriangleBucket();
                        bucket.AddTriangle(triangle);
                        added = true;
                    }
                }
            }

            if (!added)
            {
                throw new QException("Triangle hasn't been tied up to any buckets.");
            }
        }

        /// <summary>
        ///  removes a triangle from the set by detaching it from all buckets that contain it
        /// </summary>
        /// <param name="triangle">the triangle to remove</param>
        public void RemoveTriangle(IndexedTriangle triangle)
        {
            double xmin = Math.Min(triangle.Vertex1.X, Math.Min(triangle.Vertex2.X, triangle.Vertex3.X));
            double ymin = Math.Min(triangle.Vertex1.Y, Math.Min(triangle.Vertex2.Y, triangle.Vertex3.Y));
            double xmax = Math.Max(triangle.Vertex1.X, Math.Max(triangle.Vertex2.X, triangle.Vertex3.X));
            double ymax = Math.Max(triangle.Vertex1.Y, Math.Max(triangle.Vertex2.Y, triangle.Vertex3.Y));

            int rowmin, colmin, rowmax, colmax;
            if (!GetContainingBucket(xmin, ymin, out rowmin, out colmin)
                || !GetContainingBucket(xmax, ymax, out rowmax, out colmax))
            {
                throw new QException("Triangle out of bucket set boundary.");
            }

            double epsilon = Math.Min(_xdimension, _ydimension) * EpsilonRate;
            for (int row = rowmin; row <= rowmax; row++)
            {
                for (int col = colmin; col <= colmax; col++)
                {
                    double x0, y0, x1, y1;
                    GetBucketDimensions(row, col, out x0, out y0, out x1, out y1);
                    var v00 = new Vertex2d(x0, y0);
                    var v01 = new Vertex2d(x0, y1);
                    var v10 = new Vertex2d(x1, y0);
                    var v11 = new Vertex2d(x1, y1);
                    if (triangle.Intersect(v00, v01, v10, epsilon) != TriangleHelper.TriangleRelation.Separate 
                        || triangle.Intersect(v00, v11, v10, epsilon) != TriangleHelper.TriangleRelation.Separate)
                    {
                        var bucket = base[row, col] as TriangleBucket;
                        if (bucket == null)
                        {
                            throw new QException("The bucket is supposed to be avaliable and contain the triangle");
                        }
                        bucket.RemoveTriangle(triangle);
                    }
                }
            }
        }
    }
}
