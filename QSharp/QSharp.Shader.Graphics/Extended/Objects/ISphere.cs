using QSharp.Shader.Graphics.Base.Geometry;

namespace QSharp.Shader.Graphics.Extended.Objects
{
    public interface ISphere
    {
        Vector4f Center { get; }
        float Radius { get; }

        bool ContainsPoint(Vector4f point);
    }
}
