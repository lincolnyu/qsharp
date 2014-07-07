namespace QSharp.Shader.Graphics.Base.Geometry
{
    /// <summary>
    ///  interface that abstracts a transform pair that provides two (at least conceptually) mutually reverse transform operations
    ///  whose type of input and output objects are both Vector4f
    /// </summary>
    public interface IVector4fTransformerPair
    {
        /// <summary>
        ///  transforms in forward direction
        /// </summary>
        /// <param name="input">input to transform from</param>
        /// <param name="output">output to transform to</param>
        /// <returns>true if the transform succeeded</returns>
        bool ForwardTransform(Vector4f input, out Vector4f output);

        /// <summary>
        ///  transforms in backward direction
        /// </summary>
        /// <param name="input">input to transform from</param>
        /// <param name="output">output to transform to</param>
        /// <returns>true if the transform succeeded</returns>
        bool BackwardTransform(Vector4f input, out Vector4f output);
    }
}
