using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.RayTracer
{
    /// <summary>
    ///  interface that represents the luminant characteristic of a ray-trace luminant object
    /// </summary>
    public interface IRayTraceLuminant
    {
        #region Methods

        /// <summary>
        ///  get the light the object produces with respect to the tracing ray
        /// </summary>
        /// <param name="ray">the ray in which the light to return is produced by the object and is returned by the method</param>
        /// <param name="intersection">intersection of the ray and the object</param>
        /// <param name="light">light returned</param>
        void GetLight(Ray ray, Intersection intersection, out Light light);

        #endregion
    }
}
