using System;

namespace QSharp.Scheme.Classical.Sequential.HeapSort
{
    /// <summary>
    ///  A class that encapsulates constructs required for heap sorting
    /// </summary>
    public static class Heap
    {
        #region Methods
       
        /// <summary>
        ///  Elevates a newly introduced node at the end of the heap
        /// </summary>
        /// <param name="h">The heap</param>
        /// <param name="n">The node newly introduced at the end of the heap</param>
        /// <param name="cmp">The comparison method</param>
        public static void Exalt(this IHeap h, IHeapNode n, Comparison<IHeapNode> cmp)
        {
            var p = h.GetParent(n);

            while (p != null)
            {
                if(cmp(p, n) >= 0)
                {
                    break;
                }
                n = h.SwapUp(p, n);
                p = h.GetParent(n);
            }
        }

        /// <summary>
        ///  Sends an end node temporarily elevated to the root position to 
        ///  a proper place. Since it was a leaf node, it's expected to be sent
        ///  back to leaf level again.
        /// </summary>
        /// <param name="h">The heap</param>
        /// <param name="p">The node</param>
        /// <param name="cmp">The comparison method</param>
        public static void Expel(this IHeap h, IHeapNode p, Comparison<IHeapNode> cmp)
        {
            while (true)
            {
                var left = h.GetLeft(p);
                var right = h.GetRight(p);

                if (left == null && right == null)
                {
                    return;
                }

                // if p is less than any of the children swap it down
                if (right == null)
                {
                    if (cmp(p, left) < 0)
                    {
                        p = h.SwapDown(p, left);
                    }
                    else
                    {
                        return;
                    }
                }
                else if (left == null)
                {
                    if (cmp(p, right) < 0)
                    {
                        p = h.SwapDown(p, right);
                    }
                    else
                    {
                        return;
                    }
                }
                else if (cmp(left, right) < 0)
                {
                    if (cmp(p, right) < 0)
                    {
                        p = h.SwapDown(p, right);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    if (cmp(p, left) < 0)
                    {
                        p = h.SwapDown(p, left);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        ///  Pushes the root down to the leaf side in preparation for a removal.
        ///  It could be an express way to get a root node from a tree-based heap
        /// </summary>
        /// <param name="h">The heap</param>
        /// <param name="d">The node to push down</param>
        /// <param name="cmp">The comparison method</param>
        /// <returns></returns>
        public static IHeapNode Relegate(this IHeap h, IHeapNode d, Comparison<IHeapNode> cmp)
        {
            while (true)
            {
                var left = h.GetLeft(d);
                var right = h.GetRight(d);

                if (left == null && right == null)
                {
                    return d;
                }

                if (right == null)
                {
                    d = h.SwapDown(d, left);
                }
                else if (left == null)
                {
                    d = h.SwapDown(d, right);
                }
                else if (cmp(left, right) < 0)
                {
                    d = right;
                }
                else
                {
                    d = left;
                }
            }
        }

        #endregion
    }
}
