using QSharp.Shader.Graphics.Base.Geometry;
using QSharp.Shader.Graphics.RayTracer;

namespace QSharp.Shader.Graphics.Base.Optics
{
    /// <summary>
    ///  class that represents a tracing ray
    /// </summary>
    public class Ray
    {
        #region Properties

        /// <summary>
        ///  property for getting and setting where the ray starts
        /// </summary>
        public Vector4f Source { get; set; }


        /// <summary>
        ///  property for getting and setting the direction in which the ray goes
        /// </summary>
        public Vector4f Direction { get; set; }

        /// <summary>
        ///  surface if any from where the ray starts
        /// </summary>
        public IRayTraceSurface Surface { get; set; }

        /// <summary>
        ///  if the light is passing through the above surface
        /// </summary>
        public bool Refracting { get; set; }

        #endregion

        #region Constructor 

        /// <summary>
        ///  instantiates a ray with specified details
        /// </summary>
        /// <param name="source">where the ray starts</param>
        /// <param name="direction">dirction of the ray</param>
        /// <param name="surface">surface if any from where the ray starts</param>
        /// <param name="refracting">if the light is passing through the surface</param>
        public Ray(Vector4f source, Vector4f direction, IRayTraceSurface surface = null, 
            bool refracting = false)
        {
            Source = source;
            Direction = direction;
            Surface = surface;
            Refracting = refracting;
        }

        #endregion
    }
}
