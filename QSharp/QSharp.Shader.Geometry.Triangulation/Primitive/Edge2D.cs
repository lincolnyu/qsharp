using System;
using QSharp.Shader.Geometry.Euclid2D;

namespace QSharp.Shader.Geometry.Triangulation.Primitive
{
    public class Edge2D : IEdge2D, IDisposable
    {
        #region Properties

        public IVector2D Vertex1
        {
            get
            {
                return V1;
            }
        }

        public IVector2D Vertex2
        {
            get
            {
                return V2;
            }
        }

        /// <summary>
        ///  First vertex
        /// </summary>
        public Vector2D V1
        {
            get; private set;
        }

        /// <summary>
        ///  Second vertex
        /// </summary>
        public Vector2D V2
        {
            get; private set;
        }

        /// <summary>
        ///  Surface on the left
        /// </summary>
        public MeshSurface Surface1 { get; set; }

        /// <summary>
        ///  Surface on the right
        /// </summary>
        public MeshSurface Surface2 { get; set; }

        public double SquaredLength
        {
            get; private set;
        }

        public double Length
        {
            get
            {
                return Math.Sqrt(SquaredLength);
            }
        }

        #endregion

        #region Methods

        #region IDisposable members

        public void Dispose()
        {
            Disconnect();
        }

        #endregion

        public bool Contains(IVector2D vertex, double epsilon)
        {
            throw new NotImplementedException();
        }

        private void UpdateLength()
        {
            var dx = Vertex1.X - Vertex2.X;
            var dy = Vertex1.Y - Vertex2.Y;
            SquaredLength = dx*dx + dy*dy;
        }

        public void Connect(Vector2D vertex1, Vector2D vertex2)
        {
            V1 = vertex1;
            V2 = vertex2;
            V1.Edges.Add(this);
            V2.Edges.Add(this);
            UpdateLength();
        }

        public void Disconnect()
        {
            if (V1 != null)
            {
                V1.Edges.Remove(this);
                V1 = null;
            }
            if (V2 != null)
            {
                V2.Edges.Remove(this);
                V2 = null;
            }
            if (Surface1 != null)
            {
                Surface1.Dispose();
                Surface1 = null;
            }
            if (Surface2 != null)
            {
                Surface2.Dispose();
                Surface2 = null;
            }
            SquaredLength = 0;
        }

        #endregion

    }
}
