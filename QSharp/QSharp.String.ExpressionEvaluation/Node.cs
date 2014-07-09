namespace QSharp.String.ExpressionEvaluation
{
    public abstract class Node
    {
        #region Enumerations

        public enum Type
        {
            Constant,
            Symbol,
            Function,
            BinaryOperator,
            UnaryOperator,
        }

        #endregion

        #region Properties

        public Node Parent { get; set; }

        public Type NodeType { get; set; }

        public string Content { get; set; }

        #endregion
    }
}
