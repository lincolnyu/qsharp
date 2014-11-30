namespace QSharpTestG
{
    partial class MainForm
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
            this.BtnQSharp2DGraphics = new System.Windows.Forms.Button();
            this.BtnQSharp3DGraphics = new System.Windows.Forms.Button();
            this.BtnQSharpMeshing = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnQSharp2DGraphics
            // 
            this.BtnQSharp2DGraphics.Location = new System.Drawing.Point(12, 12);
            this.BtnQSharp2DGraphics.Name = "BtnQSharp2DGraphics";
            this.BtnQSharp2DGraphics.Size = new System.Drawing.Size(159, 32);
            this.BtnQSharp2DGraphics.TabIndex = 0;
            this.BtnQSharp2DGraphics.Text = "QSharp 2D Graphics";
            this.BtnQSharp2DGraphics.UseVisualStyleBackColor = true;
            this.BtnQSharp2DGraphics.Click += new System.EventHandler(this.BtnQSharp2DGraphics_Click);
            // 
            // BtnQSharp3DGraphics
            // 
            this.BtnQSharp3DGraphics.Location = new System.Drawing.Point(12, 50);
            this.BtnQSharp3DGraphics.Name = "BtnQSharp3DGraphics";
            this.BtnQSharp3DGraphics.Size = new System.Drawing.Size(159, 32);
            this.BtnQSharp3DGraphics.TabIndex = 1;
            this.BtnQSharp3DGraphics.Text = "QSharp 3D Graphics";
            this.BtnQSharp3DGraphics.UseVisualStyleBackColor = true;
            this.BtnQSharp3DGraphics.Click += new System.EventHandler(this.BtnQSharp3DGraphics_Click);
            // 
            // BtnQSharpMeshing
            // 
            this.BtnQSharpMeshing.Location = new System.Drawing.Point(12, 88);
            this.BtnQSharpMeshing.Name = "BtnQSharpMeshing";
            this.BtnQSharpMeshing.Size = new System.Drawing.Size(159, 32);
            this.BtnQSharpMeshing.TabIndex = 2;
            this.BtnQSharpMeshing.Text = "QSharp Meshing";
            this.BtnQSharpMeshing.UseVisualStyleBackColor = true;
            this.BtnQSharpMeshing.Click += new System.EventHandler(this.BtnQSharpMeshing_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(183, 130);
            this.Controls.Add(this.BtnQSharpMeshing);
            this.Controls.Add(this.BtnQSharp3DGraphics);
            this.Controls.Add(this.BtnQSharp2DGraphics);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QSharp GUI Test";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnQSharp2DGraphics;
        private System.Windows.Forms.Button BtnQSharp3DGraphics;
        private System.Windows.Forms.Button BtnQSharpMeshing;
    }
}

