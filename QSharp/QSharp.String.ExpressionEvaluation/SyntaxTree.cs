using System;
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

        public void Parse(string expression, ITokenizer tokenizer)
        {
            var tokens = tokenizer.Tokenize(expression);
            var treeBuilder = new TreeBuilder(tokenizer);
            treeBuilder.Parse(tokens);
            Root = treeBuilder.Root;
        }

        public void Parse(string expression)
        {
            var tokenizer = DefaultTokenizer.Instance;
            var tokens = tokenizer.Tokenize(expression);
            var treeBuilder = new TreeBuilder(tokenizer);
            treeBuilder.Parse(tokens);
            Root = treeBuilder.Root;
        }

        #endregion
    }
}
