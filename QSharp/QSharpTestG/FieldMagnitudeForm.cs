using System;
using System.Windows.Forms;

namespace QSharpTestG
{
    public partial class FieldMagnitudeForm : Form
    {
        public FieldMagnitudeForm()
        {
            InitializeComponent();
        }

        public double Magnitude { get; private set; }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Magnitude = double.Parse(TxtMagnitude.Text);
            Close();
        }
    }
}
