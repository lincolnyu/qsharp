using QSharp.Shader.Geometry.Common2d;

namespace QSharp.Shader.Geometry.Triangulation.Passive
{
    /// <summary>
    ///  an interface that specifies what passive triangulariser need to 
    ///  perform its task
    /// </summary>
    public interface ITrianguleManager
    {
        #region Methods

        /// <summary>
        ///  returns if the two specified vertices are considered the same vertex
        /// </summary>
        /// <param name="vertex1">the first vertex to compare</param>
        /// <param name="vertex2">the second vertex to compare</param>
        /// <returns>true if they are considered equal</returns>
        bool VerticesEqual(IVertex2d vertex1, IVertex2d vertex2);

        /// <summary>
        ///  gets the containing triangle of the vertex, the triangle either contains
        ///  the vertex or one of its edges contains the vertex which might be shared
        ///  by another vertex
        /// </summary>
        /// <param name="vertex">the vertex whose containing triangle is to be returned</param>
        /// <returns>the containing triangle</returns>
        ITriangle2d GetContainingTriangle(IVertex2d vertex);

        /// <summary>
        ///  returns whether the vertex is on the specified edge of the specified triangle
        /// </summary>
        /// <param name="vertex">the vertex is tested to see if it is on the edge</param>
        /// <param name="edge">the edge to test against</param>
        /// <param name="triangle">the triangle that owns the edge</param>
        /// <returns>true if the vertex is considered to be on the edge</returns>
        bool PointIsOnEdege(IVertex2d vertex, IEdge2d edge, ITriangle2d triangle);

        /// <summary>
        ///  returns the triangle which shared the specified edge with the specified triangle
        /// </summary>
        /// <param name="triangle">the triangle the triangle adjacent to which by edge is to return</param>
        /// <param name="edge">the edge these two triangles share</param>
        /// <returns>the adjacent triangle</returns>
        ITriangle2d GetAdjacentTriangle(ITriangle2d triangle, IEdge2d edge);

        /// <summary>
        ///  if needed changes the diagonal, common edge of the two adjacent triangles
        ///  (specified here by one of the triangles, the common edge and the vertex that
        ///   is exclusive to the other triangle) to the one linking the other two vertices 
        ///  (opposite the edge on either triangle), therefore actually the two triangles 
        ///  are turned into two different ones
        /// </summary>
        /// <param name="triangle">the outer triangle</param>
        /// <param name="edge">the common edge</param>
        /// <param name="vertex">the vertex owned only by the inner triangle</param>
        /// <param name="split1">first split triangle if any</param>
        /// <param name="split2">second split triangle if any</param>
        /// <returns>if the diagonal is flipped</returns>
        bool FlipDiagonalIfNeeded(ITriangle2d triangle, IEdge2d edge, IVertex2d vertex, 
            out ITriangle2d split1, out ITriangle2d split2);

        /// <summary>
        ///  if needed changes the diagonol, common edge of the two adjacent triangles
        ///  which already have been triangularised
        /// </summary>
        /// <param name="inner">the vertex containing the vertex</param>
        /// <param name="outer">the vertex adjacent to the above that doesn't contain the vertex</param>
        /// <param name="split1">one of the split</param>
        /// <param name="split2">the other of the split</param>
        /// <returns>if re-triangularisation is needed and performed</returns>
        bool FlipDiagonalIfNeeded(ITriangle2d inner, ITriangle2d outer, out ITriangle2d split1,
                                  out ITriangle2d split2);


        /// <summary>
        ///  splits an existing triangle at the specified vertex to be added
        /// </summary>
        /// <param name="triangle">the triangle to split</param>
        /// <param name="edge">
        ///  the edge the vertex to be added is on and is to be split at the vertex 
        ///  as the triangle is split</param>
        /// <param name="vertexToAdd">the vertex to add to split the triangle into two</param>
        void SplitTriangle(ITriangle2d triangle, IEdge2d edge, IVertex2d vertexToAdd);

        /// <summary>
        ///  extends the hull to the vertex which is guaranteed to be outside the hull
        /// </summary>
        /// <param name="vertex">the vertex to add</param>
        void ExtendHull(IVertex2d vertex);

        /// <summary>
        ///  adds a triangle to the set with an edge and a vertex to add that form the triangle specified
        /// </summary>
        /// <param name="edge">the existing edge</param>
        /// <param name="vertexToAdd">the vertex to add to the set as part of the triangle</param>
        void AddTriangle(IEdge2d edge, IVertex2d vertexToAdd);

        #endregion
    }
}
