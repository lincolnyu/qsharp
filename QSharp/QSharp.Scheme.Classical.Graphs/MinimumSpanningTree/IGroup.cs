using System.Collections.Generic;

namespace QSharp.Scheme.Classical.Graphs.MinimumSpanningTree
{
    /// <summary>
    ///  A group of connected vertices that form a subtree
    /// </summary>
    public interface IGroup
    {
        #region Properties

        /// <summary>
        ///  The vertices that are included in the group
        /// </summary>
        IEnumerable<IVertex> Vertices { get; }

        #endregion

        #region Methods

        /// <summary>
        ///  Determines if a vertex is in the group
        /// </summary>
        /// <param name="v">The vertex to test</param>
        /// <returns>true if it is</returns>
        bool IsInGroup(IVertex v);

        /// <summary>
        ///  Merges with another group by taking all the vertices in it
        ///  the method should not remove vertices from the other group
        /// </summary>
        /// <param name="other">The group to merge with</param>
        void Merge(IGroup other);

        #endregion
    }
}
