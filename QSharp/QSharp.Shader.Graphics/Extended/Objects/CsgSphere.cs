using System;
using QSharp.Shader.Graphics.Base.Geometry;
using QSharp.Shader.Graphics.Csg;
using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.Extended.Objects
{
    /// <summary>
    ///  class that represents a sphere that both has useful features and
    ///  can form a csg composite shape.
    /// </summary>
    public class CsgSphere : UsefulSphere, ICsgRayTraceLeaf
    {
        #region Nested types

        /// <summary>
        ///  intersect-state customized specifically for sphere intersection processing purposes
        /// </summary>
        protected class IntersectState : CsgRayTrace.IntersectState
        {
            public Intersection SecondIntersection { get; set; }
        }

        #endregion 

        #region Constructors

        /// <summary>
        ///  parameterless constructor that mainly calls corresponding base constructor
        /// </summary>
        public CsgSphere()
            : base()
        {
        }

        /// <summary>
        ///  constructor that initializes the sphere with specified parameters
        /// </summary>
        /// <param name="cx">x component of central position</param>
        /// <param name="cy">y component of central position</param>
        /// <param name="cz">z component of central position</param>
        /// <param name="radius">radius of the sphere</param>
        public CsgSphere(float cx, float cy, float cz, float radius)
            : base(cx, cy, cz, radius)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///  returns the intersections between the ray and the sphere 
        /// </summary>
        /// <param name="ray">the ray to intersect the sphere with</param>
        /// <param name="intsa">intersection on the near surface of the sphere (front) if any</param>
        /// <param name="intsb">intersection on the far surface of the sphere (back) if any</param>
        /// <returns></returns>
        protected bool IntersectBoth(Ray ray, out Intersection intsa, out Intersection intsb)
        {
            intsa = intsb = null;

            Vector4f p0 = ray.Source;
            Vector4f v1 = ray.Direction;

            Vector4f p = Sphere.Center - p0;
            float crPv = p.GetInnerProductNormalized(v1);
            if (crPv <= 0)
            {
                return false;
            }

            float d = v1.GetSquaredLengthNormalized();
            float sqrtLenV1 = (float)Math.Sqrt((double)d);

            Vector4f v = v1;
            v *= crPv / d;
            p -= v;
            d = p.GetSquaredLengthNormalized();

            d = Sphere.Radius * Sphere.Radius - d;
            if (d < 0)
                return false;

            d = (float)Math.Sqrt((float)d);

            v1 *= d / sqrtLenV1;

            v = p;  // store p
            v += v1;
            Vector4f point = Sphere.Center;
            point -= v;

            // the light that comes in
            Vector4f normal = point - Sphere.Center;
            normal.Unitize();
            intsa = new Intersection(point, normal, Intersection.Direction.ToInterior);

            v1 -= p;
            point = Sphere.Center;
            point += v1;

            // the light that comes out
            normal = Sphere.Center - point;
            normal.Unitize();
            intsb = new Intersection(point, normal, Intersection.Direction.ToExterior);

            return true;
        }

        /// <summary>
        ///  processes the current intersection and steps to the next intersection
        /// </summary>
        /// <param name="state">state of the intersection processing</param>
        /// <param name="ray">ray along which intersections with the sphere are made</param>
        /// <returns>the updated state</returns>
        public CsgRayTrace.IIntersectState IntersectNext(CsgRayTrace.IIntersectState state, Ray ray)
        {
            CsgRayTrace.IIntersectState local = state as CsgRayTrace.IIntersectState;

            if (local == null || local.Surface == null)
            {
                if (IsRayInside(ray))
                {
                    Intersection intersection;
                    if (IntersectLeave(ray, out intersection))
                    {
                        local = new IntersectState();
                        local.Surface = InnerSurface;
                        local.UndirectionalIntersection = new UndirectionalIntersection(intersection);
                        local.Direction = Intersection.Direction.ToExterior;
                        ((IntersectState)local).SecondIntersection = null;
                    }
                    else
                    {
                        // fatal error, regard this case as if the ray has nothing to do with the sphere
                        // and so it enter the sphere at the infintite postition.
                        local = new IntersectState();
                        // TODO check the logic here
                        // set surface to null to indicate it as an special case and make sure it will go to this upper branch again
                        local.Surface = null;   
                        local.UndirectionalIntersection = new UndirectionalIntersection(intersection);
                        local.Direction = Intersection.Direction.ToInterior;
                        ((IntersectState)local).SecondIntersection = null;
                    }
                }
                else
                {
                    Intersection intsenter, intsleave;
                    if (IntersectBoth(ray, out intsenter, out intsleave))
                    {
                        local = new IntersectState();
                        local.Surface = OuterSurface;
                        local.UndirectionalIntersection = new UndirectionalIntersection(intsenter);
                        local.Direction = Intersection.Direction.ToInterior;
                        ((IntersectState)local).SecondIntersection = intsleave;
                    }
                    else
                    {
                        local = CsgRayTrace.TerminalState.EnterInstance;
                    }
                }
            }
            else
            {
                if (((IntersectState)local).SecondIntersection != null)
                {
                    local.UndirectionalIntersection = new UndirectionalIntersection(((IntersectState)local).SecondIntersection);
                    ((IntersectState)local).SecondIntersection = null;
                    local.Surface = InnerSurface;
                    local.Direction = Intersection.Direction.ToExterior;
                }
                else
                {
                    local.Surface = null;
                    local.Direction = Intersection.Direction.ToInterior;
                    local.Distance = float.PositiveInfinity;
                }
            }

            if (local != null && local.Surface != null)
            {
                local.Distance = ray.Source.GetDistanceNormalized(local.UndirectionalIntersection.Position);
            }

            return local;
        }

        #endregion
    }
}
