using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.RayTracer
{
    /// <summary>
    ///  interface that represents any surface in a ray tracing model where the ray may
    ///  change its course some other characters
    /// </summary>
    public interface IRayTraceSurface
    {
        #region Methods

        /// <summary>
        ///  returns the total light that goes through the interface to add to the tracing ray
        /// </summary>
        /// <param name="ray">the ray that comes into the surface</param>
        /// <param name="renderer">the renderer that is responsible for the tracing</param>
        /// <param name="intersection">intersection where the ray and the surface meet</param>
        /// <param name="obj1">the object that the tracing ray enters the surface from</param>
        /// <param name="obj2">the object that the tracing ray leaves the surface for</param>
        /// <param name="depth">current level of tracing</param>
        /// <param name="light">the light that goes back through the tracing ray into the first object</param>
        void Distribute(
            Ray ray,
            RayTraceRenderer renderer,
            Intersection intersection,
            IRayTraceOptical obj1,
            IRayTraceOptical obj2,
            int depth,
            out Light light
        );

        #endregion
    }
}
