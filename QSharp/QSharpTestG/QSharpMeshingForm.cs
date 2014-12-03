using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QSharp.Shader.Geometry.Euclid2D;
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

        private Vector2D _shineV1;
        private Vector2D _shineV2;

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
            var vertices = DelaunayTest.GenerateRandomVertices(0, 0, MeshingPictureBox.Width, MeshingPictureBox.Height, 2, 200);
            _points.AddRange(vertices);
            InvalidateView();
        }

        private void triangulateVerticesToolStripMenuItem_Click(object sender, EventArgs e)
        {

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
