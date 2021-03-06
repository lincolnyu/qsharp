﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.Geometry.Triangulation.Methods;
using QSharp.Shader.Geometry.Triangulation.Primitive;
using QSharpTest.Shader.Geometry.Triangulation;
using Vector2D = QSharp.Shader.Geometry.Triangulation.Primitive.Vector2D;
using QSharp.Shader.Geometry.Triangulation.Helpers;

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
            DefiningSizeField
        }

        public enum PolygonState
        {
            Normal,
            Contained,
        }

        #endregion

        #region Delegates

        private delegate void RenderDelegate();

        #endregion

        #region Fields

        private PolygonState[] _polygonStates;
        private readonly List<List<Vector2D>> _polygons = new List<List<Vector2D>>();
        private readonly List<List<Vector2D>> _polylines = new List<List<Vector2D>>();

        private readonly List<Vector2D> _points = new List<Vector2D>();

        private readonly Dictionary<int, double> _fieldPoints = new Dictionary<int, double>();

        private readonly List<List<Vector2D>> _simplifiedPolylines = new List<List<Vector2D>>();
        private readonly List<List<Vector2D>> _simplifiedPolygons = new List<List<Vector2D>>();


        private HashSet<Edge2D> _meshEdges = new HashSet<Edge2D>();

        private Pen _polygonPen;
        private Pen _internalPolygonPen;

        private Pen _polylinePen;
        private Pen _simplifiedPolyPen;
        private Brush _simplifiedPolyPointBrush;

        private Brush _pointBrush;

        private Pen _drawnPolygonPen;

        private Pen _drawnPolylinePen;

        private Pen _inwardsPen;
        private Pen _outwardsPen;

        private Bitmap[] _bitmaps;

        private int _currentBimapIndex;

        private bool _isDrawing;

        private readonly List<Vector2D> _drawnPoly = new List<Vector2D>();

        private Pen _shinyLinePen;

        private Pen _meshPen;
        private Pen _hullPen;

        private Pen _ccPen;

        private Vector2D _shineV1;
        private Vector2D _shineV2;

        private readonly List<IVector2D> _circumcenters = new List<IVector2D>();
        private readonly List<double> _circumradius = new List<double>();
        private List<Edge2D> _hull;

        private Daft _oneStepDaft;

        private int _iterationCounter; // for debugging purposes only

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
            BeginInvoke(new RenderDelegate(Render));
        }

        private void Render()
        {
            var next = _bitmaps[(_currentBimapIndex + 1) % _bitmaps.Length];
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
                case Modes.DefiningSizeField:
                {
                    var fmf = new FieldMagnitudeForm();
                    if (fmf.ShowDialog() == DialogResult.OK)
                    {
                        _points.Add(new Vector2D { X = e.X, Y = e.Y });
                        _fieldPoints[_points.Count - 1] = fmf.Magnitude;
                    }
                    break;
                }
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
                    UpdatePolygonStates();
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
            DefineSizeFieldToolStripMenuItem.Checked = false;
            UpdateState();
        }

        private void definePolygonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            definePolylinesToolStripMenuItem.Checked = false;
            definePointsToolStripMenuItem.Checked = false;
            deleteToolStripMenuItem.Checked = false;
            DefineSizeFieldToolStripMenuItem.Checked = false;
            UpdateState();
        }

        private void definePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            definePolylinesToolStripMenuItem.Checked = false;
            definePolygonsToolStripMenuItem.Checked = false;
            deleteToolStripMenuItem.Checked = false;
            DefineSizeFieldToolStripMenuItem.Checked = false;
            UpdateState();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            definePolygonsToolStripMenuItem.Checked = false;
            definePolylinesToolStripMenuItem.Checked = false;
            definePointsToolStripMenuItem.Checked = false;
            DefineSizeFieldToolStripMenuItem.Checked = false;
            UpdateState();
        }


        private void DefineSizeFieldToolStripMenuItemOnClick(object sender, EventArgs e)
        {
            definePolylinesToolStripMenuItem.Checked = false;
            definePolygonsToolStripMenuItem.Checked = false;
            deleteToolStripMenuItem.Checked = false;
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
                20, 30);
            _points.AddRange(vertices);
            InvalidateView();
        }

        private void triangulateVerticesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HashSet<Triangle2D> triangles;
            DelaunayTest.TriangulateVertices(_points, out _meshEdges, out triangles, out _hull);
            InvalidateView();
        }

        private void triangulateAllToolStripMenuItem_Click(object sender, EventArgs args)
        {
            // NOTE two methods 
            // 1. MeshInOneGo() displays the mesh after completion
            // 2. MeshLoop() thread displays the progress
            
            MeshInOneGo();

            //var t = new Thread(MeshLoop);
            //t.Start();
        }

        private void MeshLoop()
        {
            while (MeshStep())
            {
                Thread.Sleep(3);
            }
        }
        
        private void triangulateOneStepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MeshStep();
        }

        private void MeshInOneGo()
        {
            var daft = new Daft { SizeField = GetSize };
            var meanSize = GetMeanSize();

            SetFronts(daft);

            daft.SetupQuadtree(meanSize);

            daft.LoadFronts();

            daft.GenerateMesh();

            _meshEdges.Clear();

            foreach (var e in daft.Qst.SortedEdges.Values)
            {
                _meshEdges.Add(e);
            }

            InvalidateView();
        }

        private bool MeshStep()
        {
            if (_oneStepDaft == null)
            {
                _oneStepDaft = new Daft { SizeField = GetSize };
                var meanSize = GetMeanSize();

                SetFronts(_oneStepDaft);

                _oneStepDaft.SetupQuadtree(meanSize);

                _oneStepDaft.LoadFronts();

                _iterationCounter = 0;
            }

            lock (this)
            {
                var b = _oneStepDaft.GenerateMeshOneStep();
                if (!b)
                {
                    return false;
                }
                _oneStepDaft.CheckDaftIntegrity();

                _iterationCounter++;

                UpdateMesh();
            }

            InvalidateView();
            return true;
        }

        private void UpdateMesh()
        {
            _meshEdges.Clear();

            foreach (var e in _oneStepDaft.Qst.SortedEdges.Values)
            {
                _meshEdges.Add(e);
            }
        }

        private void SetFronts(Daft daft)
        {
            SimplifyPolygonsAndPolylines();

            foreach (var polygon in _simplifiedPolygons)
            {
                var d = polygon.GetSignedPolygonArea();
                var front = new DaftFront(true);
                var poly = polygon;
                if (d > 0) // make sure they are counterclockwise
                {
                    poly = polygon.ToList();
                    poly.Reverse();
                }

                for (var i = 0; i < poly.Count; i++)
                {
                    var edge = new Edge2D();
                    edge.Connect(poly[i], poly[(i+1)%poly.Count]);
                    front.AddEdge(i, edge);
                }

                daft.Inwards.Add(front);
            }

            foreach (var polyline in _simplifiedPolylines)
            {
                var front = new DaftFront(false);
                for (var i = 0; i < polyline.Count - 1; i++)
                {
                    var edge = new Edge2D();
                    edge.Connect(polyline[i], polyline[i + 1]);
                    front.AddEdge(i, edge);
                }
                for (var i = polyline.Count - 1; i > 0; i--)
                {
                    var edge = new Edge2D();
                    edge.Connect(polyline[i], polyline[i - 1]);
                    front.AddEdge(i, edge);
                }
                daft.Outwards.Add(front);
            }
        }

        private void LoadPolygonToFronts(Daft daft)
        {
            for (int i = 0; i < _polygons.Count; i++)
            {
                var polygon = _polygons[i];
                var state = _polygonStates[i];

                if (state == PolygonState.Contained)
                {
                    var outwards = new DaftFront(false);

                    // segmentation?

                    //outwards.AddEdge();
                }
                else
                {
                    var inwards = new DaftFront(true);

                }
            }
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
                    e31.Connect(v3, v1);
                    tri.SetupU(v1, v2, v3, e12, e23, e31);
                    _circumcenters.Add(tri.Circumcenter);
                    _circumradius.Add(tri.Circumradius);
                }
            }
            InvalidateView();
        }

        private void SegmentStraightLinesToolStripMenuItemOnClick(object sender, EventArgs e)
        {
            SimplifyPolygonsAndPolylines();
            InvalidateView();
        }

        private void squareToMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fv = new Vector2D{X = MeshingPictureBox.Width/2.0, Y = MeshingPictureBox.Height/2.0};
            _points.Add(fv);
            _fieldPoints[_points.Count - 1] = 50;

            const int d = 200;
            var rect = new List<Vector2D>
            {
                new Vector2D {X = fv.X - d, Y = fv.Y - d},
                new Vector2D {X = fv.X + d, Y = fv.Y - d},
                new Vector2D {X = fv.X + d, Y = fv.Y + d},
                new Vector2D {X = fv.X - d, Y = fv.Y + d}
            };
            _polygons.Add(rect);
            UpdatePolygonStates();
            InvalidateView();
        }


        private void squareAndFieldToMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var fv = new Vector2D { X = MeshingPictureBox.Width / 2.0, Y = MeshingPictureBox.Height / 2.0 };
            _points.Add(fv);
            _fieldPoints[_points.Count - 1] = 50;

            const int d = 200;
            var rect = new List<Vector2D>
            {
                new Vector2D {X = fv.X - d, Y = fv.Y - d},
                new Vector2D {X = fv.X + d, Y = fv.Y - d},
                new Vector2D {X = fv.X + d, Y = fv.Y + d},
                new Vector2D {X = fv.X - d, Y = fv.Y + d}
            };
            _polygons.Add(rect);

            // add a point that defines the field
            _points.Add(new Vector2D { X = 420, Y = 250 });
            _fieldPoints[_points.Count - 1] = 5;

            UpdatePolygonStates();
            InvalidateView();
        }


        #endregion

        private void SimplifyPolygonsAndPolylines()
        {
            _simplifiedPolylines.Clear();
            _simplifiedPolygons.Clear();

            foreach (var polyline in _polylines)
            {
                var output = SegmentationHelper.Output(polyline, false, GetLength);
                var outputPolyline = output.Select(o => new Vector2D
                {
                    X = o.X,
                    Y = o.Y
                }).ToList();

                _simplifiedPolylines.Add(outputPolyline);
            }

            foreach (var polygon in _polygons)
            {
                var output = SegmentationHelper.Output(polygon, true, GetLength);
                var outputPolygon = output.Select(o => new Vector2D
                {
                    X = o.X,
                    Y = o.Y
                }).ToList();

                _simplifiedPolygons.Add(outputPolygon);
            }
        }

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
            DefineSizeFieldToolStripMenuItem.CheckOnClick = true;
        }

        private void InitializeGraphics()
        {
            _polygonPen = new Pen(Color.Blue, 2);
            _internalPolygonPen = new Pen(Color.Purple, 1);
            _polylinePen = new Pen(Color.Green, 2);
            _simplifiedPolyPen = new Pen(Color.Brown, 1);
            _simplifiedPolyPointBrush = new SolidBrush(Color.Brown);
            _pointBrush = new SolidBrush(Color.Red);
            _drawnPolygonPen = new Pen(Color.Cyan, 1);
            _drawnPolylinePen = new Pen(Color.Chartreuse, 1);
            _shinyLinePen = new Pen(Color.Orange, 1);
            _meshPen = new Pen(Color.Gray, 1);
            _hullPen = new Pen(Color.Brown, 2);
            _ccPen = new Pen(Color.GreenYellow, 1);

            _inwardsPen = new Pen(Color.Red, 2);
            _outwardsPen = new Pen(Color.Orange, 2);
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
            else if (DefineSizeFieldToolStripMenuItem.Checked)
            {
                CurrentMode = Modes.DefiningSizeField;
            }
            else
            {
                CurrentMode = Modes.Normal;
            }
        }

        private void Redraw(Image image)
        {
            lock (this)
            {
                using (var g = Graphics.FromImage(image))
                {
                    g.Clear(Color.White);
                    for (var i = 0; i < _polygons.Count; i++)
                    {
                        var info = _polygonStates[i];
                        var polygon = _polygons[i];
                        var pen = info == PolygonState.Normal ? _polygonPen : _internalPolygonPen;
                        DrawPolygon(g, pen, polygon);
                    }
                    foreach (var polyline in _polylines)
                    {
                        DrawPolyline(g, _polylinePen, polyline);
                    }
                    // simplified polylines
                    const float r = 2;
                    foreach (var polyline in _simplifiedPolylines)
                    {
                        DrawPolyline(g, _simplifiedPolyPen, polyline);
                        foreach (var point in polyline)
                        {
                            var x = (float)point.X;
                            var y = (float)point.Y;
                            g.FillEllipse(_simplifiedPolyPointBrush, x - r, y - r, 2 * r, 2 * r);
                        }
                    }
                    // simplified polygons
                    foreach (var polygon in _simplifiedPolygons)
                    {
                        DrawPolygon(g, _simplifiedPolyPen, polygon);
                        foreach (var point in polygon)
                        {
                            var x = (float)point.X;
                            var y = (float)point.Y;
                            g.FillEllipse(_simplifiedPolyPointBrush, x - r, y - r, 2 * r, 2 * r);
                        }
                    }

                    foreach (var point in _points)
                    {
                        var x = (float)point.X;
                        var y = (float)point.Y;
                        g.FillEllipse(_pointBrush, x - r, y - r, 2 * r, 2 * r);
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

                    if (_oneStepDaft != null)
                    {
                        foreach (var inwards in _oneStepDaft.Inwards)
                        {
                            foreach (var e in inwards.Edges)
                            {
                                var v1 = e.V1;
                                var v2 = e.V2;
                                g.DrawLine(_inwardsPen, (float)v1.X, (float)v1.Y, (float)v2.X, (float)v2.Y);
                            }
                        }

                        foreach (var outwards in _oneStepDaft.Outwards)
                        {
                            foreach (var e in outwards.Edges)
                            {
                                var v1 = e.V1;
                                var v2 = e.V2;
                                g.DrawLine(_outwardsPen, (float)v1.X, (float)v1.Y, (float)v2.X, (float)v2.Y);
                            }
                        }
                    }

                    if (_hull != null)
                    {
                        foreach (var edge in _hull)
                        {
                            var v1 = edge.V1;
                            var v2 = edge.V2;
                            g.DrawLine(_hullPen, (float)v1.X, (float)v1.Y, (float)v2.X, (float)v2.Y);
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
                            g.DrawEllipse(_ccPen, x1, y1, (float)(cr * 2), (float)(cr * 2));
                        }
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

        private void UpdatePolygonStates()
        {
            _polygonStates = new PolygonState[_polygons.Count];
            for (var i = 0; i < _polygons.Count; i++)
            {
                var pi = _polygons[i];
                _polygonStates[i] = PolygonState.Normal;
                for (var j = 0; j < _polygons.Count; j++)
                {
                    if (j == i) continue;
                    var pj = _polygons[j];
                    if (pj.PolygonIsInside(pi))
                    {
                        _polygonStates[i] = PolygonState.Contained;
                        break;
                    }
                }
            }
        }

        private double GetSize(double x, double y)
        {
            var len = GetLength(x, y);
            var size = len*len*Math.Sqrt(3)/4;
            return size;
        }

        private double GetLength(double x, double y)
        {
            var total = 0.0;
            var mtotal = 0.0;
            foreach (var kvp in _fieldPoints)
            {
                var k = kvp.Key;
                var v = kvp.Value;
                var point = _points[k];
                var dx = point.X - x;
                var dy = point.Y - y;
                var sqrdist = dx * dx + dy * dy;
                total += 1 / sqrdist;
                mtotal += v / sqrdist;
            }
            var m = mtotal / total;
            return m;
        }

        private double GetMeanSize()
        {
            var total = _fieldPoints.Sum(kvp => kvp.Value);
            return total/_fieldPoints.Count;
        }

        #endregion
    }
}
