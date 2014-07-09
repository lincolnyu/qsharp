using System;
using System.Collections.Generic;

namespace QSharp.String.ExpressionEvaluation
{
    public class PlaceHolder : Node, IHasChild
    {
        #region Properties

        public bool IsRootKeeper { get; set; }

        public bool IsBracketKeeper
        {
            get { return LeftBracketCount > 0; }
        }

        public bool IsFunction
        {
            get { return NodeType == Type.Function; }
        }

        public bool IsParemeterCell { get; set; }

        public bool IsAttractor { get; set; } // rhs of binary

        public int LeftBracketCount { get; set; }

        public int RightBracketCount { get; set; }

        public LinkedList<Node> Children { get; set; }

        #endregion

        #region Constructors

        public PlaceHolder()
        {
            Children = new LinkedList<Node>();
        }

        #endregion

        #region Methods

        #region IHasChild members

        public bool ReplaceChild(Node oldChild, Node newChild)
        {
            var node = Children.First;
            for (; node != null; node = node.Next)
            {
                var child = node.Value;
                if (child == oldChild)
                {
                    break;
                }
            }
            if (node != null)
            {
                node.Value = newChild;
                return true;
            }

            return false;
        }

        public void AddChild(Node child)
        {
            Children.AddLast(child);
        }

        #endregion

        /// <summary>
        ///  Turns the placeholder to a firm node
        /// </summary>
        public Node Close()
        {
            var parentHasChild = (IHasChild)Parent;
            var concluded = DischargeNode();

            parentHasChild.ReplaceChild(this, concluded);

            return concluded;
        }

        public void Attract(Token token)
        {
            switch (token.TokenType)
            {
                case Token.Type.Symbol:
                    NodeType = Type.Symbol;
                    break;
                case Token.Type.Constant:
                    NodeType = Type.Constant;
                    break;
                case Token.Type.Operator: // unary
                    NodeType = Type.UnaryOperator;
                    break;
            }
            
            Content = token.Content;
        }

        public Node Debracket()
        {
            if (IsAttractor)
            {
                throw new Exception("Unexpected open attractor");
            }
            if (IsParemeterCell)
            {
                return this; // retained for parameter which is not done yet
            }
            return Close();
        }

        public Node DischargeAtomForFork(string content, Type nodeType)
        {
            var node = DischargeNode();
            Children.Clear();
            Children.AddLast(node);

            NodeType = nodeType;
            Content = content;

            return node;
        }


        private Node DischargeNode()
        {
            Node concluded;
            if (Children.Count > 0)
            {
                var fork = new Fork();
                foreach (var child in Children)
                {
                    fork.Children.Add(child);
                }
                concluded = fork;

            }
            else
            {
                concluded = new Leaf();
            }
            concluded.NodeType = NodeType;
            concluded.Content = Content;
            concluded.Parent = Parent;
            return concluded;
        }

        #endregion
    }
}
