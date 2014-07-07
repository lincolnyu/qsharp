using QSharp.Shader.Graphics.Base.Geometry;

namespace QSharp.Shader.Graphics.Base.Optics
{
    /// <summary>
    ///  a class that contains intersection details regardless of direction 
    ///  (how the tracing ray approaches the surface)
    /// </summary>
    public class UndirectionalIntersection
    {
        #region Properties

        /// <summary>
        ///  position of the intersection
        /// </summary>
        public Vector4f Position { get; protected set; }

        /// <summary>
        ///  normal vector of the service at the intersection.
        ///  as a rule, in a directional case (when this class is inherited)
        ///  it should point against the ray
        /// </summary>
        public Vector4f Normal { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates the class with the required details
        /// </summary>
        /// <param name="position">position of the intersection</param>
        /// <param name="normal">normal vector</param>
        public UndirectionalIntersection(Vector4f position, Vector4f normal)
        {
            Position = position;
            Normal = normal;
        }

        /// <summary>
        ///  instantiates the class with details copied from another instance
        /// </summary>
        /// <param name="that">another instance to copy from</param>
        public UndirectionalIntersection(UndirectionalIntersection that)
            : this(that.Position, that.Normal)
        {
        }

        #endregion
    }
}
