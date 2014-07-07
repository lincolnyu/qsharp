#define USE_INTEGRATED

using System;
using System.Text;
using QSharp.Shared;

namespace QSharp.Scheme.Classical.Trees
{
    /// <summary>
    ///  An implementation of Red-black Tree algorithms
    /// </summary>
    /// <references>
    ///  It's created by following the guidelines on wikipedia:
    ///  http://en.wikipedia.org/wiki/Red-black_tree
    /// </references>
    public static class RbTreeWorker
    {
        #region Enumerations

        /// <summary>
        ///  The two colours of the nodes
        /// </summary>
        public enum Color
        {
            Black,
            Red
        }

        #endregion

        #region Nested types

        public interface INode : BinaryTree.INode<INode>
        {
            Color Color { get; set; }
        }

        public interface INode<T> : BinaryTree.INode<INode, T>, INode
        {
        }

        public class DisposableNode<T>
            : BinaryTree.DisposableNode<INode, T>, INode<T>
        {
            public Color Color { get; set; }

            public DisposableNode(T entry)
                : base(entry)
            {
                Color = Color.Red;
            }

            public DisposableNode(Color color)
            {
                Color = color;
            }

            public DisposableNode(Color color, T entry)
                : base(entry)
            {
                Color = color;
            }
        }

        public class NondisposableNode<T>
            : BinaryTree.NondisposableNode<INode, T>, INode<T>
        {
            public Color Color { get; set; }

            public NondisposableNode(T entry)
                : base(entry)
            {
                Color = Color.Red;
            }


            public NondisposableNode(Color color)
            {
                Color = color;
            }

            public NondisposableNode(Color color, T entry)
                : base(entry)
            {
                Color = color;
            }
        }

        #endregion

        #region Methods

        /**
         * <summary>
         *  Pre: treesize is the node in question, it is assumed to be marked red
         * </summary>
         */
        public static void InsertAdjustCase1(INode n, ref INode root)
        {
            if (n.Parent == null)
            {
                n.Color = Color.Black;  /* it's the root */
                root = n;
            }
            else
            {
                InsertAdjustCase2(n, ref root);
            }
        }

        /**
         * <summary>
         *  Pre: treesize is the node in question, it is assumed to be marked red
         *        since this case  can only be called from case 1, treesize.ParentPos != null
         *  
         * </summary>
         */
        public static void InsertAdjustCase2(INode n, ref INode root)
        {
            if (n.Parent.Color == Color.Black)
                return; /* The tree is still valid */

            InsertAdjustCase3(n, ref root);
        }

        /**
         * <summary>
         *  Pre: treesize is the node in question, it is assumed to be marked red
         *       since this case can only be called from case 2, the color 
         *       treesize's parent is red, then it must not be the root, hence treesize 
         *       has grandparent
         * </summary>
         */
        public static void InsertAdjustCase3(INode n, ref INode root)
        {
            var u = n.GetUncle();

            if (u != null && u.Color == Color.Red)
            {
                n.Parent.Color = Color.Black;
                u.Color = Color.Black;
                var g = n.GetGrandparent();
                g.Color = Color.Red;
                InsertAdjustCase1(g, ref root);
            }
            else
            {
                InsertAdjustCase4(n, ref root);
            }
        }

        public static void InsertAdjustIntegrated123(INode n, ref INode root)
        {
            int c = 1;
            while (true)
            {
                switch (c)
                {
                    case 1:
                        if (n.Parent == null)
                        {
                            n.Color = Color.Black;  /* it's the root */
                            root = n;
                            return;
                        }
                        c = 2;
                        break;
                    case 2:
                        if (n.Parent.Color == Color.Black)
                            return; /* The tree is still valid */
                        c = 3;
                        break;
                    case 3:
                        var u = n.GetUncle();
                        if (u != null && u.Color == Color.Red)
                        {
                            n.Parent.Color = Color.Black;
                            u.Color = Color.Black;
                            var g = n.GetGrandparent();
                            g.Color = Color.Red;
                            c = 1;
                            n = g;
                            break;
                        }
                        InsertAdjustCase4(n, ref root);
                        return;
                }
            }
        }

        /**
         * <summary>
         *  Pre: treesize is the node in question, it is assumed to be marked red
         *       since this case can only be called from case 3, 
         *       thereby treesize has grandparent
         * </summary>
         */
        public static void InsertAdjustCase4(INode n, ref INode root)
        {
            var g = n.GetGrandparent();

            if (n == n.Parent.Right && n.Parent == g.Left)
            {
                n.Parent.RotateZag(out n);
                n = n.Left;     // formerly it's treesize's parent
            }
            else if (n == n.Parent.Left && n.Parent == g.Right)
            {
                n.Parent.RotateZig(out n);
                n = n.Right;    // formerly it's treesize's parent
            }

            InsertAdjustCase5(n, ref root);
        }

        /**
         * <summary>
         *  Pre: treesize is the node in question, it is assumed to be marked red
         *       since this case can only be called from case 4, 
         *       thereby treesize has grandparent
         * </summary>
         */
        public static void InsertAdjustCase5(INode n, ref INode root)
        {
            var g = n.GetGrandparent();
            INode p;

            n.Parent.Color = Color.Black;
            g.Color = Color.Red;

            if (n == n.Parent.Left && n.Parent == g.Left)
            {
                g.RotateZig(out p);
            }
            else
            {
                g.RotateZag(out p);
            }
            if (g == root)
            {
                root = p;
            }
        }

        public static void Insert(INode n, ref INode root)
        {
#if USE_INTEGRATED
            InsertAdjustIntegrated123(n, ref root);
#else
            InsertAdjustCase1(n, ref root);
#endif
        }

        public static void DeleteOneChild(INode n, ref INode root)
        {
            var child = n.Right ?? n.Left;

            if (child == null)
            {   // delete treesize itself

                if (n.Color == Color.Black)
                {
#if USE_INTEGRATED
                    DeleteCaseIntegrated123(n, ref root);
#else
                    DeleteCase1(n, ref root);
#endif
                }

                if (n.Parent == null)   // 'n' is a root node
                    root = null;
                else if (n.Parent.Left == n)    // 'n' is the left child of its parent
                    n.Parent.Left = null;
                else // 'n' is the right child
                    n.Parent.Right = null;
            }
            else
            {
                System.Diagnostics.Debug.Assert(
                    n.Color == Color.Black && child.Color == Color.Red);

                child.Parent = n.Parent;
                if (child.Parent.Left == n)
                    child.Parent.Left = child;
                else
                    child.Parent.Right = child;

                child.Color = Color.Black;
/*
                if (treesize.Color == Color.Black)
                {
                    if (child.Color == Color.Red)
                        child.Color = Color.Black;
                    else
                    {
#if USE_INTEGRATED
                        DeleteCaseIntegrated123(treesize, ref root);
#else
                        DeleteCase1(treesize, ref root);
#endif
                    }
                }
 */
            }
        }

        public static void DeleteCase1(INode n, ref INode root)
        {
            if (n.Parent == null)
            {
                root = n;
                return;
            }
            
            DeleteCase2(n, ref root);
        }

        public static void DeleteCase2(INode n, ref INode root)
        {
            var s = n.GetSibling();

            if (s.Color == Color.Red)
            {
                n.Parent.Color = Color.Red;
                s.Color = Color.Black;
                if (n == n.Parent.Left)
                    n.Parent.RotateZag(out s);
                else
                    n.Parent.RotateZig(out s);

                if (s.Parent == null)
                    root = s;
            }
            DeleteCase3(n, ref root);
        }

        public static void DeleteCase3(INode n, ref INode root)
        {
            var s = n.GetSibling();

            if (n.Parent.Color == Color.Black && s.Color == Color.Black
                && (s.Left == null || s.Left.Color == Color.Black)
                && (s.Right == null || s.Right.Color == Color.Black))
            {
                s.Color = Color.Red;
                DeleteCase1(n.Parent, ref root);
            }
            else
            {
                DeleteCase4(n, ref root);
            }
        }

        public static void DeleteCaseIntegrated123(INode n, ref INode root)
        {
            int c = 1;

            while (true)
            {
                switch (c)
                {
                    case 1:
                        if (n.Parent == null)
                        {
                            root = n;
                            return;
                        }
                        c = 2;
                        break;
                    case 2:
                        var s = n.GetSibling();

                        if (s.Color == Color.Red)
                        {
                            n.Parent.Color = Color.Red;
                            s.Color = Color.Black;
                            if (n == n.Parent.Left)
                            {
                                n.Parent.RotateZag(out s);
                            }
                            else
                            {
                                n.Parent.RotateZig(out s);
                            }

                            if (s.Parent == null)
                            {
                                root = s;
                            }
                        }
                        c = 3;
                        break;
                    case 3:
                        s = n.GetSibling();

                        if (n.Parent.Color == Color.Black && s.Color == Color.Black
                            && (s.Left == null || s.Left.Color == Color.Black)
                            && (s.Right == null || s.Right.Color == Color.Black))
                        {
                            s.Color = Color.Red;
                            n = n.Parent;
                            c = 1;
                        }
                        else
                        {
                            DeleteCase4(n, ref root);
                            return;
                        }
                        break;
                }
            }
        }

        public static void DeleteCase4(INode n, ref INode root)
        {
            var s = n.GetSibling();

            if (n.Parent.Color == Color.Red && s.Color == Color.Black
                && (s.Left == null || s.Left.Color == Color.Black) 
                && (s.Right == null || s.Right.Color == Color.Black))
            {
                s.Color = Color.Red;
                n.Parent.Color = Color.Black;
            }
            else
            {
                DeleteCase5(n, ref root);
            }
        }

        public static void DeleteCase5(INode n, ref INode root)
        {
            var s = n.GetSibling();

            if (s.Color == Color.Black)
            {
                INode ss;
                if (n == n.Parent.Left && (s.Right == null || s.Right.Color == Color.Black)
                    && (s.Left != null && s.Left.Color == Color.Red))
                {
                    s.Color = Color.Red;
                    s.Left.Color = Color.Black;
                    s.RotateZig(out ss);
                }
                else if (n == n.Parent.Right && (s.Left == null || s.Left.Color == Color.Black)
                    && (s.Right != null && s.Right.Color == Color.Red))
                {
                    s.Color = Color.Red;
                    s.Right.Color = Color.Black;
                    s.RotateZag(out ss);
                }
            }

            DeleteCase6(n, ref root);
        }

        public static void DeleteCase6(INode n, ref INode root)
        {
            var s = n.GetSibling();

            s.Color = n.Parent.Color;
            n.Parent.Color = Color.Black;

            if (n == n.Parent.Left)
            {
                if (s.Right != null)
                    s.Right.Color = Color.Black;
                n.Parent.RotateZag(out s);
            }
            else
            {
                if (s.Left != null)
                    s.Left.Color = Color.Black;
                n.Parent.RotateZig(out s);
            }

            if (s.Parent == null)
                root = s;
        }

        public static INode CreateDisposableNode<T>(T t) where T : IDisposable
        {
            return new DisposableNode<T>(t);
        }

        public static INode CreateNondisposableNode<T>(T t)
        {
            return new NondisposableNode<T>(t);
        }

        public static INode CreateNode<T>(T t)
        {
            if (t is IDisposable) return new DisposableNode<T>(t);
            return CreateNondisposableNode(t);
        }

        /**
         * <remarks>
         *  This implementation ensures that the node removed from the 
         *  tree is exactly the one referenced by treesize, therefore it's at
         *  the callers' disposal.
         * </remarks>
         */
        public static void Remove(INode n, ref INode root)
        {
            if (n.Left != null)
            {
                n = n.GetPrevInorder();
            }
            else if (n.Right != null)
            {
                n = n.GetNextInorder();
            }

            DeleteOneChild(n, ref root);
        }

        public static string ColorToString(this Color color)
        {
            switch (color)
            {
                case Color.Red: return "R";
                case Color.Black: return "B";
            }
            return "";
        }

        public static string ToString<T>(this INode node)
        {
            var sb = new StringBuilder();
            var inode = node as INode<T>;

            if (inode == null) return "{ Empty Tree }";

            sb.Append("Node( ");
            sb.Append(inode.Entry);
            sb.Append(", ");
            sb.Append(ColorToString(node.Color));

            if (node.Left == null && node.Right == null)
            {
                sb.Append(" )");
                return sb.ToString();
            }

            sb.Append(" ) { ");
            if (node.Left != null)
            {
                if (node.Left.Parent != node)
                    throw new QException("Bad binary-tree");
                sb.Append(ToString<T>(node.Left));
            }
            sb.Append(",");
            if (node.Right != null)
            {
                if (node.Right.Parent != node)
                    throw new QException("Bad binary-tree");
                sb.Append(" ");
                sb.Append(ToString<T>(node.Right));
            }
            sb.Append(" }");
            return sb.ToString();
        }

        public static bool Check(this INode node, out int nblack)
        {
            int nb1 = 0, nb2 = 0;

            nblack = 0;
            if (node.Left != null)
            {
                if (node.Color == Color.Red && node.Left.Color == Color.Red)
                    return false;
                if (!Check(node.Left, out nb1))
                    return false;
            }

            if (node.Right != null)
            {
                if (node.Color == Color.Red && node.Right.Color == Color.Red)
                    return false;
                if (!Check(node.Right, out nb2))
                    return false;
            }

            if (nb1 != nb2)
                return false;

            nblack = nb1;
            if (node.Color == Color.Black)
                nblack++;

            return true;
        }

        public static bool CheckFromRoot(this INode root)
        {
            int nblack;

            if (root == null)
                return true;

            return root.Color != Color.Red && Check(root, out nblack);
        }

        #endregion
    }
}
