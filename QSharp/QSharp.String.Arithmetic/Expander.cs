using System;
using System.Diagnostics;
using QSharp.String.ExpressionEvaluation;

namespace QSharp.String.Arithmetic
{
    public static class Expander
    {
        public static Node PreProcessPowers(Node node)
        {
            var fork = node as Fork;
            if (fork != null)
            {
                foreach (var c in fork.Children)
                {
                    PreProcessPowers(c);
                }

                if (fork.NodeType == Node.Type.BinaryOperator && fork.Content == "^")
                {
                    var index = fork.Children[1];

                    if (index.NodeType != Node.Type.Constant)
                    {
                        return node;// won't do that
                    }

                    var ic = index.Content;
                    int ii;
                    if (!int.TryParse(ic, out ii))
                    {
                        return node; // won't do non-integer index
                    }

                    if (ii < 5 || ii > 5)
                    {
                        return node; // too big
                    }

                    if (ii == 0)
                    {
                        return ProcessZeroPower(fork);
                    }
                    if (ii == 1)
                    {
                        return ProcessPositiveTrivialPower(fork);
                    }
                    
                    if (ii > 0)
                    {
                        ProcessPositivePower(fork, ii);
                    }
                    else if (ii < 0)
                    {
                        ProcessNegativePower(fork, -ii);
                    }
                }
            }
            return node;
        }

        private static Node ProcessPositiveTrivialPower(Fork fork)
        {
            var b = fork.Children[0];
            var p = fork.Parent as Fork;
            if (p != null)
            {
                var i = GetIndexInParent(fork);
                p.Children[i] = b;
                b.Parent = p;
            }
            return b;
        }

        private static Node ProcessZeroPower(Fork fork)
        {
            var n = new Leaf
            {
                NodeType = Node.Type.Constant,
                Content = "1"
            };
            var p = fork.Parent as Fork;
            if (p != null)
            {
                var i = GetIndexInParent(fork);
                p.Children[i] = n;
                n.Parent = p;
            }
            return n;
        }

        private static void ProcessNegativePower(Fork fork, int ii)
        {
            var b = fork.Children[0];
            fork.Content = "/";

            fork.Children[0] = new Leaf {NodeType = Node.Type.Constant, Content = "1"};
            fork.Children[0].Parent = fork.Children[0];
            if (ii == 1)
            {
                fork.Children[1] = b;
                return;
            }

            fork.Children.RemoveAt(1);
            var p = fork;

            for (var i = 1; i < ii; i++)
            {
                var bb = i == 1 ? b : CopyTree(b);
                fork.AddChild(bb);

                var right = i + 1 < ii ? new Fork { NodeType = Node.Type.BinaryOperator, Content = "*" } : CopyTree(b);
                p.AddChild(right);
            }
        }

        private static void ProcessPositivePower(Fork fork, int ii)
        {
            var b = fork.Children[0];

            fork.Content = "*";
            fork.Children.RemoveAt(1);

            var p = fork;
            for (var i = 1; i < ii; i++)
            {
                var bb = CopyTree(b);
                Debug.Assert(p != null, "p != null");
                p.AddChild(bb);

                var right = i + 1 < ii ? new Fork {NodeType = Node.Type.BinaryOperator, Content = "*"} : CopyTree(b);
                p.AddChild(right);
                p = right as Fork;
            }
        }

        public static void Expand(Node node)
        {
            var fork = node as Fork;
            if (fork != null)
            {
                // NOTE this include unary
                Expand(fork);
            }
        }

        public static Node Expand(Fork fork)
        {
            foreach (var child in fork.Children)
            {
                Expand(child);
            }

            switch (fork.NodeType)
            {
                case Node.Type.BinaryOperator:
                    ExpandBinary(fork);
                    break;
                case Node.Type.UnaryOperator:
                    return ExpandUnary(fork);
            }
            return fork;
        }

        private static Node ExpandUnary(Fork fork)
        {
            switch (fork.Content)
            {
                case "+":
                    return ExpandPositive(fork);
                case "-":
                    return ExpandNegative(fork);
            }
            return fork;
        }

        private static Node ExpandNegative(Fork fork)
        {
            var child = fork.Children[0];
            var cf = child as Fork;
            if (cf == null)
            {
                return fork; // do nothing
            }

            if (cf.NodeType == Node.Type.UnaryOperator)
            {
                if (cf.Content == "-")
                {
                    // remove both
                    var grandchild = cf.Children[0];
                    grandchild.Parent = fork.Parent;
                    if (fork.Parent != null)
                    {
                        var pf = (Fork)fork.Parent;
                        var i = GetIndexInParent(fork);
                        pf.Children[i] = grandchild;
                    }
                    return grandchild;
                }
                return fork;// unknown symbol, can't do anything
            }

            if (cf.NodeType == Node.Type.BinaryOperator)
            {
                // propagate the negative
                PropagateNegative(cf);
                return fork;
            }

            return fork;// unknown symbol, can't do anything for now
        }

        private static void PropagateNegative(Node node)
        {
            var fork = node as Fork;
            if (fork != null)
            {
                var propagated = PropagateNegative(fork);
                if (propagated)
                {
                    return;
                }
            }

            // insert a negative node

            var n = new Fork {NodeType = Node.Type.UnaryOperator, Content = "-"};

            var pf = (Fork)node.Parent;
            var i  = GetIndexInParent(node);
            n.Parent = node.Parent;
            pf.Children[i] = n;

            n.AddChild(node);
        }

        private static bool PropagateNegative(Fork fork)
        {
            if (fork.NodeType == Node.Type.UnaryOperator)
            {
                if (fork.Content == "-")
                {
                    var c = fork.Children[0];
                    // remove the node
                    var i = GetIndexInParent(fork);
                    var pf = (Fork) fork.Parent;
                    pf.Children[i] = c;
                    c.Parent = pf;
                    return true;
                }
            }
            else if (fork.NodeType == Node.Type.BinaryOperator)
            {
                switch (fork.Content)
                {
                    case "+":
                    case "-":
                        foreach (var c in fork.Children)
                        {
                            PropagateNegative(c);
                        }
                        return true;
                    case "*":
                    case "/":
                        PropagateNegative(fork.Children[0]);
                        return true;
                }
            }
            return false;
        }

        private static Node ExpandPositive(Fork fork)
        {
            var child = fork.Children[0];
            child.Parent = fork.Parent;
            if (fork.Parent != null)
            {
                var pf = (Fork)fork.Parent;
                var i = GetIndexInParent(fork);
                pf.Children[i] = child;
            }
            return child;
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

        private static int GetIndexInParent(Node node)
        {
            var fork = (Fork)node.Parent;
            for (var i = 0; i < fork.Children.Count; i++)
            {
                if (fork.Children[i] == node)
                {
                    return i;
                }
            }
            return -1;
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
