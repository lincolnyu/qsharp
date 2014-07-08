using System.Collections.Generic;

namespace QSharp.String.ExpressionEvaluation
{
    public class PlaceHolder : Node
    {
        #region Enumerations

        public enum Type
        {
            NodeAccepter,
            Parentheses,
            Tuple,             // indefinite number of children
            Closed,
        }

        #endregion

        #region Properties

        public int LeftParenthesisCount { get; set; }

        public int RightParenthesisCount { get; set; }

        public Type PlaceHolderType { get; set; }

        public LinkedList<Node> Children { get; set; }

        #endregion
    }
}
