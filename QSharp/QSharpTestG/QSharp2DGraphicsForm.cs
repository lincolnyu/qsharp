using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QSharp.Shader.Graphics.Raster.Plot;

namespace QSharpTestG
{
    public partial class QSharp2DGraphicsForm : Form
    {
        private Bitmap mBitmap;

        public QSharp2DGraphicsForm()
        {
            InitializeComponent();
        }

        private void QSharp2DGraphicsForm_Load(object sender, EventArgs e)
        {
            /*
             * create an associated image object for the paint box
             * when the form is loaded
             */
            mBitmap = new Bitmap(PbxMain.Width, PbxMain.Height);
            PbxMain.Image = mBitmap;
            
            using (Graphics g = Graphics.FromImage(PbxMain.Image))
            {
                g.FillRectangle(new SolidBrush(Color.Black),
                    0, 0, PbxMain.Image.Width, PbxMain.Image.Height);

            }
        }

        private class DotMaker
        {
            private Bitmap mBitmap;
            private Color mColor;

            public DotMaker(Bitmap bmp, Color color)
            {
                this.mBitmap = bmp;
                this.mColor = color;
            }

            public void Visit(int x, int y)
            {
                if (x >= mBitmap.Width || y >= mBitmap.Height)
                    return;
                mBitmap.SetPixel(x, y, mColor);
            }
        }

        private void PbxMain_Click(object sender, EventArgs e)
        {
            using (Pen pen = new Pen(new SolidBrush(Color.White)))
            {
                //g.DrawLine(pen, new Point(100, 100), new Point(100, 100));
                DotMaker dm1 = new DotMaker(mBitmap, Color.White);
                DotMaker dm2 = new DotMaker(mBitmap, Color.Red);
                BresenhamLine.Draw(200, 200, 10, 10, dm1.Visit);
                //BresenhamLine.Draw(100.9f, 200.2f, 10.9f, 20.9f, 10f, dm1.Visit);
            }

            PbxMain.Invalidate();
        }
    }
}
