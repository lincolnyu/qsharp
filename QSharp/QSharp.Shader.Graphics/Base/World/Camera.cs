using QSharp.Shader.Graphics.Base.Geometry;

namespace QSharp.Shader.Graphics.Base.World
{
    /// <summary>
    ///  A class that represents a camera
    /// </summary>
    /// <remarks> 
    ///  A camera basically consists of 
    ///  two elements: viewer and window
    ///  the coordinate system a camera uses 
    ///  is simply named camara system, (or camera
    ///  space ) which is
    ///  centered at the viewer's eye, the z
    ///  axis of which starts from the eye 
    ///  and extends in the the direction the eye is
    ///  looking in, the x axis and the y axis of which 
    ///  start from the eye and go horizontally rightwards
    ///  and vertically downwards respectively
    ///  against the horizontal line of the eye
    ///  it is a regular 3-d coordinate system
    ///  (right-hand system)
    ///  
    ///  window represents the window in front of
    ///  the viewer which is characterized by
    ///  the position of its four sides which are
    ///  either horizontal or verticle in the 
    ///  camera system and its perpendicular 
    ///  distance to the eye ('D')
    ///  
    ///  in the camera system, the positions of 
    ///  the four corners of the window represented 
    ///  in terms of the window's parameters are:
    ///  - top-left :    (Left, Top, Near)
    ///  - top-right:    (Right, Top, Near)
    ///  - bottom-left : (Left, Bottom, Near)
    ///  - bottom-right: (Right, Bottom, Near)
    ///  in which Left < Right, Bottom < Top
    ///  
    ///  everything in the camera space, can be further
    ///  converted into the projection space through a
    ///  projection transformer (simply called projector)
    ///  in current implemention, a orthographic projection
    ///  and a perspective projection are supported
    ///  
    ///  there is a rule that the projection should 
    ///  satisfying the following map relationship
    ///  
    ///  - top-left :    (Left, Top, Near) -> (Left, Top, 0)
    ///  - top-right:    (Right, Top, Near) -> (Right, Top, 0)
    ///  - bottom-left : (Left, Bottom, Near) -> (Left, Bottom, 0)
    ///  - bottom-right: (Right, Bottom, Near) -> (Right, Bottom, 0)
    ///    
    ///    and for following rectangular screen processing only the
    ///    cubic region in camera space within the rectangle 
    ///    confined by parameters (Left, Top, Right, Button) and
    ///    with Z > 0 might be within the range of view.
    /// </remarks>
    public class Camera
    {
        #region Delegates

        /// <summary>
        ///  delegate that specifies the signature of the event fired when camera properties have been changed
        ///  and the changes affect most of the entities that work closely with the camera such as screen
        /// </summary>
        /// <param name="sender">sender of the event</param>
        public delegate void ChangedEvent(Camera sender);

        #endregion

        #region Fields

        /// <summary>
        ///  backing field for the viewer
        /// </summary>
        protected Viewer _viewer;

        /// <summary>
        ///  backing field for the rectangular window inside the camera.
        /// </summary>
        /// <remarks>
        ///  the physical rectangular window inside the camera on which the scene is the projected
        ///  and is taken as the final image to be displayed on the screen. Normally the dimensions
        ///   of the window can be set as being (-1,-1) to (1,1)
        /// </remarks>
        protected CameraWindow _window;

        /// <summary>
        ///  backing field for the distance between the far plane to the window
        /// </summary>
        protected float _far;

        /// <summary>
        ///  backing field for z position of the plane to which far plane is mapped in
        ///  perspective projection, by default it's set to 1f
        /// </summary>
        protected float _far2;

        #endregion

        #region Properties

        /// <summary>
        ///  a transformation pair that converts elements between world coordinates and camera coordinates
        /// </summary>
        protected Vector4fAffineTransformerPair WorldToCamera { get; set; }

        /// <summary>
        ///  transformation pair for conversion between camera coordinates and orthographic coordinates
        /// </summary>
        protected Vector4fAffineTransformerPair OrthographicProjector { get; set; }

        /// <summary>
        ///  transformation pair for conversion between world coordinates and orthographic coordinates
        /// </summary>
        public Vector4fAffineTransformerPair WorldToOrthographic { get; protected set; }

        /// <summary>
        ///  transformation pair for conversion between world coordinates and perspective coordinates
        /// </summary>
        public Vector4fAffineTransformerPair WorldToPerspective { get; protected set; }

        /// <summary>
        ///  property for getting and setting the viewer of the camera
        /// </summary>
        public Viewer Viewer
        {
            get
            {
                return _viewer;
            }

            set
            {
                bool changed = !_viewer.Equals(value);

                if (changed)
                {
                    if (_viewer != null)
                    {
                        _viewer.PropetiesChanged -= OnViewerChanged;
                    }

                    _viewer = value;
                    _viewer.Regularize();

                    UpdateWorldToCameraTransformer();
                    DerivePerspectiveFromWindow();
                    DeriveOrthographicFromWindow();

                    if (_viewer != null)
                    {
                        _viewer.PropetiesChanged += OnViewerChanged;
                    }

                    if (PropertiesChanged != null)
                    {
                        PropertiesChanged(this);
                    }
                }
            }
        }

        /// <summary>
        ///  property for setting the camera window
        /// </summary>
        public CameraWindow Window
        {
            get
            {
                return _window;
            }

            set
            {
                bool changed = !_window.Equals(value);

                if (changed)
                {
                    _window = value;

                    UpdateWorldToCameraTransformer();
                    DerivePerspectiveFromWindow();
                    DeriveOrthographicFromWindow();

                    if (PropertiesChanged != null)
                    {
                        PropertiesChanged(this);
                    }
                }
            }
        }

        /*
         * return the far side of the frustum
         */
        public float Far
        {
            get
            {
                return _far;
            }

            set
            {
                bool changed = _far != value;

                if (changed)
                {
                    _far = value;

                    UpdateWorldToCameraTransformer();
                    DerivePerspectiveFromWindow();
                    DeriveOrthographicFromWindow();

                    if (PropertiesChanged != null)
                    {
                        PropertiesChanged(this);
                    }
                }
            }
        }


        /*
         * return the far side of the frustum
         */
        public float Far2
        {
            get
            {
                return _far2;
            }

            set
            {
                bool changed = _far2 != value;

                if (changed)
                {
                    _far2 = value;

                    UpdateWorldToCameraTransformer();
                    DerivePerspectiveFromWindow();
                    DeriveOrthographicFromWindow();

                    if (PropertiesChanged != null)
                    {
                        PropertiesChanged(this);
                    }
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///  event fired when any critical camera properties are changed
        /// </summary>
        public event ChangedEvent PropertiesChanged;

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a camera with viewer, camera window and 
        ///  the perpendicular distance between the eye and the window
        /// </summary>
        /// <param name="viewer">viewer of the camera</param>
        /// <param name="window">camera window that specifies where the window the viewer looks through is</param>
        /// <param name="far">the distance between the viewer's eye and the plane the window is on</param>
        public Camera(Viewer viewer, CameraWindow window, float far, float far2 = 1f)
        {
            _viewer = viewer;
            _window = window;
            _far = far;
            _far2 = far2;

            if (_viewer != null)
            {
                _viewer.PropetiesChanged += OnViewerChanged;
            }

            UpdateWorldToCameraTransformer();
            DerivePerspectiveFromWindow();
            DeriveOrthographicFromWindow();
        }

        /// <summary>
        ///  finalises a camera object
        /// </summary>
        ~Camera()
        {
            if (_viewer != null)
            {
                _viewer.PropetiesChanged -= OnViewerChanged;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///  method that's invoked when viewer properties have been changed
        /// </summary>
        /// <param name="viewer">viewer that is attached to the camera and has properties changed</param>
        void OnViewerChanged(Viewer viewer)
        {
            UpdateWorldToCameraTransformer();
            DerivePerspectiveFromWindow();
            DeriveOrthographicFromWindow();

            if (PropertiesChanged != null)
            {
                PropertiesChanged(this);
            }
        }

        /// <summary>
        ///  updates world to comaera transformer pair as per the current
        ///  viewer base vectors and eye position
        /// </summary>
        protected void UpdateWorldToCameraTransformer()
        {
            // make sure that base vectors are unitised and eye position is normalised
            Vector4f x = _viewer.X;
            Vector4f y = _viewer.Y;
            Vector4f z = _viewer.Z;
            Vector4f eye = _viewer.Eye;

            var dataCameraToWorld = new float[]
            {
                x[0], y[0], z[0], eye[0],
                x[1], y[1], z[1], eye[1],
                x[2], y[2], z[2], eye[2],
                0   , 0   , 0   , 1
            };

            var mtxCameraToWorld = new Matrix4f(dataCameraToWorld);
            Matrix4f mtxWorldToCamera = mtxCameraToWorld.ToInverse();

            WorldToCamera = new Vector4fAffineTransformerPair(mtxWorldToCamera, mtxCameraToWorld);
        }

        /// <summary>
        ///  sets perspective mapping with its frustumic parameters
        /// </summary>
        /// <param name="left">left side of the camera in camera space</param>
        /// <param name="right">right side of the camera in camera space</param>
        /// <param name="top">top of the camera in camera space</param>
        /// <param name="bottom">bottom of the camera in camera space</param>
        /// <param name="near">
        ///  distance between the virtual apex of the frustom and the near plane, which is mapped
        ///  to 0 on z axis in the perspecive space
        /// </param>
        /// <param name="far">distance between the virtual apex of the frustom and the far plane</param>
        /// <param name="far2">z value of the above far plane in the perspective space</param>
        protected void SetPerspectiveByFrustum(
            float left, float right, float top, float bottom, 
            float near, float far, float far2)
        {
            // from camera to perspective
            Matrix4f mtxCameraToPerspective = Matrix4f.CreateFrustumic(left, right, top, 
                bottom, near, far, 0, far2);

            /* update perspective transformers */

            Matrix4f mtxWorldToPerspective = mtxCameraToPerspective * WorldToCamera.Forward;
            Matrix4f mtxPerspectiveToWorld = mtxWorldToPerspective.ToInverse();

            WorldToPerspective = new Vector4fAffineTransformerPair(mtxWorldToPerspective, mtxPerspectiveToWorld);
        }

        /// <summary>
        ///  sets orthographic mapping
        /// </summary>
        /// <param name="left">left side of the camera in camera space</param>
        /// <param name="right">right side of the camera in camera space</param>
        /// <param name="top">top of the camera in camera space</param>
        /// <param name="bottom">bottom of the camera in camera space</param>
        /// <param name="near">
        ///  distance between the eye and the screen which is mapped to 0 on z axis in the orthographic space
        /// </param>
        /// <remarks>
        ///  instead of a frustum, for orthographic projection the window and distance parameters define
        ///  a cube that is mapped to the same region as a frustum is for for perspective projection
        /// </remarks>
        protected void SetOrthographicByCube(float left, float right, float top, float bottom, float near)
        {
            // from camera space to orthographically projected screen 
            // where the screen is on the z = 0 plane
            var mtxCameraToOrthographic = new Matrix4f(new [] {
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, -near,
                0, 0, 0, 1});

            /* orthographic transformer */

            Matrix4f mtxWorldToOrthographic = mtxCameraToOrthographic * WorldToCamera.Forward;
            Matrix4f mtxOrthographicToWorld = mtxWorldToOrthographic.ToInverse();

            WorldToOrthographic = new Vector4fAffineTransformerPair(mtxWorldToOrthographic, mtxOrthographicToWorld);
        }

        /// <summary>
        ///  derives perspective transforming data from camera window properties
        /// </summary>
        protected void DerivePerspectiveFromWindow()
        {
            SetPerspectiveByFrustum(
                _window.Left,
                _window.Right,
                _window.Top,
                _window.Bottom,
                _window.D,
                _window.D + _far,
                _far2
            );
        }

        /// <summary>
        ///  derives orthographic transforming data from camera window properties
        /// </summary>
        protected void DeriveOrthographicFromWindow()
        {
            SetOrthographicByCube(
                _window.Left,
                _window.Right,
                _window.Top,
                _window.Bottom,
                _window.D
            );
        }

        #endregion
    }
}
