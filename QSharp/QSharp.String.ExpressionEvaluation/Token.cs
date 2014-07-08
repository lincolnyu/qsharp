namespace QSharp.String.ExpressionEvaluation
{
    public class Token
    {
        #region Enumerations

        public enum Type
        {
            None,
            LeftParenthesis,
            RightParenthesis,
            Constant,   // numeric value or string
            Symbol,     // variable name, funciton name or array name
            Comma,      // for function
            Operator,
        }

        #endregion

        #region Propertiews

        public Type TokenType { get; set; }

        public string Content { get; set; }

        #endregion
    }
}
