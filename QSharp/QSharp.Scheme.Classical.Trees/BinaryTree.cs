/**
 * <vendor>
 *  Copyright 2010 Quanben Tech.
 * </vendor>
 * 
 * <summary>
 *  Shared mechanism of binary search trees
 * </summary>
 */

using System;
using System.Collections.Generic;
using System.Text;
using QSharp.Shared;

namespace QSharp.Scheme.Classical.Trees
{
    public static class SearchTree
    {
        public interface INode
        {
        }
    }

    public interface ISearchTree<in T>
    {
        #region Methods

        SearchTree.INode Search(T t);
        SearchTree.INode Insert(T t);

        void Remove(SearchTree.INode h);

        bool Check(out int count);

        #endregion
    }

    public static class BinaryTree
    {
        #region Nested types

        public interface IFixedNode<out TNodeType> : SearchTree.INode
        {
            TNodeType Parent { get; }
            TNodeType Left { get; }
            TNodeType Right { get; }
        }

        public interface INode<TNodeType> : IFixedNode<TNodeType>
            where TNodeType : INode<TNodeType>
        {
            new TNodeType Parent { get; set; }
            new TNodeType Left { get; set; }
            new TNodeType Right { get; set; }
        }

        public interface IEntry<T>
        {
            T Entry { get; set; }
        }

        public interface IFixedNode<out TNodeType, T> : IFixedNode<TNodeType>, IEntry<T>
            where TNodeType : IFixedNode<TNodeType>
        {
        }

        public interface INode<TNodeType, T> : INode<TNodeType>, IFixedNode<TNodeType, T>
            where TNodeType : INode<TNodeType>
        {
        }

        public class Node<TNodeType> : INode<TNodeType>
            where TNodeType : INode<TNodeType>
        {
            public TNodeType Parent { get; set; }
            public TNodeType Left { get; set; }
            public TNodeType Right { get; set; }

            public Node()
            {
                Parent = default(TNodeType);
                Parent = default(TNodeType);
                Right = default(TNodeType);
            }
        }

        public class DisposableNode<TNodeType, T> : Node<TNodeType>, INode<TNodeType, T>
            where TNodeType : INode<TNodeType>
        {
            public T Entry { get; set; }

            public DisposableNode()
            {
                Entry = default(T);
            }

            public DisposableNode(T entry)
            {
                Entry = entry;
            }

            ~DisposableNode()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (Disposed)
                {
                    return;
                }

                if (disposing)
                {
                    if (Entry is IDisposable)
                    {
                        (Entry as IDisposable).Dispose();
                    }
                }

                Disposed = true;
            }

            protected bool Disposed;
        }

        public class NondisposableNode<TNodeType, T> : Node<TNodeType>, INode<TNodeType, T>
            where TNodeType : INode<TNodeType>
        {
            public T Entry { get; set; }

            public NondisposableNode()
            {
                Entry = default(T);
            }

            public NondisposableNode(T entry)
            {
                Entry = entry;
            }
        }

        public interface INodeIterator<TIterator, TNode>
            where TIterator : INodeIterator<TIterator, TNode>
            where TNode : INode<TNode>
        {
            TNode UnderlyingNode { get; }
            TIterator Left { get; set; }
            TIterator Right { get; set; }
            TIterator Parent { get; set; }

            TIterator Create(TNode node);
        }

        public abstract class NodeIterator<TIterator, TNode> 
            : INodeIterator<TIterator, TNode>
            where TIterator : INodeIterator<TIterator, TNode>
            where TNode : INode<TNode>
        {
            public TNode UnderlyingNode { get; protected set; }

            public abstract TIterator Left { get; set; }
            public abstract TIterator Right { get; set; }

            public virtual TIterator Parent
            {
                get
                {
                    return default(TIterator);
                }
                set 
                {
                    UnderlyingNode.Parent = (object)value == null ? default(TNode) : value.UnderlyingNode;
                }
            }

            protected NodeIterator(TNode underlyingNode)
            {
                UnderlyingNode = underlyingNode;
            }

            public override bool Equals(object obj)
            {
                var iObj = obj as INodeIterator<TIterator, TNode>;
                if (null == iObj)
                {
                    return false;
                }
                return (object)UnderlyingNode == (object)iObj.UnderlyingNode;
            }

            public override int GetHashCode()
            {
                return EqualityComparer<TNode>.Default.GetHashCode(UnderlyingNode);
            }

            public abstract TIterator Create(TNode node);
        }

        public class ForwardIterator<TIterator, TNode> 
            : NodeIterator<TIterator, TNode>
            where TIterator : INodeIterator<TIterator, TNode>
            where TNode : class, INode<TNode>
        {
            public override TIterator Left
            {
                get
                {
                    return UnderlyingNode.Left == null ? default(TIterator) : Create(UnderlyingNode.Left);
                }
                set 
                {
                    UnderlyingNode.Left = (object)value == null ? default(TNode) : value.UnderlyingNode;
                }
            }
            public override TIterator Right
            {
                get
                {
                    return UnderlyingNode.Right == null ? default(TIterator) : Create(UnderlyingNode.Right);
                }
                set 
                {
                    UnderlyingNode.Right = (object)value == null ? default(TNode) : value.UnderlyingNode;
                }
            }
            public override TIterator Parent
            {
                get
                {
                    return UnderlyingNode.Parent == null ? default(TIterator) : Create(UnderlyingNode.Parent);
                }
            }

            public override TIterator Create(TNode node)
            {
                return (TIterator)(INodeIterator<TIterator, TNode>)
                    new ForwardIterator<TIterator, TNode>(node);
            }

            public ForwardIterator(TNode underlyingNode)
                : base(underlyingNode)
            {
            }
        }

        public class ReverseIterator<TIterator, TNode> : NodeIterator<TIterator, TNode>
            where TIterator : INodeIterator<TIterator, TNode>
            where TNode : class, INode<TNode>
        {
            public override TIterator Left
            {
                get
                {
                    return UnderlyingNode.Right == null ? default(TIterator) : Create(UnderlyingNode.Right);
                }
                set 
                {
                    UnderlyingNode.Right = (object)value == null ? default(TNode) : value.UnderlyingNode;
                }
            }
            public override TIterator Right
            {
                get
                {
                    return UnderlyingNode.Left == null ? default(TIterator) : Create(UnderlyingNode.Left);
                }
                set 
                {
                    UnderlyingNode.Left = (object)value == null ? default(TNode) : value.UnderlyingNode;
                }
            }
            public override TIterator Parent
            {
                get
                {
                    return UnderlyingNode.Parent == null ? default(TIterator) : Create(UnderlyingNode.Parent);
                }
            }

            public override TIterator Create(TNode node)
            {
                return (TIterator)(INodeIterator<TIterator, TNode>)
                    new ReverseIterator<TIterator, TNode>(node);
            }

            public ReverseIterator(TNode underlyingNode)
                : base(underlyingNode)
            {
            }
        }

        #endregion

        #region Methods

        public static INode<TNodeType> CreateDisposableNode<TNodeType, T>(T t)
            where TNodeType : INode<TNodeType, T>
        {
            return new DisposableNode<TNodeType, T>(t);
        }

        public static INode<TNodeType> CreateNondisposableNode<TNodeType, T>(T t)
            where TNodeType : INode<TNodeType, T>
        {
            return new NondisposableNode<TNodeType, T>(t);
        }

        public static INode<TNodeType> CreateNode<TNodeType, T>(T t)
            where TNodeType : INode<TNodeType, T>
        {
            if (t is IDisposable)
            {
                return new DisposableNode<TNodeType, T>(t);
            }
            return CreateNondisposableNode<TNodeType, T>(t);
        }

        public static TNodeType GetGrandparent<TNodeType>(this TNodeType n)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            if (n != null && n.Parent != null)
            {
                return n.Parent.Parent;
            }

            return default(TNodeType);
        }

        public static TNodeType GetUncle<TNodeType>(this TNodeType n)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            var g = GetGrandparent(n);
            if (g == null)
            {
                return default(TNodeType);
            }
            return (object)n.Parent == (object)g.Left ? g.Right : g.Left;
        }

        public static TNodeType GetSibling<TNodeType>(this TNodeType n)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            if (n == null || n.Parent == null)
            {
                return default(TNodeType);
            }
            return (object)n == (object)n.Parent.Left ? n.Parent.Right : n.Parent.Left;
        }
       
        /// <summary>
        ///  Rotates around 2 nodes on a zigged line (from the top node turning left)
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="a">The node around which to rotate</param>
        /// <param name="b">The node that used to be <paramref name="a"/>'s left and will be come its parent</param>
        public static void RotateZig<TNodeType>(this TNodeType a, out TNodeType b)
            where TNodeType : class, INode<TNodeType>
        {
            b = a.Left;
            b.Parent = a.Parent;
            if (a.Parent != null)
            {
                if (a.Parent.Left == a)
                {
                    a.Parent.Left = b;
                }
                else
                {
                    a.Parent.Right = b;
                }
            }

            a.Parent = b;

            a.Left = b.Right;
            if (a.Left != null)
            {
                a.Left.Parent = a;
            }
            b.Right = a;
        }

        /// <summary>
        ///  Rotates around 2 nodes on a zagged line (from the top node turning right)
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="a">The node around which to rotate</param>
        /// <param name="b">The node that used to be <paramref name="a"/>'s right and will be come its parent</param>
        public static void RotateZag<TNodeType>(this TNodeType a, out TNodeType b)
            where TNodeType : class, INode<TNodeType>
        {
            b = a.Right;
            b.Parent = a.Parent;
            if (a.Parent != null)
            {
                if (b.Parent.Left == a)
                {
                    b.Parent.Left = b;
                }
                else
                {
                    b.Parent.Right = b;
                }
            }

            a.Parent = b;

            a.Right = b.Left;
            if (a.Right != null)
            {
                a.Right.Parent = a;
            }
            b.Left = a;
        }

        /// <summary>
        ///  Rotates around 3 nodes on a zigzagged line (from the topmost first turning left then right)
        /// </summary>
        /// <typeparam name="TNodeType"></typeparam>
        /// <param name="a">The node around which to rotate</param>
        /// <param name="b">The node that used to be the left of <paramref name="a"/></param>
        /// <param name="c">The node that used to be right of <paramref name="b"/></param>
        public static void RotateZigZag<TNodeType>(this TNodeType a, out TNodeType b, out TNodeType c)
            where TNodeType : class, INode<TNodeType>
        {
            b = a.Left;
            c = b.Right;

            c.Parent = a.Parent;
            if (a.Parent != null)
            {
                if (a.Parent.Left == a)
                {
                    a.Parent.Left = c;
                }
                else
                {
                    a.Parent.Right = c;
                }
            }
            b.Parent = c;
            a.Parent = c;

            a.Left = c.Right;
            b.Right = c.Left;
            if (a.Left != null)
            {
                a.Left.Parent = a;
            }
            if (b.Right != null)
            {
                b.Right.Parent = b;
            }

            c.Left = b;
            c.Right = a;
        }

        /// <summary>
        ///  Rotates around 3 nodes on a zagzigged line (from the topmost first turning right then left)
        /// </summary>
        /// <typeparam name="TNodeType">The type of the ndoes</typeparam>
        /// <param name="a">The node around which to rotate</param>
        /// <param name="b">The node that used to be the right of <paramref name="a"/> and will be the one in the center after rotation</param>
        /// <param name="c">The node that used to be left of <paramref name="b"/></param>
        public static void RotateZagZig<TNodeType>(this TNodeType a, out TNodeType b, out TNodeType c)
            where TNodeType : class, INode<TNodeType>
        {
            b = a.Right;
            c = b.Left;

            c.Parent = a.Parent;
            if (a.Parent != null)
            {
                if (a.Parent.Left == a)
                {
                    a.Parent.Left = c;
                }
                else
                {
                    a.Parent.Right = c;
                }
            }
            b.Parent = c;
            a.Parent = c;

            a.Right = c.Left;
            b.Left = c.Right;
            if (a.Right != null)
            {
                a.Right.Parent = a;
            }
            if (b.Left != null)
            {
                b.Left.Parent = b;
            }

            c.Left = a;
            c.Right = b;
        }

        /// <summary>
        ///  Gets the first node in in-order traversal
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="node">The node to start with as a root</param>
        /// <returns>The last node in the in-order traversal</returns>
        public static TNodeType GetFirstInorder<TNodeType>(this TNodeType node)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            for (; node.Left != null; node = node.Left)
            {
            }
            return node;
        }

        /// <summary>
        ///  Gets the last node current in in-order traversal
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="node">The node to start with as a root</param>
        /// <returns>The last node in the in-order traversal</returns>
        public static TNodeType GetLastInorder<TNodeType>(this TNodeType node)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            for (; node.Right != null; node = node.Right)
            {
            }
            return node;
        }

        /// <summary>
        ///  Gets the node succeeding current in in-order traversal
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="node">The node to start with</param>
        /// <param name="rootParent">The parent of the root that restricts the traversal</param>
        /// <returns>The next node in the in-order traversal</returns>
        public static TNodeType GetNextInorder<TNodeType>(this TNodeType node, TNodeType rootParent = null)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            if (node.Right != null)
            {
                node = node.Right;
                for (var p = node; p != null; p = p.Left)
                {
                    node = p;
                }
                return node;
            }

            while (node.Parent != rootParent)
            {
                if (node.Parent.Left == node)
                {
                    return node.Parent;
                }
                node = node.Parent;
            }

            return default(TNodeType); // indicates an end
        }

        /// <summary>
        ///  Gets the node preceding current in in-order traversal
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="rootParent">The parent of the root that restricts the traversal</param>
        /// <param name="node">The node to start with</param>
        /// <returns>The node preceding the specified one in the in-order traversal</returns>
        public static TNodeType GetPrevInorder<TNodeType>(this TNodeType node, TNodeType rootParent = null)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            if (node.Left != null)
            {
                node = node.Left;
                for (var p = node; p != null; p = p.Right)
                {
                    node = p;
                }
                return node;
            }

            while (node.Parent != rootParent)
            {
                if (node.Parent.Right == node)
                {
                    return node.Parent;
                }
                node = node.Parent;
            }

            return default(TNodeType); // indicates an end
        }

        /// <summary>
        ///  Gets the last node in pre-order traversal
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="node">The node to start with as a root</param>
        /// <returns>The last node</returns>
        public static TNodeType GetLastPreorder<TNodeType>(this TNodeType node)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            while (true)
            {
                if (node.Right != null)
                {
                    node = node.Right;
                }
                else if (node.Left != null)
                {
                    node = node.Left;
                }
                else
                {
                    return node;
                }
            }
        }

        /// <summary>
        ///  Gets the node succeeding current in pre-order traversal
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="node">The node whose succeeding node is to be found</param>
        /// <param name="rootParent">The parent of root that restricts the traversal</param>
        /// <returns>The node that succeeds the node in pre-order traversal</returns>
        public static TNodeType GetNextPreorder<TNodeType>(this TNodeType node, TNodeType rootParent = null)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            if (node.Left != null)
            {
                return node.Left;
            }

            if (node.Right != null)
            {
                return node.Right;
            }

            for (var parent = node.Parent; parent != rootParent; parent = node.Parent)
            {
                if (parent.Right != node && parent.Right != null)
                {
                    return parent.Right;
                }
            }

            return default(TNodeType);
        }

        /// <summary>
        ///  Gets the node preceding current in pre-order traversal
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="node">The node whose preceding node is to be found</param>
        /// <param name="rootParent">The parent of root that restricts the traversal</param>
        /// <returns>The node that precedes the specified node in pre-order traversal</returns>
        public static TNodeType GetPrevPreorder<TNodeType>(this TNodeType node, TNodeType rootParent = null)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            var p = node.Parent;

            if (p == null || p.Left == node)
                return p;

            p = p.Left;

            while (true)
            {
                if (p.Right != null)
                {
                    p = p.Right;
                }
                else if (p.Left != null)
                {
                    p = p.Left;
                }
                else
                {
                    return p;
                }
            }
        }

        /// <summary>
        ///  Gets the first node in post-order traversal
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="node">The node to start with as a root</param>
        /// <returns>The first node in post-order traversal</returns>
        public static TNodeType GetFirstPostorder<TNodeType>(this TNodeType node)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            while (true)
            {
                if (node.Left != null)
                {
                    node = node.Left;
                }
                else if (node.Right != null)
                {
                    node = node.Right;
                }
                else
                {
                    return node;
                }
            }
        }

        /// <summary>
        ///  Gets the node succeeding current in post-order traversal
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="node">The node to start with</param>
        /// <returns>The node succeeding the specified in post-order traversal</returns>
        public static TNodeType GetNextPostorder<TNodeType>(this TNodeType node)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            var p = node.Parent;
            if (p == null)
            {
                return p;
            }

            if (p.Right == node || p.Right == null)
            {
                return p;
            }

            p = p.Right;

            while (true)
            {
                if (p.Left != null)
                    p = p.Left;
                else if (p.Right != null)
                    p = p.Right;
                else
                    return p;
            }
        }

        /// <summary>
        ///  Gets the node preceding current in post-order traversal
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="node">The node to start with</param>
        /// <returns>The node preceding the specified in post-order traversal</returns>
        public static TNodeType GetPrevPostorder<TNodeType>(this TNodeType node)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            var p = node;

            do
            {
                while (true)
                {
                    if (p.Right != null)
                    {
                        p = p.Right;
                    }
                    else if (p.Left != null)
                    {
                        p = p.Left;
                    }
                    else
                    {
                        break;
                    }
                }

                if (p != node)
                    return p;

                p = node.Parent;
                for (; p != null; node = p, p = node.Parent)
                {
                    if (p.Right != node || p.Left == null)
                    {
                        continue;
                    }
                    p = p.Left;
                    break;
                }

                if (p == null)
                {
                    return p;
                }
            } while (true);
        }

        /// <summary>
        ///  Searches for the specified item from the specified node
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <typeparam name="T">The type of the data</typeparam>
        /// <param name="start">The node to start the search from</param>
        /// <param name="t">The target to find</param>
        /// <param name="comparison">The comparison that's used by the tree for routing and matching</param>
        /// <returns>The node that contains the target data</returns>
        public static TNodeType Search<TNodeType, T>(this TNodeType start, T t, Comparison<T> comparison)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            var p = start as IFixedNode<TNodeType, T>;

            while (p != null)
            {
                var cmp = comparison(p.Entry, t);
                if (cmp < 0)
                {
                    if (p.Right == null) break;
                    p = p.Right as IFixedNode<TNodeType, T>;
                }
                else if (cmp > 0)
                {
                    if (p.Left == null) break;
                    p = p.Left as IFixedNode<TNodeType, T>;
                }
                else    // equal
                {
                    break;
                }
            }

            return (TNodeType)p;
        }

        /// <summary>
        ///  Returns the depth of the tree from the specified node. no subnodes counted as depth of 1
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="start">The node the measurement starts with as root</param>
        /// <returns>The depth of the subtree</returns>
        public static int GetDepth<TNodeType>(this TNodeType start)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            var d1 = 0;
            var d2 = 0;
            if (start.Left != null)
            {
                d1 = GetDepth(start.Left);
            }
            if (start.Right != null)
            {
                d2 = GetDepth(start.Right);
            }
            var d = Math.Max(d1, d2) + 1;
            return d;
        }

        public static string ToString<TNodeType, T>(this TNodeType node)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            var sb = new StringBuilder();
            var inode = node as IFixedNode<TNodeType, T>;
            System.Diagnostics.Debug.Assert(inode != null);
            sb.Append("Node(");
            sb.Append(inode.Entry);

            if (node.Left == null && node.Right == null)
            {
                sb.Append(")");
                return sb.ToString();
            }

            sb.Append(" ){ ");
            if (node.Left != null)
            {
                if (node.Left.Parent != node)
                {
                    throw new QException("Bad binary-tree");
                }
                sb.Append(ToString<TNodeType, T>(node.Left));
            }
            sb.Append(",");
            if (node.Right != null)
            {
                if (node.Right.Parent != node)
                {
                    throw new QException("Bad binary-tree");
                }
                sb.Append(" ");
                sb.Append(ToString<TNodeType, T>(node.Right));
            }
            sb.Append(" } ");
            return sb.ToString();
        }

        /// <summary>
        ///  Checks the validity and integrity of the tree (parent and child relationships)
        /// </summary>
        /// <typeparam name="TNodeType">The type of the nodes</typeparam>
        /// <param name="node">The node to start with</param>
        /// <param name="count">The total number of nodes</param>
        /// <returns>True if the tree is valid</returns>
        public static bool Check<TNodeType>(this TNodeType node, out int count)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            var c1 = 0;
            var c2 = 0;
            count = 0;
            if (node.Left != null)
            {
                if (node.Left.Parent != node)
                {
                    return false;
                }

                if (!Check(node.Left, out c1))
                {
                    return false;
                }
            }

            if (node.Right != null)
            {
                if (node.Right.Parent != node)
                {
                    return false;
                }

                if (!Check(node.Right, out c2))
                {
                    return false;
                }
            }

            count = c1 + c2 + 1;

            return true;
        }

        public static bool CheckFromRoot<TNodeType>(this TNodeType root, out int count)
            where TNodeType : class, IFixedNode<TNodeType>
        {
            count = 0;
            return root.Parent == null && Check(root, out count);
        }

        #endregion
    }

    public class BinaryTree<TNodeType>
        where TNodeType : BinaryTree.INode<TNodeType>
    {
        #region Fields

        protected TNodeType RootNode;

        #endregion

        #region Properties

        public BinaryTree.IFixedNode<TNodeType> Root
        {
            get
            {
                return RootNode;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Equals(RootNode, default(TNodeType));
            }
        }

        #endregion

        #region Methods

        public BinaryTree()
        {
            RootNode = default(TNodeType);
        }

        public BinaryTree(TNodeType root)
        {
            RootNode = root;
        }

        #endregion
    }

    public class BinaryTree<TNodeType, T> : BinaryTree<TNodeType>
        where TNodeType : class, BinaryTree.INode<TNodeType>
    {
        #region Constructors

        public BinaryTree(Comparison<T> comparison)
        {
            Comparison = comparison;
        }

        public BinaryTree(Comparison<T> comparison, TNodeType root)
            : base(root)
        {
            Comparison = comparison;
        }

        #endregion

        #region Properties

        public Comparison<T> Comparison
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        public SearchTree.INode Search(T t)
        {
            return RootNode.Search(t, Comparison);
        }

        public override string ToString()
        {
            try
            {
                var s = RootNode.ToString<TNodeType, T>();
                return s;
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }
        }

        public bool Check(out int count)
        {
            if (RootNode == null)
            {
                count = 0;
                return true;
            }

            if (!RootNode.CheckFromRoot(out count))
            {
                return false;
            }

            var node = RootNode.GetFirstInorder();
            var nodeAsEntry = node as BinaryTree.IEntry<T>;
            System.Diagnostics.Debug.Assert(nodeAsEntry != null);
            var last = nodeAsEntry.Entry;
            node = node.GetNextInorder();

            for (; node != null; )
            {
                var nodeAsEntry2 = node as BinaryTree.IEntry<T>;
                System.Diagnostics.Debug.Assert(nodeAsEntry2 != null);
                if (Comparison(last, nodeAsEntry2.Entry) >= 0)
                {
                    return false;
                }
                node = node.GetNextInorder();
            }

            return true;
        }

        #endregion
    }
}
