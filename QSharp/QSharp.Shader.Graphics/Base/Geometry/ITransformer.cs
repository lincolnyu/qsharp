namespace QSharp.Shader.Graphics.Base.Geometry
{
    /// <summary>
    ///  interface that abstracts thes operation that converts an input 
    ///  of type <typeparamref name="TA"/> into an output of type
    ///  <typeparamref name="TB"/>
    /// </summary>
    /// <typeparam name="TA">type of the input object</typeparam>
    /// <typeparam name="TB">tyep of the output object</typeparam>
    public interface ITransformer<in TA, TB>
    {
        #region Methods
        
        /// <summary>
        ///  transforms <paramref name="input"/> into <paramref name="output"/>
        /// </summary>
        /// <param name="input">input to transform from</param>
        /// <param name="output">output to transform to</param>
        /// <returns></returns>
        bool Transform(TA input, out TB output);

        #endregion
    }
}
