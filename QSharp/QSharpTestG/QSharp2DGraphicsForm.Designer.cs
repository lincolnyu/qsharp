namespace QSharpTestG
{
    partial class QSharp2DGraphicsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PbxMain = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PbxMain)).BeginInit();
            this.SuspendLayout();
            // 
            // PbxMain
            // 
            this.PbxMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PbxMain.Location = new System.Drawing.Point(0, 0);
            this.PbxMain.Name = "PbxMain";
            this.PbxMain.Size = new System.Drawing.Size(292, 273);
            this.PbxMain.TabIndex = 0;
            this.PbxMain.TabStop = false;
            this.PbxMain.Click += new System.EventHandler(this.PbxMain_Click);
            // 
            // QSharp2DGraphicsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.PbxMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "QSharp2DGraphicsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QSharp2DGraphicsForm";
            this.Load += new System.EventHandler(this.QSharp2DGraphicsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PbxMain)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox PbxMain;
    }
}