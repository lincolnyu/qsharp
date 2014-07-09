using System;
using System.Collections.Generic;
using System.Text;

namespace QSharp.String.ExpressionEvaluation
{
    public class SyntaxTree
    {
        #region Properties

        public Node Root { get; private set; }

        #endregion

        #region Methods

        #region object members

        /// <summary>
        ///  
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        ///  Format: {nodetype;content;{subnode1-info}{subnode2-info}...}
        /// </remarks>
        public override string ToString()
        {
            var sb = new StringBuilder();
            NodeToString(Root, sb);
            return sb.ToString();
        }

        #endregion

        private void NodeToString(Node node, StringBuilder sb)
        {
            sb.Append('{');
            string nt;
            switch (node.NodeType)
            {
                case Node.Type.BinaryOperator:
                    nt = "B";
                    break;
                case Node.Type.UnaryOperator:
                    nt = "U";
                    break;
                case Node.Type.Constant:
                    nt = "C";
                    break;
                case Node.Type.Symbol:
                    nt = "S";
                    break;
                case Node.Type.Function:
                    nt = "F";
                    break;
                default:
                    throw new Exception("Unknown node type");
            }
            sb.Append(nt);
            sb.Append(';');
            if (node.Content.EndsWith("@"))
            {
                // a string
                foreach (var c in node.Content)
                {
                    if (c == ';')
                    {
                        sb.Append('\\');
                    }
                    sb.Append(c);
                }
            }
            else
            {
                // a number or a symbol
                sb.Append(node.Content);
            }
            // kids
            var fork = node as Fork;
            if (fork != null)
            {
                sb.Append(';');
                foreach (var child in fork.Children)
                {
                    NodeToString(child, sb);
                }
            }
            sb.Append('}');
        }

        public void Parse(string expression)
        {
            var tokens = Tokenize(expression);
            var treeBuilder = new TreeBuilder();
            treeBuilder.Parse(tokens);
            Root = treeBuilder.Root;
        }

        private IEnumerable<Token> Tokenize(string expression)
        {
            var lastIsEntity = false;
            var tokens = new LinkedList<Token>();
            for (var i = 0; i < expression.Length;)
            {
                i = SkipSpaces(expression, i);
                var c = expression[i];
                Token token;
                if (char.IsLetter(c) || c == '_')
                {
                    // symbol
                    string symbol;
                    i = ConsumeWhen(expression, i, c1 => char.IsLetterOrDigit(c1) || c1 == '_', out symbol);
                    switch (symbol.ToLower())
                    {
                        case "and":
                            token = new Token
                            {
                                Content = "&&",
                                TokenType = Token.Type.Symbol
                            };
                            lastIsEntity = false;
                            break;
                        case "or":
                            token = new Token
                            {
                                Content = "||",
                                TokenType = Token.Type.Symbol
                            };
                            lastIsEntity = false;
                            break;
                        case "not":
                            token = new Token
                            {
                                Content = "!",
                                TokenType = Token.Type.Symbol
                            };
                            lastIsEntity = false;
                            break;
                        default:
                            token = new Token
                            {
                                Content = symbol,
                                TokenType = Token.Type.Symbol
                            };
                            lastIsEntity = true;
                            break;
                    }
                }
                else if (c == '(')
                {
                    token = new Token
                    {
                        Content = "(",
                        TokenType = Token.Type.LeftBracket
                    };
                    i++;
                    lastIsEntity = false;
                }
                else if (c == ')')
                {
                    token = new Token
                    {
                        Content = ")",
                        TokenType = Token.Type.RightBracket
                    };
                    i++;
                    lastIsEntity = true;
                }
                else if (c == ',')
                {
                    token = new Token
                    {
                        Content = ",",
                        TokenType = Token.Type.Comma
                    };
                    i++;
                    lastIsEntity = false;
                }
                else if (c == '"')
                {
                    // TODO string
                    string s;
                    i = GetString(expression, i, out s);
                    token = new Token
                    {
                        Content = s + "@", // string type indicator
                        TokenType = Token.Type.Constant
                    };

                    lastIsEntity = true;
                }
                else if (char.IsDigit(c) || c =='.' && !lastIsEntity)
                {
                    // number
                    string number;
                    i = GetNumber(expression, i, out number);
                    token = new Token
                    {
                        Content = number,
                        TokenType = Token.Type.Constant
                    };
                    lastIsEntity = true;
                }
                else if (c == '+' || c == '-' || c == '*' || c == '/' || c == '.' && lastIsEntity)
                {
                    // single-character operators
                    token = new Token
                    {
                        Content = new string(c,1),
                        TokenType = Token.Type.Operator
                    };
                    i++;
                    lastIsEntity = false;
                }
                else if (c == '<' || c == '>')
                {
                    var charNum = 1;
                    if (i + 1 < expression.Length)
                    {
                        var next = expression[i];
                        if (next == '=')
                        {
                            i++;
                            charNum++;
                        }
                    }
                    i++;
                    token = new Token
                    {
                        Content = expression.Substring(i-charNum, charNum),
                        TokenType = Token.Type.Operator
                    };
                    lastIsEntity = false;
                }
                else if (c == '&' || c == '|' || c == '=')
                {
                    // NOTE we allow both '=' and '==' and interpret them the same
                    // NOTE which means bit operation is not supported yet, but it's easy to extend
                    token = new Token
                    {
                        Content = new string(c,2),
                        TokenType = Token.Type.Operator
                    };
                    if (i + 1 < expression.Length)
                    {
                        var next = expression[i];
                        if (next == c)
                        {
                            i++;
                        }
                    }
                    i++;
                }
                else
                {
                    throw new Exception("Unexpected character"); // TODO more details
                }
                tokens.AddLast(token);
            }
            return tokens;
        }

        private int SkipSpaces(string s, int i)
        {
            return ConsumeWhen(s, i, char.IsWhiteSpace);
        }

        private int GetString(string s, int i, out string sval)
        {
            // now at '"'
            var sb = new StringBuilder();
            for (i++; i < s.Length; i++)
            {
                var c = s[i];
                if (c == '\\')
                {
                    if (i+1 < s.Length)
                    {
                        c = s[++i];
                    }
                    sb.Append(c);
                }
                else if (c == '"')
                {
                    sval = sb.ToString();
                    return i+1;
                }
                else
                {
                    sb.Append(c);
                }
            }
            sval = sb.ToString();
            return i;
        }

        private int GetNumber(string s, int i, out string number)
        {
            var dotUsed = false;
            return ConsumeWhen(s, i, c =>
            {
                if (char.IsDigit(c))
                {
                    return true;
                }
                if (c == '.')
                {
                    if (dotUsed)
                    {
                        return false;
                    }
                    dotUsed = true;
                    return true;
                }
                return false;
            }, out number);
        }

        private int ConsumeWhen(string s, int i, Predicate<char> allow)
        {
            for (; i < s.Length && allow(s[i]); i++)
            {
            }

            return i;
        }

        private int ConsumeWhen(string s, int i, Predicate<char> allow, out string retrieved)
        {
            var sb = new StringBuilder();

            for (; i < s.Length && allow(s[i]); i++)
            {
                sb.Append(s[i]);
            }

            retrieved = sb.ToString();
            return i;
        }

        #endregion
    }
}
