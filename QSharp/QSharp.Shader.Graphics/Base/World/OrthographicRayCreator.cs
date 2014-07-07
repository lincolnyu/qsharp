using QSharp.Shader.Graphics.Base.Optics;
using QSharp.Shader.Graphics.Base.Geometry;

namespace QSharp.Shader.Graphics.Base.World
{
    /// <summary>
    ///  a class that creates a ray from given point on the screen using orthographic projection
    /// </summary>
    public sealed class OrthographicRayCreator : RayCreator
    {
        #region Fields

        /// <summary>
        ///  matrix that ray creation uses to figure out rays in world coordinate system
        /// </summary>
        private readonly Matrix4f _matrix;

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates the creator with specified transforming matrix
        /// </summary>
        /// <param name="matrix"></param>
        public OrthographicRayCreator(Matrix4f matrix)
        {
            _matrix = matrix;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="ray"></param>
        /// <returns></returns>
        public override bool Transform(RayOrigin origin, out Ray ray)
        {
            var screenCenterAsScreen = new Vector4f(origin.X, origin.Y, 0);
            var directionAsScreen = new Vector4f(0, 0, 1);

            var screenCenterAsScene = screenCenterAsScreen.TransformUsing(_matrix);
            /*
             * since it is orthogonal transform with translation, the 
             * normality of the vector remains.
             */
            var directionAsScene = directionAsScreen.LinearTransformUsing(_matrix);

            ray = new Ray(screenCenterAsScene, directionAsScene);

            return true;
        }

        #endregion
    }
}
