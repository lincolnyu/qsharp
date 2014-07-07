namespace QSharp.Shader.Graphics.Base.World
{
    /// <summary>
    ///  class that contains the origin of the back-tracing ray and the associated camera
    /// </summary>
    public class RayOrigin
    {
        #region Fields

        #endregion

        #region Properties

        /// <summary>
        ///  camera where the ray origin is picked from
        /// </summary>
        public Camera Camera { get; protected set; }

        /// <summary>
        ///  X coordinate of the position of the origin
        /// </summary>
        public float X { get; protected set; }

        /// <summary>
        ///  Y coordinate of the position of the origin
        /// </summary>
        public float Y { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a ray origin with specified camera and position
        /// </summary>
        /// <param name="camera">camera that the origin is attached to</param>
        /// <param name="x">X coordinate of the position of the origin</param>
        /// <param name="y">Y coordinate of the position of the origin</param>
        public RayOrigin(Camera camera, float x, float y)
        {
            Camera = camera;
            X = x;
            Y = y;
        }

        #endregion
    }
}
