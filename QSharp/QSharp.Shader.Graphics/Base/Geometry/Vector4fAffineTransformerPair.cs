namespace QSharp.Shader.Graphics.Base.Geometry
{
    /// <summary>
    ///  class that contains a pair affine transforming matrices that are inverse of each other
    /// </summary>
    public class Vector4fAffineTransformerPair : IVector4fTransformerPair
    {
        #region Properties

        /// <summary>
        ///  property for getting reference to forward transformer
        /// </summary>
        public Matrix4f Forward
        {
            get; 
            protected set;
        }

        /// <summary>
        ///  property for getting reference to backward transformer
        /// </summary>
        public Matrix4f Backward
        {
            get;
            protected set;
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a transformer pair with specified matrix pair
        /// </summary>
        /// <param name="forward">forward transformer</param>
        /// <param name="backward">backward transformer</param>
        public Vector4fAffineTransformerPair(Matrix4f forward, Matrix4f backward)
        {
            Forward = forward;
            Backward = backward;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  transforms the specified input vector to output using forward transformer
        /// </summary>
        /// <param name="input">input vector</param>
        /// <param name="output">resultant vector</param>
        /// <returns>true if the transformation succeeded</returns>
        public virtual bool ForwardTransform(Vector4f input, out Vector4f output)
        {
            output = input.TransformUsing(Forward);
            return true;
        }

        /// <summary>
        ///  transforms the specified input vector to output using backward transformer
        /// </summary>
        /// <param name="input">input vector</param>
        /// <param name="output">resultant vector</param>
        /// <returns>true if the transformation succeeded</returns>
        public virtual bool BackwardTransform(Vector4f input, out Vector4f output)
        {
            output = input.TransformUsing(Backward);
            return true;
        }

        #endregion
    }
}
