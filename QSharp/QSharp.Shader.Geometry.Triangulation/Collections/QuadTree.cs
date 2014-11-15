using System.Collections.Generic;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.SpatialIndexing.BucketMethod;
using Vector2D = QSharp.Shader.Geometry.Euclid2D.Vector2D;

namespace QSharp.Shader.Geometry.Triangulation.Collections
{
    public class QuadTree : FixedBucketSet2D
    {
        #region Constructors

        public QuadTree(int nx, int ny, double xmin, double ymin, double xsize, double ysize) 
            : base(nx, ny, xmin, ymin, xsize, ysize)
        {
        }

        #endregion

        #region Methods

        public void GetAllVerticesInCircle(IVector2D circle, double radius, ICollection<Vector2D> vertices)
        {
            var xmin = circle.X - radius;
            var xmax = circle.X + radius;
            var ymin = circle.Y - radius;
            var ymax = circle.Y + radius;

            int colmin, colmax, rowmin, rowmax;
            TryGetBucket(xmin, ymin, out rowmin, out colmin);
            TryGetBucket(xmax, ymax, out rowmax, out colmax);

            var rr = radius*radius;

            for (var row = rowmin; row <= rowmax; row++)
            {
                for (var col = colmin; col <= colmax; col++)
                {
                    IBucket bucket;
                    TryGetBucket(row, col, out bucket);
                    var vbucket = (Bucket) bucket;
                    foreach (var v in vbucket)
                    {
                        var dd = v.GetSquareDistance(circle);
                        if (dd < rr)
                        {
                            vertices.Add(v);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
