using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace QSharp.String.ExpressionEvaluation
{
    public class TreeBuilder : IHasChild
    {
        #region Properties

        public Node Root { get; set; }

        public NodeBuilder NearestBracketKeeper { get; set; }

        public NodeBuilder ParameterCell { get; set; }

        public NodeBuilder Attractor { get; set; }

        public Node AtomicNode { get; set; }

        public Token LastToken { get; set; }

        public Token.Type LastTokenType
        {
            get { return LastToken != null? LastToken.TokenType : Token.Type.None; }
        }

        public ITokenizer Precedence
        {
            get; private set;
        }

        #endregion

        #region Constructors

        public TreeBuilder(ITokenizer precedence)
        {
            Precedence = precedence;
            var rootHolder = new NodeBuilder {IsRootKeeper = true};
            Root = rootHolder;
            NearestBracketKeeper = rootHolder;
            Attractor = rootHolder;
        }

        #endregion

        #region Methods

        #region IHasChild members

        public bool ReplaceChild(Node oldChild, Node newChild)
        {
            if (Root == oldChild)
            {
                Root = newChild;
                return true;
            }
            return false;
        }

        public void AddChild(Node newChild)
        {
            throw new InvalidOperationException();
        }

        #endregion

        public void Parse(IEnumerable<Token> tokens)
        {
            foreach (var token in tokens)
            {
                switch (token.TokenType)
                {
                    case Token.Type.LeftBracket:
                        ConsumeLeftBracket();
                        break;
                    case Token.Type.RightBracket:
                        ConsumeRightBracket();
                        break;
                    case Token.Type.Constant:
                    case Token.Type.Symbol:
                        AttractSymbol(token);
                        break;
                    case Token.Type.Comma:
                        ConsumeComma();
                        break;
                    case Token.Type.Operator:
                        if (LastTokenType == Token.Type.None ||
                            LastTokenType == Token.Type.LeftBracket ||
                            LastTokenType == Token.Type.Comma ||
                            LastTokenType == Token.Type.Operator)
                        {
                            // unary operator: it's guaranteed in an empty cell
                            // TODO if operator, check tokens before last
                            AttractUnaryOperator(token);
                        }
                        else
                        {
                            // binary operator: from closed node up find one whose priority is lower or is a cell
                            // shift its content left, take its place, retain the node's place cell property
                            // and create an acceptor on the right
                            AddBinaryOperator(token);
                        }

                        break;
                }
                LastToken = token;
            }

            Root = ((NodeBuilder) Root).Solidify();
        }

        private void ConsumeLeftBracket()
        {
            if (AtomicNode != null)
            {
                AddFunction();
                AtomicNode = null;
            }
            else if (LastTokenType == Token.Type.LeftBracket)
            {
                // charge the existing one
                NearestBracketKeeper.LeftBracketCount++;
            }
            else if (Attractor != null)
            {
                // create a new bracket keeper
                NearestBracketKeeper = Attractor;
                Attractor.LeftBracketCount = 1;
            }
            else
            {
                throw new Exception("Unexpected left bracket");
            }
        }

        private void ConsumeRightBracket()
        {
            if (Attractor != null && Attractor != ParameterCell)
            {
                throw new Exception("Unexpected right bracket"); // expecting symbols for the attractor
            }

            if (ParameterCell != null && !ParameterContainsBracket())
            {
                // function concludes

                var omitted = ParameterCell.IsAttractor;

                var functionHolder = (NodeBuilder)ParameterCell.Parent;

                if (omitted)
                {
                    functionHolder.Children.RemoveLast();
                }
                else
                {
                    ParameterCell.Close();
                }

                if (!functionHolder.IsBracketKeeper)
                {
                    functionHolder.Close();
                }
                AtomicNode = functionHolder;
                Attractor = null;

                // Find the upper parameter cell
                PopParameterCell();
            }
            else
            {
                NearestBracketKeeper.RightBracketCount++;
                
                if (NearestBracketKeeper.LeftBracketCount == NearestBracketKeeper.RightBracketCount)
                {
                    AtomicNode = NearestBracketKeeper.Debracket();
                    // Find the next bracket keeper
                    PopBracketKeeper();
                }
                else
                {
                    AtomicNode = NearestBracketKeeper;
                }
            }
        }

        private void ConsumeComma()
        {
            if (Attractor != null && Attractor != ParameterCell || ParameterCell == null || ParameterContainsBracket())
            {
                // TODO any other erroneous conditions?
                throw new Exception("Unexpected comma");
            }

            // close the parameter cell and create a new one
            var functionHolder = (IHasChild)ParameterCell.Parent;
            ParameterCell.Close();

            var newParameter = new NodeBuilder {IsParemeterCell = true, IsAttractor = true};
            functionHolder.AddChild(newParameter);

            Attractor = newParameter;
            ParameterCell = newParameter;
        }


        private Node FindInsertPoint(Node startNode, int priority)
        {
            // find the place to insert the operator
            var p = startNode;

            for (; p != null; p = p.Parent)
            {
                if (p is NodeBuilder) // parameter, bracket keeper
                {
                    break;
                }
                if (p.Parent.NodeType == Node.Type.BinaryOperator)
                {
                    var prioCurrParent = Precedence.GetBinaryOperatorPrecedence(p.Parent.Content);
                    if (priority < prioCurrParent)
                    {
                        break;
                    }
                }
            }

            return p;
        }

        private void AddBinaryOperator(Token token)
        {
            if (Attractor != null || AtomicNode == null)
            {
                throw new Exception("Unexpected binary operator");
            }
            
            // find the place to insert the operator
            var prioThis = Precedence.GetBinaryOperatorPrecedence(token.Content);
            var p = FindInsertPoint(AtomicNode, prioThis);

            var placeHolderToUse = p as NodeBuilder;
            var rhsHolder = new NodeBuilder { IsAttractor = true };
            if (placeHolderToUse != null)
            {
                placeHolderToUse.DischargeAtomForFork(token.Content, Node.Type.BinaryOperator);
                placeHolderToUse.AddChild(rhsHolder);
            }
            else
            {
                var opNode = new Fork {NodeType = Node.Type.BinaryOperator, Content = token.Content};
                Rotate(p, opNode);
                opNode.AddChild(rhsHolder);
            }
            Attractor = rhsHolder;
            AtomicNode = null;
        }

        private void AttractUnaryOperator(Token token)
        {
            if (Attractor != null && AtomicNode == null)
            {
                Attractor.Attract(token);

                var newAttractor = new NodeBuilder { IsAttractor = true };
                Attractor.AddChild(newAttractor);
                Attractor.Close(); // NOTE it should be closed here right?
                Attractor = newAttractor;
            }
            else
            {
                throw new Exception("Unexpected unary operator");
            }
        }


        /// <summary>
        ///  
        /// </summary>
        private void AddFunction()
        {
            // find entry node
            var functionHolder = GetEntryNodeForFunction();

            var parameterHolder = new NodeBuilder {IsParemeterCell = true, IsAttractor = true};
            functionHolder.AddChild(parameterHolder);

            Attractor = parameterHolder;
            ParameterCell = parameterHolder;
        }

        private void AttractSymbol(Token token)
        {
            if (Attractor != null)
            {
                Attractor.Attract(token);

                Node symbolNode = Attractor;
                // see if the attractor can be solidifed
                if (!Attractor.IsBracketKeeper && !Attractor.IsParemeterCell && !Attractor.IsRootKeeper)
                {
                    symbolNode = Attractor.Close();
                }
                
                // updates Atomic node
                for (var p = symbolNode; p != null; p = p.Parent)
                {
                    var pAsPlaceHolder = p as NodeBuilder;
                    if (pAsPlaceHolder != null && (pAsPlaceHolder.IsBracketKeeper || pAsPlaceHolder.IsParemeterCell)
                        || p.Parent == null || p.Parent.NodeType != Node.Type.UnaryOperator)
                    {
                        AtomicNode = p;
                        break;
                    }
                }

                Attractor = null;
                return;
            }

            throw new Exception("Unexpected symbol");
        }

        private NodeBuilder GetEntryNodeForFunction()
        {
            Debug.Assert(AtomicNode != null);

            var prioThis = Precedence.FunctionPrecedence;
            var insertPoint = FindInsertPoint(AtomicNode, prioThis);

            var placeHolderToUse = insertPoint as NodeBuilder;

            if (placeHolderToUse != null)
            {
                // reuse the place holder
                placeHolderToUse.DischargeAtomForFork("", Node.Type.Function);
                return placeHolderToUse;
            }
            
            var functionHolder = new NodeBuilder
            {
                Content = "",
                NodeType = Node.Type.Function
            };

            Rotate(insertPoint, functionHolder);

            return functionHolder;
        }

        private void Rotate(Node original, Node newNode)
        {
            var childOwner = (IHasChild) original.Parent;
            childOwner.ReplaceChild(original, newNode);

            var newNodeAsParent = (IHasChild) newNode;
            newNodeAsParent.AddChild(original);
        }

        /// <summary>
        ///  returns if the nearest bracket is within the current parameter
        /// </summary>
        /// <returns></returns>
        private bool ParameterContainsBracket()
        {
            if (ParameterCell.IsBracketKeeper)
            {
                return true;
            }
            for (var p = ParameterCell.Parent; p != null; p = p.Parent)
            {
                var pAsPlaceHolder = p as NodeBuilder;
                if (pAsPlaceHolder != null && (pAsPlaceHolder.IsBracketKeeper || pAsPlaceHolder.IsRootKeeper))
                {
                    return (pAsPlaceHolder != NearestBracketKeeper);
                }
            }
            throw new Exception("Invalid internal state");
        }

        private void PopParameterCell()
        {
            var p = ParameterCell.Parent;
            for (; p != null; p = p.Parent)
            {
                var pAsPlaceHolder = p as NodeBuilder;
                if (pAsPlaceHolder != null && pAsPlaceHolder.IsParemeterCell)
                {
                    break;
                }
            }
            ParameterCell = (NodeBuilder)p;
        }

        private void PopBracketKeeper()
        {
            for (var p = NearestBracketKeeper.Parent; p != null; p = p.Parent)
            {
                var pAsPlaceHolder = p as NodeBuilder;
                if (pAsPlaceHolder != null && (pAsPlaceHolder.IsBracketKeeper || pAsPlaceHolder.IsRootKeeper))
                {
                    NearestBracketKeeper = pAsPlaceHolder;
                    break;
                }
            }
        }

        #endregion
    }
}
