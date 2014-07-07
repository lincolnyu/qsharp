namespace QSharp.Shader.Geometry.Common2d
{
    /// <summary>
    ///  interface that represents a 2D triangle
    /// </summary>
    public interface ITriangle2d
    {
        /// <summary>
        ///  first vertex of the triangle appearing in the counterclockwise order
        /// </summary>
        IVertex2d Vertex1 { get; }

        /// <summary>
        ///  second vertex of the triangle appearing in the counterclockwise order
        /// </summary>
        IVertex2d Vertex2 { get; }

        /// <summary>
        ///  thrid vertex of the triangle appearing in the counterclockwise order
        /// </summary>
        IVertex2d Vertex3 { get; }

        /// <summary>
        ///  the edge that links vertex 2 and 3
        /// </summary>
        IEdge2d Edge23 { get; }

        /// <summary>
        ///  the edge that links vertex 3 and 1
        /// </summary>
        IEdge2d Edge31 { get; }

        /// <summary>
        ///  the edge that links vertex 1 and 2
        /// </summary>
        IEdge2d Edge12 { get; }

        /// <summary>
        ///  returns if the triangle contains the vertex or the vertex is on 
        ///  one of its edges or is one of its forming vertices
        /// </summary>
        /// <param name="vertex">the vertex to test</param>
        /// <returns>if the vertex meets the criteria</returns>
        bool Contains(IVertex2d vertex);

        /// <summary>
        ///  returns the point opposite the edge
        /// </summary>
        /// <param name="edge">
        ///  the edge whose opposite point is to be returend. 
        ///  the edge object may not necessarily be the same instance as the 
        ///  edge it represents returned by the triangle
        /// </param>
        /// <returns>the point opposite the edge</returns>
        IVertex2d GetOpposite(IEdge2d edge);

        /// <summary>
        ///  returns the edge opposite the vertex
        /// </summary>
        /// <param name="vertex">
        ///  the vertex whose opposite edge is to be returned.
        ///  the vertex object may not necessarily be the same instance as the
        ///  edge it represents returned by the triangle
        /// </param>
        /// <returns>the point opposite the edge</returns>
        IEdge2d GetOpposite(IVertex2d vertex);
    }
}
