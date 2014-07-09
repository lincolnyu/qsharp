using System.Collections.Generic;

namespace QSharp.String.ExpressionEvaluation
{
    public class Fork : Node, IHasChild
    {
        #region Constructors

        public Fork()
        {
            Children = new List<Node>();
        }

        #endregion

        #region Properties

        public IList<Node> Children { get; private set; }

        #endregion

        #region Methods

        public bool ReplaceChild(Node oldChild, Node newChild)
        {
            var i = 0;
            for (; i < Children.Count; i++)
            {
                var child = Children[i];
                if (child == oldChild)
                {
                    break;
                }
            }
            if (i < Children.Count)
            {
                Children[i] = newChild;
                return true;
            }
            return false;
        }

        #endregion
    }
}
