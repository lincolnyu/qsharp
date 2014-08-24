using System;
using System.Collections.Generic;
using QSharp.String.ExpressionEvaluation;

namespace QSharp.String.Arithmetic
{
    public class Monomial
    {
        #region Constructors

        public Monomial()
        {
            FactorToPower = new Dictionary<string, int>();
            Factors = new SortedSet<string>();
        }

        #endregion

        #region Properties

        public bool Negative { get; set; }

        public string Coefficient { get; set; }
        
        public SortedSet<string> Factors { get; private set; }

        public Dictionary<string, int> FactorToPower { get; private set; }

        #endregion

        #region Methods

        public void Collect(Node node)
        {
            if (node.NodeType == Node.Type.BinaryOperator)
            {
                var fork = (Fork) node;
                if (fork.Content != "*")
                {
                    throw new ArgumentException("Invalid node for monomial");
                }
                Collect(fork);
            }
            else if (node.NodeType == Node.Type.UnaryOperator)
            {
            }
            else if (node.NodeType == Node.Type.Symbol)
            {
                var content = node.Content;
                var lowerContent = content.ToLower();
                switch (lowerContent)
                {
                    case "pi":
                        break;
                    case "e":
                        break;
                }
                if (!Factors.Contains(content))
                {
                    Factors.Add(content);
                    FactorToPower[content] = 1;
                }
                else
                {
                    FactorToPower[content]++;
                }
            }
        }

        private void Collect(Fork fork)
        {
            var left = fork.Children[0];
            var right = fork.Children[1];
        }

        #endregion
    }
}
