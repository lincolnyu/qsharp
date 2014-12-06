using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.Geometry.Triangulation.Primitive;
using QSharpTest.Shader.Geometry.Triangulation;
using Vector2D = QSharp.Shader.Geometry.Triangulation.Primitive.Vector2D;

namespace QSharpTestG
{
    public partial class QSharpMeshingForm : Form
    {
        #region Enumerations

        public enum Modes
        {
            Normal,
            DefiningPolygons,
            DefiningPolylines,
            DefiningPoints,
            Deleting,
        }

        #endregion

        #region Fields

        private readonly List<List<Vector2D>> _polygons = new List<List<Vector2D>>();

        private readonly List<List<Vector2D>> _polylines = new List<List<Vector2D>>();

        private readonly List<Vector2D> _points = new List<Vector2D>();

        private HashSet<Edge2D> _meshEdges = new HashSet<Edge2D>();

        private Pen _polygonPen;

        private Pen _polylinePen;

        private Brush _pointBrush;

        private Pen _drawnPolygonPen;

        private Pen _drawnPolylinePen;

        private Bitmap[] _bitmaps;

        private int _currentBimapIndex;

        private bool _isDrawing;

        private readonly List<Vector2D> _drawnPoly = new List<Vector2D>();

        private Pen _shinyLinePen;

        private Pen _meshPen;

        private Pen _ccPen;

        private Vector2D _shineV1;
        private Vector2D _shineV2;

        private readonly List<IVector2D> _circumcenters = new List<IVector2D>();
        private readonly List<double> _circumradius = new List<double>();

        #endregion

        #region Constructors

        public QSharpMeshingForm()
        {
            InitializeComponent();

            InitializeMode();

            InitializeGraphics();
        }

        #endregion

        #region Properties

        public Modes CurrentMode
        {
            get; private set;
        }

        #endregion

        #region Methods

        #region Event handlers

        private void QSharpMeshingForm_Load(object sender, EventArgs e)
        {
            var bmp1 = new Bitmap(MeshingPictureBox.Width, MeshingPictureBox.Height);
            var bmp2 = new Bitmap(MeshingPictureBox.Width, MeshingPictureBox.Height);
            _bitmaps = new[] {bmp1, bmp2};
            MeshingPictureBox.Image = bmp1;
            _currentBimapIndex = 0;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MeshingPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
        }

        private void MeshingPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDrawing)
            {
                return;
            }
            var last = _drawnPoly.Last();
            last.X = e.X;
            last.Y = e.Y;
            InvalidateView();
        }

        private void InvalidateView()
        {
            var next = _bitmaps[(_currentBimapIndex + 1)%_bitmaps.Length];
            Redraw(next);
            MeshingPictureBox.Image = next;
        }

        private void MeshingPictureBox_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void MeshingPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            Trace.WriteLine(string.Format("MouseClick at {0}", DateTime.Now));
            switch (CurrentMode)
            {
                case Modes.DefiningPolygons:
                case Modes.DefiningPolylines:
                    if (!_isDrawing)
                    {
                        _drawnPoly.Clear();
                        _isDrawing = true;
                        _drawnPoly.Add(new Vector2D { X = e.X, Y = e.Y });
                    }
                    _drawnPoly.Add(new Vector2D { X = e.X, Y = e.Y });
                    break;
                case Modes.DefiningPoints:
                    _points.Add(new Vector2D { X = e.X, Y = e.Y });
                    break;
                case Modes.Deleting:
                    DeleteAt(e.X, e.Y);
                    break;
            }
            InvalidateView();
        }

        private void MeshingPictureBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Trace.WriteLine(string.Format("MouseDoubleClick at {0}", DateTime.Now));
            FinializeDraw(true);
        }

        private void FinializeDraw(bool removeLastVertex)
        {
            if (!_isDrawing)
            {
                return;
            }

            if (removeLastVertex && (CurrentMode == Modes.DefiningPolygons || CurrentMode == Modes.DefiningPolylines))
            {
                _drawnPoly.RemoveAt(_drawnPoly.Count - 1); // 2 mouse click events must have been fired for the last vertex
            }

            switch (CurrentMode)
            {
                case Modes.DefiningPolygons:
                   
                    _polygons.Add(_drawnPoly.ToList());
                    break;
                case Modes.DefiningPolylines:
                    _polylines.Add(_drawnPoly.ToList());
                    break;
            }
            _drawnPoly.Clear();
            _isDrawing = false;
            InvalidateView();
        }

        private void definePolylinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            definePolygonsToolStripMenuItem.Checked = false;
            definePointsToolStripMenuItem.Checked = false;
            deleteToolStripMenuItem.Checked = false;
            UpdateState();
        }

        private void definePolygonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            definePolylinesToolStripMenuItem.Checked = false;
            definePointsToolStripMenuItem.Checked = false;
            deleteToolStripMenuItem.Checked = false;
            UpdateState();
        }

        private void definePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            definePolylinesToolStripMenuItem.Checked = false;
            definePolygonsToolStripMenuItem.Checked = false;
            deleteToolStripMenuItem.Checked = false;
            UpdateState();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            definePolygonsToolStripMenuItem.Checked = false;
            definePolylinesToolStripMenuItem.Checked = false;
            definePointsToolStripMenuItem.Checked = false;
            UpdateState();
        }

        private void randomVerticesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _points.Clear();
            var marginX = MeshingPictureBox.Width*0.1;
            var marginY = MeshingPictureBox.Height*0.1;
            var vertices = DelaunayTest.GenerateRandomVertices(marginX, marginY, 
                MeshingPictureBox.Width - marginX * 2,
                MeshingPictureBox.Height - marginY, 
                20, 8);
            _points.AddRange(vertices);
            InvalidateView();
        }

        private void triangulateVerticesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Edge2D> hull;
            HashSet<Triangle2D> triangles;
            DelaunayTest.TriangulateVertices(_points, out _meshEdges, out triangles, out hull);
            InvalidateView();
        }

        private void shineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var polygon = _polygons.FirstOrDefault();
            var point = _points.FirstOrDefault();
            if (polygon == null || point == null)
            {
                return;
            }

            int start, end;
            polygon.GetConvexHullEnds(point, out start, out end);

            _shineV1 = polygon[start];
            _shineV2 = polygon[end];

            InvalidateView();
        }

        private void circumcirclesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _circumcenters.Clear();
            _circumradius.Clear();
            foreach (var polygon in _polygons)
            {
                if (polygon.Count == 3)
                {
                    var tri = new Triangle2D();
                    var v1 = polygon[0];
                    var v2 = polygon[1];
                    var v3 = polygon[2];
                    var e12 = new Edge2D();
                    var e23 = new Edge2D();
                    var e31 = new Edge2D();
                    e12.Connect(v1, v2);
                    e23.Connect(v2, v3);
                    e31.Connect(v3, v1);;
                    tri.SetupU(v1, v2, v3, e12, e23, e31);
                    _circumcenters.Add(tri.Circumcenter);
                    _circumradius.Add(tri.Circumradius);
                }
            }
            InvalidateView();
        }

        #endregion

        private void DeleteAt(int x, int y)
        {
            var v = new Vector2D {X = x, Y = y};
            var idel = new List<int>();
            for (var index = 0; index < _polygons.Count; index++)
            {
                var polygon = _polygons[index];
                if (polygon.VertexIsInside(v))
                {
                    idel.Add(index);
                }
            }
            foreach (var i in ((IEnumerable<int>)idel).Reverse())
            {
                _polygons.RemoveAt(i);
            }
            InvalidateView();
        }

        private void InitializeMode()
        {
            definePolylinesToolStripMenuItem.CheckOnClick = true;
            definePolygonsToolStripMenuItem.CheckOnClick = true;
            definePointsToolStripMenuItem.CheckOnClick = true;
            deleteToolStripMenuItem.CheckOnClick = true;
        }

        private void InitializeGraphics()
        {
            _polygonPen = new Pen(Color.Blue, 1);
            _polylinePen = new Pen(Color.Green, 2);
            _pointBrush = new SolidBrush(Color.Red);
            _drawnPolygonPen = new Pen(Color.Cyan, 1);
            _drawnPolylinePen = new Pen(Color.Chartreuse, 1);
            _shinyLinePen = new Pen(Color.Orange, 1);
            _meshPen = new Pen(Color.DeepPink, 1);
            _ccPen = new Pen(Color.GreenYellow, 1);
        }

        private void UpdateState()
        {
            FinializeDraw(true);
            if (definePolygonsToolStripMenuItem.Checked)
            {
                CurrentMode = Modes.DefiningPolygons;
            }
            else if (definePolylinesToolStripMenuItem.Checked)
            {
                CurrentMode = Modes.DefiningPolylines;
            }
            else if (definePointsToolStripMenuItem.Checked)
            {
                CurrentMode = Modes.DefiningPoints;
            }
            else if (deleteToolStripMenuItem.Checked)
            {
                CurrentMode = Modes.Deleting;
            }
            else
            {
                CurrentMode = Modes.Normal;
            }
        }

        private void Redraw(Image image)
        {
            using (var g = Graphics.FromImage(image))
            {
                g.Clear(Color.White);
                foreach (var polygon in _polygons)
                {
                    DrawPolygon(g, _polygonPen, polygon);
                }
                foreach (var polyline in _polylines)
                {
                    DrawPolyline(g, _polylinePen, polyline);
                }
                const float r = 2;
                foreach (var point in _points)
                {
                    var x = (float)point.X;
                    var y = (float)point.Y;
                    g.FillEllipse(_pointBrush, x - r, y - r, 2*r, 2*r);
                }
                if (_isDrawing)
                {
                    switch (CurrentMode)
                    {
                        case Modes.DefiningPolygons:
                            DrawPolygon(g, _drawnPolygonPen, _drawnPoly);
                            break;
                        case Modes.DefiningPolylines:
                            DrawPolyline(g, _drawnPolylinePen, _drawnPoly);
                            break;
                    }
                }
                if (_shineV1 != null && _shineV2 != null)
                {
                    var point = _points.First();
                    g.DrawLine(_shinyLinePen, (float)_shineV1.X, (float)_shineV1.Y, (float)point.X, (float)point.Y);
                    g.DrawLine(_shinyLinePen, (float)_shineV2.X, (float)_shineV2.Y, (float)point.X, (float)point.Y);
                }
                if (_meshEdges != null && _meshEdges.Count > 0)
                {
                    foreach (var edge in _meshEdges)
                    {
                        var v1 = edge.V1;
                        var v2 = edge.V2;
                        g.DrawLine(_meshPen, (float)v1.X, (float)v1.Y, (float)v2.X, (float)v2.Y);
                    }
                }
                if (_circumcenters.Any())
                {
                    for (var i = 0; i < _circumcenters.Count; i++)
                    {
                        var cc = _circumcenters[i];
                        var cr = _circumradius[i];
                        var x1 = (float)(cc.X - cr);
                        var y1 = (float)(cc.Y - cr);
                        g.DrawEllipse(_ccPen, x1, y1, (float)(cr*2), (float)(cr*2));
                    }
                }
            }
        }

        private void DrawPolygon(Graphics g, Pen pen, IList<Vector2D> polygon)
        {
            for (var i = 0; i < polygon.Count; i++)
            {
                var v1 = polygon[i];
                var v2 = polygon[(i + 1) % polygon.Count];
                g.DrawLine(pen, (float)v1.X, (float)v1.Y, (float)v2.X, (float)v2.Y);
            }
        }

        private void DrawPolyline(Graphics g, Pen pen, IList<Vector2D> polyline)
        {
            for (var i = 0; i < polyline.Count - 1; i++)
            {
                var v1 = polyline[i];
                var v2 = polyline[i + 1];
                g.DrawLine(pen, (float)v1.X, (float)v1.Y, (float)v2.X, (float)v2.Y);
            }
        }

        #endregion
    }
}
