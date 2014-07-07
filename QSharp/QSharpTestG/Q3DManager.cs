using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using QSharp.Shader.Graphics.Base.Geometry;
using QSharp.Shader.Graphics.Base.World;
using QSharp.Shader.Graphics.Csg;
using QSharp.Shader.Graphics.Base.Objects;
using QSharp.Shader.Graphics.Base.Optics;
using QSharp.Shader.Graphics.Extended.Objects;
using QSharp.Shader.Graphics.Extended.Wireframe;
using QSharp.Shader.Graphics.RayTracer;
using System.Drawing;

namespace QSharpTestG
{
    /// <summary>
    ///  defines an object that manages 3D model creation, and related
    ///  environmental settings for 3D rendering demonstration
    /// </summary>
    public class Q3DManager
    {
        #region Enumerations

        /// <summary>
        ///  two projection modes
        /// </summary>
        public enum ProjectionMode
        {
            Orthographic,
            Perspective,
        }

        /// <summary>
        ///  describes the direction in which the camera is to be
        ///  moved in its own coordinate system
        /// </summary>
        public enum MoveDiriction
        {
            Forward,
            Backward,
            Upward,
            Downward,
            Leftward,
            Rightward
        }

        #endregion

        #region Fields

        /// <summary>
        ///  the scene to be viewed
        /// </summary>
        readonly SimpleScene _scene = new SimpleScene();

        /// <summary>
        ///  camera through which the scene is viewed
        /// </summary>
        Camera _camera;

        /// <summary>
        ///  screen currently picked to use
        /// </summary>
        RectangularScreen _currentScreen;

        /// <summary>
        ///  instance of perspective screen tied up to the camera
        /// </summary>
        PerspectiveScreen _perspectiveScreen;

        /// <summary>
        ///  instance of orthographic screen tied up to the camera
        /// </summary>
        OrthographicScreen _orthographicScreen;

        /// <summary>
        ///  renderer currently selected to use, corresponding to the current screen
        /// </summary>
        RayTraceRenderer _currentRenderer;

        /// <summary>
        ///  backing field for the flag that indicating the current projection mode
        /// </summary>
        ProjectionMode _currentProjectionMode;

        /// <summary>
        ///  instance of ray-trace renderer that renders the scene with perspective screen
        /// </summary>
        RayTraceRenderer _perspectiveRenderer;

        /// <summary>
        ///  instance of ray-trace renderer that renders the scene with orthographic screen
        /// </summary>
        RayTraceRenderer _orthographicRenderer;

        /// <summary>
        ///  all wireframe objects added to the model
        /// </summary>
        readonly List<IWireframedShape> _wireframeShapes = new List<IWireframedShape>();

        #endregion

        #region Properties

        protected Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                    var window = new CameraWindow(-1, 1, 1, 1, 1);
                    var eye = new Vector4f(0, 0, 0);
                    var cx = new Vector4f(1, 0, 0);
                    var cy = new Vector4f(0, 1, 0);
                    var cz = new Vector4f(0, 0, 1);
                    var viewer = new Viewer(eye, cx, cy, cz);
                    _camera = new Camera(viewer, window, 5); 
                }
                return _camera;
            }
        }

        
        /// <summary>
        ///  property for getting and setting camera position
        /// </summary>
        public Vector4f CameraPosition
        {
            get
            {
                return Camera.Viewer.Eye;
            }

            set
            {
                Camera.Viewer.Eye = value;
            }
        }

        /// <summary>
        ///  property for getting and setting camera's X axis
        /// </summary>
        public Vector4f CameraAxisX
        {
            get
            {
                return Camera.Viewer.X;
            }

            set
            {
                Camera.Viewer.X = value;
            }
        }

        /// <summary>
        ///  property for getting and setting camera's Y axis
        /// </summary>
        public Vector4f CameraAxisY
        {
            get
            {
                return Camera.Viewer.Y;
            }

            set
            {
                Camera.Viewer.Y = value;
            }
        }

        /// <summary>
        ///  property for getting and setting camera's Z axis
        /// </summary>
        public Vector4f CameraAxisZ
        {
            get
            {
                return Camera.Viewer.Z;
            }

            set
            {
                Camera.Viewer.Z = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates the manager with default settings
        /// </summary>
        public Q3DManager()
        {
            _currentProjectionMode = ProjectionMode.Perspective;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  sets a specified image as the current image with window (screen) 
        ///  position and dimensions, window distance and rendering depth
        /// </summary>
        /// <param name="image">image that the viewer sees on the canvas</param>
        /// <param name="left">left side of the window in camera space</param>
        /// <param name="right">right side of the window in camera space</param>
        /// <param name="top">top of the window in camera space</param>
        /// <param name="bottom"></param>
        /// <param name="d">
        ///  perpendicular distance in camera space between the viewer's eye and the canvas
        /// </param>
        /// <param name="far">distance between screen and far end of frustum in camera coordinates</param>
        /// <param name="renderDepth">depth of rendering</param>
        public Q3DManager SetImage(RectangularScreen.IImage image, float left, float right,
            float top, float bottom, float d, float far, int renderDepth)
        {
            var window = new CameraWindow(left, right, top, bottom, d);
            if (_camera == null)
            {
                var eye = new Vector4f(0, 0, 0);
                var cx = new Vector4f(1, 0, 0);
                var cy = new Vector4f(0, 1, 0);
                var cz = new Vector4f(0, 0, 1);
                var viewer = new Viewer(eye, cx, cy, cz);
                _camera = new Camera(viewer, window, far); 
            }
            else
            {
                _camera.Window = window;
                _camera.Far = far;
            }
            
            _perspectiveScreen = new PerspectiveScreen(_camera, image);
            _orthographicScreen = new OrthographicScreen(_camera, image);
            _perspectiveRenderer = new RayTraceRenderer(_perspectiveScreen, _scene, renderDepth);
            _orthographicRenderer = new RayTraceRenderer(_orthographicScreen, _scene, renderDepth);

            SetProjectionMode(_currentProjectionMode);
            return this;
        }

        /// <summary>
        ///  sets a specified image as the current image with window (screen) 
        ///  position and dimensions, window distance and rendering depth
        /// </summary>
        /// <param name="image">image that the viewer sees on the canvas</param>
        /// <param name="width">width of the centralised camera window</param>
        /// <param name="height">height of the centralised camera window</param>
        /// <param name="d">
        ///  perpendicular distance in camera space between the viewer's eye and the canvas
        /// </param>
        /// <param name="far">distance between screen and far end of frustum in camera coordinates</param>
        /// <param name="renderDepth">depth of rendering</param>
        public void SetImage(RectangularScreen.IImage image, float width, float height,
            float d, float far, int renderDepth)
        {
            float left = -width / 2;
            float right = width / 2;
            float top = -height / 2;
            float bottom = height / 2;
            SetImage(image, left, right, top, bottom, d, far, renderDepth);
        }

        /// <summary>
        ///  sets image to the camera with specified image object, distance measured in 
        ///  camera space between eye and image plane, ratio of image dimension to widnow 
        ///  dimension and rendering depth
        /// </summary>
        /// <param name="image">image to draw objects on the screen on</param>
        /// <param name="targetDiagonal">image diagonal measured in the camera space</param>
        /// <param name="dr">ratio of perpendicular distance between viewer's eye and the screen to diagonal of the window</param>
        /// <param name="farr">ration of distance between screen and far end of frustum in camera coordinates to diagnoal of the window</param>
        /// <param name="renderDepth">depth of rendering</param>
        public void SetImage(RectangularScreen.IImage image, float targetDiagonal, float dr, float farr, int renderDepth)
        {
            float imageWidth = image.Width;
            float imageHeight = image.Height;
            var imageDiagonal = (float)Math.Sqrt(imageWidth*imageWidth+imageHeight*imageHeight);
            float ratio = targetDiagonal / imageDiagonal;
            float width = imageWidth * ratio;
            float height = imageHeight * ratio;
            float d = dr * targetDiagonal;
            float far = farr * targetDiagonal;
            SetImage(image, width, height, d, far, renderDepth); 
        }

        /// <summary>
        ///  sets the current projection mode, it sets the current screen to the screen corresponding to the specified projection mode
        /// </summary>
        public void SetProjectionMode(ProjectionMode projMode)
        {
            switch (projMode)
            {
                case ProjectionMode.Perspective:
                    _currentScreen = _perspectiveScreen;
                    _currentRenderer = _perspectiveRenderer;
                    _currentProjectionMode = projMode;
                    break;
                case ProjectionMode.Orthographic:
                    _currentScreen = _orthographicScreen;
                    _currentRenderer = _orthographicRenderer;
                    _currentProjectionMode = projMode;
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        ///  sets the position of the camera, the direction it points in, and the angle it rotates around
        ///  the axis of that direction 
        /// </summary>
        /// <param name="x">x component of the camera position</param>
        /// <param name="y">y component of the camera position</param>
        /// <param name="z">z component of the camera position</param>
        /// <param name="ax">x component of the looking direction</param>
        /// <param name="ay">y component of the looking direction</param>
        /// <param name="az">z component of the looking direction</param>
        /// <param name="tilt">angle by which the camera rotate around the above looking direction</param>
        public void SetCameraAttitude(float x, float y, float z, float ax, float ay, float az, float tilt)
        {
            var eye = new Vector4f(x, y, z);
            var vz = new Vector4f(ax, ay, az);

            Camera.Viewer = new Viewer(eye, vz, tilt);
        }

        /// <summary>
        ///  rotates the camera around specified axis by specified angle
        /// </summary>
        /// <param name="ax">x component of the axis</param>
        /// <param name="ay">y component of the axis</param>
        /// <param name="az">z component of the axis</param>
        /// <param name="angle">angle by which it rotates</param>
        public void RotateCamera(float ax, float ay, float az, float angle)
        {
            Camera.Viewer.Rotate(new Vector4f(ax, ay, az),  angle);
        }

        /// <summary>
        ///  translates the camera in the specified direction in the camera
        ///  coordinate system
        /// </summary>
        /// <param name="diriction">the direction in which to translate the camera</param>
        /// <param name="distance">the amount by which to move</param>
        public void MoveCamera(MoveDiriction diriction, float distance)
        {
            float x = CameraPosition.X;
            float y = CameraPosition.Y;
            float z = CameraPosition.Z;

            // it's assumed that the axes are unitised
            Vector4f vmove;

            switch (diriction)
            {
                case MoveDiriction.Backward:
                    vmove = -CameraAxisZ;
                    break;
                case MoveDiriction.Forward:
                    vmove = CameraAxisZ;
                    break;
                case MoveDiriction.Upward:
                    vmove = -CameraAxisY;
                    break;
                case MoveDiriction.Downward:
                    vmove = CameraAxisY;
                    break;
                case MoveDiriction.Leftward:
                    vmove = -CameraAxisX;
                    break;
                case MoveDiriction.Rightward:
                    vmove = CameraAxisX;
                    break;
                default:
                    return;
            }
            x += vmove.X*distance;
            y += vmove.Y*distance;
            z += vmove.Z*distance;

            CameraPosition.Set(x, y, z);
        }

        /// <summary>
        ///  translates camera to specified position
        /// </summary>
        /// <param name="x">x component of the position</param>
        /// <param name="y">y component of the position</param>
        /// <param name="z">z component of the position</param>
        public void SetCameraPosition(float x, float y, float z)
        {
            Camera.Viewer.Eye = new Vector4f(x, y, z);
        }

        /// <summary>
        ///  add an optical object to the scene
        /// </summary>
        /// <param name="optical">optical object</param>
        public void AddOptical(IOptical optical)
        {
            _scene.Opticals.Insert(0, optical);
            var wireframedShape = optical as IWireframedShape;
            if(wireframedShape != null)
            {
                _wireframeShapes.Add(wireframedShape);
            }
        }

        /// <summary>
        ///  creates a point light with specified properties and adds it to the scene
        /// </summary>
        /// <param name="cx">X coordinate of the position</param>
        /// <param name="cy">Y coordinate of the position</param>
        /// <param name="cz">Z coordinate of the position</param>
        /// <param name="rd">red component of the diffuse component of the light</param>
        /// <param name="gd">green component of the diffuse component of the light</param>
        /// <param name="bd">blue component of the diffuse component of the light</param>
        /// <param name="rs">red component of the specular component of the light</param>
        /// <param name="gs">green component of the specular component of the light</param>
        /// <param name="bs">blue component of the specular component of the light</param>
        /// <returns>the point light that is created and added to the scene</returns>
        public PointLight AddPointLight(float cx, float cy, float cz,
            float rd = 0.5f, float gd = 0.5f, float bd = 0.5f,
            float rs = 0.5f, float gs = 0.5f, float bs = 0.5f)
        {
            var lightD = new Light(rd, gd, bd);
            var lightS = new Light(rs, gs, bs);
            var position = new Vector4f(cx, cy, cz);

            var pointLight = new PointLight(position, lightD, lightS);
            _scene.PointLights.Insert(0, pointLight);

            return pointLight;
        }

        /// <summary>
        ///  removes an object, which can be either an optical object or a light
        /// </summary>
        /// <param name="obj">the object to remove</param>
        public void RemoveObject(IObject obj)
        {
            var pl = obj as PointLight;
            if (pl != null)
            {
                _scene.PointLights.Remove(pl);
            }
            var opt = obj as IOptical;
            if (opt != null)
            {
                _scene.Opticals.Remove(opt);
            }
            var ws = obj as IWireframedShape;
            if (ws != null)
            {
                _wireframeShapes.Remove(ws);
            }
        }

        /// <summary>
        ///  load a sample model to render
        /// </summary>
        public void LoadDemo()
        {
            CsgSphere leftSphere = Q3DHelper.CreateSphere(0, 0, 0, 3);
            CsgSphere rightSphere = Q3DHelper.CreateSphere(4, 0, 0, 3);
            ICsgShape csg = Q3DHelper.CreateCsg(leftSphere, rightSphere, CsgNode.Operation.Subtraction);
            leftSphere.SetOpticEasy(0.1f, 1f, 0.1f, 0f, 0f);
            rightSphere.SetOptic(0.1f, 1f, 0.1f, 0f, 0f, 0.9f, 0.1f, 0f);

            AddOptical((IOptical)csg);

            PointLight pl1 = AddPointLight(2f - 10f, 0f, 0f);
            PointLight pl2 = AddPointLight(2f + 10f, 0f, 0f);
            pl1.SetPointLightEasy(1f, 1f, 1f);
            pl2.SetPointLightEasy(1f, 1f, 1f);
        }

        /// <summary>
        ///  load a model from XML file in specific format 
        /// </summary>
        /// <param name="sr">stream reader where the xml is to be read from</param>
        public void LoadFromXml(StreamReader sr)
        {
            using (var xtr = new XmlTextReader(sr))
            {
                throw new NotImplementedException();                
            }
        }

        /// <summary>
        ///  initiates the rendering process
        /// </summary>
        public void Render()
        {
            _currentRenderer.Render();
        }

        /// <summary>
        ///  Draws wireframe objects with default colour (gray)
        /// </summary>
        public void DrawWireframe()
        {
            DrawWireframe(Color.Gray);
        }

        /// <summary>
        ///  draw wireframe objects with specified colour
        /// </summary>
        public void DrawWireframe(Color color)
        {
            var sp = new ScreenPlotter(_currentScreen);
            var sws = new SimpleWireframeStyle(new PixelColor8Bit(color.R, color.G, color.B));
            foreach (IWireframedShape ws in _wireframeShapes)
            {
                ws.DrawWireframe(sp, sws);
            }
        }

        #endregion
    }
}
