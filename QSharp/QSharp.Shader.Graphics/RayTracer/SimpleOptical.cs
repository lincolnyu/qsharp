using QSharp.Shader.Graphics.Base.Exceptions;
using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.RayTracer
{
    /// <summary>
    ///  an abstract extention to ray-trace optical object with a simplied
    ///  optical modeling
    ///  the optical object is simple in that the inner surface
    ///  and outer surface share the same set of optical characteristics
    ///  represented here by '_characteristics', which is not necessarily
    ///  the case in those more stringent circumstances
    /// </summary>
    public abstract class SimpleOptical : RayTraceOptical
    {
        #region Properties

        /// <summary>
        ///  inner surface of the object
        /// </summary>
        protected SimpleSurface InnerSurface { get; set; }

        /// <summary>
        ///  outer surface of the object
        /// </summary>
        protected SimpleSurface OuterSurface { get; set; }

        /// <summary>
        ///  optical characteristics the above two surfaces share
        /// </summary>
        protected SimpleSurfaceCharacteristics Characteristics { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  parameterless constructor that instantiates the class with default settings
        /// </summary>
        protected SimpleOptical()
        {
            const float eta = 1.5f;
            Characteristics = SimpleSurfaceCharacteristics.QuickCreate(new BlendedColor(0, 0, 1), 
                SimpleSurfaceCharacteristics.OpticType.Opaque);
            InnerSurface = new SimpleSurface(Characteristics, eta);
            OuterSurface = new SimpleSurface(Characteristics, 1f / eta);
        }

        /// <summary>
        ///  instantiates the class with optical details
        /// </summary>
        /// <param name="clr">colour of the object</param>
        /// <param name="s">specular reflection coeff</param>
        /// <param name="d">diffuse reflection coeff</param>
        /// <param name="st">specular refraction (transmission) coeff</param>
        /// <param name="dt">diffuse refraciton coeff</param>
        /// <param name="si">internal specular reflection coeff</param>
        /// <param name="di">internal diffuse reflection coeff</param>
        /// <param name="a">ambient reflection coeff</param>
        /// <param name="rg">fraction of reflection for tracing</param>
        /// <param name="tg">fraction of refraction for tracing</param>
        /// <param name="ri">fraction of internal reflection for tracing</param>
        /// <param name="eta">ratio of the indices of refraction</param>
        /// <param name="f">exponent of specular sharpness</param>
        protected SimpleOptical(BlendedColor clr, float s, float d, float st,
            float dt, float si, float di, float a, float rg, float tg,
            float ri, float eta, float f)
        {
            Set(clr, s, d, st, dt, si, di, a, rg, tg, ri, eta, f);
        }

        /// <summary>
        ///  instantiates the class with optical details
        /// </summary>
        /// <param name="characteristics">combined optical characteristics</param>
        /// <param name="eta">ratio of the indices of refraction</param>
        protected SimpleOptical(SimpleSurfaceCharacteristics characteristics, float eta)
        {
            Set(characteristics, eta);
        }

        #endregion

        #region Methods

        /// <summary>
        ///  set the optical characteristics of this object
        /// </summary>
        /// <param name="clr">colour of the object</param>
        /// <param name="s">specular reflection coeff</param>
        /// <param name="d">diffuse reflection coeff</param>
        /// <param name="st">specular refraction (transmission) coeff</param>
        /// <param name="dt">diffuse refraciton coeff</param>
        /// <param name="si">internal specular reflection coeff</param>
        /// <param name="di">internal diffuse reflection coeff</param>
        /// <param name="a">ambient reflection coeff</param>
        /// <param name="rg">fraction of reflection for tracing</param>
        /// <param name="tg">fraction of refraction for tracing</param>
        /// <param name="ri">fraction of internal reflection for tracing</param>
        /// <param name="eta">ratio of the indices of refraction</param>
        /// <param name="f">exponent of specular sharpness</param>
        public void Set(BlendedColor clr, float s, float d, float st,
            float dt, float si, float di, float a, float rg, float tg,
            float ri, float eta, float f)
        {
            Characteristics = new SimpleSurfaceCharacteristics(clr, s,
                d, st, dt, si, di, a, rg, tg, ri, f);

            InnerSurface = new SimpleSurface(Characteristics, eta);
            OuterSurface = new SimpleSurface(Characteristics, 1f / eta);
        }

        public void Set(SimpleSurfaceCharacteristics characteristics, float eta)
        {
            Characteristics = characteristics;
            InnerSurface = new SimpleSurface(characteristics, eta);
            OuterSurface = new SimpleSurface(characteristics, 1f / eta);
        }

        /// <summary>
        ///  returns true and gives intersection and surface details 
        ///  if <paramref name="ray"/> hits the object and enters
        ///  and where the hit happens
        /// </summary>
        /// <param name="renderer">the renderer that carries out the tracing</param>
        /// <param name="ray">a ray that is outside the optical object and may come in</param>
        /// <param name="intersection">the intersection where it hits if it does</param>
        /// <param name="surface">the surface structure</param>
        /// <returns>true if it hits</returns>
        public override bool Import(RayTraceRenderer renderer, Ray ray,
            out Intersection intersection, out IRayTraceSurface surface)
        {
            bool res = IntersectNearest(ray, RayStartMode.Outside, out intersection);
            surface = res ? OuterSurface : null;
            return res;
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
        /// <returns>true if it hits</returns>
        public override bool Export(RayTraceRenderer renderer, Ray ray, 
            out Intersection intersection, out IRayTraceSurface surface,
            out IRayTraceOptical adjacent)
        {
            bool res = IntersectNearest(ray, RayStartMode.Inside, out intersection);
            if (res)
            {
                surface = InnerSurface;
                var scene = renderer.Scene as SimpleScene;
                if (scene == null)
                {
                    throw new GraphicsException("Invalid scene");
                }
                adjacent = scene.Ether;
            }
            else
            {
                surface = null;
                adjacent = null;
            }
            return res;
        }

        /// <summary>
        ///  gets the intersection where the ray line first meets the object
        /// </summary>
        /// <param name="ray">the ray that may potentially intersect the object</param>
        /// <param name="rsm">a flag that indicates where the ray starts</param>
        /// <param name="intersection">the intersection where the ray hits the object if it does</param>
        /// <returns>true if the line intersects the object</returns>
        public abstract bool IntersectNearest(Ray ray, RayStartMode rsm, out Intersection intersection);

        /// <summary>
        ///  returns true if the ray starts from inside the object
        /// </summary>
        /// <param name="ray">the ray to exam</param>
        /// <returns>true if the ray starts from inside the object</returns>
        public abstract bool IsRayInside(Ray ray);

        #endregion

    }
}
