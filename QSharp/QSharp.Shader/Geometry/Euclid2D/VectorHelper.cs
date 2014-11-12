namespace QSharp.Shader.Geometry.Euclid2D
{
    public static class VectorHelper
    {
        #region Methods

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

        #endregion
    }
}
