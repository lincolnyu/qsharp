﻿using System;
using System.Collections.Generic;

namespace QSharp.String.ExpressionEvaluation
{
    public class NodeBuilder : Node, IHasChild
    {
        #region Constructors

        public NodeBuilder()
        {
            Children = new LinkedList<Node>();
        }

        #endregion

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
                newChild.Parent = this;
                return true;
            }

            return false;
        }

        public void AddChild(Node child)
        {
            Children.AddLast(child);
            child.Parent = this;
        }

        #endregion

        /// <summary>
        ///  Turns the placeholder to a firm node
        /// </summary>
        public Node Close()
        {
            var parentHasChild = (IHasChild)Parent;
            var concluded = Solidify();

            if (parentHasChild != null)
            {
                parentHasChild.ReplaceChild(this, concluded);    
            }

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
            IsAttractor = false;
        }

        public Node Debracket()
        {
            if (IsAttractor)
            {
                throw new Exception("Unexpected open attractor");
            }
            if (IsParemeterCell || IsRootKeeper)
            {
                return this; // retained for parameter/root which is not done yet
            }
            return Close();
        }

        public Node DischargeAtomForFork(string content, Type nodeType)
        {
            var node = Solidify();
            Children.Clear();
            AddChild(node);

            NodeType = nodeType;
            Content = content;

            return node;
        }

        public Node Solidify()
        {
            Node concluded;
            if (Children.Count > 0)
            {
                var fork = new Fork();
                foreach (var child in Children)
                {
                    fork.AddChild(child);
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
