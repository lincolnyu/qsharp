using QSharp.Shader.Geometry.Common2d;

namespace QSharp.Shader.Geometry.Triangulation.Proactive
{
    /// <summary>
    ///  interface that provides methods related to the characteristics
    ///  of the envelope (contour) of a current vertex set
    /// </summary>
    public interface IEnvelope
    {
        #region Methods

        /// <summary>
        ///  gets the vertex next to the specified one in clockwise direction
        /// </summary>
        /// <param name="vertex">the vertex whose neighbour is to be found</param>
        /// <returns>the neighbouring vertex</returns>
        IVertex2d GetNeighbourClockwise(IVertex2d vertex);
        
        /// <summary>
        ///  gets the vertex next to the specified one in counterclockwise direction
        /// </summary>
        /// <param name="vertex">hte vertex whose neighbour is to be found</param>
        /// <returns>the neighbouring vertex</returns>
        IVertex2d GetNeighbourCounterClockwise(IVertex2d vertex);

        /// <summary>
        ///  gets one vertex from the envelope currently being convex to extend the 
        ///  convex set from a smart implementation should return an optimum vertex, 
        ///  at least it should never return a vertices marked as inextensible
        /// </summary>
        /// <returns>the optimal vertex on the envelop to extend from</returns>
        IVertex2d GetVertexToExtendFrom();

        /// <summary>
        ///  marks the specified vertex so it is known as not extensible
        /// </summary>
        /// <param name="v">the vertex to mark</param>
        void MarkAsInextensible(IVertex2d v);

        /// <summary>
        ///  adds a vertex to the envelope after <paramref name="followed"/> counterclockwise
        /// </summary>
        /// <param name="vertex">the vertex to add</param>
        /// <param name="followed">the vertex after which the above vertex is inserted</param>
        void Add(IVertex2d vertex, IVertex2d followed);

        /// <summary>
        ///  removes the specified vertex from the envelope
        /// </summary>
        /// <param name="vertex">vertex to remove from the envelope</param>
        void Remove(IVertex2d vertex);

        /// <summary>
        ///  removes all vertices from the envelope
        /// </summary>
        void Clear();

        #endregion
    }
}
