using System;
using QSharp.Shader.Graphics.Base.Geometry;
using QSharp.Shader.Graphics.RayTracing;

namespace QSharp.Shader.Graphics.Extended.Objects
{
    /// <summary>
    ///  it's a simple implementation of object that embodies both
    ///  sphere and some optical properties
    /// </summary>
    public class SimpleOpticalSphere : SimpleOptical, ISphere
    {
        #region Properties

        /// <summary>
        ///  internal sphere that provides spherical properties for this object
        /// </summary>
        protected Sphere Sphere { get; set; }

        #endregion

        #region Properties

        #region Implementation of ISphere

        /// <summary>
        ///  proeprty that returns the centre of the sphere
        /// </summary>
        public Vector4f Center
        {
            get { return Sphere.Center; }
        }

        /// <summary>
        ///  property that returns the radius of the sphere
        /// </summary>
        public float Radius
        {
            get { return Sphere.Radius; }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        ///  parameterless constructor that instantiates the object with default settings
        /// </summary>
        public SimpleOpticalSphere() 
            /*: base()*/
        {
            Sphere = new Sphere();
        }

        /// <summary>
        ///  constructor that takes a few paramters that specify the size and position of the sphere
        /// </summary>
        /// <param name="cx">x component of central position</param>
        /// <param name="cy">y component of central position</param>
        /// <param name="cz">z component of central position</param>
        /// <param name="radius">radius of the sphere</param>
        public SimpleOpticalSphere(float cx, float cy, float cz, float radius)
            /*: base()*/
        {
            Sphere = new Sphere(cx, cy, cz, radius);
        }

        #endregion

        #region Methods

        #region Implementation of ISphere

        /// <summary>
        ///  checks to see if the sphere contains the specified point
        /// </summary>
        /// <param name="point">the point to check and see whether this sphere contains</param>
        /// <returns>true if the sphere contains the point</returns>
        public bool ContainsPoint(Vector4f point)
        {
            return Sphere.ContainsPoint(point);
        }

        #endregion

        #region Implementation of SimpleOptical

        /// <summary>
        ///  finds out the intersection of the ray and the sphere
        /// </summary>
        /// <param name="ray">the ray to intersect with the sphere</param>
        /// <param name="rsm">a flag suggests if whereabout the ray starts</param>
        /// <param name="intersection">the first intersection of the ray and the sphere </param>
        /// <returns></returns>
        public override bool IntersectNearest(Base.Optics.Ray ray, RayStartMode rsm, out Base.Optics.Intersection intersection)
        {
            if (rsm == RayStartMode.Uknown)
            {
                rsm = IsRayInside(ray) ? RayStartMode.Inside : RayStartMode.Outside;
            }
            switch (rsm)
            {
                case RayStartMode.Inside:
                    return IntersectLeave(ray, out intersection);
                case RayStartMode.Outside:
                    return IntersectEnter(ray, out intersection);
                default:
                    intersection = null;
                    return false;
            }
        }

        /// <summary>
        ///  check to see if the ray starts from inside this sphere
        /// </summary>
        /// <param name="ray">the ray to check</param>
        /// <returns>true if the ray starts from inside the sphere</returns>
        public override bool IsRayInside(Base.Optics.Ray ray)
        {
            const float epsilon = 0.01f;
            Vector4f raySource = ray.Source;
            Vector4f r = raySource - Sphere.Center;
            float dist = r.GetLengthNormalized();

            if (dist < Sphere.Radius - epsilon)
            {
                return true;
            }
            if (dist > Sphere.Radius + epsilon)
            {
                return false;
            }
            if (ray.Surface == null)
            {
                return dist < Sphere.Radius;
            }

            if (ray.Surface == InnerSurface)
            {
                return !ray.Refracting;
            }
            else if (ray.Surface == OuterSurface)
            {
                return ray.Refracting;
            }

            return dist < Sphere.Radius;
        }

        #endregion

        /// <summary>
        ///  finds out the intersection of the ray and the sphere provided that the ray is known to 
        ///  start within the sphere
        /// </summary>
        /// <param name="ray">the ray to intersect the sphere</param>
        /// <param name="intersection">the intersection figured out by the method if any</param>
        /// <returns>true if they intersect</returns>
        protected bool IntersectEnter(Base.Optics.Ray ray, out Base.Optics.Intersection intersection)
        {
            // assuming the ray position and direction vectors are normalized
            intersection = null;

            Vector4f p0 = ray.Source;
            Vector4f v1 = ray.Direction;

            Vector4f p = Sphere.Center - p0;

            float crPv = p.GetInnerProductNormalized(v1);
            if (crPv <= 0)
                return false;

            float d = v1.GetSquaredLengthNormalized();
            float sqrtLenV1 = (float)Math.Sqrt(d);

            Vector4f v = v1;
            v *= crPv / d;
            p -= v;
            d = p.GetSquaredLengthNormalized();
            d = Sphere.Radius * Sphere.Radius - d;

            if (d < 0)
                return false;

            d = (float)Math.Sqrt(d);

            v1 *= d / sqrtLenV1;
            p += v1;
            Vector4f point = Sphere.Center;
            point -= p;

            Vector4f normal = point - Sphere.Center;
            normal.Unitize();
            intersection = new Base.Optics.Intersection(point, normal,
                Base.Optics.Intersection.Direction.ToInterior);

            return true;
        }

        /// <summary>
        ///  finds out the intersection of the ray and the sphere provided that the ray is known to 
        ///  start outside the sphere
        /// </summary>
        /// <param name="ray">the ray to intersect the sphere</param>
        /// <param name="intersection">the intersection figured out by the method if any</param>
        /// <returns>true if they intersect</returns>
        protected bool IntersectLeave(Base.Optics.Ray ray, out Base.Optics.Intersection intersection)
        {
            // assuming the ray position and direction vectors are normalized
            // the ray start is assumed to be inside the sphere
            Vector4f p0 = ray.Source;
            Vector4f v1 = ray.Direction;

            Vector4f p = Sphere.Center - p0;

            float crPv = p.GetInnerProductNormalized(v1);
            float d = v1.GetSquaredLengthNormalized();
            float sqrtLenV1 = (float)Math.Sqrt(d);

            Vector4f v = v1;
            v *= crPv / d;
            p -= v;

            d = p.GetSquaredLengthNormalized();
            d = Sphere.Radius * Sphere.Radius - d;

            // 'd' is always positive if the ray is actually emitted from inside
            d = (float)Math.Sqrt(d);

            v1 *= d / sqrtLenV1;
            v1 -= p;
            Vector4f point = Sphere.Center;
            point += v1;

            Vector4f normal = Sphere.Center - point;
            normal.Unitize();
            intersection = new Base.Optics.Intersection(point, normal, Base.Optics.Intersection.Direction.ToExterior);

            return true;
        }

        #endregion
    }
}
