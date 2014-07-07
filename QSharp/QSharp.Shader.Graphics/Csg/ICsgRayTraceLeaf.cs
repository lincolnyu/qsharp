namespace QSharp.Shader.Graphics.Csg
{
    /// <summary>
    ///  leaf node of csg model, which is a single object without involving child nodes
    /// </summary>
    public interface ICsgRayTraceLeaf : ICsgLeaf, CsgRayTrace.IIntersectable
    {
    }
}
