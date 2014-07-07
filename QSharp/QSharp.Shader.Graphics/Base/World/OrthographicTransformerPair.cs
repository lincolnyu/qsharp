using System;
using QSharp.Shader.Graphics.Base.Geometry;

namespace QSharp.Shader.Graphics.Base.World
{
    public class OrthographicTransformerPair : Vector4fAffineTransformerPair
    {
        public OrthographicTransformerPair(Matrix4f forward, Matrix4f backward)
            : base(forward, backward)
        {
        }

        public override bool BackwardTransform(Vector4f input, out Vector4f output)
        {
            if (!base.BackwardTransform(input, out output))
            {
                return false;
            }
            return output[2] > 0;
        }

        public override bool ForwardTransform(Vector4f input, out Vector4f output)
        {
            if (!base.ForwardTransform(input, out output))
            {
                return false;
            }
            return output[2] > 0;
        }
    }
}
