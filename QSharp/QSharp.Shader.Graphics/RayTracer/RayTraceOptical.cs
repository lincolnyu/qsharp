using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.RayTracer
{
    /// <summary>
    ///  an abstract class that specifies functionalities a ray-trace optical
    ///  object should have as well as provides common implementation if any
    ///  such an object would use
    /// </summary>
    public abstract class RayTraceOptical : IRayTraceOptical
    {
        #region Enumerations
        
        /// <summary>
        ///  the mode of how the ray starts from the object
        ///  i.e. whether the ray goes inwards (starts from inside)
        ///  or goes outwards (starts from outside)
        /// </summary>
        public enum RayStartMode
        {
            Uknown,
            Inside,
            Outside
        }
        
        #endregion

        #region Methods

        #region Implementation of IRayTraceOptical

        /// <summary>
        ///  adds to <paramref name="light"/> the total light that goes in the opposite
        ///  direction as <paramref name="ray"/> goes
        /// </summary>
        /// <param name="renderer">the renderer that carries out the ray-tracing process</param>
        /// <param name="ray">the direction of which is used as a reference</param>
        /// <param name="depth">current depth of tracing</param>
        /// <param name="light">light to add current calculation to</param>
        public virtual void Travel(RayTraceRenderer renderer, Ray ray, 
            int depth, out Light light)
        {
            /*
             * the total light adds up from zero.
             */
            light = new Light(0f, 0f, 0f);

            if (depth >= renderer.TotalDepth)
            {
                return;
            }

            IRayTraceOptical adjacentRto/* = null*/;
            Intersection intersection/* = null*/;
            IRayTraceSurface surface/* = null*/;

            bool res = Export(renderer, ray, out intersection,
                out surface, out adjacentRto);

            if (!res)
            {
                return;
            }

            Light tempLight;
            var adjacentRtl = adjacentRto as IRayTraceLuminant;

            if (adjacentRtl != null)
            {
                adjacentRtl.GetLight(ray, intersection, out tempLight);
                light.AddBy(tempLight);
            }

            if (surface != null)
            {
                surface.Distribute(ray, renderer, intersection, this,
                    adjacentRto, depth + 1, out tempLight);
                light.AddBy(tempLight);
            }
        }

        /// <summary>
        ///  returns true and gives intersection and surface details 
        ///  if <paramref name="ray"/> hits the object and enters
        ///  and where the hit happens
        /// </summary>
        /// <param name="renderer">the renderer that carries out the tracing</param>
        /// <param name="ray">a ray that is outside the optical object and may come in</param>
        /// <param name="intersection">the intersection where it hits if it does</param>
        /// <param name="surface">the surface structure</param>
        /// <returns>true if it hits</returns>
        public abstract bool Import(
            RayTraceRenderer renderer,
            Ray ray,
            out Intersection intersection,
            out IRayTraceSurface surface
        );

        /// <summary>
        ///  returns true and gives the details of intersection, surface and adjacent object
        ///  if <paramref name="ray"/> hits the object and goes out and where the hit happens
        /// </summary>
        /// <param name="renderer">the renderer that carries out the tracing</param>
        /// <param name="ray">a ray that's inside the optical object and may come out</param>
        /// <param name="intersection">the intersection where it hits if it does</param>
        /// <param name="surface">the surface structure</param>
        /// <param name="adjacent">the object that is adjacent to the current at the intersection</param>
        /// <returns>true if it hits</returns>
        public abstract bool Export(
            RayTraceRenderer renderer,
            Ray ray,
            out Intersection intersection,
            out IRayTraceSurface surface,
            out IRayTraceOptical adjacent
        );

        #endregion

        #endregion
    }
}
