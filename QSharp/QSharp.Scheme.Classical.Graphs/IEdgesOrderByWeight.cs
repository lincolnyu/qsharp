namespace QSharp.Scheme.Classical.Graphs
{
    /// <summary>
    ///  An interface that provides a sorted list of edges
    /// </summary>
    /// <remarks>
    ///  NOTE: 
    ///   If the graph is undirectional or equivalently bidirectional with equal
    ///   weight on the opposite links, then at most only one link for each two 
    ///   adjacent vertices should be included in the sorted list
    /// </remarks>
    public interface IEdgesOrderByWeight
    {
        #region Properties

        /// <summary>
        ///  Returns the number of edges
        /// </summary>
        int EdgeCount { get; }

        #endregion

        #region Methods

        /// <summary>
        ///  Gets the first vertex of the edge with the specified index
        /// </summary>
        /// <param name="index">The index of the edge whose first vertex is to be returned</param>
        /// <returns>The first vertex of the edge</returns>
        IVertex GetVertex1(int index);

        /// <summary>
        ///  Gets the second vertex of the edge with the specified index
        /// </summary>
        /// <param name="index">The index of the edge whose second vertex is to be returned</param>
        /// <returns>The second vertex of the edge</returns>
        IVertex GetVertex2(int index);

        /// <summary>
        ///  Gets the weight of the edge with the specified index
        /// </summary>
        /// <param name="index">The index of the edge whose weight is to be returned</param>
        /// <returns>The weight of the edge</returns>
        IDistance GetWeight(int index);

        #endregion
    }
}
