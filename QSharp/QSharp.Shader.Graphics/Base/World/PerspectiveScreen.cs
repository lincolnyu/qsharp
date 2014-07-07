using System;
using QSharp.Shader.Graphics.Base.Geometry;
using QSharp.Shader.Graphics.Base.Optics;

namespace QSharp.Shader.Graphics.Base.World
{
    /// <summary>
    ///  rectangular screen that displays objects in the world
    ///  projected onto it in perspective manner
    /// </summary>
    public class PerspectiveScreen : RectangularScreen
    {
        #region Fields

        /// <summary>
        ///  backing field for transformer pair for conversion
        ///  between world coordinates and screen coordinates
        /// </summary>
        private PerspectiveTransformerPair _worldToScreen;

        /// <summary>
        ///  backing field for tracing 
        /// </summary>
        private PerspectiveRayCreator _rayCreator;

        #endregion

        #region Properties

        /// <summary>
        ///  property for getting the transformer pair that converts
        ///  points between world and screen coordinates
        /// </summary>
        public override Vector4fAffineTransformerPair WorldToScreen
        {
            get { return _worldToScreen; }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates the screen with specified camera from which
        ///  the view is catured and specifed image where the view is 
        ///  to be drawn on
        /// </summary>
        /// <param name="camera">camera through which the view is made</param>
        /// <param name="image">image object on which the view is drawn</param>
        public PerspectiveScreen(Camera camera, IImage image)
            : base(camera, image)
        {
            ConformWithCamera();
        }

        #endregion

        #region Methods

        /// <summary>
        ///  makes the screen settings compliant with camera properties
        /// </summary>
        public override sealed void ConformWithCamera()
        {
            Matrix4f screenAdjuster = GetScreenAdjuster();

            Matrix4f mtxPrspFwd = screenAdjuster * _camera.WorldToPerspective.Forward;
            Matrix4f mtxPrspBwd = mtxPrspFwd.ToInverse();

            _worldToScreen = new PerspectiveTransformerPair(mtxPrspFwd, mtxPrspBwd);
            _rayCreator = new PerspectiveRayCreator(mtxPrspBwd, _camera.Viewer);
        }

        /// <summary>
        ///  creates a ray starting at the given position on the screen
        /// </summary>
        /// <param name="x">x coordinate of the position</param>
        /// <param name="y">y coordinate of the position</param>
        /// <param name="ray">ray that fulfills the requirement</param>
        public override void CreateRay(int x, int y, out Ray ray)
        {
            _rayCreator.Transform(new RayOrigin(_camera, x, y), out ray);
        }

        #endregion
    }
}
