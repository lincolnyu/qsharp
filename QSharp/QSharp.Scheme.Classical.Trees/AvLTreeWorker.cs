#define USE_SIMPLE_LOGIC
//#define USE_ITERATOR

#if USE_SIMPLE_LOGIC
#undef USE_ITERATOR
#endif

using System;
using System.Text;
using QSharp.Shared;

namespace QSharp.Scheme.Classical.Trees
{
    /// <summary>
    ///  An implementation of AVL Tree algorithms
    ///  based on the general knowledge of this sort of binary search tree
    /// </summary>
    public static class AvlTreeWorker
    {
        #region Enumerations

        public enum Skew
        {
            None,
            Left,
            Right
        }

        #endregion

        #region Nested types

        public interface INode : BinaryTree.INode<INode>
        {
            Skew Skew { get; set; }
        }

        public interface INode<T> : BinaryTree.INode<INode, T>, INode
        {
        }

        public class DisposableNode<T>
            : BinaryTree.DisposableNode<INode, T>, INode<T>
        {
            public Skew Skew { get; set; }

            public DisposableNode()
            {
                Skew = Skew.None;
            }

            public DisposableNode(T entry)
                : base(entry)
            {
                Skew = Skew.None;
            }
        }

        public class NondisposableNode<T>
            : BinaryTree.NondisposableNode<INode, T>, INode<T>
        {
            public Skew Skew { get; set; }

            public NondisposableNode()
            {
                Skew = Skew.None;
            }

            public NondisposableNode(T entry)
                : base(entry)
            {
                Skew = Skew.None;
            }
        }

        public interface INodeIterator : BinaryTree.INodeIterator<INodeIterator, INode>
        {
            Skew Skew { get; set; }
            Skew SkewLeft { get; }
            Skew SkewRight { get; }
        }

        public class ForwardIterator : BinaryTree.ForwardIterator<INodeIterator, INode>, 
            INodeIterator
        {
            public Skew Skew
            {
                get { return UnderlyingNode.Skew; }
                set { UnderlyingNode.Skew = value; }
            }

            public Skew SkewLeft { get { return Skew.Left; } }
            public Skew SkewRight { get { return Skew.Right; } }

            public override INodeIterator Create(INode node)
            {
                return new ForwardIterator(node);
            }

            public ForwardIterator(INode node)
                : base (node)
            {
            }
        }

        public class ReverseIterator : BinaryTree.ReverseIterator<INodeIterator, INode>,
            INodeIterator
        {
            public Skew Skew
            {
                get { return UnderlyingNode.Skew; }
                set { UnderlyingNode.Skew = value; }
            }

            public Skew SkewLeft { get { return Skew.Right; } }
            public Skew SkewRight { get { return Skew.Left; } }

            public override INodeIterator Create(INode node)
            {
                return new ReverseIterator(node);
            }

            public ReverseIterator(INode node)
                : base (node)
            {
            }
        }

        #endregion

        #region Methods

        /**
         * <summary>
         *  Appending, case 1:
         *           A                   B
         *         /   \               /   \
         *        B     N     ->     N+1    A   
         *       / \                       / \
         *    [N+1] N                     N   N
         *     
         *   The skew of the root of A (initially) unchanged (N+2)
         * </summary>
         */
        public static void BalanceA1L(INode a, out INode b)
        {
            a.RotateZig(out b);

            a.Skew = Skew.None;
            b.Skew = Skew.None;
        }

        public static void BalanceA1R(INode a, out INode b)
        {
            a.RotateZag(out b);

            a.Skew = Skew.None;
            b.Skew = Skew.None;
        }

        /**
         * <summary>
         *  Appending, case 2:
         *           A                    C
         *         /   \               /    \
         *        B     N     ->      B      A   
         *       / \                 / \    / \
         *      N   C               N  c1  c2  N
         *         / \
         *        c1 c2
         *   
         *   either c1 or c2 is N, but not both are, and the other is N-1
         * 
         *   The skew of the root of A (initially) unchanged (N+2)
         * </summary>
         */
        public static void BalanceA2L(INode a, out INode c)
        {
            INode b;

            a.RotateZigZag(out b, out c);

            if (c.Skew == Skew.Left)
            {
                a.Skew = Skew.Right;
                b.Skew = Skew.None;
            }
            else if (c.Skew == Skew.Right)
            {
                a.Skew = Skew.None;
                b.Skew = Skew.Left;
            }
            else
            {   // c1, c2 must be null, thus N = 0
                a.Skew = b.Skew = Skew.None;
            }

            c.Skew = Skew.None;
        }

        public static void BalanceA2R(INode a, out INode c)
        {
            INode b;

            a.RotateZagZig(out b, out c);

            if (c.Skew == Skew.Right)
            {
                a.Skew = Skew.Left;
                b.Skew = Skew.None;
            }
            else if (c.Skew == Skew.Left)
            {
                a.Skew = Skew.None;
                b.Skew = Skew.Right;
            }
            else
            {   // c1, c2 must be null, thus N = 0
                a.Skew = b.Skew = Skew.None;
            }

            c.Skew = Skew.None;
        }

        /**
         * <summary>
         *  Removing, case 1:
         *           A                   B
         *         /   \               /   \
         *        B   [N-1]    ->     N     A   
         *       / \                       / \
         *      N   b2                    b2  N-1
         *      
         *   b2 = N/N-1
         *     
         * The skew of the root of A (initially) :
         *   1. unchanged (N+2)
         *   2. changed from N+2 to N+1, equilibrious after rotation
         * 
         * It is concluded that the subtree is shortened only when 
         * the root b is equilibrious
         * 
         * </summary>
         */

        public static void BalanceR1L(INode a, out INode b)
        {
            a.RotateZig(out b);

            if (b.Skew == Skew.Left)
            {
                a.Skew = Skew.None;
                b.Skew = Skew.None;
            }
            else    //b.Skew == Skew.None
            {
                a.Skew = Skew.Left;
                b.Skew = Skew.Right;
            }
        }

        public static void BalanceR1R(INode a, out INode b)
        {
            a.RotateZag(out b);

            if (b.Skew == Skew.Right)
            {
                a.Skew = Skew.None;
                b.Skew = Skew.None;
            }
            else    //b.Skew == Skew.None
            {
                a.Skew = Skew.Right;
                b.Skew = Skew.Left;
            }
        }

        /**
         * <summary>
         *  Removing, case 2:
         *           A                   C
         *         /   \               /   \
         *        B   [N-1]    ->     B     A   
         *       / \                 / \   / \
         *     N-1  C(N)           N-1 c1 c2  N-1
         *         / \
         *        c1 c2
         * 
         *   The skew of the root of A (initially) CHANGED from N+2 to N+1
         *   Equilibrious after rotation
         * </summary>
         */
        public static void BalanceR2L(INode a, out INode c)
        {
            INode b;

            a.RotateZigZag(out b, out c);

            if (c.Skew == Skew.Left)
            {   // N-1, N-2
                b.Skew = Skew.None;
                a.Skew = Skew.Right;
            }
            else if (c.Skew == Skew.Right)
            {   // N-2, N-1
                a.Skew = Skew.None;
                b.Skew = Skew.Left;
            }
            else
            {   // N-1, N-1
                a.Skew = Skew.None;
                b.Skew = Skew.None;
            }

            c.Skew = Skew.None;
        }

        public static void BalanceR2R(INode a, out INode c)
        {
            INode b;

            a.RotateZagZig(out b, out c);

            if (c.Skew == Skew.Left)
            {   // N-1, N-2
                a.Skew = Skew.None;
                b.Skew = Skew.Right;
            }
            else if (c.Skew == Skew.Right)
            {   // N-2, N-1
                a.Skew = Skew.Left;
                b.Skew = Skew.None;
            }
            else
            {   // N-1, N-1
                a.Skew = Skew.None;
                b.Skew = Skew.None;
            }

            c.Skew = Skew.None;
        }

        public static void AppendAdjust(INode current, ref INode root)
        {
            INode parent = current.Parent;

            while (parent != null)
            {
                /**
                 * <remarks>
                 *  condition for execution:
                 *    subtree with `current' as root heightens, thus Skew value 
                 *    for `parent' becomes outdated
                 *    it is certain that `current' is SKEWED
                 *  current
                 * </remarks>
                 */

                INode np;
                if (current == parent.Left)
                {
                    if (parent.Skew == Skew.Left)
                    {
                        if (current.Skew == Skew.Left)
                        {
                            BalanceA1L(parent, out np);
                            if (parent == root) root = np;
                            break;
                        }

                        if (current.Skew == Skew.Right)
                        {
                            BalanceA2L(parent, out np);
                            if (parent == root) root = np;
                            break;
                        }

                        throw new QException("Bad binary-tree");
                    }

                    if (parent.Skew == Skew.Right)
                    {
                        parent.Skew = Skew.None;
                        break;
                    }

                    // parent.Skew == Skew.None
                    parent.Skew = Skew.Left;    // subtree from `parent' increases
                }
                else
                {   // current == parent.Right
                    if (parent.Skew == Skew.Right)
                    {
                        if (current.Skew == Skew.Right)
                        {
                            BalanceA1R(parent, out np);
                            if (parent == root) root = np;
                            break;
                        }

                        if (current.Skew == Skew.Left)
                        {
                            BalanceA2R(parent, out np);
                            if (parent == root) root = np;
                            break;
                        }

                        throw new QException("Bad binary-tree");
                    }

                    if (parent.Skew == Skew.Left)
                    {
                        parent.Skew = Skew.None;
                        break;
                    }

                    // parent.Skew == Skew.None
                    parent.Skew = Skew.Right;    // subtree from `parent' increases
                }

                current = parent;
                parent = parent.Parent;
            }
        }

        public static void RemoveAdjust(INode parent, bool onleft, ref INode root)
        {
            while (parent != null)
            {
                /**
                 * <remarks>
                 *  condition for execution:
                 *    subtree with `current' as root shortens, thus Skew value 
                 *    for `parent' becomes outdated
                 *    it is certain that `current' is equilibrious or null
                 *  
                 *  current is not specified at first, however its relation
                 *  to the parent is provided by the onleft parameter
                 * </remarks>
                 */

                INode current;
                if (onleft)
                {
                    if (parent.Skew == Skew.Right)
                    {
                        if (parent.Right.Skew == Skew.Left)
                        {   // case 2
                            BalanceR2R(parent, out current);
                            if (root == parent)
                            {
                                root = current;
                                break;
                            }
                        }
                        else
                        {
                            BalanceR1R(parent, out current);
                            if (root == parent)
                            {
                                root = current;
                                break;
                            }
                            if (current.Skew != Skew.None)
                                break;
                        }
                    }
                    else if (parent.Skew == Skew.Left)
                    {
                        parent.Skew = Skew.None;
                        current = parent;
                    }
                    else
                    {
                        parent.Skew = Skew.Right;
                        break;
                    }
                }
                else    // current == parent.Right
                {
                    if (parent.Skew == Skew.Left)
                    {
                        if (parent.Left.Skew == Skew.Right)
                        {   // case 2
                            BalanceR2L(parent, out current);
                            if (root == parent)
                            {
                                root = current;
                                break;
                            }
                        }
                        else
                        {
                            BalanceR1L(parent, out current);
                            if (root == parent)
                            {
                                root = current;
                                break;
                            }
                            if (current.Skew != Skew.None)
                                break;
                        }
                    }
                    else if (parent.Skew == Skew.Right)
                    {
                        parent.Skew = Skew.None;
                        current = parent;
                    }
                    else
                    {
                        parent.Skew = Skew.Left;
                        break;
                    }
                }

                parent = current.Parent;
                if (parent == null) break;
                onleft = current == parent.Left;
            }
        }

        public static void AppendLeft(INode twig, INode addend, ref INode root)
        {
            twig.Left = addend;
            addend.Parent = twig;

            if (twig.Right != null)
            {
                twig.Skew = Skew.None;
                return;
            }

            twig.Skew = Skew.Left;

            AppendAdjust(twig, ref root);
        }

        public static void AppendRight(INode twig, INode addend, ref INode root)
        {
            twig.Right = addend;
            addend.Parent = twig;

            if (twig.Left != null)
            {
                twig.Skew = Skew.None;
                return;
            }

            twig.Skew = Skew.Right;

            AppendAdjust(twig, ref root);
        }

#if USE_SIMPLE_LOGIC
        public static void RemoveNeighbor(INode n, INode nb, ref INode root)
        {
            INode child = nb.Left ?? nb.Right;

            if (child == null)
            {
                // nb takes the place of treesize

                INode parent = nb.Parent;
                bool nbIsLeft = parent.Left == nb;
                if (parent == n)
                {
                    nb.Parent = n.Parent;
                    if (nb.Parent == null)
                        root = nb;
                    else if (nb.Parent.Left == n)
                        nb.Parent.Left = nb;
                    else
                        nb.Parent.Right = nb;

                    if (nbIsLeft)
                    {
                        nb.Right = n.Right;
                        if (nb.Right != null)
                            nb.Right.Parent = nb;
                    }
                    else
                    {
                        nb.Left = n.Left;
                        if (nb.Left != null)
                            nb.Left.Parent = nb;
                    }

                    nb.Skew = n.Skew;
                    RemoveAdjust(nb, nbIsLeft, ref root);
                }
                else
                {
                    if (nbIsLeft)
                        parent.Left = null;
                    else
                        parent.Right = null;

                    nb.Parent = n.Parent;
                    if (nb.Parent == null)
                        root = nb;
                    else if (nb.Parent.Left == n)
                        nb.Parent.Left = nb;
                    else
                        nb.Parent.Right = nb;

                    nb.Left = n.Left;
                    if (nb.Left != null)
                        nb.Left.Parent = nb;
                    nb.Right = n.Right;
                    if (nb.Right != null)
                        nb.Right.Parent = nb;
                    nb.Skew = n.Skew;

                    RemoveAdjust(parent, nbIsLeft, ref root);
                }
            }
            else
            {
                // child takes the place of nb
                bool nbIsLeft = nb.Parent.Left == nb;

                child.Parent = nb.Parent;
                if (child.Parent.Left == nb)
                    child.Parent.Left = child;
                else
                    child.Parent.Right = child;

               // nb takes the place of treesize
                nb.Parent = n.Parent;
                if (nb.Parent == null)
                    root = nb;
                else if (nb.Parent.Left == n)
                    nb.Parent.Left = nb;
                else
                    nb.Parent.Right = nb;

                nb.Left = n.Left;
                if (nb.Left != null)
                    nb.Left.Parent = nb;
                nb.Right = n.Right;
                if (nb.Right != null)
                    nb.Right.Parent = nb;
                nb.Skew = n.Skew;

                RemoveAdjust(child.Parent, nbIsLeft, ref root);
            }
        }
#endif  // USE_SIMPLE_LOGIC

#if !USE_SIMPLE_LOGIC
        public static void RemoveNeighbor(INodeIterator n, INodeIterator nb, ref INode root)
        {
            INodeIterator left = nb.Left;
            if (left != null)
            {   /* remove left leaf of nb */
                if (nb.Parent.UnderlyingNode != n.UnderlyingNode)
                {   /* in most cases... */
                    left.Parent = nb.Parent;
                    nb.Parent.Right = left;

                    nb.Left = n.Left;
                    n.Left.Parent = nb;
                }

                nb.Right = n.Right;
                if (nb.Right != null)
                    nb.Right.Parent = nb;

                nb.Skew = n.Skew;
                nb.Parent = n.Parent;
                if (root == n.UnderlyingNode)
                    root = nb.UnderlyingNode;
                else if (nb.Parent.Left.UnderlyingNode == n.UnderlyingNode)
                    nb.Parent.Left = nb;
                else
                    nb.Parent.Right = nb;

                RemoveAdjust(left.Parent.UnderlyingNode, 
                    left.Parent.UnderlyingNode.Left == left.UnderlyingNode, ref root);
            }
            else
            {   /* remove nb itself */
                INodeIterator parent = nb.Parent;
                if (parent.UnderlyingNode == n.UnderlyingNode)
                {   // parent.Left == nb

                    nb.Right = n.Right;
                    if (nb.Right != null)
                    {
                        nb.Right.Parent = nb;
                        nb.Skew = nb.SkewRight;
                    }
                    else
                        nb.Skew = Skew.None;

                    nb.Parent = n.Parent;
                    if (root == n.UnderlyingNode)
                        root = nb.UnderlyingNode;
                    else if (nb.Parent.Left.UnderlyingNode == n.UnderlyingNode)
                        nb.Parent.Left = nb;
                    else
                        nb.Parent.Right = nb;

                    if (root != nb.UnderlyingNode && nb.Skew == Skew.None)
                    {
                        RemoveAdjust(nb.Parent.UnderlyingNode, 
                            nb.UnderlyingNode == nb.Parent.UnderlyingNode.Left, ref root);
                    }
                }
                else
                {   // parent.Right == nb
                    parent.Right = null;

                    nb.Left = n.Left;
                    if ((object)nb.Left != null)
                        nb.Left.Parent = nb;
                    nb.Right = n.Right;
                    if ((object)nb.Right != null)
                        nb.Right.Parent = nb;

                    nb.Skew = n.Skew;
                    nb.Parent = n.Parent;
                    if (root == n.UnderlyingNode)
                        root = nb.UnderlyingNode;
                    else if (nb.Parent.Left.UnderlyingNode == n.UnderlyingNode)
                        nb.Parent.Left = nb;
                    else
                        nb.Parent.Right = nb;

                    if (parent.Skew == parent.SkewLeft)
                    {
                        RemoveAdjust(parent.UnderlyingNode, 
                            parent.SkewLeft == Skew.Right, ref root);
                    }
                    else if (parent.Skew == parent.SkewRight)
                    {
                        parent.Skew = Skew.None;
                        if ((object)parent.Parent != null)
                        {
                            RemoveAdjust(parent.Parent.UnderlyingNode,
                                parent.UnderlyingNode == parent.Parent.UnderlyingNode.Left, 
                                ref root);
                        }
                    }
                    else
                    {
                        parent.Skew = parent.SkewLeft;
                    }
                }
            }
        }
#endif  // if !USE_SIMPLE_LOGIC

#if !USE_SIMPLE_LOGIC
        /*
         * <summary>
         *  n != neighbor
         * </summary>
         */
        public static void RemoveLeftNeighbor(INode n, INode nb, ref INode root)
        {
#if USE_ITERATOR
            ForwardIterator fn = new ForwardIterator(n);
            ForwardIterator fnb = new ForwardIterator(nb);
            RemoveNeighbor(fn, fnb, ref root);
#else
            INode left = nb.Left;
            if (left != null)
            {   /* remove left leaf of nb */
                if (nb.Parent != n)
                {   /* in most cases... */
                    left.Parent = nb.Parent;
                    nb.Parent.Right = left;

                    nb.Left = n.Left;
                    n.Left.Parent = nb;
                }

                nb.Right = n.Right;
                if (nb.Right != null)
                    nb.Right.Parent = nb;

                nb.Skew = n.Skew;
                nb.Parent = n.Parent;
                if (root == n)
                    root = nb;
                else if (nb.Parent.Left == n)
                    nb.Parent.Left = nb;
                else
                    nb.Parent.Right = nb;

                RemoveAdjust(left.Parent, left.Parent == nb, ref root);
            }
            else
            {   /* remove nb itself */
                INode parent = nb.Parent;
                if (parent == n)
                {   // parent.Left == nb

                    nb.Right = n.Right;
                    if (nb.Right != null)
                    {
                        nb.Right.Parent = nb;
                        nb.Skew = Skew.Right;
                    }
                    else
                        nb.Skew = Skew.None;

                    nb.Parent = n.Parent;
                    if (root == n)
                        root = nb;
                    else if (nb.Parent.Left == n)
                        nb.Parent.Left = nb;
                    else
                        nb.Parent.Right = nb;

                    if (root != nb && nb.Skew == Skew.None)
                    {
                        RemoveAdjust(nb.Parent, nb == nb.Parent.Left, ref root);
                    }
                }
                else
                {   // parent.Right == nb
                    parent.Right = null;

                    nb.Left = n.Left;
                    if (nb.Left != null) 
                        nb.Left.Parent = nb;
                    nb.Right = n.Right;
                    if (nb.Right != null)
                        nb.Right.Parent = nb;

                    nb.Skew = n.Skew;
                    nb.Parent = n.Parent;
                    if (root == n)
                        root = nb;
                    else if (nb.Parent.Left == n)
                        nb.Parent.Left = nb;
                    else
                        nb.Parent.Right = nb;

                    if (parent.Skew == Skew.Left)
                    {
                        RemoveAdjust(parent, false, ref root);
                    }
                    else if (parent.Skew == Skew.Right)
                    {
                        parent.Skew = Skew.None;
                        if (parent.Parent != null)
                        {
                            RemoveAdjust(parent.Parent, parent.Parent.Left == parent, ref root);
                        }
                    }
                    else
                    {
                        parent.Skew = Skew.Left;
                    }
                }
            }
#endif  // if !USE_ITERATOR
        }
#endif  // if !USE_SIMPLE_LOGIC

#if !USE_SIMPLE_LOGIC
        public static void RemoveRightNeighbor(INode n, INode nb, ref INode root)
        {
#if USE_ITERATOR
            ReverseIterator fn = new ReverseIterator(n);
            ReverseIterator fnb = new ReverseIterator(nb);
            RemoveNeighbor(fn, fnb, ref root);
#else
            INode right = nb.Right;
            if (right != null)
            {   /* remove right leaf of nb */
                if (nb.Parent != n)
                {   /* in most cases... */
                    right.Parent = nb.Parent;
                    nb.Parent.Left = right;

                    nb.Right = n.Right;
                    n.Right.Parent = nb;
                }

                nb.Left = n.Left;
                if (nb.Left != null)
                    nb.Left.Parent = nb;

                nb.Skew = n.Skew;
                nb.Parent = n.Parent;
                if (root == n)
                    root = nb;
                else if (nb.Parent.Right == n)
                    nb.Parent.Right = nb;
                else
                    nb.Parent.Left = nb;

                RemoveAdjust(right.Parent, right.Parent != nb, ref root);
            }
            else
            {   /* remove nb itself */
                INode parent = nb.Parent;
                if (parent == n)
                {   // parent.Right == nb

                    nb.Left = n.Left;
                    if (nb.Left != null)
                    {
                        nb.Left.Parent = nb;
                        nb.Skew = Skew.Left;
                    }
                    else
                        nb.Skew = Skew.None;

                    nb.Parent = n.Parent;
                    if (root == n)
                        root = nb;
                    else if (n.Parent.Right == n)
                        nb.Parent.Right = nb;
                    else
                        nb.Parent.Left = nb;

                    if (root != nb && nb.Skew == Skew.None)
                    {
                        RemoveAdjust(nb.Parent, nb == nb.Parent.Left, ref root);
                    }
                }
                else
                {   // parent.Left == nb
                    parent.Left = null;

                    nb.Right = n.Right;
                    if (nb.Right != null)
                        nb.Right.Parent = nb;
                    nb.Left = n.Left;
                    if (nb.Left != null)
                        nb.Left.Parent = nb;

                    nb.Skew = n.Skew;
                    nb.Parent = n.Parent;
                    if (root == n)
                        root = nb;
                    else if (nb.Parent.Right == n)
                        nb.Parent.Right = nb;
                    else
                        nb.Parent.Left = nb;

                    if (parent.Skew == Skew.Right)
                    {
                        RemoveAdjust(parent, true, ref root);
                    }
                    else if (parent.Skew == Skew.Left)
                    {
                        parent.Skew = Skew.None;
                        if (parent.Parent != null)
                        {
                            RemoveAdjust(parent.Parent, parent.Parent.Left == parent, ref root);
                        }
                    }
                    else
                    {
                        parent.Skew = Skew.Right;
                    }
                }
            }
#endif  // if !USE_ITERATOR
        }
#endif  // if !USE_SIMPLE_LOGIC

        public static void RemoveLeaf(INode n, ref INode root)
        {
            if (root == n)
            {
                root = null;
                return;
            }

            if (n == n.Parent.Left)
            {
                n.Parent.Left = null;
                if (n.Parent.Right == null)
                {
                    n.Parent.Skew = Skew.None;
                    var gp = n.Parent.Parent;
                    if (gp != null)
                        RemoveAdjust(gp, gp.Left == n.Parent, ref root);
                }
                else if (n.Parent.Skew == Skew.Right)
                {
                    RemoveAdjust(n.Parent, true, ref root);
                }
                else    /* treesize.ParentPos.Skew == Skew.None */
                {
                    n.Parent.Skew = Skew.Right;
                }
            }
            else
            {
                n.Parent.Right = null;
                if (n.Parent.Left == null)
                {
                    n.Parent.Skew = Skew.None;
                    INode gp = n.Parent.Parent;
                    if (gp != null)
                        RemoveAdjust(gp, gp.Left == n.Parent, ref root);
                }
                else if (n.Parent.Skew == Skew.Left)
                {
                    RemoveAdjust(n.Parent, false, ref root);
                }
                else    /* treesize.ParentPos.Skew == Skew.None */
                {
                    n.Parent.Skew = Skew.Left;
                }
            }
        }

        /**
         * <summary>
         *  Pre: the node must not be the root
         * </summary>
         * 
         * <remarks>
         *  This implementation ensures that the node removed from the 
         *  tree is exactly the one referenced by treesize, therefore it's at
         *  the callers' disposal.
         * </remarks>
         */
        public static void Remove(INode n, ref INode root)
        {
            INode neighbor;

            if (n.Skew == Skew.Left)
            {
                neighbor = n.GetPrevInorder();
#if USE_SIMPLE_LOGIC
                RemoveNeighbor(n, neighbor, ref root);
#else
                RemoveLeftNeighbor(n, neighbor, ref root);
#endif
            }
            else
            {
                if (n.Right != null)
                {
                    neighbor = n.GetNextInorder();
#if USE_SIMPLE_LOGIC
                    RemoveNeighbor(n, neighbor, ref root);
#else
                    RemoveRightNeighbor(n, neighbor, ref root);
#endif
                }
                else
                {   // remove treesize itself
                    RemoveLeaf(n, ref root);
                }
            }
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

        public static string SkewToString(Skew skew)
        {
            switch (skew)
            {
                case Skew.Left: return ">";
                case Skew.Right: return "<";
                case Skew.None: return "=";
            }
            return "";
        }

        public static string ToString<T>(INode node)
        {
            var sb = new StringBuilder();
            var inode = node as INode<T>;

            if (node == null) return "{ Empty Tree }";
            System.Diagnostics.Debug.Assert(inode != null);

            sb.Append("Node( ");
            sb.Append(inode.Entry);
            sb.Append(", ");
            sb.Append(SkewToString(node.Skew));

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

        public static bool Check(INode node, out int depth)
        {
            bool b1, b2;
            int d1, d2;

            if (node == null)
            {
                depth = 0;
                return true;
            }

            if (node.Left != null)
            {
                b1 = Check(node.Left, out d1);
            }
            else
            {
                b1 = true;
                d1 = 0;
            }

            if (node.Right != null)
            {
                b2 = Check(node.Right, out d2);
            }
            else
            {
                b2 = true;
                d2 = 0;
            }

            var b = b1 && b2;
            if (Math.Abs(d1 - d2) > 1)
                b = false;

            if (d1 == d2 && node.Skew != Skew.None) b = false;
            if (d1 > d2 && node.Skew != Skew.Left) b = false;
            if (d1 < d2 && node.Skew != Skew.Right) b = false;

            depth = Math.Max(d1, d2) + 1;

            return b;
        }

        #endregion
    }
}
