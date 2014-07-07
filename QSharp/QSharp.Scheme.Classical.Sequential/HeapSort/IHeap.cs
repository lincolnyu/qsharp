namespace QSharp.Scheme.Classical.Sequential.HeapSort
{
    /// <summary>
    ///  An interface that a heap should implement
    /// </summary>
    public interface IHeap
    {
        #region Properties

        /// <summary>
        ///  The root of the heap
        /// </summary>
        IHeapNode Root { get; }

        #endregion

        #region Methods

        /// <summary>
        ///  Swaps the child and the parent and returns the parent
        /// </summary>
        /// <param name="a">The child</param>
        /// <param name="b">The parent</param>
        /// <returns>The parent (may not be necessarily the same object as the parent passed in)</returns>
        IHeapNode SwapUp(IHeapNode a, IHeapNode b);

        /// <summary>
        ///  Swaps the parent and the child and returns the child
        /// </summary>
        /// <param name="a">The parent</param>
        /// <param name="b">The child</param>
        /// <returns>The child (may not necessarily be the same object as the parent passed in</returns>
        IHeapNode SwapDown(IHeapNode a, IHeapNode b);

        /// <summary>
        ///  Returns the parent of the specified node
        /// </summary>
        /// <param name="n">The node whose parent is to be returned</param>
        /// <returns>The parent</returns>
        IHeapNode GetParent(IHeapNode n);

        /// <summary>
        ///  Returns the left child
        /// </summary>
        /// <param name="n">The node whose left child is to be returned</param>
        /// <returns>The left child</returns>
        IHeapNode GetLeft(IHeapNode n);

        /// <summary>
        ///  Returns the right child
        /// </summary>
        /// <param name="n">The node whose right child is to be returned</param>
        /// <returns>The right child</returns>
        IHeapNode GetRight(IHeapNode n);

        #endregion
    }
}
