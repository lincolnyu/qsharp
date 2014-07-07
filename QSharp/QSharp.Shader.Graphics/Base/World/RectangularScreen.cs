using System;
using QSharp.Shader.Graphics.Base.Optics;
using QSharp.Shader.Graphics.Base.Geometry;

namespace QSharp.Shader.Graphics.Base.World
{
    /// <summary>
    ///  Rectangule-shaped, square-pixel-based screen
    /// </summary>
    /// <remarks>
    ///  Therefore, its dimensions are represented by integer values
    ///  in terms of number of pixels
    /// </remarks>
    public abstract class RectangularScreen
    {
        #region Nested types

        /// <summary>
        ///  interface of image object to which the view is projected
        /// </summary>
        public interface IImage
        {
            #region Properties
            /// <summary>
            ///  height of the image
            /// </summary>
            int Width { get; }

            /// <summary>
            ///  width of the image
            /// </summary>
            int Height { get; }

            /// <summary>
            ///  property for getting and setting pixel at specified
            ///  position on the image
            /// </summary>
            /// <param name="x">x component of the position of the pixel (column index)</param>
            /// <param name="y">y component of the position of the pixel (row index)</param>
            /// <returns></returns>
            PixelColor8Bit this[int x, int y]
            {
                get;
                set;
            }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        ///  backing field for camera through which the view is made
        /// </summary>
        protected Camera _camera;

        /// <summary>
        ///  backing field for image where the view of the screen is drawn 
        /// </summary>
        protected IImage _image;

        #endregion

        #region Properties

        /// <summary>
        ///  returns the transform pair for transformation between objects
        ///  in real world and object presetation on the screen
        /// </summary>
        public abstract Vector4fAffineTransformerPair WorldToScreen
        {
            get;
        }

        public int Width
        {
            get
            {
                return _image.Width;
            }
        }

        public int Height
        {
            get
            {
                return _image.Height;
            }
        }

        public Camera Camera
        {
            get
            {
                return _camera;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a screen with given camera and image
        /// </summary>
        /// <param name="camera">camera through which the view is made</param>
        /// <param name="image">image where the view of the screen is drawn</param>
        public RectangularScreen(Camera camera, IImage image)
        {
            _camera = camera;
            _image = image;

            _camera.PropertiesChanged += CameraPropertiesChanged;
        }

        /// <summary>
        ///  finalises a screen
        /// </summary>
        ~RectangularScreen()
        {
            if (_camera != null)
            {
                _camera.PropertiesChanged -= CameraPropertiesChanged; 
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///  responds to camera change and takes actions accordingly
        /// </summary>
        /// <param name="sender"></param>
        void CameraPropertiesChanged(Camera sender)
        {
            ConformWithCamera();
        }

        /// <summary>
        ///  returns a matrix that converts screen coordinates further 
        ///  to (0,0)-(width,height) coordinates
        /// </summary>
        /// <returns></returns>
        protected Matrix4f GetScreenAdjuster()
        {
            CameraWindow window = this._camera.Window;

            float windowWidth = window.Width;
            float windowHeight = window.Height;

            float kx = Width / windowWidth;
            float ky = Height / windowHeight;

            float dx = -kx * window.Left;
            float dy = -ky * window.Top;

            /*
             * this matrix maps:
             *  Left -> 0
             *  Right -> 'Width'
             *  Top -> 0
             *  Bottom -> 'Height'
             * 
             */
            float[] saData = new float[]
            {
                kx, 0, 0, dx,
                0, ky, 0, dy,
                0,  0, 1, 0,
                0,  0, 0, 1
            };

            return new Matrix4f(saData);
        }

        /// <summary>
        ///  sets a pixel at specified position with specified colour
        /// </summary>
        /// <param name="x">x coordinate of the position</param>
        /// <param name="y">y coordinate of the position</param>
        /// <param name="color">colour to be applied onto the pixel</param>
        public virtual void SetPixel(int x, int y, PixelColor8Bit color)
        {
            _image[x, y] = color;
        }

        /// <summary>
        ///  creates a ray starting at the given position on the screen
        /// </summary>
        /// <param name="x">x coordinate of the position</param>
        /// <param name="y">y coordinate of the position</param>
        /// <param name="ray">ray that fulfills the requirement</param>
        public abstract void CreateRay(int x, int y, out Ray ray);

        /// <summary>
        ///  makes the screen settings compliant with camera properties
        /// </summary>
        public abstract void ConformWithCamera();

        #endregion
    }
}
