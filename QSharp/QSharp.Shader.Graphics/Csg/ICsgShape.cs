using QSharp.Shader.Graphics.Base.Objects;

namespace QSharp.Shader.Graphics.Csg
{
    /// <summary>
    ///  base class of all objects in a CSG model
    /// </summary>
    /// <remarks>
    ///  CSG-based shapes normally suggest the handling of composite shapes.
    ///  For more details about CSG, refer to the following sites,
    ///  <list type="URLs">
    ///   <see cref="http://en.wikipedia.org/wiki/Constructive_solid_geometry"/>
    ///   <see cref="http://en.wikipedia.org/wiki/Constructive_solid_geometry"/>
    ///  </list>
    /// </remarks>
    public interface ICsgShape : IShape
    {
    }
}
