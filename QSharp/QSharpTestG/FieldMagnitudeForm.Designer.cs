namespace QSharpTestG
{
    partial class FieldMagnitudeForm
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
            this.TxtMagnitude = new System.Windows.Forms.TextBox();
            this.BtnOk = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TxtMagnitude
            // 
            this.TxtMagnitude.Location = new System.Drawing.Point(50, 12);
            this.TxtMagnitude.Name = "TxtMagnitude";
            this.TxtMagnitude.Size = new System.Drawing.Size(100, 20);
            this.TxtMagnitude.TabIndex = 0;
            // 
            // BtnOk
            // 
            this.BtnOk.Location = new System.Drawing.Point(61, 38);
            this.BtnOk.Name = "BtnOk";
            this.BtnOk.Size = new System.Drawing.Size(75, 23);
            this.BtnOk.TabIndex = 1;
            this.BtnOk.Text = "OK";
            this.BtnOk.UseVisualStyleBackColor = true;
            this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // FieldMagnitudeForm
            // 
            this.AcceptButton = this.BtnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(198, 70);
            this.Controls.Add(this.BtnOk);
            this.Controls.Add(this.TxtMagnitude);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FieldMagnitudeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Field Magnitude";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TxtMagnitude;
        private System.Windows.Forms.Button BtnOk;
    }
}