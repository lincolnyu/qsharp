using System;
using QSharp.Shader.Graphics.Base.Geometry;
using QSharp.Shader.Graphics.Base.Optics;
using QSharp.Shader.Graphics.Base.World;
using QSharp.Shader.Graphics.Base.Shapes;
using QSharp.Shader.Graphics.Raster.Plot;
using QSharp.Shader.Graphics.Raster.Clip;

namespace QSharp.Shader.Graphics.Extended.Wireframe
{
    public class ScreenPlotter
    {
        #region Nested types

        class PointDrawer
        {
            private readonly RectangularScreen _screen;
            private readonly PixelColor8Bit _color;

            /// <summary>
            ///  constructor that initializes the object to create with
            ///  screen and color
            /// </summary>
            /// <param name="screen">screen to work on</param>
            /// <param name="color">color to draw the points with</param>
            public PointDrawer(RectangularScreen screen, PixelColor8Bit color)
            {
                _screen = screen;
                _color = color;
            }

            /// <summary>
            ///  method to draw a point on the screen as the result of being invoked by
            ///  the line plotting method when it visits the point as part of the line
            ///  it draws
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public void Draw(int x, int y)
            {
                _screen.SetPixel(x, y, _color);
            }
        }

        #endregion

        #region Fields

        /// <summary>
        ///  backing field for screen the plotter work on
        /// </summary>
        private readonly RectangularScreen _screen;

        private OrthographicTransformerPair _othoTp /*= null*/;

        #endregion

        #region Constructors

        /// <summary>
        ///  constructor that initializes the plotter with screen
        ///  provided for it to work on
        /// </summary>
        /// <param name="screen"></param>
        public ScreenPlotter(RectangularScreen screen)
        {
            _screen = screen;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  draw a line between two given points using given color
        /// </summary>
        /// <param name="x0">x coord of the first point</param>
        /// <param name="y0">y coord of the first point</param>
        /// <param name="x1">x coord of the second point</param>
        /// <param name="y1">y coord of the second point</param>
        /// <param name="color">color of the line to draw</param>
        public void DrawLine(float x0, float y0, float x1, float y1, PixelColor8Bit color)
        {
            var x0i = (int)Math.Round(x0);
            var y0i = (int)Math.Round(y0);
            var x1i = (int)Math.Round(x1);
            var y1i = (int)Math.Round(y1);

            var pd = new PointDrawer(_screen, color);
            BresenhamLine.Draw(x0i, y0i, x1i, y1i, pd.Draw);
        }

        /// <summary>
        ///  draw a line as the result of projecting a straightline in the 3D world
        ///  on to the screen
        /// </summary>
        /// <param name="v0">point at the beginning of the 3D line</param>
        /// <param name="v1">point at the end of the 3D line</param>
        /// <param name="color">color of the line</param>
        public void ProjectLine(Vector4f v0, Vector4f v1, PixelColor8Bit color)
        {
            Vector4f v0t, v1t;

            bool b0 = _screen.WorldToScreen.ForwardTransform(v0, out v0t);
            bool b1 = _screen.WorldToScreen.ForwardTransform(v1, out v1t);

            if (!b0 && !b1)
                return;

            if (!b0)
            {
                v0t = GetOnScreen(v0, v1);
                if (v0t == null)
                    return;
            }
            else if (!b1)
            {
                v1t = GetOnScreen(v1, v0);
                if (v1t == null)
                    return;
            }

            float x0 = v0t.X;
            float y0 = v0t.Y;
            float x1 = v1t.X;
            float y1 = v1t.Y;

            // clip (note that the boundary points are inclusive)
            if (LineClipLB.Clip(ref x0, ref y0, ref x1, ref y1, 0, _screen.Width-1, 0, _screen.Height-1))
            {
                DrawLine(x0, y0, x1, y1, color);
            }
        }

        /// <summary>
        ///  draws a polygon in the 3D world on the screen
        /// </summary>
        /// <param name="polygon">
        ///  the polygon represented by vertices in 3D world coordinates
        /// </param>
        /// <param name="color"> color the polygon is to be drawn in </param>
        public void ProjectPolygon(Polygon4f polygon, PixelColor8Bit color)
        {
            int count = polygon.NumVertices;

            var v4d0 = polygon[count - 1]; // last vertex (the vertex before the first)

            for (int i = 0; i < count; i++)
            {
                var v4d1 = polygon[i];
                ProjectLine(v4d0, v4d1, color);
                v4d0 = v4d1;
            }
        }

        /// <summary>
        ///  tries to get the point where the vector from va to vb intersects the screen;
        ///  mathematically it doesn't matter which of the two ends of the vector comes first
        /// </summary>
        /// <param name="va">starting point of the vector</param>
        /// <param name="vb">ending point of the vector</param>
        /// <returns></returns>
        Vector4f GetOnScreen(Vector4f va, Vector4f vb)
        {
            Vector4f vat;
            Vector4f tmp, tmp1;

            if (_othoTp == null)
            {
                Matrix4f fwd = _screen.Camera.WorldToOrthographic.Forward;
                Matrix4f bwd = _screen.Camera.WorldToOrthographic.Backward;
                _othoTp = new OrthographicTransformerPair(fwd, bwd);
            }

            _othoTp.ForwardTransform(va, out vat);
            _othoTp.ForwardTransform(vb, out tmp);
            float vatz = vat.Z;
            float vbtz = tmp.Z;
            
            tmp = tmp * vatz - vat * vbtz;
            vatz -= vbtz;

            if (vatz < 0.001)
                return null;    // instable solution

            tmp *= 1 / vatz;

            _othoTp.BackwardTransform(tmp, out tmp1);
            _screen.WorldToScreen.ForwardTransform(tmp1, out vat);

            return vat;
        }

        #endregion
    }
}
