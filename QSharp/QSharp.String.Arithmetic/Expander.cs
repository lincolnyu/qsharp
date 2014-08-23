using System;
using QSharp.String.ExpressionEvaluation;

namespace QSharp.String.Arithmetic
{
    public static class Expander
    {
        public static void Expand(Fork fork)
        {
            foreach (var child in fork.Children)
            {
                var childAsFork = child as Fork;
                if (childAsFork != null)
                {
                    Expand(childAsFork);
                }
            }

            if (fork.NodeType == Node.Type.BinaryOperator)
            {
                ExpandBinary(fork);
            }
        }

        private static void ExpandBinary(Fork fork)
        {
            switch (fork.Content)
            {
                case "+":
                case "-":
                    ExpandPlusMinus(fork);
                    break;
                case "*":
                    ExpandMultiply(fork);
                    break;
            }
        }

        private static void ExpandPlusMinus(Fork fork)
        {
            var left = fork.Children[0];
            var right = fork.Children[1];

            var leftIsBOp = left.NodeType == Node.Type.BinaryOperator;
            var rightIsBOp = right.NodeType == Node.Type.BinaryOperator;

            var leftDiv = leftIsBOp && left.Content == "/";
            var rightDiv = rightIsBOp && right.Content == "/";

            if (leftDiv && rightDiv)
            {
                ExpandPlusMinusBothDivs(fork);
            }
            else if (leftDiv)
            {
                ExpandPlusMinusLeftDiv(fork);
            }
            else if (rightDiv)
            {
                ExpandPlusMinusRightDiv(fork);
            }
        }

        private static void ExpandMultiply(Fork fork)
        {
            var left = fork.Children[0];
            var right = fork.Children[1];

            var leftIsBOp = left.NodeType == Node.Type.BinaryOperator;
            var rightIsBOp = right.NodeType == Node.Type.BinaryOperator;

            if (leftIsBOp && (left.Content == "+" || left.Content == "-"))
            {
                ExpandMultiplyLeftPlusMinus(fork);
            }
            else if (rightIsBOp && (right.Content == "+" || right.Content == "-"))
            {
                ExpandMultiplyRightPlusMinus(fork);
            }

            var leftDiv = leftIsBOp && left.Content == "/";
            var rightDiv = rightIsBOp && right.Content ==  "/";

            if (leftDiv && rightDiv)
            {
                ExpandMultiplyBothDivs(fork);
            }
            else if (leftDiv)
            {
                ExpandMultiplyLeftDiv(fork);
            }
            else if (rightDiv)
            {
                ExpandMultiplyRightDiv(fork);
            }
        }

        private static void ExpandMultiplyLeftPlusMinus(Fork fork)
        {
            var op = fork.Content;

            var left = (Fork)fork.Children[0];
            var c = fork.Children[1];
            var b = left.Children[1];

            var cc = CopyTree(c);

            fork.Content = op;
            left.Content = "*";

            var nm = new Fork {NodeType = Node.Type.BinaryOperator, Content = "*"};
            fork.Children[1] = nm;
            nm.Parent = fork;

            left.Children[1] = c;
            c.Parent = left;

            nm.AddChild(b);
            nm.AddChild(cc);

            ExpandMultiply(left);
            ExpandMultiply(nm);
        }

        private static void ExpandMultiplyRightPlusMinus(Fork fork)
        {
            var op = fork.Content;

            var a = fork.Children[0];
            var right = (Fork)fork.Children[1];
            var b = right.Children[0];

            var aa = CopyTree(a);

            fork.Content = op;
            right.Content = "*";

            var nm = new Fork { NodeType = Node.Type.BinaryOperator, Content = "*" };
            fork.Children[0] = nm;
            nm.Parent = fork;

            right.Children[0] = a;
            a.Parent = right;

            nm.AddChild(aa);
            nm.AddChild(b);

            ExpandMultiply(nm);
            ExpandMultiply(right);
        }

        private static void ExpandPlusMinusBothDivs(Fork fork)
        {
            var op = fork.Content;

            var left = (Fork)fork.Children[0];
            var right = (Fork)fork.Children[1];

            var a = left.Children[0];
            var b = left.Children[1];

            var c = right.Children[0];
            var d = right.Children[1];

            var bb = CopyTree(b);
            var dd = CopyTree(d);

            fork.Content = "/";
            left.Content = op;
            right.Content = "*";

            var nm1 = new Fork {NodeType = Node.Type.BinaryOperator, Content = "*"};
            var nm2 = new Fork { NodeType = Node.Type.BinaryOperator, Content = "*" };
            left.Children[0] = nm1;
            nm1.Parent = left;
            left.Children[1] = nm2;
            nm2.Parent = left;

            nm1.AddChild(a);
            nm1.AddChild(dd);
            nm2.AddChild(bb);
            nm2.AddChild(c);

            right.Children[0] = b;
            b.Parent = right;

            ExpandMultiply(nm1);
            ExpandMultiply(nm2);
        }

        private static void ExpandPlusMinusLeftDiv(Fork fork)
        {
            var op = fork.Content;

            var left = (Fork)fork.Children[0];
            var c = fork.Children[1];
            var b = left.Children[1];

            var bb = CopyTree(b);

            fork.Content = "/";
            left.Content = op;

            fork.Children[1] = bb;
            bb.Parent = fork;

            var nm = new Fork {NodeType = Node.Type.BinaryOperator, Content = "*"};
            left.Children[1] = nm;
            nm.Parent = left;

            nm.AddChild(b);
            nm.AddChild(c);

            ExpandMultiply(nm);
        }

        private static void ExpandPlusMinusRightDiv(Fork fork)
        {
            var op = fork.Content;

            var oldRight = (Fork) fork.Children[1];

            var a = fork.Children[0];
            var b = oldRight.Children[0];
            var c = oldRight.Children[1];

            var cc = CopyTree(c);

            fork.Content = "/";
            oldRight.Content = op;

            fork.Children[0] = oldRight;
            fork.Children[1] = c;
            c.Parent = fork;

            var nm = new Fork { NodeType = Node.Type.BinaryOperator, Content = "*" };
            oldRight.Children[0] = nm;
            nm.Parent = oldRight;

            oldRight.Children[1] = b;

            nm.AddChild(a);
            nm.AddChild(cc);

            ExpandMultiply(nm);
        }

        private static void ExpandMultiplyBothDivs(Fork fork)
        {
            var left = (Fork)fork.Children[0];
            var right = (Fork)fork.Children[1];

            fork.Content = "/";
            left.Content = "*";
            right.Content = "*";

            var rightleft = right.Children[0];
            var leftright = left.Children[1];

            right.Children[0] = leftright;
            leftright.Parent = right;

            left.Children[1] = rightleft;
            rightleft.Parent = left;

            ExpandMultiply(left);
            ExpandMultiply(right);
        }

        private static void ExpandMultiplyLeftDiv(Fork fork)
        {
            var left = (Fork) fork.Children[0];
            var c = fork.Children[1];

            fork.Content = "/";
            left.Content = "*";

            var b = left.Children[1];

            fork.Children[1] = b;
            b.Parent = fork;

            left.Children[1] = c;
            c.Parent = left;

            ExpandMultiply(left);
        }

        private static void ExpandMultiplyRightDiv(Fork fork)
        {
            var a = (Fork)fork.Children[0];
            var oldRight = (Fork)fork.Children[1];
            var b = oldRight.Children[0];
            var c = oldRight.Children[1];

            fork.Content = "/";
            oldRight.Content = "*";

            fork.Children[0] = oldRight; // new left
            oldRight.Children[0] = a;
            a.Parent = oldRight;
            oldRight.Children[1] = b;

            fork.Children[1] = c;
            c.Parent = fork;

            ExpandMultiply(oldRight);
        }

        private static Node CopyTree(Node node)
        {
            var f = node as Fork;
            if (f != null)
            {
                var nf = new Fork
                {
                    NodeType = f.NodeType,
                    Content = f.Content
                };
                foreach (var c in f.Children)
                {
                    var nc = CopyTree(c);
                    nf.AddChild(nc);
                }
                return nf;
            }

            var l = node as Leaf;
            if (l != null)
            {
                var nl = new Leaf
                {
                    NodeType = l.NodeType,
                    Content = l.Content
                };
                return nl;
            }

            throw new NotSupportedException();
        }
    }
}
