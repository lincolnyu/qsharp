using QSharp.Shader.Graphics.Base.Geometry;
using QSharp.Shader.Graphics.Base.Objects;
using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.RayTracing
{
    /// <summary>
    ///  class that represents point light type
    /// </summary>
    /// <remarks>
    ///  although it is not a typical luminant object,
    ///  we still make it implement the interface ILuminant
    ///  because it is conceptually convenient to do so
    /// </remarks>
    public class PointLight : ILuminant
    {
        #region Properties

        /// <summary>
        ///  property for setting and getting the position of the light
        /// </summary>
        public Vector4f Position { get; protected set; }

        /// <summary>
        ///  property for setting and getting the intensity of the diffuse component of the light
        /// </summary>
        public Light LightD { get; protected set; }

        /// <summary>
        ///  property for setting and getting the intensity of the specular component of the light
        /// </summary>
        public Light LightS { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates the point light with specified position and default intensity properties
        /// </summary>
        /// <param name="position"></param>
        public PointLight(Vector4f position)
            : this(position, 
                   new Light(0.3f, 0.3f, 0.3f), 
                   new Light(0.3f, 0.3f, 0.3f))
        {
        }

        /// <summary>
        ///  instantiates the point light with specified position and intensity properties
        /// </summary>
        /// <param name="position">position of the light</param>
        /// <param name="lightD">diffuse component</param>
        /// <param name="lightS">specular component</param>
        public PointLight(Vector4f position, Light lightD, Light lightS)
        {
            Position = position;
            LightD = lightD;
            LightS = lightS;
        }

        #endregion
    }
}
