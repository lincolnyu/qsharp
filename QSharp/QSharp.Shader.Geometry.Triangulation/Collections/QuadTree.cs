using QSharp.Shader.SpatialIndexing.BucketMethod;

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
        #endregion
    }
}
