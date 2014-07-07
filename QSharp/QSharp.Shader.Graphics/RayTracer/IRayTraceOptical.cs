using QSharp.Shader.Graphics.Base.Objects;
using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.RayTracer
{
    /// <summary>
    ///  an interface with ray-trace specific features added to IOptical
    /// </summary>
    public interface IRayTraceOptical : IOptical
    {
        #region Methods

        /// <summary>
        ///  returns the total light intensity that goes into the tracer ray
        ///  segment that starts from the optical, probably succeeding another
        ///  tracer ray segment.
        ///  it actually adds to <paramref name="light"/> the total physical light 
        ///  that goes in the opposite direction as tracer ray <paramref name="ray"/> 
        ///  goes
        /// </summary>
        /// <param name="renderer">the ray-trace renderer that looks after the ray-tracing process</param>
        /// <param name="ray">the tracer ray that comes out from the optical</param>
        /// <param name="depth">depth of the tracing</param>
        /// <param name="light">light to add the current calculation to</param>
        /// <remarks>
        ///  A tracer ray is an imaginary ray that normally originates from a certain point on the 
        ///  screen that is presented to the observer and goes reversely along one of the physical
        ///  rays that contribute to that point. The calculation of its optical characteristics it 
        ///  follows the same rule of reflection and refraction as the that for physical ray does
        ///  and produces a reasonably correct result due to the law of reversiblity of light paths. 
        ///  It is the fundamental technique for ray-trace approach.
        /// </remarks>
        void Travel(RayTraceRenderer renderer, Ray ray,
            int depth, out Light light);

        /// <summary>
        ///  returns if <paramref name="ray"/> hits the object and enters
        ///  and where the hit happens
        /// </summary>
        /// <param name="renderer">the renderer that carries out the tracing</param>
        /// <param name="ray">a ray that is outside the optical object and may come in</param>
        /// <param name="intersection">the intersection where it hits if it does</param>
        /// <param name="surface">the surface structure</param>
        /// <returns>true if it hits</returns>
        bool Import(RayTraceRenderer renderer, Ray ray,
            out Intersection intersection, out IRayTraceSurface surface);

        /// <summary>
        ///  returns if <paramref name="ray"/> hits the object and goes out
        ///  and where the hit happens
        /// </summary>
        /// <param name="renderer">the renderer that carries out the tracing</param>
        /// <param name="ray">a ray that's inside the optical object and may come out</param>
        /// <param name="intersection">the intersection where it hits if it does</param>
        /// <param name="surface">the surface structure</param>
        /// <param name="adjacent">the object that is adjacent to the current at the intersection</param>
        /// <returns>true if it hits</returns>
        bool Export(RayTraceRenderer renderer, Ray ray,
            out Intersection intersection, out IRayTraceSurface surface,
            out IRayTraceOptical adjacent);

        #endregion
    }
}
