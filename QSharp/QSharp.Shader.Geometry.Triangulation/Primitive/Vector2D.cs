using System.Collections.Generic;
using BaseVector2D = QSharp.Shader.Geometry.Euclid2D.Vector2D;

namespace QSharp.Shader.Geometry.Triangulation.Primitive
{
    /// <summary>
    ///  A class that represetns a 2D vector object for triangulation
    /// </summary>
    public class Vector2D : BaseVector2D
    {
        #region Constructors

        /// <summary>
        ///  Instantiates a Vector2D object
        /// </summary>
        public Vector2D()
        {
            Edges = new HashSet<Edge2D>();
        }

        private Vector2D(double x, double y) : base(x, y)
        {
            Edges = new HashSet<Edge2D>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///  All the incidental edges
        /// </summary>
        public ISet<Edge2D> Edges { get; private set; }

        #endregion

        #region Methods

        protected override BaseVector2D Instantiate(double x, double y)
        {
            return new Vector2D(x, y);
        }

        public Edge2D TestConnection(Vector2D other)
        {
            var c1 =Edges.Count;
            var c2 = other.Edges.Count;
            Vector2D v1, v2;
            if (c1 < c2)
            {
                v1 = this;
                v2 = other;
            }
            else
            {
                v1 = other;
                v2 = this;
            }
            foreach (var e in v1.Edges)
            {
                if (e.V1 == v2 || e.V2 == v2)
                {
                    return e;
                }
            }
            return null;
        }

        public static Vector2D operator -(Vector2D v)
        {
            return (Vector2D)v.Negate();
        }

        #endregion
    }
}
