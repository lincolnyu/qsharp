using System;
using QSharp.Shader.Geometry.Common2d;
using QSharp.Shader.SpatialIndexing.BucketMethod;

namespace QSharp.Shader.Geometry.Triangulation.Passive
{
    /// <summary>
    ///  an implementation of ITriangleManager
    /// </summary>
    public class TriangleManager : ITrianguleManager
    {
        #region Fields

        /// <summary>
        ///  a bucket set the manager is based on to store shape entities
        ///  with indexing
        /// </summary>
        private readonly FixedBucketSet2d _buckets;
        
        #endregion

        #region Constructors

        /// <summary>
        ///  instantiate a manager
        /// </summary>
        public TriangleManager(double xmin, double ymin, double xmax, double ymax,
            int nx, int ny)
        {
            double dx = xmax - xmin;
            double dy = ymax - ymin;
            double xdim = dx * 1.02 /nx;
            double ydim = dy * 1.02 / ny;
            xmin -= dx*0.01;
            ymin -= dy*0.01;
            _buckets = new FixedBucketSet2d(nx, ny, xmin, ymin, xdim, ydim);
        }

        #endregion

        #region Methods

        #region Implementation of ITriangleManager

        /// <summary>
        ///  returns if the two specified vertices are considered the same vertex
        /// </summary>
        /// <param name="vertex1">the first vertex to compare</param>
        /// <param name="vertex2">the second vertex to compare</param>
        /// <returns>true if they are considered equal</returns>
        public bool VerticesEqual(IVertex2d vertex1, IVertex2d vertex2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  returns whether the vertex is on the specified edge of the specified triangle
        /// </summary>
        /// <param name="vertex">the vertex is tested to see if it is on the edge</param>
        /// <param name="edge">the edge to test against</param>
        /// <param name="triangle">the triangle that owns the edge</param>
        /// <returns>true if the vertex is considered to be on the edge</returns>
        public bool PointIsOnEdege(IVertex2d vertex, IEdge2d edge, ITriangle2d triangle)
        {
            return edge.Contains(vertex, IndexedTriangle.EpsilonRate);
        }

        /// <summary>
        ///  gets the containing triangle of the vertex, the triangle either contains
        ///  the vertex or one of its edges contains the vertex which might be shared
        ///  by another vertex
        /// </summary>
        /// <param name="vertex">the vertex whose containing triangle is to be returned</param>
        /// <returns>the containing triangle</returns>
        public ITriangle2d GetContainingTriangle(IVertex2d vertex)
        {
            throw new NotImplementedException();
        }

        public ITriangle2d GetAdjacentTriangle(ITriangle2d triangle, IEdge2d edge)
        {
            throw new NotImplementedException();
        }

        public bool FlipDiagonalIfNeeded(ITriangle2d triangle, IEdge2d edge, IVertex2d vertex, out ITriangle2d split1, out ITriangle2d split2)
        {
            throw new NotImplementedException();
        }

        public bool FlipDiagonalIfNeeded(ITriangle2d inner, ITriangle2d outer, out ITriangle2d split1, out ITriangle2d split2)
        {
            throw new NotImplementedException();
        }

        public void SplitTriangle(ITriangle2d triangle, IEdge2d edge, IVertex2d vertexToAdd)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  extends the hull to the vertex which is guaranteed to be outside the hull
        /// </summary>
        /// <param name="vertex">the vertex to add</param>
        public void ExtendHull(IVertex2d vertex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  feeds a triangle just triangularised to the collector
        /// </summary>
        /// <param name="edge">existing edge</param>
        /// <param name="point">new point to add to the set</param>
        /// <remarks>
        ///  the three vertices of the triangle has to come in counterclockwise order
        /// </remarks>
        public void AddTriangle(IEdge2d edge, IVertex2d vertexToAdd)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}
