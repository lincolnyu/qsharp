using System;
using QSharp.Shader.Graphics.Base.Geometry;
using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.Base.World
{
    /// <summary>
    ///  abstract class of ray creator which figures out tracing ray based on origin on the screen
    /// </summary>
    public abstract class RayCreator : ITransformer<RayOrigin, Ray>
    {
        public abstract bool Transform(RayOrigin origin, out Ray ray);
    }
}
