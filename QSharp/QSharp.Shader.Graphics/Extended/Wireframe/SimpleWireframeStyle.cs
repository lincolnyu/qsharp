using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.Extended.Wireframe
{
    /// <summary>
    ///  class that implements a simple style for drawing wireframes
    /// </summary>
    public class SimpleWireframeStyle : IWireframeStyle
    {
        #region Properties

        /// <summary>
        ///  colour of the wireframe to draw
        /// </summary>
        public PixelColor8Bit Color { get; protected set; }

        #endregion

        public SimpleWireframeStyle(PixelColor8Bit color)
        {
            Color = color;
        }
    }
}
