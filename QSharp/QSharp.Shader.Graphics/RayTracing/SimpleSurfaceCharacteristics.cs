using System;
using QSharp.Shader.Graphics.Base.Exceptions;
using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.RayTracing
{
    /// <summary>
    ///  Phong lighting model
    /// </summary>
    /// <remarks>
    ///  Point light splitter
    ///  theoretically (in simplified terms), as these coefficients are
    ///  used on both sides, they must observe the law of conservation
    ///  of energy, i.e.
    ///  rou_s + rou_st <= 1
    ///  rou_d + rou_dt <= 1
    ///  rou_rg + tou_tg <= 1
    /// </remarks>
    public class SimpleSurfaceCharacteristics
    {
        #region Enumerations
        /// <summary>
        ///  general type of the optical object
        /// </summary>
        public enum OpticType
        {
            Opaque,
            Transparent
        }
        #endregion

        #region Properties

        /// <summary>
        ///  specular reflection coeff
        /// </summary>
        public BlendedColor RouS { get; protected set; }

        /// <summary>
        ///  diffuse reflection coeff
        /// </summary>
        public BlendedColor RouD { get; protected set; }

        /// <summary>
        ///  specular refraction (transmission) coeff
        /// </summary>
        public BlendedColor RouSt { get; protected set; }

        /// <summary>
        ///  diffuse refraction
        /// </summary>
        public BlendedColor RouDt { get; protected set; }

        /// <summary>
        ///  internal specular reflection coeff
        /// </summary>
        public BlendedColor RouSi { get; protected set; }

        /// <summary>
        ///  internal diffusive reflection coeff
        /// </summary>
        public BlendedColor RouDi { get; protected set; }

        /// <summary>
        ///  the exponent of specular sharpness
        /// </summary>
        public float F { get; protected set; }

        /// <summary>
        ///  backing field for ambient reflection coeff
        /// </summary>
        public BlendedColor RouA { get; protected set; }

        /// <summary>
        ///  backing field for fraction of reflection for tracing
        /// </summary>
        public BlendedColor RouRg { get; protected set; }

        /// <summary>
        ///  backing field for fraction of transmission for tracing
        /// </summary>
        public BlendedColor RouTg { get; protected set; }

        /// <summary>
        ///  backing field for fraction of internal reflection for tracing
        /// </summary>
        public BlendedColor RouRi { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates the class with optical characteristics in colour component
        ///  based form (which is also a result of the synthesis of the colour of 
        ///  the object and the corresponding coefficient)
        /// </summary>
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
        /// <param name="f">exponent of specular sharpness</param>
        public SimpleSurfaceCharacteristics(BlendedColor s, BlendedColor d,
            BlendedColor st, BlendedColor dt, BlendedColor si,
            BlendedColor di, BlendedColor a, BlendedColor rg,
            BlendedColor tg, BlendedColor ri, float f)
        {
            RouS = s;
            RouD = d;
            RouSt = st;
            RouSi = si;
            RouDi = di;
            RouA = a;
            RouRg = rg;
            RouTg = tg;
            RouRi = ri;
            F = f;
        }

        /// <summary>
        ///  instantiates the class with optical characteristics in colour component
        ///  based form (which is also a result of the synthesis of the colour of 
        ///  the object and the corresponding coefficient)
        /// </summary>
        /// <param name="clr">colour of the object</param>
        /// <param name="r_s">specular reflection coeff</param>
        /// <param name="r_d">diffuse reflection coeff</param>
        /// <param name="r_st">specular refraction (transmission) coeff</param>
        /// <param name="r_dt">diffuse refraciton coeff</param>
        /// <param name="r_si">internal specular reflection coeff</param>
        /// <param name="r_di">internal diffuse reflection coeff</param>
        /// <param name="r_a">ambient reflection coeff</param>
        /// <param name="r_rg">fraction of reflection for tracing</param>
        /// <param name="r_tg">fraction of refraction for tracing</param>
        /// <param name="r_ri">fraction of internal reflection for tracing</param>
        /// <param name="f">exponent of specular sharpness</param>
        public SimpleSurfaceCharacteristics(BlendedColor clr, float r_s, float r_d,
            float r_st, float r_dt, float r_si, float r_di, float r_a,
            float r_rg, float r_tg, float r_ri, float f)
        {
            RouS = clr * r_s;
            RouD = clr * r_d;
            RouSt = clr * r_st;
            RouDt = clr * r_dt;
            RouSi = clr * r_si;
            RouDi = clr * r_di;
            RouA = clr * r_a;
            RouRg = clr * r_rg;
            RouTg = clr * r_tg;
            RouRi = clr * r_ri;
            F = f;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  quickly creates an instance with limited parameters
        /// </summary>
        /// <param name="clr">colour of the object</param>
        /// <param name="ot">type of the optical</param>
        /// <returns>the instance</returns>
        public static SimpleSurfaceCharacteristics QuickCreate(BlendedColor clr, OpticType ot)
        {
            if (ot == OpticType.Opaque)
            {
                return new SimpleSurfaceCharacteristics(clr, 0.95f, 0.2f, 0f, 0f, 0.95f,
                    0.2f, 0.05f, 0.95f, 0f, 0.95f, 50f);
            }
            if (ot == OpticType.Transparent)
            {
                return new SimpleSurfaceCharacteristics(clr, 0.2f, 0.04f, 0.75f, 0.16f,
                    0.95f, 0.2f, 0.05f, 0.2f, 0.75f, 0.95f, 50f);
            }
            
            throw new GraphicsException("Unknown optic type");
        }

        #endregion
    }
}
