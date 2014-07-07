using QSharp.Shader.Graphics.Base.Objects;

namespace QSharp.Shader.Graphics.Extended.Wireframe
{
    /// <summary>
    ///  interface that specifies what a class that represents a shape that can be
    ///  drawn in wireframe form should implement
    /// </summary>
    public interface IWireframedShape : IShape
    {
        #region Methods

        /// <summary>
        ///  draws wireframe representation of the shape
        /// </summary>
        /// <param name="screen">screen to draw on</param>
        /// <param name="style">style of drawing</param>
        void DrawWireframe(ScreenPlotter screen, IWireframeStyle style);

        #endregion
    }
}
