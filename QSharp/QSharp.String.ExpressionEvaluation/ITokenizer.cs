using System.Collections.Generic;

namespace QSharp.String.ExpressionEvaluation
{
    public interface ITokenizer
    {
        #region Properties

        int FunctionPrecedence
        {
            get;
        }

        #endregion

        #region Methods

        int GetBinaryOperatorPrecedence(string content);

        IEnumerable<Token> Tokenize(string expression);

        #endregion
    }
}
