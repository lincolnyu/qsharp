using System.Collections.Generic;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.SpatialIndexing.BucketMethod;

namespace QSharp.Shader.Geometry.Triangulation.Collections
{
    public class VertexBucket : HashSet<Vector2D>, IBucket
    {
    }
}
