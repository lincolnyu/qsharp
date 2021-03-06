﻿using System;
using System.Drawing;
using System.Windows.Forms;
using QSharp.Shader.Graphics.Base.Geometry;

namespace QSharpTestG
{
    public partial class QSharp3DGraphicsForm : Form
    {
        #region Fields

        /// <summary>
        ///  backing field for bitmap to draw on and display
        /// </summary>
        Bitmap _bitmap;

        /// <summary>
        ///  3d manager that simplifies 3d rendering setting up
        /// </summary>
        Q3DManager _q3D;

        private bool _doWireframe;
        private bool _doRayTracing;

        #endregion

        #region Properties

        private Vector4f _initX, _initY, _initZ;

        Q3DManager Q3D
        {
            get
            {
                if (_q3D == null)
                {
                    _q3D = new Q3DManager();
                    SetupCamera();
                }
                return _q3D;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public QSharp3DGraphicsForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        #region Operational

        void SetCameraUpright()
        {
            if (_initX != null)
            {
                Q3D.CameraAxisX = _initX;
                Q3D.CameraAxisY = _initY;
                Q3D.CameraAxisZ = _initZ;
            }
        }

        void SetupCamera()
        {
            pbxMain.Image = _bitmap = new Bitmap(pbxMain.Width, pbxMain.Height);

            float imageWidth = pbxMain.Image.Width;
            float imageHeight = pbxMain.Image.Height;
            const float width = 12;
            float height = width * imageHeight / imageWidth;

            Q3D.SetImage(new BitmapImage(_bitmap), width, height, 5, 20, 5);
            Q3D.SetCameraAttitude(2, -10, 0, 0, 1, 0, 0);
            _initX = new Vector4f(Q3D.CameraAxisX);
            _initY = new Vector4f(Q3D.CameraAxisY);
            _initZ = new Vector4f(Q3D.CameraAxisZ);

            Q3D.SetProjectionMode(Q3DManager.ProjectionMode.Perspective);
        }

        private void ClearScreen()
        {
            ClearScreen(Color.Black);
        }

        private void ClearScreen(Color color)
        {
            using (var g = Graphics.FromImage(_bitmap))
            {
                g.Clear(color);
            }
        }

        private void UpdateView()
        {
            ClearScreen();
            if (_doRayTracing)
            {
                Q3D.Render();
            }

            if (_doWireframe)
            {
                Q3D.DrawWireframe(Color.FromArgb(0xff,0x20,0x20,0x20));
            }
           
            pbxMain.Invalidate();
        }

        #endregion

        #region Event handlers

        /// <summary>
        ///  responds to the event on form load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QSharp3DGraphicsForm_Load(object sender, EventArgs e)
        {
            /*
             * create an associated image object for the paint box
             * when the form is loaded
             */
            _bitmap = new Bitmap(pbxMain.Width, pbxMain.Height);
            pbxMain.Image = _bitmap;

            using (var g = Graphics.FromImage(pbxMain.Image))
            {
                g.FillRectangle(new SolidBrush(Color.Black),
                    0, 0, pbxMain.Image.Width, pbxMain.Image.Height);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        ///  handler that responds to event of single click on the picture box by drawing the wireframe
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event details</param>
        private void pbxMain_Click(object sender, EventArgs e)
        {
        }

        /// <summary>
        ///  handler that responds to event of double click on the picture box by rendering the scene
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event details</param>
        private void pbxMain_DoubleClick(object sender, EventArgs e)
        {
            UpdateView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Q3D files (*.q3d)|*.q3d";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadDemoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Q3D.LoadDemo();
        }

        private void pbxMain_Resize(object sender, EventArgs e)
        {
            SetupCamera();
        }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _doWireframe = wireframeToolStripMenuItem.Checked;
            UpdateView();
        }

        private void raytracingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _doRayTracing = raytracingToolStripMenuItem.Checked;
            UpdateView();
        }

        private void QSharp3DGraphicsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {   // to rotate
                var vx = Q3D.CameraAxisX;
                var vy = Q3D.CameraAxisY;
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        Q3D.RotateCamera(vx.X, vx.Y, vx.Z, 0.05f);
                        break;
                    case Keys.Down:
                        Q3D.RotateCamera(vx.X, vx.Y, vx.Z, -0.05f);
                        break;
                    case Keys.Left:
                        Q3D.RotateCamera(vy.X, vy.Y, vy.Z, -0.05f);
                        break;
                    case Keys.Right:
                        Q3D.RotateCamera(vy.X, vy.Y, vy.Z, 0.05f);
                        break;
                }
            }
            else
            {   // to slide
                const float amountOfSlide = 0.3f;
                var vz = Q3D.CameraAxisZ;

                switch (e.KeyCode)
                {
                    case Keys.Left:
                        if (e.Shift)
                        {
                            Q3D.RotateCamera(vz.X, vz.Y, vz.Z, -0.05f);
                        }
                        else
                        {
                            Q3D.MoveCamera(Q3DManager.MoveDiriction.Leftward, amountOfSlide);
                        }
                        break;
                    case Keys.Right:
                        if (e.Shift)
                        {
                            Q3D.RotateCamera(vz.X, vz.Y, vz.Z, 0.05f);
                        }
                        else
                        {
                            Q3D.MoveCamera(Q3DManager.MoveDiriction.Rightward, amountOfSlide);
                        }
                        break;
                    case Keys.Up:
                        Q3D.MoveCamera(e.Shift ? Q3DManager.MoveDiriction.Upward : Q3DManager.MoveDiriction.Forward,
                                        amountOfSlide);
                        break;
                    case Keys.Down:
                        Q3D.MoveCamera(
                            e.Shift ? Q3DManager.MoveDiriction.Downward : Q3DManager.MoveDiriction.Backward,
                            amountOfSlide);
                        break;
                    case Keys.Escape:
                        SetCameraUpright();
                        break;
                }
            }
            
            UpdateView();
        }

        private void orthographicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Q3D.SetProjectionMode(orthographicToolStripMenuItem.Checked
                                       ? Q3DManager.ProjectionMode.Orthographic
                                       : Q3DManager.ProjectionMode.Perspective);

            UpdateView();
        }

        #endregion

        #endregion      
    }
}
