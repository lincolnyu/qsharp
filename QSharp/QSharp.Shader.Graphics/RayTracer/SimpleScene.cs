using System;
using System.Collections.Generic;
using QSharp.Shader.Graphics.Base.Objects;
using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.RayTracer
{
    /// <summary>
    ///  a simple implementation of ray-trace scene where objects and lights are placed 
    ///  and tracing 
    /// </summary>
    public class SimpleScene : IRayTraceScene
    {
        #region Properties

        /// <summary>
        ///  point lights
        /// </summary>
        public List<PointLight> PointLights { get; protected set; }

        /// <summary>
        ///  optical objects
        /// </summary>
        public List<IOptical> Opticals { get; protected set; }

        /// <summary>
        ///  the ether object
        /// </summary>
        public SimpleEther Ether { get; protected set; }

        /// <summary>
        ///  ambient light
        /// </summary>
        public Light AmbientLight { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  parameterless constructor that initializes internal data fields 
        ///  and ties them up to this instance of scene if proper
        /// </summary>
        public SimpleScene()
        {
            Ether = new SimpleEther(this);
            PointLights = new List<PointLight>();
            Opticals = new List<IOptical>();
            AmbientLight = new Light(0f, 0f, 0f);
        }

        #endregion

        #region Methods

        /// <summary>
        ///  initiates tracing process for specified ray with specified depth
        /// </summary>
        /// <param name="renderer">the renderer that conducts the ray tracing process</param>
        /// <param name="ray">a ray from the scene for which the light is to be figured out</param>
        /// <param name="depth">depth of the tracing (number of levels)</param>
        /// <param name="light">light that is calculated out</param>
        public virtual void Trace(RayTraceRenderer renderer, 
            Ray ray, int depth, ref Light light)
        {
            Ether.Travel(renderer, ray, depth, out light);
        }

        #endregion
    }
}
