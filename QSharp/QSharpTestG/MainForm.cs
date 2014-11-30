using System;
using System.Windows.Forms;

namespace QSharpTestG
{
    public partial class MainForm : Form
    {
        #region Contructors

        public MainForm()
        {
            InitializeComponent();
        }

        #endregion
        
        #region Methods

        private void BtnQSharp2DGraphics_Click(object sender, EventArgs e)
        {
            var form = new QSharp2DGraphicsForm();
            form.Show();
        }

        private void BtnQSharp3DGraphics_Click(object sender, EventArgs e)
        {
            var form = new QSharp3DGraphicsForm();
            form.Show();
        }

        private void BtnQSharpMeshing_Click(object sender, EventArgs e)
        {
            var form = new QSharpMeshingForm();
            form.Show();
        }

        #endregion
    }
}
