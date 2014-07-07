using System.Collections.Generic;

namespace QSharp.Scheme.Classical.Graphs
{
    /// <summary>
    ///  An interface a class to implement to claim its capability of 
    ///  enumerating vertices 
    /// </summary>
    public interface IGetAllVertices
    {
        #region Methods

        /// <summary>
        ///  Returns all the vertices
        /// </summary>
        /// <returns>The vertices</returns>
        IEnumerable<IVertex> GetAllVertices();

        #endregion
    }
}
