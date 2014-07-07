using QSharp.Shader.Geometry.Common2d;
using QSharp.Shader.SpatialIndexing.BucketMethod;

namespace QSharp.Shader.Geometry.Triangulation.Passive
{
    /// <summary>
    ///  an implementation of IBucket for keeping triangles intersect with the box the bucket is created on
    /// </summary>
    public sealed class TriangleBucket : IBucket
    {
        #region Fields

        /// <summary>
        ///  underlying array that keeps all the triangles with spatial locating features available
        /// </summary>
        private readonly SpatialArray2d _triangles = new SpatialArray2d();

        #endregion

        #region Methods

        /// <summary>
        ///  adds a triangle to the bucket
        /// </summary>
        /// <param name="triangle">the triangle to add, which has to be an indexed triangle</param>
        public void AddTriangle(IndexedTriangle triangle)
        {
            _triangles.AddObject(triangle);
        }

        public void RemoveTriangle(IndexedTriangle triangle)
        {
            _triangles.RemoveObject(triangle);
        }

        /// <summary>
        ///  returns triangles involved in the bucket that contains the specified vertex
        ///  the vertex is guaranteed to be in side the box the bucket represents
        /// </summary>
        /// <param name="vertex">the vertex</param>
        /// <returns>the triangle that contains the vertex</returns>
        public ITriangle2d GetContainingTriangle(IVertex2d vertex)
        {
            var iv = vertex as IndexedVertex ?? new IndexedVertex(vertex.X, vertex.Y);
            // its just a reference point to start with
            int m = _triangles.GetObjectIndex(iv);
            // goes from the point to both sides at the same speed
            for (int dj = 1; ; dj++)
            {
                int j1 = m - dj;
                int j2 = m + dj;
                if (j1 < 0 && j2 >= _triangles.Count)
                {
                    return null;
                }
                if (j1 >= 0)
                {
                    var t = (IndexedTriangle)_triangles[j1];
                    if (t.Contains(vertex))
                        return t;
                }
                if (j2 < _triangles.Count)
                {
                    var t = (IndexedTriangle)_triangles[j2];
                    if (t.Contains(vertex))
                        return t;
                }
            }
        }

        #endregion
    }
}
