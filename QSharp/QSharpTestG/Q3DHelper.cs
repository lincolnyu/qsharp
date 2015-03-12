using System;

using QSharp.Shader.Graphics.Csg;
using QSharp.Shader.Graphics.Base.Optics;
using QSharp.Shader.Graphics.Extended.Objects;
using QSharp.Shader.Graphics.RayTracing;

namespace QSharpTestG
{
    /// <summary>
    ///  static methods that extend operations on graphical objects
    /// </summary>
    public static class Q3DHelper
    {
        #region Methods

        public static CsgSphere CreateSphere(float cx, float cy, float cz, float r)
        {
            CsgSphere sphere = new CsgSphere(cx, cy, cz, r);
            return sphere;
        }

        public static ICsgShape CreateCsg(ICsgShape left, ICsgShape right, CsgNode.Operation oper)
        {
            CsgUsefulNode csg = new CsgUsefulNode(left, right, oper);
            return csg;
        }

        public static void SetOpticEasy(this SimpleOptical obj, float r, float g, float b,
            float transparency, float eta)
        {
            SetOptic(obj, r, g, b, transparency, 0, 0.5f, 0.5f, eta);
        }

        /// <summary>
        ///  set the optic characteristics of an object with optical features
        /// </summary>
        /// <param name="obj">obj to set optical variables for</param>
        /// <param name="r">red component</param>
        /// <param name="g">green component</param>
        /// <param name="b">blue component</param>
        /// <param name="transparency">transparency of the object</param>
        /// <param name="ambientRate">ambient rate</param>
        /// <param name="specularRate">specular rate</param>
        /// <param name="diffuseRate">diffuse rate</param>
        /// <param name="eta">eta</param>
        public static void SetOptic(this SimpleOptical obj, float r, float g, float b,
            float transparency, float ambientRate, float specularRate,
            float diffuseRate, float eta)
        {
            BlendedColor color = new BlendedColor(r, g, b);
            const float attenuationRate = 0.5f;

            // derive attenuant from diffuse rate
            float attenuant = attenuationRate *
                (float)Math.Pow(specularRate / (specularRate + diffuseRate), 2);

            float tg = transparency * attenuant;
            float rg = attenuant - tg;
            float ri = 1 * attenuant;

            float st = transparency * specularRate;
            float s = specularRate - st;

            float dt = transparency * diffuseRate;
            float d = diffuseRate - dt;

            float si = specularRate;
            float di = diffuseRate;

            obj.Set(color, s, d, st, dt, si, di, ambientRate, rg, tg, ri, eta, 70);
        }

        public static void SetPointLightEasy(this PointLight pl, float r, float g, float b)
        {
            const float specularRate = 1f;
            const float diffuseRate = 1f;
            pl.SetPointLight(r * diffuseRate, g * diffuseRate, b * diffuseRate,
                r * specularRate, g * specularRate, b * specularRate);
        }

        public static void SetPointLight(this PointLight pl, float rd, float gd, float bd,
            float rs, float gs, float bs)
        {
            pl.LightD.Set(rd, gd, bd);
            pl.LightS.Set(rs, gs, bs);
        }

        #endregion

    }
}
