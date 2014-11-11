using System;

namespace QSharp.Shader.Geometry.Common2D
{
    public class Vector2D
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector2D(IVertex2D a, IVertex2D b)
        {
            X = b.X - a.X;
            Y = b.Y - a.Y;
        }

        public double InnerProductWith(Vector2D other)
        {
            return X * other.X + Y * other.Y;
        }

        public Vector2D Add(Vector2D other)
        {
            return new Vector2D(X + other.X, Y + other.Y);
        }

        public Vector2D Subtract(Vector2D other)
        {
            return new Vector2D(X - other.X, Y - other.Y);
        }

        public Vector2D Multiply(double scale)
        {
            return new Vector2D(X * scale, Y * scale);
        }

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

        public static Vector2D operator *(Vector2D v, double scale)
        {
            return v.Multiply(scale);
        }
    }
}
