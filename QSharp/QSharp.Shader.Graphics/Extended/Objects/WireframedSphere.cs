using System;
using QSharp.Shader.Graphics.Base.Geometry;
using QSharp.Shader.Graphics.Base.Shapes;
using QSharp.Shader.Graphics.Base.Optics;
using QSharp.Shader.Graphics.Extended.Wireframe;

namespace QSharp.Shader.Graphics.Extended.Objects
{
    public class WireframedSphere : ISphere, IWireframedShape
    {
        #region Fields

        /// <summary>
        ///  backing field for sphere data
        /// </summary>
        private readonly ISphere _sphere;

        #endregion

        #region Properties

        #region Implementation of ISphere

        public Vector4f Center
        {
            get { return _sphere.Center; }
        }

        public float Radius
        {
            get { return _sphere.Radius; }
        }

        public int NumHalfWoofs
        {
            get;
            set;
        }

        public int NumLongitudes
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public WireframedSphere(ISphere sphere)
        {
            NumHalfWoofs = 7;
            NumLongitudes = 18;
            _sphere = sphere;
        }

        #endregion

        #endregion

        #region Methods

        #region Implementation of IWireframedShape

        public void DrawWireframe(ScreenPlotter screen, IWireframeStyle style)
        {
            SimpleWireframeStyle simpleStyle = (SimpleWireframeStyle)style;
            PixelColor8Bit color = simpleStyle.Color;

            int woofPolyOrder = NumLongitudes * 2;
            int longitudePolyOrder = (NumHalfWoofs + 1) * 4;
            Polygon4f polygon = new Polygon4f();

            float angleInc = (float)(Math.PI / (2 * NumHalfWoofs));
            float thetaInc = (float)(Math.PI * 2 / woofPolyOrder);
            for (int i = 0; i < NumHalfWoofs; i++)
            {
                float woofAngle = angleInc * i;
                float cr = (float)(_sphere.Radius * Math.Cos(woofAngle));
                float h = (float)(_sphere.Radius * Math.Sin(woofAngle));

                polygon.Clear();
                for (int j = 0; j < woofPolyOrder; j++)
                {
                    float woofTheta = thetaInc * j;
                    float x = _sphere.Center[Vector4f.IX] + cr * (float)Math.Cos(woofTheta);
                    float y = _sphere.Center[Vector4f.IY] + cr * (float)Math.Sin(woofTheta);
                    float z = _sphere.Center[Vector4f.IZ] + h;
                    polygon.AddVertex(new Vector4f(x, y, z));
                }

                // draw
                screen.ProjectPolygon(polygon, color);

                for (int j = 0; j < woofPolyOrder; j++)
                {
                    polygon[j][Vector4f.IZ] = _sphere.Center[Vector4f.IZ] - h;
                }
                screen.ProjectPolygon(polygon, color);
            }

            angleInc = (float)(Math.PI / NumLongitudes);
            thetaInc = (float)(Math.PI * 2 / longitudePolyOrder);

            for (int i = 0; i < NumLongitudes; i++)
            {
                float longitudeAngle = angleInc * i;
                float xr = _sphere.Radius * (float)Math.Cos(longitudeAngle);
                float yr = _sphere.Radius * (float)Math.Sin(longitudeAngle);

                polygon.Clear();
                for (int j = 0; j < longitudePolyOrder; j++)
                {
                    float longitudeTheta = thetaInc * j;
                    float cosLongitudeTheta = (float)Math.Cos(longitudeTheta);
                    float x = _sphere.Center[Vector4f.IX] + xr * cosLongitudeTheta;
                    float y = _sphere.Center[Vector4f.IY] + yr * cosLongitudeTheta;
                    float z = _sphere.Center[Vector4f.IZ] + _sphere.Radius * (float)Math.Sin(longitudeTheta);
                    polygon.AddVertex(new Vector4f(x, y, z));
                }

                screen.ProjectPolygon(polygon, color);
            }
        }

        #endregion

        #region Implementation of IShape

        public bool ContainsPoint(Vector4f point)
        {
            return _sphere.ContainsPoint(point);
        }

        #endregion

        #endregion
    }
}
