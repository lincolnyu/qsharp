using QSharp.Shader.Graphics.Base.Geometry;

namespace QSharp.Shader.Graphics.Base.World
{
    public class PerspectiveTransformerPair : Vector4fAffineTransformerPair
    {
        public PerspectiveTransformerPair(Matrix4f forward, Matrix4f backward)
            : base(forward, backward)
        {
        }

        public override bool BackwardTransform(Vector4f input, out Vector4f output)
        {
            if (!base.BackwardTransform(input, out output))
            {
                return false;
            }
            return output[3] > 0;
        }

        public override bool ForwardTransform(Vector4f input, out Vector4f output)
        {
            if (!base.ForwardTransform(input, out output))
            {
                return false;
            }
            return output[3] > 0;
        }
    }
}
