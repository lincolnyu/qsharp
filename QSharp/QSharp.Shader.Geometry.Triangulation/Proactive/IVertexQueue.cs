using QSharp.Shader.Geometry.Common2d;

namespace QSharp.Shader.Geometry.Triangulation.Proactive
{
    /// <summary>
    ///  interface of entity that maintains a queue of vertices FIFO
    /// </summary>
    public interface IVertexQueue
    {
        #region Methods

        /// <summary>
        ///  returns the number of vertices in the queue
        /// </summary>
        int Count { get; }

        /// <summary>
        ///  tells if the queue contains the specified vertex
        /// </summary>
        /// <param name="vertex">the vertex to check and see if the queue contains</param>
        /// <returns>true if the queue contains the specified vertex</returns>
        bool Contains(IVertex2d vertex);

        /// <summary>
        ///  adds a vertex to the queue only if it's not in the queue
        /// </summary>
        /// <param name="vertex"></param>
        void EnqueueIfNotContaining(IVertex2d vertex);
        
        /// <summary>
        ///  adds a vertex to the queue
        /// </summary>
        /// <param name="vertex"></param>
        void Enqueue(IVertex2d vertex);

        /// <summary>
        ///  removes the first vertex from the queue and returns it
        /// </summary>
        /// <returns></returns>
        IVertex2d Dequeue();
        
        /// <summary>
        ///  removes all vertices from the queue
        /// </summary>
        void Clear();

        #endregion
    }
}
