using System;

namespace QSharp.Scheme.Classical.Trees
{
    public class AvlTree<T> : BinaryTree<AvlTreeWorker.INode, T>, ISearchTree<T>
    {
        #region Constructors

        public AvlTree(Comparison<T> comparison) : base(comparison)
        {
        }

        #endregion

        #region Methods

        public SearchTree.INode Insert(T t)
        {
            if (RootNode == null)
            {
                RootNode = AvlTreeWorker.CreateNode(t);
                return RootNode;
            }

            var s = Search(t);
            var at = s as AvlTreeWorker.INode;
            var att = at as AvlTreeWorker.INode<T>;
            System.Diagnostics.Debug.Assert(att != null);
            var cmp = Comparison(att.Entry, t);

            if (cmp == 0)
            {
                return null;
            }

            var addend = AvlTreeWorker.CreateNode(t);

            if (cmp < 0)
            {
                AvlTreeWorker.AppendRight(at, addend, ref RootNode);
            }
            else /*cmp > 0*/
            {
                AvlTreeWorker.AppendLeft(at, addend, ref RootNode);
            }

            return addend;
        }

        public void Remove(SearchTree.INode h)
        {
            var node = h as AvlTreeWorker.INode;
            /*
            if (node == RootNode && RootNode.Left == null && RootNode.Right == null)
            {
                RootNode = null;
                return;
            }*/

            AvlTreeWorker.Remove(node, ref RootNode);
        }

        public override string ToString()
        {
            return AvlTreeWorker.ToString<T>(RootNode);
        }

        public new bool Check(out int count)
        {
            if (!base.Check(out count))
                return false;

            int d;
            return AvlTreeWorker.Check(RootNode, out d);
        }

        #endregion
    }
}
