//#define USE_REF_01

using System;
using System.Collections.Generic;
using QSharp.Shader.Graphics.Base.Geometry;
using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.RayTracing
{
    /// <summary>
    ///  a surface class that provides ray-trace feature
    /// </summary>
    /// <remarks>
    ///  referneces:
    ///   Cambrige - 3D Computer Graphics - A Mathematical Introduction with OpenGL - 2005
    /// </remarks>
    public class SimpleSurface : IRayTraceSurface
    {
        #region Properties

        /// <summary>
        ///  all optical characteristics of the surface which is connected with the 
        ///  incoming tracer ray which goes against the normal
        /// </summary>
        protected SimpleSurfaceCharacteristics Characteristics { get; set; }

        /// <summary>
        ///  the ratio of the sine of angle between the incoming tracer light and 
        ///  the normal to that of angle between the outcoming light and the 
        ///  normal. the normal is agaist the incoming light
        /// </summary>
        protected float Eta { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates the class with specified characteristics and eta
        /// </summary>
        /// <param name="characteristics">optical characteristics of the surface</param>
        /// <param name="eta"></param>
        public SimpleSurface(SimpleSurfaceCharacteristics characteristics, float eta)
        {
            Characteristics = characteristics;
            Eta = eta;
        }

        #endregion

        #region Methods

        #region Implementation of IRayTraceSurface

        /// <summary>
        ///  returns the total light that goes through the interface to add to the tracing ray
        /// </summary>
        /// <param name="ray">the ray that comes into the surface</param>
        /// <param name="renderer">the renderer that is responsible for the tracing</param>
        /// <param name="intersection">intersection where the ray and the surface meet</param>
        /// <param name="obj1">the object that the tracing ray enters the surface from</param>
        /// <param name="obj2">the object that the tracing ray leaves the surface for</param>
        /// <param name="depth">current level of tracing</param>
        /// <param name="light">the light that goes back through the tracing ray into the first object</param>
        public void Distribute(Ray ray, RayTraceRenderer renderer,
            Intersection intersection, IRayTraceOptical obj1, IRayTraceOptical obj2,
            int depth, out Light light)
        {
            Vector4f dirIn_u = ray.Direction;
            dirIn_u.Unitize();

            CountPointLights(dirIn_u, renderer, intersection, obj1, obj2, out light);

            Ray rayOut = new Ray(intersection.Position, null, this);
            Vector4f dirOut;
            Light lightRefl;
            Light lightRefr;

            /* tracing reflect */
            CalculateRelection(intersection, dirIn_u, out dirOut);
            rayOut.Direction = dirOut;
            rayOut.Refracting = false;
            obj1.Travel(renderer, rayOut, depth, out lightRefl);

            if (Characteristics.RouTg.IsDark)
            {   /* is opaque */
                lightRefl.ScaleBy(Characteristics.RouRg);
            }
            else if (CalculateRefraction(intersection, Eta, dirIn_u, out dirOut))
            {   /* 
                 * refraction plus reflection 
                 * note that the refraction is tracing back to the light source
                 * so a false returning value from the method indicates that
                 * all lights come through the internal reflection from this side
                 * which is the case following
                 */
                rayOut.Direction = dirOut;
                rayOut.Refracting = true;
                obj2.Travel(renderer, rayOut, depth, out lightRefr);
                lightRefr.ScaleBy(Characteristics.RouTg);

                light.AddBy(lightRefr);

                lightRefr.ScaleBy(Characteristics.RouRi);
            }
            else
            {   /* internal reflection from this side */
                lightRefl.ScaleBy(Characteristics.RouRi);
            }

            light.AddBy(lightRefl);
        }

        #endregion

        /// <summary>
        ///  get light from all point lights
        /// </summary>
        /// <param name="dirIn_u"></param>
        /// <param name="renderer">the ray-trace renderer</param>
        /// <param name="intersection">intersection the light is applied onto</param>
        /// <param name="obj1">the object that the tracing ray enters the surface from</param>
        /// <param name="obj2">the object that the tracing ray leaves the surface for</param>
        /// <param name="light">the total light that all applicable point lights are combined to apply to the intersection</param>
        public void CountPointLights(Vector4f dirIn_u, RayTraceRenderer renderer,
            Intersection intersection, IRayTraceOptical obj1, IRayTraceOptical obj2,
            out Light light)
        {
            light = new Light(0f, 0f, 0f);

            System.Diagnostics.Trace.Assert(intersection.Normal.IsNormalized);

            Ray rayToLight = new Ray(intersection.Position, null);

            IRayTraceSurface dummySurface;
            IRayTraceOptical dummyOptical;

            SimpleScene scene = renderer.Scene as SimpleScene;

            List<PointLight> pointLights = scene.PointLights;

            foreach (PointLight pointLight in pointLights)
            {
                Vector4f dirToLight = pointLight.Position - intersection.Position;

                /*
                 * it is assumed that the subtraction does the normalization
                 */
                float sqrDistIntfToLight = dirToLight.GetSquaredLengthNormalized();

                float dotOut = dirToLight.GetInnerProduct(intersection.Normal);

                /*
                 * directions
                 */
                Vector4f dirTemp;
                Vector4f dirFromLight = -dirToLight;

                /*
                 *  intersection test
                 */
                Intersection intsTest;

                /*
                 * physical
                 */
                BlendedColor degreeDr = new BlendedColor(0f, 0f, 0f);
                BlendedColor degreeSr = new BlendedColor(0f, 0f, 0f);
                float phqTemp;

                bool lit = false;

                dirFromLight.Unitize();
                dirToLight.Unitize();
                rayToLight.Direction = dirToLight;

                if (dotOut >= 0)
                {   /* light on the same side as the normal points to */

                    /*
                     * if the ray doesn't encounter any other objects as it
                     * goes in the direction as specified by 'rayToLight'
                     * then a direct irradiate is ensured
                     */
                    bool possiblyObstructed = obj1.Export(renderer, rayToLight, 
                        out intsTest, out dummySurface, out dummyOptical);

                    if (possiblyObstructed)
                    {
                        float sqrDistIntfToOut
                            = rayToLight.Source.GetSquaredDistanceNormalized(
                            intsTest.Position);
                        /* 
                         * check to see the object is behind the light source
                         * if it is, then it's a direct irradiation
                         */
                        lit = sqrDistIntfToOut >= sqrDistIntfToLight;
                    }
                    else
                    {
                        lit = true;
                    }

                    if (lit)
                    {
                        BlendedColor rou_d;
                        BlendedColor rou_s;

                        /* 
                         * whether totally internal reflected
                         */

                        bool internalReflection = false;

                        /*
                         * if the surface is transmittable, say if the light from
                         * the source is possible to penetrate (refract) the surface
                         */
                        bool transmittable = !(Characteristics.RouDt.IsDark &&
                            Characteristics.RouSt.IsDark);

                        if (transmittable)
                        {
                            if (Eta > 1f)
                            {   /* 
                                 * the light goes from dense to sparse, 
                                 * the normal points from spars to dense
                                 */

                                /*
                                 * this refraction calculation is only for the 
                                 * purpose of checking if it is total internal
                                 * reflection
                                 */
                                internalReflection = !CalculateRefraction(
                                    intersection, Eta, dirFromLight, out dirTemp);
                            }
                        }

                        if (internalReflection)
                        {   /* internally reflected */
                            rou_d = Characteristics.RouDi;
                            rou_s = Characteristics.RouSi;
                        }
                        else
                        {
                            rou_d = Characteristics.RouD;
                            rou_s = Characteristics.RouS;
                        }

                        /* local diffuse reflection */

                        phqTemp = dirToLight.GetInnerProductNormalized(intersection.Normal);
                        degreeDr = phqTemp * rou_d;

                        /* local specular reflection */
                        CalculateRelection(intersection, dirIn_u, out dirTemp);

                        System.Diagnostics.Trace.Assert(dirTemp.IsNormalized);

                        /*
                         * the angle between the vector from the intersection to the light
                         * and that points to the direction of reflection, which 
                         * determines the intensity of the specular reflection
                         */
                        phqTemp = dirToLight.GetInnerProductNormalized(dirTemp);

                        if (phqTemp > 0)
                        {
                            phqTemp = (float)Math.Pow(phqTemp, Characteristics.F);
                            degreeSr = phqTemp * rou_s;
                        }
                        else
                        {
                            degreeSr.Set(0f, 0f, 0f);
                        }
                    }
                }
                else if (!(Characteristics.RouDt.IsDark && Characteristics.RouSt.IsDark))
                {   /* light on the other side and the object is not opaque */
                    float n21 = 1f / Eta;

                    bool lightThrough;

                    /*
                     * the only difference between the two segments of code below
                     * is that the second one always does the refraction calculation
                     * in order to provide the non-REF-01 version of refraction
                     * intensity measurement with the refracted ray from the light
                     */
#if USE_REF_01
                    if (n21 > 1)
                    {   /* internal reflection is possible */
                        lightThrough = CalculateRefraction(intersection, n21, 
                            dirFromLight, out dirTemp);
                    }
                    else
                    {
                        lightThrough = true;
                    }
#else
                    bool refractable = CalculateRefraction(intersection, n21, dirFromLight, out dirTemp);
                    lightThrough = n21 <= 1 || refractable;
#endif
                    if (lightThrough)
                    {
                        /*
                         * if the ray doesn't encounter any other objects as it
                         * goes in the direction as specified by 'rayToLight'
                         * then a direct irradiate is ensured
                         */
                        bool possiblyObstructed = !obj2.Export(renderer, rayToLight,
                            out intsTest, out dummySurface, out dummyOptical);

                        if (possiblyObstructed)
                        {
                            float sqrDistIntfToOut
                                = rayToLight.Source.GetSquaredDistanceNormalized(
                                intsTest.Position);
                            /* 
                             * check to see the object is behind the light source
                             * if it is, then it's a direct irradiation
                             */
                            lit = sqrDistIntfToOut >= sqrDistIntfToLight;
                        }
                        else
                        {
                            lit = true;
                        }

                        if (lit)
                        {
                            /* local diffuse refraction */
                            phqTemp = dirFromLight.GetInnerProduct(intersection.Normal);
                            degreeDr = phqTemp * Characteristics.RouSt;

                            /* local diffuse reflection */
                            
                            /*
                             * when USE_REF_01 is enabled, the intensity is calculated based 
                             * on the angle between the line connecting the intersection
                             * and the light and the virtual line along which the ray from
                             * eye goes after refraction
                             * 
                             * when USE_REF_01 is disabled, the intensity is calculated based
                             * on the angle between the line along which the refracted ray
                             * from the light and the line connecting the eye and the intersection
                             * 
                             * Note that the two angles above are not necessarily the same.
                             */
#if USE_REF_01
                            CalculateRefraction(intersection, Eta, dirIn_u, out dirTemp);
                            phqTemp = dirToLight.GetInnerProductNormalized(dirTemp);
#else
                            phqTemp = dirIn_u.GetInnerProductNormalized(dirTemp);
#endif
                            if (phqTemp > 0)
                            {
                                phqTemp = (float)Math.Pow(phqTemp, Characteristics.F);
                                degreeSr = phqTemp * Characteristics.RouSt;
                            }
                            else
                            {
                                degreeSr.Set(0f, 0f, 0f);
                            }
                        }
                    }
                }

                if (lit)
                {
                    /*
                     * get light from source times degree
                     * use operator is not only for conciseness but
                     * also for creating a new object to be assigned
                     */
                    Light light_s = pointLight.LightS * degreeSr;
                    Light light_d = pointLight.LightD * degreeDr;

                    /*
                     * ambient
                     */
                    Light lightAmbient = scene.AmbientLight * Characteristics.RouA;

                    /*
                     * add up
                     */
                    light.AddBy(light_s);
                    light.AddBy(light_d);
                    light.AddBy(lightAmbient);
                }
            }
        }

        /// <summary>
        ///  calculates the refection at the intersection for the tracer ray
        ///  entering as dirIn and leaving as dirOut
        /// </summary>
        /// <param name="intersection">the normal has to be unitised</param>
        /// <param name="dirIn">
        ///  direction in which the tracer ray enters the intersection 
        ///  it must be normalised
        /// </param>
        /// <param name="dirOut">direction in which the tracer ray leaves the intersection</param>
        /// <remarks>
        ///  it doesn't actually matter whether or not the normal points to the side
        ///  from which the light comes in, this method it always gives the reflected 
        ///  ray from the surface that is specified by the normal
        /// </remarks>
        public static void CalculateRelection(Intersection intersection,
            Vector4f dirIn, out Vector4f dirOut)
        {
            Vector4f normal = intersection.Normal;
            float a = dirIn.GetInnerProductNormalized(normal);
            a += a;
            /* 
             * this way, 'intersection.Normal' and 'dirIn'
             * are unchanged as intended 
             */
            normal *= a;    
            dirOut = dirIn - normal;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="intersection"></param>
        /// <param name="eta12">
        ///  the ratio of sine of the (negative) normal and the 
        ///  outcoming light to sine of the normal and incoming light
        ///  1 denotes the side the light comes from 2 the side it goes into
        /// </param>
        /// <param name="dirIn_u">
        ///  direction in which the tracer ray enters the intersection, which 
        ///  has to be normalised
        /// </param>
        /// <param name="dirOut_u">direction in which the tracer ray leaves the intersection</param>
        /// <returns>true if it's not complete internal reflection where ther's not refraction</returns>
        /// <remarks>
        ///  the method is also normal direction inpendent which is because 
        ///  the vector that represents the incoming light is in the  
        /// </remarks>
        public static bool CalculateRefraction(Intersection intersection,
            float eta12, Vector4f dirIn_u, out Vector4f dirOut_u)
        {
            Vector4f normal = intersection.Normal;
            float cosTheta1 = dirIn_u.GetInnerProductNormalized(normal);
            float sinTheta1 = (float)Math.Sqrt(1 - cosTheta1 * cosTheta1);

            Vector4f normalOut = normal * cosTheta1;

            dirOut_u = dirIn_u - normalOut;

            float sinTheta2 = sinTheta1 * eta12;
            if (sinTheta2 >= 1)
            {   /* total internal reflection */
                return false;
            }

            float cosTheta2 = (float)Math.Sqrt(1 - sinTheta2 * sinTheta2);
            float tanTheta2 = sinTheta2 / cosTheta2;

            dirOut_u.Unitize();
            dirOut_u.ScaleBy(tanTheta2);
            if (cosTheta1 < 0)
            {   /* 
                 * normal points to the side from which the incoming
                 * light comes
                 */
                dirOut_u.SubtractBy(normal);
            }
            else
            {   /*
                 * normal points to the side to which the light goes
                 */
                dirOut_u.AddBy(normal);
            }

            return true;
        }

        #endregion

    }
}
