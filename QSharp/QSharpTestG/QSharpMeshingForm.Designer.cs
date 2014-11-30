﻿namespace QSharpTestG
{
    partial class QSharpMeshingForm
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
            this.MainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.definePolygonsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.definePolylinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.definePointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MeshingPictureBox = new System.Windows.Forms.PictureBox();
            this.MainMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MeshingPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // MainMenuStrip
            // 
            this.MainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
            this.MainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MainMenuStrip.Name = "MainMenuStrip";
            this.MainMenuStrip.Size = new System.Drawing.Size(784, 24);
            this.MainMenuStrip.TabIndex = 0;
            this.MainMenuStrip.Text = "MainMenuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.definePolygonsToolStripMenuItem,
            this.definePolylinesToolStripMenuItem,
            this.definePointsToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // definePolygonsToolStripMenuItem
            // 
            this.definePolygonsToolStripMenuItem.Name = "definePolygonsToolStripMenuItem";
            this.definePolygonsToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.definePolygonsToolStripMenuItem.Text = "Define Poly&gons";
            this.definePolygonsToolStripMenuItem.Click += new System.EventHandler(this.definePolygonsToolStripMenuItem_Click);
            // 
            // definePolylinesToolStripMenuItem
            // 
            this.definePolylinesToolStripMenuItem.Name = "definePolylinesToolStripMenuItem";
            this.definePolylinesToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.definePolylinesToolStripMenuItem.Text = "Define Poly&lines";
            this.definePolylinesToolStripMenuItem.Click += new System.EventHandler(this.definePolylinesToolStripMenuItem_Click);
            // 
            // definePointsToolStripMenuItem
            // 
            this.definePointsToolStripMenuItem.Name = "definePointsToolStripMenuItem";
            this.definePointsToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.definePointsToolStripMenuItem.Text = "Define &Points";
            this.definePointsToolStripMenuItem.Click += new System.EventHandler(this.definePointsToolStripMenuItem_Click);
            // 
            // MeshingPictureBox
            // 
            this.MeshingPictureBox.BackColor = System.Drawing.Color.White;
            this.MeshingPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MeshingPictureBox.Location = new System.Drawing.Point(0, 24);
            this.MeshingPictureBox.Name = "MeshingPictureBox";
            this.MeshingPictureBox.Size = new System.Drawing.Size(784, 537);
            this.MeshingPictureBox.TabIndex = 1;
            this.MeshingPictureBox.TabStop = false;
            this.MeshingPictureBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MeshingPictureBox_MouseClick);
            this.MeshingPictureBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MeshingPictureBox_MouseDoubleClick);
            this.MeshingPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MeshingPictureBox_MouseDown);
            this.MeshingPictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MeshingPictureBox_MouseMove);
            this.MeshingPictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MeshingPictureBox_MouseUp);
            // 
            // QSharpMeshingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.MeshingPictureBox);
            this.Controls.Add(this.MainMenuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "QSharpMeshingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QSharpMeshingForm";
            this.Load += new System.EventHandler(this.QSharpMeshingForm_Load);
            this.MainMenuStrip.ResumeLayout(false);
            this.MainMenuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MeshingPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip MainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.PictureBox MeshingPictureBox;
        private System.Windows.Forms.ToolStripMenuItem definePolygonsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem definePolylinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem definePointsToolStripMenuItem;
    }
}