using QSharp.Shader.Geometry.Euclid2D;

namespace QSharp.Shader.Geometry.Triangulation.Primitive
{
    public class Triangle2D : MeshSurface, ITriangle2D
    {
        #region Methods

        public IVector2D Vertex1 { get; set; }
        public IVector2D Vertex2 { get; set; }
        public IVector2D Vertex3 { get; set; }

        public IEdge2D Edge23 { get; set; }
        public IEdge2D Edge31 { get; set; }
        public IEdge2D Edge12 { get; set; }

        public IVector2D Circumcenter { get; set; }

        public double Circumradius { get; set; }

        #endregion

        #region Methods

        public bool Contains(IMutableVector2D vertex)
        {
            throw new System.NotImplementedException();
        }

        public IVector2D GetOpposite(IEdge2D edge)
        {
            throw new System.NotImplementedException();
        }

        public IEdge2D GetOpposite(IMutableVector2D vertex)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
