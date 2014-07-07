namespace QSharp.Shader.Graphics.Base.Geometry
{
    public class Vector4fAffineTransformer : IVector4fTransformer
    {
        protected Matrix4f mTr;

        public Vector4fAffineTransformer(Matrix4f tr)
        {
            mTr = tr;
        }

        public bool Transform(Vector4f input, out Vector4f output)
        {
            output = input.TransformUsing(mTr);
            return true;
        }

    }
}
