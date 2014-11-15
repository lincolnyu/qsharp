using System;

namespace QSharp.Shader.Geometry.Euclid2D
{
    public static class VectorHelper
    {
        #region Methods

        /// <summary>
        ///  Adds two vectors
        /// </summary>
        /// <param name="augend">The augend</param>
        /// <param name="addend">The addend</param>
        /// <param name="sum">The sum</param>
        public static void Add(this IVector2D augend, IVector2D addend, IMutableVector2D sum)
        {
            sum.X = augend.X + addend.X;
            sum.Y = augend.Y + addend.Y;
        }

        /// <summary>
        ///  Subtracts one vector from the other
        /// </summary>
        /// <param name="minuend">The minuend</param>
        /// <param name="subtrahend">The subtrahend</param>
        /// <param name="diff">The difference</param>
        public static void Subtract(this IVector2D minuend, IVector2D subtrahend, IMutableVector2D diff)
        {
            diff.X = minuend.X - subtrahend.X;
            diff.Y = minuend.Y - subtrahend.Y;
        }

        /// <summary>
        ///  The magnitude of the outer product of two 2D vectors (direction being parellel to z always)
        /// </summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <returns>The magnitude of the outer product, positive meaning in the direction of z coordinate</returns>
        public static double OuterProduct(this IVector2D v1, IVector2D v2)
        {
            return v1.X*v2.Y - v1.Y*v2.X;
        }

        /// <summary>
        ///  The inner product of the two products
        /// </summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <returns>The inner product of the two products</returns>
        public static double InnerProduct(this IVector2D v1, IVector2D v2)
        {
            return v1.X*v2.X + v1.Y*v2.Y;
        }

        /// <summary>
        ///  Normalizes the vector and stores the result in <paramref name="result"></paramref>
        /// </summary>
        /// <param name="v">The vector to normalize</param>
        /// <param name="result">The normalized result</param>
        public static void Normalize(this IVector2D v, IMutableVector2D result)
        {
            var s = v.X*v.X + v.Y*v.Y;
            s = Math.Sqrt(s);
            result.X = v.X/s;
            result.Y = v.Y/s;
        }

        /// <summary>
        ///  Normalizes the vector and stores the result in itself
        /// </summary>
        /// <param name="v">The vector to normalize</param>
        public static void Normalize(this IMutableVector2D v)
        {
            v.Normalize(v);
        }

        /// <summary>
        ///  Return the normal of the specified vector on its left hand side
        ///  Note the normal is not normalized
        /// </summary>
        /// <param name="v">The vector to get the left normal of</param>
        /// <param name="normal">The normal vector</param>
        public static void GetLeftNormal(this IVector2D v, IMutableVector2D normal)
        {
            normal.X = -v.Y;
            normal.Y = v.X;
        }

        /// <summary>
        ///  Return the normal of the specified vector on its right hand side
        ///  Note the normal is not normalized
        /// </summary>
        /// <param name="v">The vector to get the right normal of</param>
        /// <param name="normal">The normal vector</param>
        public static void GetRightNorm(this IVector2D v, IMutableVector2D normal)
        {
            normal.X = v.Y;
            normal.Y = -v.X;
        }


        /// <summary>
        ///  Returns the angle formed by sweeping from <paramref name="a"/> counter-clockwise to <paramref name="b"/>
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b"></param>
        public static double GetAngle(this IVector2D a, IVector2D b)
        {
            var thA = Math.Atan2(a.Y, a.X);
            var thB = Math.Atan2(b.Y, b.X);
            var angle = thB - thA;
            if (angle < 0)
            {
                angle += Math.PI * 2;
            }
            return angle;
        }

        #endregion
    }
}
