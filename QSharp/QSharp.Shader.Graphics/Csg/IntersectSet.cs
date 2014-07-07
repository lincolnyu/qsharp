using QSharp.Shader.Graphics.Base.Optics;
using QSharp.Shader.Graphics.RayTracer;

namespace QSharp.Shader.Graphics.Csg
{
    /// <summary>
    ///  class that represents intersection set which interates through
    ///  all the intersections on the given tracing ray with its associated
    ///  CSG shape 
    /// </summary>
    public class IntersectSet
    {
        #region Properties
        
        /// <summary>
        ///  the details of the current intersection
        /// </summary>
        public CsgRayTrace.IIntersectState State { get; protected set; }

        /// <summary>
        ///  The CSG object associated with this intersection set
        /// </summary>
        protected CsgRayTrace.IIntersectable Intersectable { get; set; }

        /// <summary>
        ///  The tracing ray that the intersections are on
        /// </summary>
        protected Ray Ray { get; set; }

        /// <summary>
        ///  if the current intersection with the CSG ( or equivalently the intersection set associated) 
        ///  is currently being selected (hit by the current probe)
        /// </summary>
        public bool Selected
        {
            get { return State.Selected; }
            set { State.Selected = value; }
        }

        /// <summary>
        ///  returns whether selected (just hit) or not the current examined point 
        ///  is inside or outside the CSG object
        /// </summary>
        public bool IsIn
        {
            get
            {
                return Selected? State.Direction == Intersection.Direction.ToInterior
                           : State.Direction == Intersection.Direction.ToExterior;
            }
        }

        /// <summary>
        ///  returns whether the current intersection in the set is at infinity
        /// </summary>
        public bool IsFinite
        {
            get
            {
                return !float.IsPositiveInfinity(State.Distance);
            }
        }

        /// <summary>
        ///  returns the surface of the current intersection
        /// </summary>
        public IRayTraceSurface Surface
        {
            get
            {
                return State.Surface;
            }
        }

        /// <summary>
        ///  returns the unidirectional component of the current intersection
        /// </summary>
        public UndirectionalIntersection UndirectionalIntersection
        {
            get
            {
                return State.UndirectionalIntersection;
            }
        }

        /// <summary>
        ///  returns the direction of the current intersection
        /// </summary>
        public Intersection.Direction Direction
        {
            get
            {
                return State.Direction;
            }
        }

        /// <summary>
        ///  returns the distance between the start of the ray and the current intersection
        /// </summary>
        public float Distance
        {
            get
            {
                return State.Distance;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates an intersection set with specified ray, initial intersection state (details) and CSG
        /// </summary>
        /// <param name="ray">ray to search intersections on</param>
        /// <param name="state">state (details) of the initial (first) intersection</param>
        /// <param name="intersectable"></param>
        public IntersectSet(Ray ray, CsgRayTrace.IIntersectState state,
            CsgRayTrace.IIntersectable intersectable)
        {
            Ray = ray;
            Intersectable = intersectable;

            if (state == null)
            {
                // if initial intersection state is not specified (in most cases it is not)
                // the constructor automatically searches for the first intersection along the ray
                state = Intersectable.IntersectNext(null, Ray);
            }
            State = state;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  move the intersect state to next if possible
        ///  State has to be non-null
        /// </summary>
        protected void MoveNextIfAllowed()
        {
            if (Selected)
            {   // only selected interseciton set is eligible to move forward.
                State = Intersectable.IntersectNext(State, Ray);
            }
        }

        /// <summary>
        ///  select between two candidates of IntersectionSet types
        ///  from possibly two CSG siblings based on their availability
        ///  distance to the ray source. as a rule, it is always the
        ///  candidate that is closer to the source that is selected
        ///  unavailable intersection set is equivalent to intersection
        ///  at infinity position
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Select(IntersectSet a, IntersectSet b)
        {
            a.MoveNextIfAllowed();
            b.MoveNextIfAllowed();

            if (float.IsPositiveInfinity(a.State.Distance) && float.IsPositiveInfinity(b.State.Distance))
            {
                a.Selected = true;
                b.Selected = true;
                return false;
            }

            if (float.IsPositiveInfinity(b.State.Distance))
            {
                a.Selected = true;
                b.Selected = false;
                return true;
            }

            if (float.IsPositiveInfinity(a.State.Distance))
            {
                b.Selected = true;
                a.Selected = false;
                return true;
            }

            if (a.Distance < b.Distance)
            {
                a.Selected = true;
                b.Selected = false;
                return true;
            }

            if (b.Distance < a.Distance)
            {
                a.Selected = false;
                b.Selected = true;
                return true;
            }

            a.Selected = true;
            b.Selected = true;
            return true;
        }

        #endregion
    }
}
