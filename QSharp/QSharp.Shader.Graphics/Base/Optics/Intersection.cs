using QSharp.Shader.Graphics.Base.Geometry;

namespace QSharp.Shader.Graphics.Base.Optics
{
    /// <summary>
    ///  a class that contains intersection details with direction
    ///  (which side the tracing ray approaches surface from)
    /// </summary>
    public class Intersection : UndirectionalIntersection
    {
        #region Enumerations

        /// <summary>
        ///  enumeration of values indicating whether the tracing ray
        ///  approaches the intersection from outside of the object or
        ///  inside
        /// </summary>
        public enum Direction
        {
            ToExterior, /* from inside to ouside */
            ToInterior  /* from outside to inside */
        }

        #endregion

        #region Properties

        /// <summary>
        ///  direction in which the tracing ray approaches the surface 
        /// </summary>
        public Direction AccessDirection { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates the class with all required details
        /// </summary>
        /// <param name="position">position of the intersection</param>
        /// <param name="normal">normal vector of the surface at the intersection</param>
        /// <param name="accessDirection">direction in which the tracing ray approaches the surface</param>
        public Intersection(Vector4f position, Vector4f normal, Direction accessDirection)
            : base (position, normal)
        {
            AccessDirection = accessDirection;
        }

        /// <summary>
        ///  instantiates the class with a unidirectional intersection and the access direction
        /// </summary>
        /// <param name="undirectional">unidirection intersection of which all the details should be copied to this</param>
        /// <param name="accessDirection">direction in which the tracing ray approaches the surface</param>
        public Intersection(UndirectionalIntersection undirectional, Direction accessDirection)
            : base (undirectional)
        {
            AccessDirection = accessDirection;
        }

        #endregion
    }
}
