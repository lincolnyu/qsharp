using System;

namespace QSharp.Scheme.Classical.Graphs
{
    /// <summary>
    ///  The interface that any class that represents the distances or lengths in a graph should implement
    /// </summary>
    public interface IDistance : IComparable<IDistance>
    {
        #region Methods

        /// <summary>
        ///  Returns true if the distance is infinity or indicating being inaccessible
        /// </summary>
        /// <returns>True if inifinity</returns>
        /// <remarks>
        ///  NOTE it may not necessarily be the same as maximum value for comparison purpose
        /// </remarks>
        bool IsInfinity();

        /// <summary>
        ///  Returns the result of adding the specified to the current
        /// </summary>
        /// <param name="that">The addend</param>
        /// <returns>The result of hte summation</returns>
        IDistance Add(IDistance that);

        #endregion
    }
}
