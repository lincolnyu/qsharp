﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace QSharp.String.ExpressionEvaluation
{
    public class TreeBuilder : IHasChild
    {
        #region Properties

        public Node Root { get; set; }

        public PlaceHolder NearestBracketKeeper { get; set; }

        public PlaceHolder ParameterCell { get; set; }

        public PlaceHolder Attractor { get; set; }

        public Node AtomicNode { get; set; }

        public Token LastToken { get; set; }

        public Token.Type LastTokenType
        {
            get { return LastToken != null? LastToken.TokenType : Token.Type.None; }
        }

        #endregion

        #region Constructors

        public TreeBuilder()
        {
            var rootHolder = new PlaceHolder {IsRootKeeper = true};
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
        }

        private void ConsumeLeftBracket()
        {
            if (AtomicNode != null)
            {
                AddFunction(LastToken);
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

                var concludedParameter = ParameterCell.Close();

                var functionHolder = (PlaceHolder)concludedParameter.Parent;
                functionHolder.Close();
                AtomicNode = functionHolder;

                // Find the upper parameter cell
                PopParameterCell();
            }
            else
            {
                NearestBracketKeeper.RightBracketCount++;
                AtomicNode = NearestBracketKeeper;
                if (NearestBracketKeeper.LeftBracketCount == NearestBracketKeeper.RightBracketCount)
                {
                    NearestBracketKeeper.Debracket();
                    // Find the next bracket keeper
                    PopBracketKeeper();
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

            var newParameter = new PlaceHolder {IsParemeterCell = true, IsAttractor = true};
            functionHolder.AddChild(newParameter);

            Attractor = newParameter;
            ParameterCell = newParameter;
        }

        private void AddBinaryOperator(Token token)
        {
            if (Attractor != null || AtomicNode == null)
            {
                throw new Exception("Unexpected binary operator");
            }
            
            // find the place to insert the operator
            var p = AtomicNode;
            var prioThis = GetBinaryOperatorPriority(token.Content);
            for (; p != null; p = p.Parent)
            {
                if (p is PlaceHolder) // parameter, bracket keeper
                {
                    break;
                }
                if (p.Parent.NodeType == Node.Type.BinaryOperator)
                {
                    var prioCurrParent = GetBinaryOperatorPriority(p.Parent.Content);
                    if (prioThis < prioCurrParent)
                    {
                        break;
                    }
                }
            }

            var placeHolderToUse = p as PlaceHolder;
            var rhsHolder = new PlaceHolder { IsAttractor = true };
            if (placeHolderToUse != null)
            {
                placeHolderToUse.DischargeAtomForFork(token.Content, Node.Type.BinaryOperator);
                placeHolderToUse.AddChild(rhsHolder);
            }
            else
            {
                var opNode = new Fork {NodeType = Node.Type.BinaryOperator, Content = token.Content};
                Rotate(p, opNode);
                opNode.Children.Add(rhsHolder);
            }
            Attractor = rhsHolder;
            AtomicNode = null;
        }

        private void AttractUnaryOperator(Token token)
        {
            PlaceHolder unaryHolder;
            if (Attractor != null && AtomicNode == null)
            {
                Attractor.Attract(token);
                unaryHolder = Attractor;
            }
            else
            {
                throw new Exception("Unexpected unary operator");
            }

            Attractor = new PlaceHolder { IsAttractor = true };
            unaryHolder.Children.AddLast(Attractor);
        }


        /// <summary>
        ///  
        /// </summary>
        /// <param name="token">Token contains the function name</param>
        private void AddFunction(Token token)
        {
            // find entry node
            var functionHolder = GetEntryNodeForFunction(token);

            var parameterHolder = new PlaceHolder {IsParemeterCell = true, IsAttractor = true};
            functionHolder.AddChild(parameterHolder);

            Attractor = parameterHolder;
            ParameterCell = parameterHolder;
        }

        private void AttractSymbol(Token token)
        {
            if (Attractor != null)
            {
                Attractor.Attract(token);
                
                // updates Atomic node
                for (var p = (Node)Attractor; p != null; p = p.Parent)
                {
                    var pAsPlaceHolder = p as PlaceHolder;
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

        private PlaceHolder GetEntryNodeForFunction(Token token)
        {
            Debug.Assert(AtomicNode != null);
            var placeHolderToUse = AtomicNode as PlaceHolder;
            if (placeHolderToUse != null)
            {
                // reuse the place holder
                placeHolderToUse.DischargeAtomForFork(token.Content, Node.Type.Function);
                return placeHolderToUse;
            }
            
            var functionHolder = new PlaceHolder
            {
                Content = token.Content,
                NodeType = Node.Type.Function
            };

            Rotate(AtomicNode, functionHolder);

            return functionHolder;
        }

        private void Rotate(Node original, Node newNode)
        {
            var childOwner = (IHasChild) original.Parent;
            childOwner.ReplaceChild(original, newNode);

            var newNodeAsParent = (IHasChild) newNode;
            newNodeAsParent.AddChild(original);
            original.Parent = newNode;
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
                var pAsPlaceHolder = p as PlaceHolder;
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
                var pAsPlaceHolder = p as PlaceHolder;
                if (pAsPlaceHolder != null && pAsPlaceHolder.IsParemeterCell)
                {
                    break;
                }
            }
            ParameterCell = (PlaceHolder)p;
        }

        private void PopBracketKeeper()
        {
            for (var p = NearestBracketKeeper.Parent; p != null; p = p.Parent)
            {
                var pAsPlaceHolder = p as PlaceHolder;
                if (pAsPlaceHolder != null && (pAsPlaceHolder.IsBracketKeeper || pAsPlaceHolder.IsRootKeeper))
                {
                    NearestBracketKeeper = pAsPlaceHolder;
                    break;
                }
            }
        }

        private int GetBinaryOperatorPriority(string operatorContent)
        {
            switch (operatorContent)
            {
                case ".":
                    return 1;
                case "*":
                case "/":
                    return 2;
                case "+":
                case "-":
                    return 3;
                case "!=":
                case "==":
                case ">":
                case "<":
                case ">=":
                case "<=":
                    return 4;
                case "&&":
                    return 5;
                case "||":
                    return 6;
            }
            throw new Exception("Unknown operator");
        }

        #endregion
    }
}
