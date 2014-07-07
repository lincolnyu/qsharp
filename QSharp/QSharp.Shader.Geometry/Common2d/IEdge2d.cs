namespace QSharp.Shader.Geometry.Common2d
{
    /// <summary>
    ///  interface that represents a 2D edge
    ///  if it's part of a triangle, the order of the vertices should
    ///  be same as the order in which the vertices in the triangle appear
    ///  which is counterclockwise
    /// </summary>
    public interface IEdge2d
    {
        /// <summary>
        ///  the first vertex of the edge
        /// </summary>
        IVertex2d Vertex1 { get; }

        /// <summary>
        ///  the second vertex of the edge
        /// </summary>
        IVertex2d Vertex2 { get; }

        /// <summary>
        ///  returns if the specified vertex is on the edge (line segment) with specified
        ///  epsilon value
        /// </summary>
        /// <param name="vertex">the vertex to test</param>
        /// <param name="epsilon">
        ///  the minimum distance from the edge to the vertex 
        ///  for the vertex to be considered to be on edge
        /// </param>
        /// <returns>true if the vertex is on the edge</returns>
        bool Contains(IVertex2d vertex, double epsilon);

        /// <summary>
        ///  returns the length of the edge
        /// </summary>
        double Length { get; }

        /// <summary>
        ///  returns the squared length of the edge
        /// </summary>
        double SquaredLength { get; }
    }
}
