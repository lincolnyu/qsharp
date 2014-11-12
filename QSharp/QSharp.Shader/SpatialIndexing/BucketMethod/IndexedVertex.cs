using QSharp.Shader.Geometry.Euclid2D;

namespace QSharp.Shader.SpatialIndexing.BucketMethod
{
    public class IndexedVertex : SpatialObject2D, IVector2D
    {
        #region Fields

        private readonly double _x;
        private readonly double _y;

        #endregion

        #region Properties

        #region Overriding SpatialObject2d and Implementing IVertex2d

        public override double X { get { return _x; } }

        public override double Y { get { return _y; } }

        #endregion

        #endregion

        #region Constructors

        public IndexedVertex(double x, double y)
        {
            _x = x;
            _y = y;
        }

        #endregion
    }
}
