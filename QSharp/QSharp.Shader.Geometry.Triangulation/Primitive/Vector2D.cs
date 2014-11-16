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

        #endregion
    }
}
