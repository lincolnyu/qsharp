namespace QSharp.String.ExpressionEvaluation
{
    public interface IHasChild
    {
        #region Methods

        bool ReplaceChild(Node oldChild, Node newChild);

        void AddChild(Node newChild);

        #endregion
    }
}
