using QSharp.Shader.Graphics.Extended.Wireframe;

namespace QSharp.Shader.Graphics.Extended.Objects
{
    /// <summary>
    ///  a sphere class that is useful for creating renderable spheres which involves
    ///  simple optical sphere characteristics and wireframe feature
    /// </summary>
    public class UsefulSphere : SimpleOpticalSphere, IWireframedShape
    {
        #region Fields

        /// <summary>
        ///  an instance of wireframed sphere that provides wireframe functionality of this sphere instance
        ///  it takes this instance and wraps it and consumes sphere related features provided by it on which 
        ///  wireframe related feature it provides is based 
        /// </summary>
        private readonly WireframedSphere _wireframedSphere;

        #endregion

        #region Constructors

        /// <summary>
        ///  constructor that initializes and creates the aggregated members that provide 
        ///  additional functionalities
        /// </summary>
        public UsefulSphere()
        {
            _wireframedSphere = new WireframedSphere(this);
        }

        /// <summary>
        ///  constructor that initializes the sphere with specified parameters and creates aggregated
        ///  members that provide additional functionalities
        /// </summary>
        /// <param name="cx">x component of the central position</param>
        /// <param name="cy">y component of the central position</param>
        /// <param name="cz">z component of the central position</param>
        /// <param name="radius">radius of the sphere</param>
        public UsefulSphere(float cx, float cy, float cz, float radius)
            : base(cx, cy, cz, radius)
        {
            _wireframedSphere = new WireframedSphere(this);
        }

        #endregion

        #region Methods

        #region Implementation of IWireframedShape

        /// <summary>
        ///   draws wireframe representation of the sphere
        ///   it gets the implementation provided by the aggregated member field of the 
        ///   wireframed sphere by invoking the function of same signature on it
        /// </summary>
        /// <param name="screen">the screen to draw on</param>
        /// <param name="style"></param>
        public void DrawWireframe(ScreenPlotter screen, IWireframeStyle style)
        {
            _wireframedSphere.DrawWireframe(screen, style);
        }

        #endregion

        #endregion
    }
}
