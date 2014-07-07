using QSharp.Scheme.Classical.Graphs.MinimumSpanningTree;

namespace QSharp.Scheme.Classical.Graphs
{
    /// <summary>
    ///  An interface for group creators to implement
    /// </summary>
    public interface IGroupCreator
    {
        #region Methods

        /// <summary>
        ///  Creates an initial group that contains only one vertex
        /// </summary>
        /// <param name="vertex">The vertex to include initially in the group</param>
        /// <returns>The group object created</returns>
        IGroup CreateSingleVertexGroup(IVertex vertex);

        #endregion
    }
}
