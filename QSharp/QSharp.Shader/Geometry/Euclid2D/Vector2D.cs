using System;

namespace QSharp.Shader.Geometry.Euclid2D
{
    public class Vector2D : IMutableVector2D
    {
        #region Properties

        #region IMutableVector2D members

        public double X { get; set; }
        public double Y { get; set; }

        #endregion

        public double SquaredLength
        {
            get
            {
                return X * X + Y * Y;
            }
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

        public Vector2D()
        {
        }

        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        ///  Returns a vector that points from the first give vector to the second
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector</param>
        public Vector2D(IVector2D a, IVector2D b)
        {
            X = b.X - a.X;
            Y = b.Y - a.Y;
        }

        public double InnerProductWith(IVector2D other)
        {
            return this.InnerProduct(other);
        }

        public double OuterProductWith(IVector2D other)
        {
            return this.OuterProduct(other);
        }

        public Vector2D Add(IVector2D other)
        {
            return Instantiate(X + other.X, Y + other.Y);
        }

        public Vector2D Subtract(IVector2D other)
        {
            return Instantiate(X - other.X, Y - other.Y);
        }

        public Vector2D Multiply(double scale)
        {
            return Instantiate(X * scale, Y * scale);
        }

        public Vector2D Negate()
        {
            return Instantiate(-X, -Y);
        }

        public static double operator *(Vector2D vA, Vector2D vB)
        {
            return vA.InnerProductWith(vB);
        }

        public static Vector2D operator +(Vector2D vA, Vector2D vB)
        {
            return vA.Add(vB);
        }

        public static Vector2D operator -(Vector2D vA, Vector2D vB)
        {
            return vA.Subtract(vB);
        }

        public static Vector2D operator -(Vector2D v)
        {
            return v.Negate();
        }

        public static Vector2D operator *(Vector2D v, double scale)
        {
            return v.Multiply(scale);
        }

        public static Vector2D operator /(Vector2D v, double div)
        {
            return v.Multiply(1/div);
        }

        protected virtual Vector2D Instantiate(double x, double y)
        {
            return new Vector2D(x, y);
        }

        #endregion
    }
}
