using System;

namespace QSharp.Shader.Geometry.Common2d
{
    public class Vector2d
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector2d(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector2d(IVertex2d a, IVertex2d b)
        {
            X = b.X - a.X;
            Y = b.Y - a.Y;
        }

        public double InnerProductWith(Vector2d other)
        {
            return X * other.X + Y * other.Y;
        }

        public Vector2d Add(Vector2d other)
        {
            return new Vector2d(X + other.X, Y + other.Y);
        }

        public Vector2d Subtract(Vector2d other)
        {
            return new Vector2d(X - other.X, Y - other.Y);
        }

        public Vector2d Multiply(double scale)
        {
            return new Vector2d(X * scale, Y * scale);
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

        public static double operator *(Vector2d vA, Vector2d vB)
        {
            return vA.InnerProductWith(vB);
        }

        public static Vector2d operator +(Vector2d vA, Vector2d vB)
        {
            return vA.Add(vB);
        }

        public static Vector2d operator -(Vector2d vA, Vector2d vB)
        {
            return vA.Subtract(vB);
        }

        public static Vector2d operator *(Vector2d v, double scale)
        {
            return v.Multiply(scale);
        }
    }
}
