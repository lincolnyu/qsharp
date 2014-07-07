using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QSharpTestG
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void BtnQSharp2DGraphics_Click(object sender, EventArgs e)
        {
            QSharp2DGraphicsForm form = new QSharp2DGraphicsForm();
            form.Show();
        }

        private void BtnQSharp3DGraphics_Click(object sender, EventArgs e)
        {
            QSharp3DGraphicsForm form = new QSharp3DGraphicsForm();
            form.Show();
        }
    }
}
