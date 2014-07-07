using QSharp.Shader.Graphics.Base.Exceptions;
using QSharp.Shader.Graphics.Base.Optics;
using QSharp.Shader.Graphics.RayTracer;

namespace QSharp.Shader.Graphics.Csg
{
    /// <summary>
    ///  a static class that contains CSG ray tracing related fundamental data structures
    /// </summary>
    public static class CsgRayTrace
    {
        #region Nested types

        /// <summary>
        ///  interface for specifying what the state of intersection process should contain
        /// </summary>
        public interface IIntersectState
        {
            #region Properties

            /// <summary>
            ///  the distance between the start of the ray and the intersection
            /// </summary>
            /// <remarks>
            ///  the reason why a Distance property is added to
            ///  the state class is that to avoid re-triggering
            ///  intersection calculation, we need to save 
            ///  the distance obtained in last calculation
            ///  (the entire invocation of IntersectNext) for
            ///  use in the next round of calculation
            /// </remarks>
            float Distance { get; set; }

            /// <summary>
            ///  the direction in which the tracing ray approaches the surface
            /// </summary>
            /// <remarks>
            ///  note that we need to keep directional and undirectional
            ///  intersection separate, so the intersection state has its
            ///  undirectional part plus a direction indicator
            /// </remarks>
            Intersection.Direction Direction { get; set; }

            /// <summary>
            ///  unidirecitonal component of the intersection the state is currently associated with
            /// </summary>
            UndirectionalIntersection UndirectionalIntersection { get; set; }

            /// <summary>
            ///  surface where the intersection the state is currenlty associated with is on
            /// </summary>
            IRayTraceSurface Surface { get; set; }

            /// <summary>
            ///  true if the intersection of this branch is activated 
            /// </summary>
            bool Selected { get; set; }

            #endregion
        }

        /// <summary>
        ///  trivial implementation of IIntersectState
        /// </summary>
        public class IntersectState : IIntersectState
        {
            #region Properties

            #region Implementation of IIntersectState

            /// <summary>
            ///  the distance between the start of the ray and the intersection
            /// </summary>
            public float Distance { get; set; }

            /// <summary>
            ///  the direction in which the tracing ray approaches the surface
            /// </summary>
            public Intersection.Direction Direction { get; set; }

            /// <summary>
            ///  unidirecitonal component of the intersection the state is currently associated with
            /// </summary>
            public UndirectionalIntersection UndirectionalIntersection { get; set; }

            /// <summary>
            ///  surface where the intersection the state is currenlty associated with is on
            /// </summary>
            public IRayTraceSurface Surface { get; set; }

            /// <summary>
            ///  true if the intersection of this branch is activated 
            /// </summary>
            public bool Selected { get; set; }

            #endregion

            #endregion
        }

        /// <summary>
        ///  terminal state which represents the imaginary last intersection on the tracing ray at infinity
        /// </summary>
        public class TerminalState : IIntersectState
        {
            #region Fields

            /// <summary>
            ///  backing field for the direction in which the tracing ray approaches the surface
            /// </summary>
            private readonly Intersection.Direction _direction;

            #endregion

            /// <summary>
            ///  
            /// </summary>
            /// <param name="direction"></param>
            public TerminalState(Intersection.Direction direction)
            {
                _direction = direction;
            }

            /// <summary>
            ///  the terminal state is always the last to select, so this property is always false
            /// </summary>
            public bool Selected 
            {
                get { return false; }
                set { /*do nothing*/ /*throw new GraphicsException("Not to be invoked");*/ }
            }

            /// <summary>
            ///   the distance between the start of the ray and the intersection, which is infinite in this case
            /// </summary>
            public float Distance
            {
                get { return float.PositiveInfinity; }
                set { throw new GraphicsException("Not to be invoked"); }
            }

            /// <summary>
            ///  the direction in which the tracing ray approaches the surface
            /// </summary>
            public Intersection.Direction Direction
            {
                get { return _direction; }
                set { throw new GraphicsException("Not to be invoked"); }
            }

            /// <summary>
            ///  unidirecitonal component of the intersection the state is currently associated with
            /// </summary>
            public UndirectionalIntersection UndirectionalIntersection
            {
                get { return null; }
                set { throw new GraphicsException("Not to be invoked"); }
            }

            /// <summary>
            ///  surface where the intersection the state is currenlty associated with is on
            ///  in this case there is no substantive surface attached so it returns null
            /// </summary>
            public IRayTraceSurface Surface
            {
                get { return null; }
                set { throw new GraphicsException("Not to be invoked"); }
            }

            /// <summary>
            ///  one of the two practically possible terminal states, where the tracing ray approaches
            ///  the infinity from outside an object
            /// </summary>
            public static TerminalState EnterInstance = new TerminalState(
                Intersection.Direction.ToInterior);

            /// <summary>
            ///  one of the two practically possible terminal states, where the tracing ray approaches
            ///  the infinity from inside an object
            /// </summary>
            public static TerminalState ExitInstance = new TerminalState(
                Intersection.Direction.ToExterior);
        }

        /// <summary>
        ///  interface for all classes to implement that allow intersections to be 
        ///  investegated along tracing ray that potentially passes their instances
        /// </summary>
        public interface IIntersectable
        {
            /// <param name="state">
            ///  the state left off by last invocation. distance direction of access
            ///  intersection and surface can be retrieved from the state object
            /// </param>
            /// <param name="ray">
            ///  the tracing ray along which the intersections with the current interesectable
            ///  object are to be found.
            /// </param>
            /// <returns></returns>
            /// <remarks>
            ///  The implementation must involve a mechanism that keeps track of 
            ///  the intersection position in the 'state' object after each invocation 
            ///  of the method. so the ray source is not necessarily updated to the
            ///  new position after each invocation; the implementation should get 
            ///  information regarding the start point from the state object
            /// </remarks>
            IIntersectState IntersectNext(IIntersectState state, Ray ray);
        }

        #endregion
    }
}
