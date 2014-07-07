using System;
using QSharp.Shader.Graphics.Base.Exceptions;
using QSharp.Shader.Graphics.Base.Optics;
using QSharp.Shader.Graphics.RayTracer;
using QSharp.Shared;

namespace QSharp.Shader.Graphics.Csg
{
    /// <summary>
    ///  csg-based optical node that enables ray tracing mechanism to work on it
    /// </summary>
    public abstract class CsgRayTraceOpticalNode : CsgNode, CsgRayTrace.IIntersectable, IRayTraceOptical
    {
        #region Constants
        
        /// <summary>
        ///  the amount of distance within which it is so close as to be
        ///  considered as there being no distance at all
        ///  distance at all
        /// </summary>
        public const float AlmostOn = 0.01f;

        #endregion

        #region Nested types

        /// <summary>
        ///  class that represents the state of intersection process
        /// </summary>
        public class IntersectState : CsgRayTrace.IIntersectState
        {
            #region Properties

            #region Implementation of CsgRayTrace.IntersectState

            /// <summary>
            ///  if the state is activated
            /// </summary>
            public bool Selected { get; set; }

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

            #endregion

            /// <summary>
            ///  left object
            /// </summary>
            public CsgRayTrace.IIntersectState Left { get; set; }

            /// <summary>
            ///  right object
            /// </summary>
            public CsgRayTrace.IIntersectState Right { get; set; }

            #endregion

            #region Constructors

            /// <summary>
            ///  instantiates the class with specified left and right objects
            /// </summary>
            /// <param name="left">left object</param>
            /// <param name="right">right object</param>
            public IntersectState(CsgRayTrace.IIntersectState left, CsgRayTrace.IIntersectState right)
            {
                Selected = false;
                Left = left;
                Right = right;
            }

            #endregion
        }

        /// <summary>
        ///  class that encapsulates implementation of ray-trace optical functionalities 
        ///  and is to be aggregated with the nesting class
        /// </summary>
        protected class AggregatedRaytraceOptical : RayTraceOptical
        {
            #region Properties

            /// <summary>
            ///  owner of this object
            /// </summary>
            protected CsgRayTraceOpticalNode Owner { get; set; }

            #endregion

            #region Consttructors

            /// <summary>
            ///  instantiates the class with specified owner
            /// </summary>
            /// <param name="owner"></param>
            public AggregatedRaytraceOptical(CsgRayTraceOpticalNode owner)
            {
                Owner = owner;
            }

            #endregion

            #region Methods

            /// <summary>
            ///  figures out the details of intersection when the light is entering this object
            /// </summary>
            /// <param name="renderer">renderer responsible for providing rendering context and functionalities</param>
            /// <param name="ray">ray that is supposed to enter the object</param>
            /// <param name="intersection">intersection between the ray and the object</param>
            /// <param name="surface">surface where the ray enters</param>
            /// <returns>true if the ray enters ok</returns>
            public override bool Import(RayTraceRenderer renderer, Ray ray, 
                out Intersection intersection, out IRayTraceSurface surface)
            {
                intersection = null;
                surface = null;

                var state = Owner.IntersectNext(null, ray) as IntersectState;
                if (state == null) return false;
                
                intersection = new Intersection(state.UndirectionalIntersection, state.Direction);
                surface = state.Surface;

                bool enter = state.Direction == Intersection.Direction.ToInterior;

                float distance = state.Distance;
                if (!enter)
                {
                    /*
                     * the tracer ray may come from inside, which is not supposed
                     * to be like that
                     */
                    if (distance < AlmostOn)
                    {
                        /*
                         * <remarks>
                         *  due to proximity to the surface possibly as a result
                         *  of inaccuracy, give it after the first hit from inside
                         *  another chance to hit the object from outside
                         *  
                         *  TODO: review the reasonability of this approach
                         * </remarks>
                         */
                        state = Owner.IntersectNext(state, ray) as IntersectState;
                        if (state == null) return false;    // not accessible with the second attempt
                        intersection = new Intersection(state.UndirectionalIntersection, state.Direction);
                        surface = state.Surface;

                        enter = state.Direction == Intersection.Direction.ToInterior;

                        /*
                         * whether it is from outside (perhaps if it is accessible, it's always
                         * from outside)
                         */
                        return enter;
                    }
                }

                if (surface == null)
                {
                    surface = Owner.OuterSurface;
                }

                return true;
            }

            /// <summary>
            ///  figures out the details of intersection when the light is leaving this object
            /// </summary>
            /// <param name="renderer">renderer responsible for providing rendering context and functionalities</param>
            /// <param name="ray">ray that is supposed to leave the object</param>
            /// <param name="intersection">intersection between the ray and the object</param>
            /// <param name="surface">surface where the ray leaves</param>
            /// <param name="adjacent">the adjacent object if any the ray enters after leaving this object</param>
            /// <returns>true if the ray leaves ok</returns>
            public override bool Export(RayTraceRenderer renderer, Ray ray,
                out Intersection intersection, out IRayTraceSurface surface,
                out IRayTraceOptical adjacent)
            {
                intersection = null;
                surface = null;
                adjacent = null;

                var state = Owner.IntersectNext(null, ray) as IntersectState;
                if (state == null) return false;
                intersection = new Intersection(state.UndirectionalIntersection, state.Direction);
                surface = state.Surface;

                bool enter = state.Direction == Intersection.Direction.ToInterior;

                if (enter)
                {
                    /*
                     * <remarks>
                     *  the tracer ray may come from outside, which is not supposed
                     *  to be like that.
                     *  the process is similar to that in the method 'Import'
                     * </remarks>
                     */
                    if (state.Distance < AlmostOn)
                    {
                        /* 
                         * <remarks>
                         *  TODO: review the reasonability of this approach
                         * </remarks>
                         */
                        state = Owner.IntersectNext(state, ray) as IntersectState;
                        if (state == null) return false;
                        intersection = new Intersection(state.UndirectionalIntersection, state.Direction);
                        surface = state.Surface;
                        enter = state.Direction == Intersection.Direction.ToInterior;

                        return !enter;
                    }
                }

                if (surface == null)
                {
                    surface = Owner.OuterSurface;
                }

                var scene = renderer.Scene as SimpleScene;
                if (scene == null)
                {
                    throw new GraphicsException("SimpleScene expected in CSG exporting");
                }

                adjacent = scene.Ether;

                return true;
            }

            #endregion
        }

        #endregion

        #region Properties

        /// <summary>
        ///  aggregated object that backs raytrace optical functionalities of this object
        /// </summary>
        protected AggregatedRaytraceOptical Rto { get; set; }

        /// <summary>
        ///  outer surface of this instance
        /// </summary>
        public abstract IRayTraceSurface OuterSurface { get; }

        /// <summary>
        ///  inner surface of this instance
        /// </summary>
        public abstract IRayTraceSurface InnerSurface { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///  constructor that involves parameters for initializing base class and
        ///  instantiates the aggregating field
        /// </summary>
        /// <param name="left">left object</param>
        /// <param name="right">right object</param>
        /// <param name="oper">operation on these two objects</param>
        protected CsgRayTraceOpticalNode(ICsgShape left, ICsgShape right, Operation oper)
            : base(left, right, oper)
        {
            Rto = new AggregatedRaytraceOptical(this);
        }

        #endregion

        #region Methods

        #region Implementation of IRayTraceOptical

        /// <summary>
        ///  travels through into this object (view IRayTraceOptical for more detail)
        ///  the process is performed by the backing aggregating object
        /// </summary>
        /// <param name="renderer">renderer responsible for providing rendering context and functionalities</param>
        /// <param name="ray">ray that travels through the object</param>
        /// <param name="depth">current depth of tracing</param>
        /// <param name="light">light that comes from this trace</param>
        public virtual void Travel(RayTraceRenderer renderer, Ray ray, int depth, out Light light)
        {
            Rto.Travel(renderer, ray, depth, out light);
        }

        /// <summary>
        ///  returns if the ray enters the object and how (view IRayTraceOptical for more detail)
        ///  the process is performed by the backing aggregating object
        /// </summary>
        /// <param name="renderer">renderer responsible for providing rendering context and functionalities</param>
        /// <param name="ray">ray that travels through the object</param>
        /// <param name="intersection">intersection where the ray enters the object</param>
        /// <param name="surface">surface where the ray enters the object</param>
        /// <returns>true if it enters ok</returns>
        public virtual bool Import(RayTraceRenderer renderer, Ray ray,
            out Intersection intersection, out IRayTraceSurface surface)
        {
            return Rto.Import(renderer, ray, out intersection, out surface);
        }

        /// <summary>
        ///  returns if the ray exits the object and how (view IRayTraceOptical for more detail)
        ///  the process is performed by the backing aggregating object
        /// </summary>
        /// <param name="renderer">renderer responsible for providing rendering context and functionalities</param>
        /// <param name="ray">ray that travels through the object</param>
        /// <param name="intersection">intersection where the ray leaves the object</param>
        /// <param name="surface">surface where the ray leaves the object</param>
        /// <param name="adjacent">the object the ray enters after leaving the current</param>
        /// <returns>true if exits ok</returns>
        public virtual bool Export(RayTraceRenderer renderer, Ray ray,
            out Intersection intersection, out IRayTraceSurface surface,
            out IRayTraceOptical adjacent)
        {
            return Rto.Export(renderer, ray, out intersection, 
                out surface, out adjacent);
        }

        #endregion

        #region Implementation of CsgRayTrace.IIntersectable

        /// <summary>
        ///  returns the surface the ray hits on the first occasion
        /// </summary>
        /// <param name="state">details of the current intersection</param>
        /// <param name="ray">ray along which intersections are to be found</param>
        /// <returns>details of the intersection after the current</returns>
        public virtual CsgRayTrace.IIntersectState IntersectNext(CsgRayTrace.IIntersectState state, Ray ray)
        {
            if (state == null)
            {
                /*
                 * it's looking for the first intersection on the ray
                 */
                state = new IntersectState(null, null);
            }

            var localState = state as IntersectState;

            if (localState == null)
            {
                throw new GraphicsException("Invalid intersection state");
            }

            localState.UndirectionalIntersection = null;
            localState.Surface = null;
            localState.Distance = float.PositiveInfinity;

            var setL = new IntersectSet(ray, localState.Left, Left as CsgRayTrace.IIntersectable);
            var setR = new IntersectSet(ray, localState.Right, Right as CsgRayTrace.IIntersectable);


            /*
             * either is selected
             * and now it has got across the first surface
             */

            bool inside;

            switch (Oper)
            {
                case Operation.Union:
                    inside = setL.IsIn || setR.IsIn;
                    break;
                case Operation.Intersection:
                    inside = setL.IsIn && setR.IsIn;
                    break;
                case Operation.Subtraction:
                    inside = setL.IsIn && !setR.IsIn;
                    break;
                default:
                    throw new QException("Unknown CSG operation type");
            }

            var enter = !inside;

            while (true)
            {
                if (!IntersectSet.Select(setL, setR))
                {
                    return GetTerminalState(setL, setR);
                }

                switch (Oper)
                {
                    case Operation.Union:
                        /*
                         * actually the purpose of variable 'enter' here should be interpreted as 
                         * 'whether the probe is expected to be inside or not after selection'
                         */
                        if ((setL.IsIn || setR.IsIn) == enter)
                        {
                            if (setL.Selected)
                            {
                                localState.Surface = setL.Surface;
                                localState.UndirectionalIntersection = setL.UndirectionalIntersection;
                                localState.Distance = setL.Distance;
                            }
                            else /* setR.IsSelected */
                            {
                                localState.Surface = setR.Surface;
                                localState.UndirectionalIntersection = setR.UndirectionalIntersection;
                                localState.Distance = setR.Distance;
                            }
                            /*
                             * update the states
                             */
                            localState.Direction = enter ? Intersection.Direction.ToInterior
                                : Intersection.Direction.ToExterior;
                            localState.Left = setL.State;
                            localState.Right = setR.State;
                            return localState;
                        }

                        if (!enter)
                        {   /* 
                             * going out but the following two conditions make it
                             * impossible, quit in advance
                             */
                            if (!setL.IsFinite && setL.IsIn)
                            {   /*
                                 * no longer able to go out 
                                 * may only happen with some open shapes
                                 * like hyperboloid, however these shapes
                                 * are not recommended to be used as 
                                 * grapchical shapes, same as below
                                 */
                                return CsgRayTrace.TerminalState.ExitInstance;
                            }
                            if (!setR.IsFinite && setR.IsIn)
                            {
                                return CsgRayTrace.TerminalState.ExitInstance;
                            }
                        }
                        break;
                    case Operation.Intersection:
                        /*
                         * this condition judgement works in similar way as that in Union case does
                         */
                        if ((setL.IsIn && setR.IsIn) == enter)
                        {
                            if (setL.Selected)
                            {
                                localState.Surface = setL.Surface;
                                localState.UndirectionalIntersection = setL.UndirectionalIntersection;
                                localState.Distance = setL.Distance;
                            }
                            else /* setR.IsSelected */
                            {
                                localState.Surface = setR.Surface;
                                localState.UndirectionalIntersection = setR.UndirectionalIntersection;
                                localState.Distance = setR.Distance;
                            }

                            localState.Left = setL.State;
                            localState.Right = setR.State;
                            localState.Direction = enter ? Intersection.Direction.ToInterior
                                : Intersection.Direction.ToExterior;
                            return localState;
                        }
                        if (enter)
                        {   /*
                             * expected to get in but the following two conditions make it
                             * impossible, quit in advance
                             */
                            if (!setL.IsFinite && !setL.IsIn)
                            {
                                return CsgRayTrace.TerminalState.EnterInstance;
                            }
                            if (!setR.IsFinite && !setR.IsIn)
                            {
                                return CsgRayTrace.TerminalState.EnterInstance;
                            }
                        }
                        break;
                    case Operation.Subtraction:
                        /*
                         * this condition judgement works in similar way
                         * as that in Union case does
                         */
                        if ((setL.IsIn && !setR.IsIn) == enter)
                        {
                            if (setL.Selected)
                            {
                                localState.Surface = setL.Surface;
                                localState.UndirectionalIntersection = setL.UndirectionalIntersection;
                                localState.Distance = setL.Distance;
                            }
                            else /* setR.IsSelected */
                            {
                                localState.Surface = setR.Surface;
                                localState.UndirectionalIntersection = setR.UndirectionalIntersection;
                                localState.Distance = setR.Distance;
                            }

                            localState.Left = setL.State;
                            localState.Right = setR.State;
                            localState.Direction = enter ? Intersection.Direction.ToInterior
                                : Intersection.Direction.ToExterior;
                            return localState;
                        }
                        if (enter)
                        {   /* 
                             * hopes to get in 
                             * however it's impossible, quit in advance
                             */
                            if (!setL.IsFinite && !setL.IsIn)
                            {
                                return CsgRayTrace.TerminalState.EnterInstance;
                            }
                            if (!setR.IsFinite && setR.IsIn)
                            {
                                return CsgRayTrace.TerminalState.EnterInstance;
                            }
                        }
                        break;
                }
            }
        }

        #endregion

        /// <summary>
        ///  returns corresponding terminal state for the given pair of interseciton sets
        /// </summary>
        /// <param name="setL">intersection set for the left object</param>
        /// <param name="setR">intersection set for the right object</param>
        /// <returns>the terminal state as per the states of the objects</returns>
        protected CsgRayTrace.IIntersectState GetTerminalState(IntersectSet setL, IntersectSet setR)
        {
            switch (Oper)
            {
                case Operation.Union:
                    return (setL.IsIn || setR.IsIn)
                               ? CsgRayTrace.TerminalState.ExitInstance
                               : CsgRayTrace.TerminalState.EnterInstance;
                case Operation.Intersection:
                    return (setL.IsIn && setR.IsIn)
                               ? CsgRayTrace.TerminalState.ExitInstance
                               : CsgRayTrace.TerminalState.EnterInstance;
                case Operation.Subtraction:
                    return (setL.IsIn && !setR.IsIn)
                               ? CsgRayTrace.TerminalState.ExitInstance
                               : CsgRayTrace.TerminalState.EnterInstance;
                default:
                    throw new QException("Unknown CSG operation type");
            }
        }

        #endregion

    }
}
