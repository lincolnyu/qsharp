namespace QSharp.Scheme.Classical.Sequential.HeapSort
{
    /// <summary>
    ///  An interface that a heap based on a list should implement
    /// </summary>
    public interface IListBasedHeap : IHeap
    {
        #region Methods

        /// <summary>
        ///  Returns the last node that is not a leaf
        /// </summary>
        /// <returns>The last node that is not a leaf</returns>
        IHeapNode GetLastNonLeaf();

        /// <summary>
        ///  Get the linearly preceding item
        /// </summary>
        /// <param name="n">The node whose linear predecessor</param>
        /// <returns>The linear predecessor of the specified node</returns>
        IHeapNode GetPrevNodeLinearly(IHeapNode n);

        #endregion
    }
}
