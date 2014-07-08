using System.Collections.Generic;

namespace QSharp.String.ExpressionEvaluation
{
    public class Fork : Node
    {
        public IList<Node> Children { get; private set; }

        public Fork()
        {
            Children = new List<Node>();
        }
    }
}
