using System;
using System.Collections.Generic;

namespace QSharp.String.ExpressionEvaluation
{
    public class SyntaxTree
    {
        public void Parse(string expression)
        {
            var tokens = Tokenize(expression);
            Parse(tokens);
        }

        private IList<Token> Tokenize(string expression)
        {
            throw new NotImplementedException();
        }

        private void Parse(IList<Token> tokens)
        {
            var lastWasHead = false;
            var leftParentheses = 0;
            Node lastNode = null;
            PlaceHolder lastPlaceHolder;
            var lastTokenType = Token.Type.None;
            var lastOpOrder = -1;
            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                switch (token.TokenType)
                {
                    case Token.Type.LeftParenthesis:
                        if (lastTokenType == Token.Type.Symbol)
                        {
                            // function
                        }
                        else
                        {
                            // charge leaf placeholder
                        }
                        break;
                    case Token.Type.RightParenthesis:
                        break;
                    case Token.Type.Symbol:

                        break;
                    case Token.Type.Constant:
                        // add it to the current cell
                        break;
                    case Token.Type.Operator:
                        if (lastTokenType == Token.Type.None ||
                            lastTokenType == Token.Type.LeftParenthesis ||
                            lastTokenType == Token.Type.Comma || 
                            lastTokenType == Token.Type.Operator)
                        {
                            // unary operator: it's guaranteed in an empty cell
                            // TODO if operator, check tokens before last

                            // create a operator node in the cell and create an acceptor under it
                        }
                        else
                        {
                            // binary operator: from closed node up find one whose priority is lower or is a cell
                            // shift its content left, take its place, retain the node's place cell property
                            // and create an acceptor on the right
                        }

                        break;
                }

                lastTokenType = token.TokenType;

            }
        }

        private void RotateLeft(Node original, Node newNode)
        {
            var origAsPH = original as PlaceHolder;
            if (origAsPH != null)
            {

                return;
            }
        }
    }
}
