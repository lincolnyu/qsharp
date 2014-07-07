using System;
using System.Collections.Generic;
using QSharp.Shader.Graphics.Base.Objects;
using QSharp.Shader.Graphics.Base.Optics;
using QSharp.Shared;

namespace QSharp.Shader.Graphics.RayTracer
{
    /// <summary>
    ///  simple ether space where lights can freely travels
    /// </summary>
    public class SimpleEther : RayTraceOptical
    {
        #region Properties

        /// <summary>
        ///  backing field for the scene that the ether is attached to
        /// </summary>
        public SimpleScene Scene { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates an ether that is attached to the specified scene
        /// </summary>
        /// <param name="scene">the scene the ether is attached to</param>
        public SimpleEther(SimpleScene scene)
        {
            Scene = scene;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  returns true and describes how the intersection and surface are like 
        ///  if <paramref name="ray"/> enters the ether; as lights freely go through
        ///  ether, this method is not necessary and not supposed to be called
        /// </summary>
        /// <param name="renderer">the renderer that carries out the tracing</param>
        /// <param name="ray">a ray that is outside the optical object and may come in</param>
        /// <param name="intersection">the intersection where it hits if it does</param>
        /// <param name="surface">the surface structure</param>
        /// <returns>true if it hits</returns>
        public override bool Import(RayTraceRenderer renderer, Ray ray, 
            out Intersection intersection, out IRayTraceSurface surface)
        {
            throw new QException("Method not supposed to be invoked");
        }

        /// <summary>
        ///  returns true and gives the details of intersection, surface and adjacent object
        ///  if <paramref name="ray"/> hits the object and goes out and where the hit happens
        /// </summary>
        /// <param name="renderer">the renderer that carries out the tracing</param>
        /// <param name="ray">a ray that's inside the optical object and may come out</param>
        /// <param name="intersection">the intersection where it hits if it does</param>
        /// <param name="surface">the surface structure</param>
        /// <param name="adjacent">the object that is adjacent to the current at the intersection</param>
        /// <returns>true if it comes into the esther</returns>
        public override bool Export(RayTraceRenderer renderer, Ray ray, 
            out Intersection intersection, out IRayTraceSurface surface,
            out IRayTraceOptical adjacent)
        {
            List<IOptical> opticals = Scene.Opticals;
            float minSqrDist = float.PositiveInfinity;

            intersection = null;
            surface = null;
            adjacent = null;

            /*
             * go through all opticals to find a candidate to be hit
             * by the tracer ray
             */
            foreach (IOptical optical in opticals)
            {
                var rtoTest = optical as IRayTraceOptical;

                if (rtoTest == null)
                {
                    continue;
                }

                Intersection intsTest;
                IRayTraceSurface srfcTest;

                /*
                 * check if the object is reachable from ether (outside)
                 */
                bool res = rtoTest.Import(renderer, ray, out intsTest, out srfcTest);
                if (!res)
                {   /*
                     * impossible to reach object, simply skip it
                     */
                    continue;
                }

                /*
                 * get the distance to the object, if it is smaller than the 
                 * current minimum, it replaces the current mimium as a candidate
                 */
                float sqrDist = ray.Source.GetSquaredDistance(intsTest.Position);
                if (sqrDist < minSqrDist)
                {
                    intersection = intsTest;
                    minSqrDist = sqrDist;
                    adjacent = rtoTest;
                    surface = srfcTest;
                }
            }

            /*
             * as we are considering exporting from ether, it's equivalent in the
             * current design to importing to the optical object
             */
            if (intersection != null)
            {
                intersection.AccessDirection = Intersection.Direction.ToInterior;
            }
            // TODO check consistency between non-nullity of intersection and adjacent object?

            return adjacent != null;
        }

        #endregion
    }
}
