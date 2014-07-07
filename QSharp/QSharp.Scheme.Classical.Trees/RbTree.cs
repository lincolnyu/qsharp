using System;
using QSharp.Shared;

namespace QSharp.Scheme.Classical.Trees
{
    public class RbTree<T> : BinaryTree<RbTreeWorker.INode, T>, ISearchTree<T>
    {
        #region Constructors

        public RbTree(Comparison<T> comparison)
            : base(comparison)
        {
        }

        #endregion

        #region Methods

        #region object members

        public override string ToString()
        {
            return RootNode.ToString<T>();
        }

        #endregion

        #region ISearchTree<T> members

        public SearchTree.INode Insert(T t)
        {
            if (RootNode == null)
            {
                RootNode = RbTreeWorker.CreateNode(t);
                RbTreeWorker.InsertAdjustIntegrated123(RootNode, ref RootNode);
                return RootNode;
            }

            var s = Search(t);

            var at = s as RbTreeWorker.INode;
            var att = at as RbTreeWorker.INode<T>;
            if (att == null)
            {
                throw new QException("The insertion point cannot be found or is invalid");
            }

            var cmp = Comparison(att.Entry, t);

            if (cmp == 0)
            {
                return null; // already exists
            }

            var addend = RbTreeWorker.CreateNode(t);

            if (cmp < 0)
            {
                at.Right = addend;
                addend.Parent = at;
            }
            else /* cmp > 0 */
            {
                at.Left = addend;
                addend.Parent = at;
            }

            RbTreeWorker.InsertAdjustIntegrated123(addend, ref RootNode);

            return addend as SearchTree.INode;
        }

        public void Remove(SearchTree.INode h)
        {
            var node = h as RbTreeWorker.INode;

            if (node == RootNode && RootNode != null && RootNode.Left == null && RootNode.Right == null)
            {
                RootNode = null;
                return;
            }

            RbTreeWorker.Remove(node, ref RootNode);
        }

        public new bool Check(out int count)
        {
            return base.Check(out count) && RootNode.CheckFromRoot();
        }

        #endregion

        #endregion
    }
}
