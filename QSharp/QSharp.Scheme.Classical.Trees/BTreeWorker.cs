using System;
using System.Text;

namespace QSharp.Scheme.Classical.Trees
{
    /// <summary>
    ///  An implementation of B-tree
    /// </summary>
    public static class BTreeWorker
    {
        #region Nested types

        /// <summary>
        ///  A b-tree node
        /// </summary>
        /// <typeparam name="TNode">The concrete type of the node</typeparam>
        /// <typeparam name="T">The type of the data the node contains</typeparam>
        public interface INode<TNode, T> where TNode : INode<TNode, T>
        {
            #region Properties

            TNode Parent { get; set; }

            int EntryCount { get; }
            int ChildCount { get; }

            #endregion
            
            #region Methods

            TNode GetChild(int index);
            void SetChild(int index, TNode node);
            void AppendChild(TNode node);
            void InsertChild(int index, TNode node);
            void RemoveChild(int index);

            T GetEntry(int index);
            void SetEntry(int index, T entry);
            void AppendEntry(T entry);
            void InsertEntry(int index, T entry);
            void RemoveEntry(int index);

            /// <summary>
            ///  removes entries from `start'-th and all relevant child nodes
            ///  (after that `start' entries and `start+1' nodes remain
            /// </summary>
            /// <param name="start">position from where to remove</param>
            void RemoveFrom(int start);

            /// <summary>
            ///  Determines if the node is a leaf
            /// </summary>
            /// <returns>true if the node is a leaf</returns>
            bool IsLeaf();

            #endregion

        }

        /// <summary>
        ///  An interface the algorithm uses to create nodes
        /// </summary>
        /// <typeparam name="TNode">The type of the node to create</typeparam>
        /// <typeparam name="T">The type of the data the node contains</typeparam>
        public interface INodeCreator<out TNode, T> where TNode : INode<TNode, T>
        {
            TNode Create();
        }

        #endregion

        #region Methods

        public static int MaximalEntryCount(int order)
        {
            return order - 1;
        }

        public static int MinimalEntryCount(int order)
        {
            return (order + 1) / 2 - 1;
        }

        public static int MaximalChildCount(int order)
        {
            return order;
        }

        public static int MinimalChildCount(int order)
        {
            return (order + 1) / 2;
        }

        public static void SearchForChildLinear<TNode, T>(TNode parent, TNode child, out int index)
            where TNode : INode<TNode, T>
        {
            var c = parent.ChildCount;
            for (index = 0; index < c; index++)
            {
                var p = parent.GetChild(index);
                if (Equals(p ,child))
                    return;
            }
        }

        public static void SearchForChildBinary<TNode, T>(TNode parent, TNode child, Comparison<T> comparison, out int index)
            where TNode : INode<TNode, T>
        {
            TNode p;
            var c = parent.ChildCount;
            if (child.EntryCount == 0)
            {
                for (index = 0; index < c; index++)
                {
                    p = parent.GetChild(index);
                    if (Equals(p ,child))
                        return;
                }
                return;
            }

            var t = child.GetEntry(0);

            int i;
            SearchOnNodeBinary(parent, t, comparison, out i);

            p = parent.GetChild(i);
            if (Equals(p ,child))
            {
                index = i;
                return;
            }

            var ce = parent.EntryCount;
            for (index = i - 1; index >= 0; index--)
            {
                if (comparison(t, parent.GetEntry(index)) > 0)
                {
                    break;
                }
                p = parent.GetChild(index);
                if (Equals(p, child))
                {
                    return;
                }
            }
            
            for (index = i + 1; index < ce; index++)
            {
                if (comparison(t, parent.GetEntry(index)) < 0)
                {
                    break;
                }
                p = parent.GetChild(index);
                if (Equals(p, child))
                {
                    return;
                }
            }

#if false
            // the last candidate which may possibly be the target
            p = parent.GetChild(index);
#endif
        }


        /**
         * <summary>
         *  Linear search
         *  
         *  Search the position of an entry with specified value `t' on `node' 
         *  if not found, the method gives the index where a new entry is
         *  supposed to be inserted (bigger entry)
         *  
         *  a node is not allowed to be with 0 entries
         *  
         * </summary>
         */
        public static bool SearchOnNodeLinear<TNode, T>(TNode node, IComparable<T> t, out int index)
            where TNode : INode<TNode, T>
        {
            int i;

            for (i = 0; i < node.EntryCount; i++)
            {
                var cmp = t.CompareTo(node.GetEntry(i));
                if (cmp < 0)
                {
                    break;
                }
                if (cmp == 0)
                {
                    index = i;
                    return true;
                }
            }
            index = i;
            return false;
        }

        /*
         * <remarks>
         *  if not matched, index is set at the bigger entry (or one after the last entry)
         * </remarks>
         */

        public static bool SearchOnNodeBinary<TNode, T>(TNode node, T t, Comparison<T> comparison, out int index)
            where TNode : INode<TNode, T>
        {
            var b = 0;
            var e = node.EntryCount;
            for (;;)
            {
                if (b == e)
                {
                    index = b;
                    return false;
                }
                index = (b + e)/2;

                var cmp = comparison(t, node.GetEntry(index));
                if (cmp < 0)
                {
                    e = index;
                }
                else if (cmp > 0)
                {
                    b = index + 1;
                }
                else
                {
                    return true;
                }
            }
        }

        public delegate bool SearchOnNode<in TNode, T>(TNode node, T t, Comparison<T> comparison, out int index);

        /**
         * <summary>
         *  Search the node with specified value `t' from `node'
         *  The position to insert at is given by `target' and `index'
         *  
         *  a node is not allowed to have no entry
         *  
         * </summary>
         */
        public static bool Search<TNode, T>(TNode node, T t, Comparison<T> comparison, 
            out TNode target, out int index, SearchOnNode<TNode, T> searchOnNode)
            where TNode : INode<TNode, T>
        {
            if (Equals(node, default(TNode)) || node.EntryCount == 0)
            {
                target = node;
                index = 0;
                return false;
            }

            while (true)
            {
                if (searchOnNode(node, t, comparison, out index))
                {
                    target = node;
                    return true;
                }

                TNode next = node.GetChild(index);
                if (Equals(next, default(TNode)))
                {
                    target = node;
                    return false;
                }
                node = next;
            }
        }

        public static bool BinarySearch<TNode, T>(TNode node, T t, Comparison<T> comparison,
            out TNode target, out int index)
            where TNode : INode<TNode, T>
        {
            return Search(node, t, comparison, out target, out index, SearchOnNodeBinary);
        }

        /**
         * <summary>
         *  Search the node with specified value `t' from `node'
         *  The position to insert at is given by `target' and `index'
         *  
         *  a node is not allowed to have no entry
         *  
         *  TODO: test it
         *  
         * </summary>
         */
        public static void SearchToLeaf<TNode, T>(TNode node, T t, 
            Comparison<T> comparison, out TNode target, out int index, 
            SearchOnNode<TNode, T> searchOnNode, bool left)
            where TNode : INode<TNode, T>
        {
            if (Equals(node, default(TNode)) || node.EntryCount == 0)
            {
                target = node;
                index = -1;
                return;
            }

            while (true)
            {
                if (searchOnNode(node, t, comparison, out index))
                {
                    if (!node.IsLeaf())
                    {
                        /**
                         * <remarks>
                         *  in-order neighbour of an entry on a non-leaf node 
                         *  must be at a leaf.
                         * </remarks>
                         */

                        if (left)
                        {
                            GotoPrevInorder(ref node, ref index, comparison);
                            index++;
                        }
                        else
                        {
                            GotoNextInorder(ref node, ref index, comparison);
                        }
                    }
                    target = node;
                    return;
                }

                var next = node.GetChild(index);
                if (Equals(next, default(TNode)))
                {
                    target = node;
                    return;
                }
                node = next;
            }
        }

        public static void BinarySearchToLeaf<TNode, T>(TNode node, T t, Comparison<T> comparison,
            out TNode target, out int index, bool left)
            where TNode : INode<TNode, T>
        {
            SearchToLeaf(node, t, comparison, out target, out index, SearchOnNodeBinary, left);
        }

        /**
         * <summary>
         *  Pre: p now has exactly `order' entries (accordingly, `order+1' children)
         *  Post: p has `order/2' entries, and ap has `order - 1 - order/2 = (order-1)/2' entries
         *        the key originally p's order/2-th is pushed up to the parent of p and ap,
         * </summary>
         */

        public static bool Split<TNode, T>(ref TNode p, int order, 
            INodeCreator<TNode, T> creator, Comparison<T> comparison)
            where TNode : INode<TNode, T>
        {
            var ap = creator.Create();

            var knm = order / 2;
            var srcidx = knm + 1;   // spare the order/2-th item
            TNode child;

            for ( ; srcidx < order; srcidx++)
            {
                child = p.GetChild(srcidx);
                ap.AppendChild(child);
                ap.AppendEntry(p.GetEntry(srcidx));
                if (!Equals(child, default(TNode)))
                {
                    child.Parent = ap;
                }
            }
            child = p.GetChild(srcidx);
            ap.AppendChild(child);
            if (!Equals(child, default(TNode)))
            {
                child.Parent = ap;
            }

            var x = p.GetEntry(knm);
            p.RemoveFrom(knm);

            var q = p.Parent;
            if (!Equals(q, default(TNode)))
            {
                int i;
                SearchForChildBinary(q, p, comparison, out i);

                q.InsertChild(i + 1, ap);
                q.InsertEntry(i, x);
                ap.Parent = q;
                p = q;

                return true;
            }

            // p is the root
            // create a new root which takes the two pieces
            q = creator.Create();
            q.AppendChild(p);
            q.AppendChild(ap);
            q.AppendEntry(x);
            p.Parent = q;
            ap.Parent = q;
            p = q;

            return false;
        }

        /**
         * <summary>
         *  Insert a new item at the specific position which is on a leaf
         * </summary>
         */
        public static void Insert<TNode, T>(T t, TNode at, int index, int order, 
            Comparison<T> comparison, ref TNode root, INodeCreator<TNode, T> creator)
            where TNode : INode<TNode, T>
        {
            if (!Equals(at, default(TNode)))
            {
                at.InsertEntry(index, t);
                at.AppendChild(default(TNode));
                while (true)
                {
                    if (at.EntryCount <= MaximalEntryCount(order))
                    {
                        break;
                    }

                    if (!Split(ref at, order, creator, comparison))
                    {
                        root = at;
                        break;
                    }
                }
            }
            else
            {
                root = creator.Create();
                root.AppendEntry(t);
                root.AppendChild(default(TNode));
                root.AppendChild(default(TNode));
            }
        }

        public static void GetOneFromLeft<TNode, T>(TNode q, int iself)
            where TNode : INode<TNode, T>
        {
            var ileft = iself - 1;
            var left = q.GetChild(ileft);

            var entryFromLeft = left.GetEntry(left.EntryCount - 1);
            var nodeFromLeft = left.GetChild(left.ChildCount - 1);
            left.RemoveFrom(left.EntryCount - 1);

            var m = q.GetEntry(ileft);
            q.SetEntry(ileft, entryFromLeft);

            var self = q.GetChild(iself);
            self.InsertEntry(0, m);
            self.InsertChild(0, nodeFromLeft);
            if (!Equals(nodeFromLeft, default(TNode)))
            {
                nodeFromLeft.Parent = self;
            }
        }

        public static void GetOneFromRight<TNode, T>(TNode q, int iself)
            where TNode : INode<TNode, T>
        {
            var iright = iself + 1;
            var right = q.GetChild(iright);

            var entryFromRight = right.GetEntry(0);
            var nodeFromRight = right.GetChild(0);
            right.RemoveChild(0);
            right.RemoveEntry(0);

            var m = q.GetEntry(iself);
            q.SetEntry(iself, entryFromRight);

            var self = q.GetChild(iself);
            self.AppendEntry(m);
            self.AppendChild(nodeFromRight);
            if (!Equals(nodeFromRight, default(TNode)))
            {
                nodeFromRight.Parent = self;
            }
        }

        public static void Merge<TNode, T>(ref TNode q, int ientry, ref TNode root)
            where TNode : INode<TNode, T>
        {
            var left = q.GetChild(ientry);
            var right = q.GetChild(ientry + 1);
            TNode child;

            var m = q.GetEntry(ientry);

            left.AppendEntry(m);

            int i;
            for (i = 0; i < right.EntryCount; i++)
            {
                child = right.GetChild(i);
                left.AppendEntry(right.GetEntry(i));
                left.AppendChild(child);
                if (!Equals(child, default(TNode)))
                {
                    child.Parent = left;
                }
            }
            child = right.GetChild(i);
            left.AppendChild(child);
            if (!Equals(child, default(TNode)))
            {
                child.Parent = left;
            }

            // dispose right
            if (right is IDisposable)
            {
                (right as IDisposable).Dispose();
            }

            if (Equals(q ,root) && q.EntryCount == 1)
            {
                // dispose q
                if (q is IDisposable)
                {
                    (q as IDisposable).Dispose();
                }

                left.Parent = default(TNode);
                q = root = left;
                return;
            }

            q.RemoveEntry(ientry);
            q.RemoveChild(ientry + 1);

            /**
             * q as a non-root node may be temporarily contains only one child
             * in the case of `order' = 4, however it is ok
             */ 
        }

 
        /// <summary>
        ///  
        /// </summary>
        /// <typeparam name="TNode">The type of the nodes</typeparam>
        /// <typeparam name="T">The type of the data each node contain</typeparam>
        /// <param name="q">a leaf (with only entries and no children)</param>
        /// <param name="index"></param>
        /// <param name="order"></param>
        /// <param name="comparison"></param>
        /// <param name="root">The root node, which might be changed during the process</param>
        public static void RemoveFromLeaf<TNode, T>(TNode q, int index, int order, Comparison<T> comparison, ref TNode root)
            where TNode : INode<TNode, T>
        {
            if (Equals(q ,root))
            {
                if (q.EntryCount == 1)
                {   // too small as a root
                    if (root is IDisposable)
                        (root as IDisposable).Dispose();
                    root = default(TNode);
                }
                else
                {
                    q.RemoveEntry(index);
                    q.RemoveChild(q.ChildCount - 1);
                }
                return;
            }

            q.RemoveEntry(index);
            q.RemoveChild(q.ChildCount - 1);

            while (true)
            {
                if (q.ChildCount >= MinimalChildCount(order) )
                {   // not too small
                    break;
                }

                var p = q;
                q = q.Parent;

                if (Equals(q, default(TNode)))
                {
                    break;
                }

                int iself;
                SearchForChildBinary(q, p, comparison, out iself);

                var left = default(TNode);

                if (iself > 0)
                {
                    left = q.GetChild(iself - 1);
                    if (left.ChildCount > MinimalChildCount(order))
                    {
                        GetOneFromLeft<TNode, T>(q, iself);
                        break;
                    }
                }

                if (iself < q.ChildCount - 1)
                {
                    TNode right = q.GetChild(iself + 1);
                    if (right.ChildCount > MinimalChildCount(order))
                    {
                        GetOneFromRight<TNode, T>(q, iself);
                        break;
                    }
                }

                if (!Equals(left, default(TNode)))
                {   // merge with left sibling
                    Merge<TNode, T>(ref q, iself - 1, ref root);
                }
                else    // right != null
                {   // merge with right sibling
                    Merge<TNode, T>(ref q, iself, ref root);
                }
            }
        }

        public static void Lift<TNode, T>(ref TNode q, ref int index)
            where TNode : INode<TNode, T>
        {
            var p = q.GetChild(index + 1); // always lift from the right side

            while (true)
            {
                if (p.IsLeaf())
                {
                    q.SetEntry(index, p.GetEntry(0));
                    index = 0;
                    q = p;
                    break;
                }
                p = p.GetChild(0);
            }
        }

        public static void Remove<TNode, T>(TNode q, int index, int order, Comparison<T> comparison,
            ref TNode root)
            where TNode : INode<TNode, T>
        {
            if (!q.IsLeaf())
            {   // non-leaf
                Lift<TNode, T>(ref q, ref index);
            }

            RemoveFromLeaf(q, index, order, comparison, ref root);
        }

        public static string ToString<TNode, T>(TNode q)
            where TNode : INode<TNode, T>
        {
            if (Equals(q, default(TNode)))
            {
                return "{ Empty Tree }";
            }

            var sb = new StringBuilder();

            sb.Append("Node( ");
            for (var i = 0; i < q.EntryCount; i++)
            {
                var t = q.GetEntry(i);
                sb.Append(t);
                if (i != q.EntryCount - 1)
                    sb.Append(", ");
            }
            sb.Append(" ) { ");

            var n = default(TNode);
            for (var i = 0; i < q.ChildCount; i++)
            {
                n = q.GetChild(i);
                if (!Equals(n, default(TNode)))
                {
                    string s = ToString<TNode, T>(n);
                    sb.Append(s);
                }
                if (i != q.ChildCount - 1)
                    sb.Append(", ");
            }
            sb.Append(Equals(n, default(TNode)) ? "}" : " }");
            return sb.ToString();
        }

        public static void GotoFirstInorder<TNode, T>(ref TNode q, out int index)
            where TNode : INode<TNode, T>
        {
            while (true)
            {
                if (q.IsLeaf())
                {
                    index = 0;
                    return;
                }

                q = q.GetChild(0);
            }
        }

        public static void GotoLastInorder<TNode, T>(ref TNode q, out int index)
            where TNode : INode<TNode, T>
        {
            while (true)
            {
                if (q.IsLeaf())
                {
                    index = q.EntryCount - 1;
                    return;
                }

                q = q.GetChild(q.ChildCount - 1);
            }
        }

        public static bool GotoNextInorder<TNode, T>(ref TNode q, ref int index, Comparison<T> comparison)
            where TNode : INode<TNode, T>
        {
            var iright = index + 1;

            if (q.IsLeaf())
            {
                if (iright < q.EntryCount)
                {
                    index = iright;
                    return true;
                }
                /* the next key is on neigboring subtree */
                while (true)
                {
                    var p = q.Parent;

                    if (Equals(p, default(TNode)))
                    {
                        q = p;
                        index = -1;
                        return false;
                    }

                    int i;
                    SearchForChildBinary(p, q, comparison, out i);

                    if (i < p.ChildCount - 1)
                    {
                        q = p;
                        index = i;
                        return true;
                    }
                    q = p;
                }
                // the routine quits above here
            }

            q = q.GetChild(iright); /* get to the node on the right */
            while (true)
            {
                if (q.IsLeaf())
                {
                    index = 0;
                    return true;
                }
                q = q.GetChild(0);
            }
        }

        public static bool GotoPrevInorder<TNode, T>(ref TNode q, ref int index, Comparison<T> comparison)
            where TNode : INode<TNode, T>
        {
            if (q.IsLeaf())
            {
                if (index > 0)
                {
                    index--;
                    return true;
                }
                /* the next key is on neigboring subtree */
                while (true)
                {
                    var p = q.Parent;

                    if (Equals(p, default(TNode)))
                    {
                        q = p;
                        index = -1;
                        return false;
                    }

                    int i;
                    SearchForChildBinary(p, q, comparison, out i);

                    if (i > 0)
                    {
                        q = p;
                        index = i - 1;
                        return true;
                    }

                    q = p;
                }
                // the routine quits above here
            }

            q = q.GetChild(index);  /* get to the node on the right */
            while (true)
            {
                if (q.IsLeaf())
                {
                    index = q.EntryCount - 1;
                    return true;
                }
                q = q.GetChild(q.ChildCount - 1);
            }
        }

        public static bool CheckNode<TNode, T>(TNode q, int order, out int depth, out int count)
            where TNode : INode<TNode, T>
        {
            var entrynum = q.EntryCount;
            count = entrynum;
            depth = 1;

            if (entrynum > MaximalEntryCount(order))
            {
                return false;
            }
            if (!Equals(q.Parent, default(TNode)) && entrynum < MinimalEntryCount(order))
            {
                return false;
            }
            if (entrynum == 0)
            {
                return false;
            }
            if (entrynum + 1 != q.ChildCount)
            {
                return false;
            }

            depth = -1;
            for (var i = 0; i < q.ChildCount; i++)
            {
                var c = 0;
                var d = 0;
                var p = q.GetChild(i);
                if (!Equals(p, default(TNode)))
                {
                    if ((object) q != (object) p.Parent)
                    {
                        return false;
                    }
                    if (!CheckNode<TNode, T>(p, order, out d, out c))
                    {
                        return false;
                    }
                }
                if (depth == -1)
                {
                    depth = d;
                }
                else if (depth != d)
                {
                    return false;   /* violates the condition that all leaves are at the same depth */
                }
                count += c;
            }

            depth++;    /* to include the current level */

            return true;
        }

        public static bool CheckFromRoot<TNode, T>(TNode q, int order, Comparison<T> comparison, out int count)
            where TNode : INode<TNode, T>
        {
            count = 0;
            if (Equals(q, default(TNode)))
            {
                return true;
            }

            if (!Equals(q.Parent, default(TNode)))
            {
                return false;
            }

            int depth;
            if (!CheckNode<TNode, T>(q, order, out depth, out count))
            {
                return false;
            }

            int index;
            GotoFirstInorder<TNode, T>(ref q, out index);

            var prev = q.GetEntry(index);

            while (true)
            {
                if (!GotoNextInorder(ref q, ref index, comparison))
                {
                    return true;
                }

                var t = q.GetEntry(index);

                if (comparison(prev, t) >= 0)
                {
                    return false;
                }

                prev = t;
            }
        }

        #endregion
    }
}

