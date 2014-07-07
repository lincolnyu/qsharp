using System.Collections.Generic;

namespace QSharp.Scheme.Classical.Graphs
{
    /// <summary>
    ///  An interface for class to implement to claim its capability of showing
    ///  all incidental vertices of a specified vertex should implement,
    ///  where being incidental normally means
    ///   - in a directional graph all downstream vertices
    ///   - in a nondirectional / bidirectional graph all adjacent vertices
    /// </summary>
    public interface IGetIncidentalVertices
    {
        #region Methods

        /// <summary>
        ///  Returns all incidental vertices of the specified vertex
        /// </summary>
        /// <param name="c">The vertex of which the method returns all of its incidental vertices</param>
        /// <returns>The incidental vertices of the specified vertex</returns>
        IEnumerable<IVertex> GetIncidentalVertices(IVertex c);

        #endregion
    }
}
