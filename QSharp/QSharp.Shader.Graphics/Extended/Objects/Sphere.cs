using QSharp.Shader.Graphics.Base.Geometry;
using QSharp.Shader.Graphics.Csg;

namespace QSharp.Shader.Graphics.Extended.Objects
{
    public class Sphere : ICsgShape, ISphere
    {
        #region Fields

        /// <summary>
        ///  center of the sphere
        /// </summary>
        private readonly Vector4f _center = new Vector4f();

        /// <summary>
        ///  radius of the sphere
        /// </summary>
        private readonly float _radius = 1f;

        #endregion

        #region Properties

        /// <summary>
        ///  gets (and sets if later on permitted) the center of the sphere
        /// </summary>
        public Vector4f Center
        {
            get { return _center; }
        }

        /// <summary>
        ///  gets (and sets if later on permitted) the radius of the sphere
        /// </summary>
        public float Radius
        {
            get { return _radius; }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  parameterless constructor that initializes the object with 
        ///  default values
        /// </summary>
        public Sphere()
            /*: base()*/
        {
        }

        public Sphere(float x, float y, float z, float radius)
            /*: base()*/
        {
            _center = new Vector4f(x, y, z);
            _radius = radius;
        }

        #endregion

        #region Methods

        #region Implementation of ISphere

        public bool ContainsPoint(Vector4f point)
        {
            Vector4f r = point - Center;
            float dist = r.GetLengthNormalized();
            return dist < Radius;
        }

        #endregion

        #endregion
    }
}
