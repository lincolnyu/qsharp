using System.Collections.Generic;
using QSharp.Shader.Geometry.Common2d;

namespace QSharp.Shader.Geometry.Triangulation.Proactive
{
    class VertexQueue : Queue<IVertex2d>, IVertexQueue
    {
        public void EnqueueIfNotContaining(IVertex2d vertex)
        {
            if (!Contains(vertex))
            {
                Enqueue(vertex);
            }
        }
    }
}
