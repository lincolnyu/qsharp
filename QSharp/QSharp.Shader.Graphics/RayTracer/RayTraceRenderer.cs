using QSharp.Shader.Graphics.Base.World;
using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.RayTracer
{
    /// <summary>
    ///  class whose instance renders the model using ray-tracing approach
    /// </summary>
    public class RayTraceRenderer
    {
        #region Fields

        #endregion

        #region Properties

        /// <summary>
        ///  readonly property for setting and getting the screeen the renderer is drawing on
        /// </summary>
        public RectangularScreen Screen { get; protected set; }

        /// <summary>
        ///  readonly property for setting and getting the scene
        /// </summary>
        public IRayTraceScene Scene { get; protected set; }

        /// <summary>
        ///  readonly property for setting and getting the number of levels to trace the ray to
        /// </summary>
        public int TotalDepth { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a renderer with specified details corresponding to the fields
        /// </summary>
        /// <param name="screen">backing field for Screen the renderer draws on</param>
        /// <param name="scene">the scene to draw</param>
        /// <param name="totalDepth">number of level to go in the ray-tracing process</param>
        public RayTraceRenderer(RectangularScreen screen, IRayTraceScene scene, int totalDepth)
        {
            Screen = screen;
            Scene = scene;
            TotalDepth = totalDepth;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  carries out the rendering process
        /// </summary>
        public void Render()
        {
            int width = Screen.Width;
            int height = Screen.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Ray ray;

                    Screen.CreateRay(x, y, out ray);

                    var light = new Light(0f, 0f, 0f);
                    Scene.Trace(this, ray, 0, ref light);

                    PixelColor8Bit color = light.ToPixelColor8Bit();
                    Screen.SetPixel(x, y, color);
                }
            }
        }

        #endregion
    }
}
