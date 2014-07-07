namespace QSharp.Scheme.Classical.Graphs.MinimumSpanningTree
{
    /// <summary>
    ///  An interface that a marker that depicts a MST should implement
    /// </summary>
    public interface ITreeMarker
    {
        #region Methods

        /// <summary>
        ///  Connects two vertices as part of the MST
        /// </summary>
        /// <param name="v1">The first vertex</param>
        /// <param name="v2">The second vertex</param>
        void Connect(IVertex v1, IVertex v2);

        #endregion
    }
}
