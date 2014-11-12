using System;
using QSharp.Shader.Geometry.Euclid2D;

namespace QSharp.Shader.Geometry.Triangulation.Primitive
{
    public class Edge2D : IEdge2D
    {
        #region Fields

        private IVector2D _vertex1;
        private IVector2D _vertex2;

        #endregion

        #region Properties

        public IVector2D Vertex1
        {
            get
            {
                return _vertex1;
            }
            set
            {
                _vertex1 = value;
                UpdateLength();
            }
        }

        public IVector2D Vertex2
        {
            get
            {
                return _vertex2;
            }
            set
            {
                _vertex2 = value;
                UpdateLength();
            }
        }

        public MeshSurface Surface1 { get; set; }
        public MeshSurface Surface2 { get; set; }


        public double Length { get; private set; }
        public double SquaredLength { get; private set; }

        #endregion

        #region Methods

        public bool Contains(IVector2D vertex, double epsilon)
        {
            throw new NotImplementedException();
        }

        private void UpdateLength()
        {
            var dx = Vertex1.X - Vertex2.X;
            var dy = Vertex1.Y - Vertex2.Y;
            SquaredLength = dx*dx + dy*dy;
            Length = Math.Sqrt(SquaredLength);
        }
        #endregion
    }
}
