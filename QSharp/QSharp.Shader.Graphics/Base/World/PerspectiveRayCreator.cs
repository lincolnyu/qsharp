using QSharp.Shader.Graphics.Base.Optics;
using QSharp.Shader.Graphics.Base.Geometry;

namespace QSharp.Shader.Graphics.Base.World
{
    /// <summary>
    ///  a class that creates a ray from given point on the screen using perspective projection
    /// </summary>
    public class PerspectiveRayCreator : RayCreator
    {
        #region Properties

        /// <summary>
        ///  matrix that ray creation uses to figure out rays in world coordinate system
        /// </summary>
        protected Matrix4f Matrix { get; set; }

        /// <summary>
        ///  viewer the tracing ray is starting from
        /// </summary>
        protected Viewer Viewer { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a perspective ray creator with specified matrix and viewer
        /// </summary>
        /// <param name="matrix">matrix that ray creation uses to figure out rays in world coordinate system</param>
        /// <param name="viewer">viewer the tracing ray is starting from</param>
        public PerspectiveRayCreator(Matrix4f matrix, Viewer viewer)
        {
            Matrix = matrix;
            Viewer = viewer;
        }

        #endregion

        #region Methods


        /// <summary>
        ///  works out the ray in world coordinate system for given point on the screen
        /// </summary>
        /// <param name="origin">point on the screen</param>
        /// <param name="ray">the ray that is sent out from the point</param>
        /// <returns>if a ray is successfully worked out</returns>
        public override bool Transform(RayOrigin origin, out Ray ray)
        {
            Vector4f eye = Viewer.Eye;
            var screenCenterAsScreen = new Vector4f(origin.X, origin.Y, 0);

            Vector4f screenCenterAsScene = screenCenterAsScreen.TransformUsing(Matrix);
            screenCenterAsScene.Normalize();

            Vector4f directionAsScene = screenCenterAsScene - eye;

            ray = new Ray(eye, directionAsScene);

            return true;
        }

        #endregion
    }
}
