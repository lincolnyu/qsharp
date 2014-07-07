using QSharp.Shader.Graphics.Base.Geometry;
using QSharp.Shader.Graphics.Base.Optics;


namespace QSharp.Shader.Graphics.Base.World
{
    public sealed class OrthographicScreen : RectangularScreen
    {
        #region Fields
        
        /// <summary>
        ///  backing field for the transform
        /// </summary>
        private OrthographicTransformerPair _worldToScreen;

        /// <summary>
        ///  ray creator that enables ray to be created from the screen
        /// </summary>
        private OrthographicRayCreator _rayCreator;

        #endregion

        #region Properties

        /// <summary>
        ///  returns the transformer pair associated with the screen that
        ///  transforms the objects in the world to that on the screen
        ///  and reverse
        /// </summary>
        public override Vector4fAffineTransformerPair WorldToScreen
        {
            get { return _worldToScreen; }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  constructor that initializes the instance with all required data
        /// </summary>
        /// <param name="camera">camera the screen is used for</param>
        /// <param name="image">image that the camera captures on the screen</param>
        public OrthographicScreen(Camera camera, IImage image)
            : base(camera, image)
        {
            ConformWithCamera();
        }

        #endregion

        #region Methods

        /// <summary>
        ///  makes the screen conform with the camera it is associated with
        /// </summary>
        public override void ConformWithCamera()
        {
            Matrix4f screenAdjuster = GetScreenAdjuster();

            Matrix4f mtxOrthFwd = screenAdjuster * _camera.WorldToOrthographic.Forward;
            Matrix4f mtxOrthBwd = mtxOrthFwd.ToInverse();

            _worldToScreen = new OrthographicTransformerPair(mtxOrthFwd, mtxOrthBwd);
            _rayCreator = new OrthographicRayCreator(mtxOrthBwd);
        }

        /// <summary>
        ///  creates a (conceptual) back tracing ray from specified position on the screen
        /// </summary>
        /// <param name="x">x coordinate of the position</param>
        /// <param name="y">y coordinate of the position</param>
        /// <param name="ray">ray that starts from the given position</param>
        public override void CreateRay(int x, int y, out Ray ray)
        {
            _rayCreator.Transform(new RayOrigin(_camera, x, y), out ray);
        }

        #endregion

    }
}
