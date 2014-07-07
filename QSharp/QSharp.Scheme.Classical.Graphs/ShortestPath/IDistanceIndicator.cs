namespace QSharp.Scheme.Classical.Graphs.ShortestPath
{
    /// <summary>
    ///  The interface for any class to implement that keeps the current state in terms of
    ///  connectivity and distances of an ongoing shortest path finding process
    /// </summary>
    public interface IDistanceIndicator
    {
        #region Properties

        /// <summary>
        ///  The maximum possible distance, normally used as initial value for getting minimum
        /// </summary>
        /// <returns>The maximum possible distance</returns>
        IDistance MaxDistance { get; }

        #endregion

        #region Methods

        /// <summary>
        ///  Returns the current distance between the source and the specified vertex
        /// </summary>
        /// <param name="v">The vertex from which the distance to the source is to be returned</param>
        /// <returns>The distance</returns>
        IDistance GetCurrentDistanceFromSource(IVertex v);

        /// <summary>
        ///  Updates the distance from the source to the specified vertex
        /// </summary>
        /// <param name="v">The specified vertex to which from the source the distance is to be updated </param>
        /// <param name="d"></param>
        void UpdateDistanceFromSource(IVertex v, IDistance d);

        /// <summary>
        ///  Establishes connectivity between source and target as part of the current path
        /// </summary>
        /// <param name="source">The source vertex</param>
        /// <param name="target">The target vertex</param>
        void UpdateConnectivity(IVertex source, IVertex target);

        #endregion
    }
}
