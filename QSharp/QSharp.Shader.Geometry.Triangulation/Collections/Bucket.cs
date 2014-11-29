using System.Collections.Generic;
using QSharp.Shader.Geometry.Triangulation.Primitive;
using QSharp.Shader.SpatialIndexing.BucketMethod;

namespace QSharp.Shader.Geometry.Triangulation.Collections
{
    public class Bucket : IBucket
    {
        #region Constructors

        public Bucket()
        {
#if false
            DelaunayCircles = new Dictionary<Vector2D, double>();
#endif
            Vertices = new HashSet<Vector2D>();
        }

        #endregion

        #region Properties

#if false
        public Dictionary<Vector2D, double> DelaunayCircles { get; private set; }
#endif
        public HashSet<Vector2D> Vertices { get; private set; }

        #endregion
    }
}
