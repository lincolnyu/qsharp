using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.RayTracing
{
    /// <summary>
    ///  interface that represents a scene where objects and atmosphere are potentially
    ///  set up and ray-tracing process over them is enabled so that a particular ray from
    ///  the scene can be traced and the light through the ray can be calculated
    /// </summary>
    public interface IRayTraceScene
    {
        #region Methods

        /// <summary>
        ///  initiates tracing process for specified ray with specified depth
        /// </summary>
        /// <param name="renderer">the renderer that conducts the ray tracing process</param>
        /// <param name="ray">a ray from the scene for which the light is to be figured out</param>
        /// <param name="depth">depth of the tracing (number of levels)</param>
        /// <param name="light">light that is calculated out</param>
        void Trace(RayTraceRenderer renderer, Ray ray, int depth, ref Light light);

        #endregion
    }
}
