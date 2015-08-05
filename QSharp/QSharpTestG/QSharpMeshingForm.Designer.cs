namespace QSharpTestG
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
            this.MyMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.definePolygonsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.definePolylinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.definePointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DefineSizeFieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.demoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.randomVerticesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.squareToMeshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.solveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.triangulateVerticesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.triangulateAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.triangulateOneStepToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.circumcirclesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SegmentStraightLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MeshingPictureBox = new System.Windows.Forms.PictureBox();
            this.squareAndFieldToMeshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MyMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MeshingPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // MyMenuStrip
            // 
            this.MyMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MyMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.demoToolStripMenuItem,
            this.solveToolStripMenuItem,
            this.testToolStripMenuItem});
            this.MyMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MyMenuStrip.Name = "MyMenuStrip";
            this.MyMenuStrip.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.MyMenuStrip.Size = new System.Drawing.Size(1045, 28);
            this.MyMenuStrip.TabIndex = 0;
            this.MyMenuStrip.Text = "Main Menu Strip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(108, 26);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.definePolygonsToolStripMenuItem,
            this.definePolylinesToolStripMenuItem,
            this.definePointsToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.DefineSizeFieldToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(47, 24);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // definePolygonsToolStripMenuItem
            // 
            this.definePolygonsToolStripMenuItem.Name = "definePolygonsToolStripMenuItem";
            this.definePolygonsToolStripMenuItem.Size = new System.Drawing.Size(195, 26);
            this.definePolygonsToolStripMenuItem.Text = "Define Poly&gons";
            this.definePolygonsToolStripMenuItem.Click += new System.EventHandler(this.definePolygonsToolStripMenuItem_Click);
            // 
            // definePolylinesToolStripMenuItem
            // 
            this.definePolylinesToolStripMenuItem.Name = "definePolylinesToolStripMenuItem";
            this.definePolylinesToolStripMenuItem.Size = new System.Drawing.Size(195, 26);
            this.definePolylinesToolStripMenuItem.Text = "Define Poly&lines";
            this.definePolylinesToolStripMenuItem.Click += new System.EventHandler(this.definePolylinesToolStripMenuItem_Click);
            // 
            // definePointsToolStripMenuItem
            // 
            this.definePointsToolStripMenuItem.Name = "definePointsToolStripMenuItem";
            this.definePointsToolStripMenuItem.Size = new System.Drawing.Size(195, 26);
            this.definePointsToolStripMenuItem.Text = "Define &Points";
            this.definePointsToolStripMenuItem.Click += new System.EventHandler(this.definePointsToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(195, 26);
            this.deleteToolStripMenuItem.Text = "Dele&te";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // DefineSizeFieldToolStripMenuItem
            // 
            this.DefineSizeFieldToolStripMenuItem.Name = "DefineSizeFieldToolStripMenuItem";
            this.DefineSizeFieldToolStripMenuItem.Size = new System.Drawing.Size(195, 26);
            this.DefineSizeFieldToolStripMenuItem.Text = "Define Size Field";
            this.DefineSizeFieldToolStripMenuItem.Click += new System.EventHandler(this.DefineSizeFieldToolStripMenuItemOnClick);
            // 
            // demoToolStripMenuItem
            // 
            this.demoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.randomVerticesToolStripMenuItem,
            this.squareToMeshToolStripMenuItem,
            this.squareAndFieldToMeshToolStripMenuItem});
            this.demoToolStripMenuItem.Name = "demoToolStripMenuItem";
            this.demoToolStripMenuItem.Size = new System.Drawing.Size(62, 24);
            this.demoToolStripMenuItem.Text = "&Demo";
            // 
            // randomVerticesToolStripMenuItem
            // 
            this.randomVerticesToolStripMenuItem.Name = "randomVerticesToolStripMenuItem";
            this.randomVerticesToolStripMenuItem.Size = new System.Drawing.Size(252, 26);
            this.randomVerticesToolStripMenuItem.Text = "Random Vertices";
            this.randomVerticesToolStripMenuItem.Click += new System.EventHandler(this.randomVerticesToolStripMenuItem_Click);
            // 
            // squareToMeshToolStripMenuItem
            // 
            this.squareToMeshToolStripMenuItem.Name = "squareToMeshToolStripMenuItem";
            this.squareToMeshToolStripMenuItem.Size = new System.Drawing.Size(252, 26);
            this.squareToMeshToolStripMenuItem.Text = "Square to Mesh";
            this.squareToMeshToolStripMenuItem.Click += new System.EventHandler(this.squareToMeshToolStripMenuItem_Click);
            // 
            // solveToolStripMenuItem
            // 
            this.solveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.triangulateVerticesToolStripMenuItem,
            this.triangulateAllToolStripMenuItem,
            this.triangulateOneStepToolStripMenuItem});
            this.solveToolStripMenuItem.Name = "solveToolStripMenuItem";
            this.solveToolStripMenuItem.Size = new System.Drawing.Size(57, 24);
            this.solveToolStripMenuItem.Text = "&Solve";
            // 
            // triangulateVerticesToolStripMenuItem
            // 
            this.triangulateVerticesToolStripMenuItem.Name = "triangulateVerticesToolStripMenuItem";
            this.triangulateVerticesToolStripMenuItem.Size = new System.Drawing.Size(248, 26);
            this.triangulateVerticesToolStripMenuItem.Text = "Triangulate Vertices";
            this.triangulateVerticesToolStripMenuItem.Click += new System.EventHandler(this.triangulateVerticesToolStripMenuItem_Click);
            // 
            // triangulateAllToolStripMenuItem
            // 
            this.triangulateAllToolStripMenuItem.Name = "triangulateAllToolStripMenuItem";
            this.triangulateAllToolStripMenuItem.Size = new System.Drawing.Size(248, 26);
            this.triangulateAllToolStripMenuItem.Text = "Triangulate All";
            this.triangulateAllToolStripMenuItem.Click += new System.EventHandler(this.triangulateAllToolStripMenuItem_Click);
            // 
            // triangulateOneStepToolStripMenuItem
            // 
            this.triangulateOneStepToolStripMenuItem.Name = "triangulateOneStepToolStripMenuItem";
            this.triangulateOneStepToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.triangulateOneStepToolStripMenuItem.Size = new System.Drawing.Size(248, 26);
            this.triangulateOneStepToolStripMenuItem.Text = "Triangulate One Step";
            this.triangulateOneStepToolStripMenuItem.Click += new System.EventHandler(this.triangulateOneStepToolStripMenuItem_Click);
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.shineToolStripMenuItem,
            this.circumcirclesToolStripMenuItem,
            this.SegmentStraightLinesToolStripMenuItem});
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(48, 24);
            this.testToolStripMenuItem.Text = "&Test";
            // 
            // shineToolStripMenuItem
            // 
            this.shineToolStripMenuItem.Name = "shineToolStripMenuItem";
            this.shineToolStripMenuItem.Size = new System.Drawing.Size(236, 26);
            this.shineToolStripMenuItem.Text = "Shine";
            this.shineToolStripMenuItem.Click += new System.EventHandler(this.shineToolStripMenuItem_Click);
            // 
            // circumcirclesToolStripMenuItem
            // 
            this.circumcirclesToolStripMenuItem.Name = "circumcirclesToolStripMenuItem";
            this.circumcirclesToolStripMenuItem.Size = new System.Drawing.Size(236, 26);
            this.circumcirclesToolStripMenuItem.Text = "Circumcircles";
            this.circumcirclesToolStripMenuItem.Click += new System.EventHandler(this.circumcirclesToolStripMenuItem_Click);
            // 
            // SegmentStraightLinesToolStripMenuItem
            // 
            this.SegmentStraightLinesToolStripMenuItem.Name = "SegmentStraightLinesToolStripMenuItem";
            this.SegmentStraightLinesToolStripMenuItem.Size = new System.Drawing.Size(236, 26);
            this.SegmentStraightLinesToolStripMenuItem.Text = "Segment Straight Lines";
            this.SegmentStraightLinesToolStripMenuItem.Click += new System.EventHandler(this.SegmentStraightLinesToolStripMenuItemOnClick);
            // 
            // MeshingPictureBox
            // 
            this.MeshingPictureBox.BackColor = System.Drawing.Color.White;
            this.MeshingPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MeshingPictureBox.Location = new System.Drawing.Point(0, 28);
            this.MeshingPictureBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MeshingPictureBox.Name = "MeshingPictureBox";
            this.MeshingPictureBox.Size = new System.Drawing.Size(1045, 713);
            this.MeshingPictureBox.TabIndex = 1;
            this.MeshingPictureBox.TabStop = false;
            this.MeshingPictureBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MeshingPictureBox_MouseClick);
            this.MeshingPictureBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MeshingPictureBox_MouseDoubleClick);
            this.MeshingPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MeshingPictureBox_MouseDown);
            this.MeshingPictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MeshingPictureBox_MouseMove);
            this.MeshingPictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MeshingPictureBox_MouseUp);
            // 
            // squareAndFieldToMeshToolStripMenuItem
            // 
            this.squareAndFieldToMeshToolStripMenuItem.Name = "squareAndFieldToMeshToolStripMenuItem";
            this.squareAndFieldToMeshToolStripMenuItem.Size = new System.Drawing.Size(252, 26);
            this.squareAndFieldToMeshToolStripMenuItem.Text = "Square and Field to Mesh";
            this.squareAndFieldToMeshToolStripMenuItem.Click += new System.EventHandler(this.squareAndFieldToMeshToolStripMenuItem_Click);
            // 
            // QSharpMeshingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1045, 741);
            this.Controls.Add(this.MeshingPictureBox);
            this.Controls.Add(this.MyMenuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "QSharpMeshingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QSharpMeshingForm";
            this.Load += new System.EventHandler(this.QSharpMeshingForm_Load);
            this.MyMenuStrip.ResumeLayout(false);
            this.MyMenuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MeshingPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip MyMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.PictureBox MeshingPictureBox;
        private System.Windows.Forms.ToolStripMenuItem definePolygonsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem definePolylinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem definePointsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem demoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem randomVerticesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem solveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem triangulateVerticesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem triangulateAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem circumcirclesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SegmentStraightLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DefineSizeFieldToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem triangulateOneStepToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem squareToMeshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem squareAndFieldToMeshToolStripMenuItem;
    }
}