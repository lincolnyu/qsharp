using System;
using QSharp.Scheme.Mathematics.Analytical;
using QSharp.String.ExpressionEvaluation;

namespace QSharp.String.Arithmetic
{
    public static class FractionBuilder
    {
        #region Methods

        public static Fraction Build(Node node)
        {
            var fork = node as Fork;
            if (fork != null)
            {
                return Build(fork);
            }

            switch (node.NodeType)
            {
                case Node.Type.Constant:
                {
                    var r = Rational.CreateFromString(node.Content);
                    return r;
                }
                case Node.Type.Symbol:
                {
                    // TODO we should support common irrational number notations such as PI and e shortly
                    // TODO but we need to implement the data type in the mathematics library first.
                    var m = Monomial.GetOneDegreeVariable(node.Content);
                    return m;
                }
                default:
                    throw new InvalidOperationException("Unexpected node type");
            }
        }

        private static Fraction Build(Fork fork)
        {
            switch (fork.NodeType)
            {
                case Node.Type.BinaryOperator:
                    switch (fork.Content)
                    {
                        case "+":
                        {
                            Fraction ret = (Rational) 0;
                            foreach (var c in fork.Children)
                            {
                                var e = Build(c);
                                ret += e;
                            }
                            return ret;
                        }
                        case "-":
                        {
                            var a = fork.Children[0];
                            var b = fork.Children[1];
                            var ba = Build(a);
                            var bb = Build(b);
                            var ret = ba - bb;
                            return ret;
                        }
                        case "*":
                        {
                            Fraction ret = (Rational)1;
                            foreach (var c in fork.Children)
                            {
                                var e = Build(c);
                                ret *= e;
                            }
                            return ret;
                        }
                        case "/":
                        {
                            var a = fork.Children[0];
                            var b = fork.Children[1];
                            var ba = Build(a);
                            var bb = Build(b);
                            var ret = ba/bb;
                            return ret;
                        }
                        case "^":
                        {
                            var b = fork.Children[0];
                            var i = fork.Children[1];
                            var bb = Build(b);
                            var bi = Build(i);
                            int bii;
                            var biisucc = bi.TryConvertToInt(out bii);
                            if (!biisucc)
                            {
                                throw new InvalidOperationException("Only integer index supported");
                            }
                            var ret = bb ^ bii;
                            return ret;
                        }
                        default:
                            throw new InvalidOperationException("Unexpected binary operator");
                    }
                case Node.Type.UnaryOperator:
                    switch (fork.Content)
                    {
                        case "+":
                        {
                            var ret = Build(fork.Children[0]);
                            return ret;
                        }
                        case "-":
                        {
                            var ret = Build(fork.Children[0]);
                            ret.NegateSelf();
                            return ret;
                        }
                        default:
                            throw new InvalidOperationException("Unexpected unary operator");
                    }
                case Node.Type.Function:
                    throw new NotSupportedException();
                default:
                    throw new InvalidOperationException("Unexpected node type");
            }
        }

        #endregion
    }
}
