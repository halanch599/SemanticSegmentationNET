
namespace SemanticSegmentationNET
{
    partial class formBrightnessANDContrast
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblMinBrightness = new System.Windows.Forms.Label();
            this.lblMaxBrightness = new System.Windows.Forms.Label();
            this.lblMinContrast = new System.Windows.Forms.Label();
            this.lblMaxContrast = new System.Windows.Forms.Label();
            this.lblCurrentBrightness = new System.Windows.Forms.Label();
            this.lblCurrentContrast = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.lblCurrentBrightness);
            this.panel1.Controls.Add(this.lblMaxBrightness);
            this.panel1.Controls.Add(this.lblMinBrightness);
            this.panel1.Controls.Add(this.trackBar1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 521);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1007, 68);
            this.panel1.TabIndex = 0;
            // 
            // trackBar1
            // 
            this.trackBar1.AutoSize = false;
            this.trackBar1.Dock = System.Windows.Forms.DockStyle.Top;
            this.trackBar1.Location = new System.Drawing.Point(0, 0);
            this.trackBar1.Maximum = 500;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(1007, 44);
            this.trackBar1.TabIndex = 0;
            this.trackBar1.Value = 100;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            this.trackBar1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackBar1_MouseUp);
            // 
            // trackBar2
            // 
            this.trackBar2.AutoSize = false;
            this.trackBar2.Dock = System.Windows.Forms.DockStyle.Left;
            this.trackBar2.Location = new System.Drawing.Point(0, 0);
            this.trackBar2.Maximum = 100;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar2.Size = new System.Drawing.Size(44, 521);
            this.trackBar2.TabIndex = 1;
            this.trackBar2.Scroll += new System.EventHandler(this.trackBar2_Scroll);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lblCurrentContrast);
            this.panel2.Controls.Add(this.lblMaxContrast);
            this.panel2.Controls.Add(this.lblMinContrast);
            this.panel2.Controls.Add(this.trackBar2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(906, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(101, 521);
            this.panel2.TabIndex = 1;
            // 
            // lblMinBrightness
            // 
            this.lblMinBrightness.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMinBrightness.AutoSize = true;
            this.lblMinBrightness.Location = new System.Drawing.Point(13, 39);
            this.lblMinBrightness.Name = "lblMinBrightness";
            this.lblMinBrightness.Size = new System.Drawing.Size(46, 17);
            this.lblMinBrightness.TabIndex = 1;
            this.lblMinBrightness.Text = "label1";
            // 
            // lblMaxBrightness
            // 
            this.lblMaxBrightness.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMaxBrightness.AutoSize = true;
            this.lblMaxBrightness.Location = new System.Drawing.Point(949, 39);
            this.lblMaxBrightness.Name = "lblMaxBrightness";
            this.lblMaxBrightness.Size = new System.Drawing.Size(46, 17);
            this.lblMaxBrightness.TabIndex = 2;
            this.lblMaxBrightness.Text = "label2";
            // 
            // lblMinContrast
            // 
            this.lblMinContrast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMinContrast.AutoSize = true;
            this.lblMinContrast.Location = new System.Drawing.Point(47, 498);
            this.lblMinContrast.Name = "lblMinContrast";
            this.lblMinContrast.Size = new System.Drawing.Size(46, 17);
            this.lblMinContrast.TabIndex = 2;
            this.lblMinContrast.Text = "label3";
            // 
            // lblMaxContrast
            // 
            this.lblMaxContrast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMaxContrast.AutoSize = true;
            this.lblMaxContrast.Location = new System.Drawing.Point(42, 9);
            this.lblMaxContrast.Name = "lblMaxContrast";
            this.lblMaxContrast.Size = new System.Drawing.Size(46, 17);
            this.lblMaxContrast.TabIndex = 3;
            this.lblMaxContrast.Text = "label4";
            // 
            // lblCurrentBrightness
            // 
            this.lblCurrentBrightness.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblCurrentBrightness.AutoSize = true;
            this.lblCurrentBrightness.Location = new System.Drawing.Point(502, 39);
            this.lblCurrentBrightness.Name = "lblCurrentBrightness";
            this.lblCurrentBrightness.Size = new System.Drawing.Size(46, 17);
            this.lblCurrentBrightness.TabIndex = 3;
            this.lblCurrentBrightness.Text = "label5";
            // 
            // lblCurrentContrast
            // 
            this.lblCurrentContrast.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCurrentContrast.AutoSize = true;
            this.lblCurrentContrast.Location = new System.Drawing.Point(47, 215);
            this.lblCurrentContrast.Name = "lblCurrentContrast";
            this.lblCurrentContrast.Size = new System.Drawing.Size(46, 17);
            this.lblCurrentContrast.TabIndex = 4;
            this.lblCurrentContrast.Text = "label6";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.pictureBox1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(906, 521);
            this.panel3.TabIndex = 2;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(906, 521);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(815, 39);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(90, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // formBrightnessANDContrast
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1007, 589);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "formBrightnessANDContrast";
            this.Text = "formBrightnessANDContrast";
            this.Load += new System.EventHandler(this.formBrightnessANDContrast_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblMaxBrightness;
        private System.Windows.Forms.Label lblMinBrightness;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblMaxContrast;
        private System.Windows.Forms.Label lblMinContrast;
        private System.Windows.Forms.Label lblCurrentBrightness;
        private System.Windows.Forms.Label lblCurrentContrast;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button1;
    }
}