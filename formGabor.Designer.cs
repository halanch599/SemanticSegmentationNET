namespace SemanticSegmentationNET
{
    partial class formGabor
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
            this.lblMin = new System.Windows.Forms.Label();
            this.lblCurrentValue = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.trackBarGamma = new System.Windows.Forms.TrackBar();
            this.trackBarLambda = new System.Windows.Forms.TrackBar();
            this.trackBarPsi = new System.Windows.Forms.TrackBar();
            this.trackBarTheta = new System.Windows.Forms.TrackBar();
            this.trackBarSize = new System.Windows.Forms.TrackBar();
            this.trackBarSigma = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTheta = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.lblLambda = new System.Windows.Forms.Label();
            this.lblPsi = new System.Windows.Forms.Label();
            this.lblSigma = new System.Windows.Forms.Label();
            this.lblGamma = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGamma)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarLambda)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPsi)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTheta)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSigma)).BeginInit();
            this.SuspendLayout();
            // 
            // lblMin
            // 
            this.lblMin.AutoSize = true;
            this.lblMin.Location = new System.Drawing.Point(14, 99);
            this.lblMin.Name = "lblMin";
            this.lblMin.Size = new System.Drawing.Size(47, 17);
            this.lblMin.TabIndex = 8;
            this.lblMin.Text = "Sigma";
            // 
            // lblCurrentValue
            // 
            this.lblCurrentValue.AutoSize = true;
            this.lblCurrentValue.Location = new System.Drawing.Point(14, 13);
            this.lblCurrentValue.Name = "lblCurrentValue";
            this.lblCurrentValue.Size = new System.Drawing.Size(57, 17);
            this.lblCurrentValue.TabIndex = 7;
            this.lblCurrentValue.Text = "Gamma";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(432, 177);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(136, 32);
            this.button1.TabIndex = 6;
            this.button1.Text = "Apply";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 17);
            this.label3.TabIndex = 11;
            this.label3.Text = "Psi";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 45);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 17);
            this.label6.TabIndex = 15;
            this.label6.Text = "Lambda";
            // 
            // trackBarGamma
            // 
            this.trackBarGamma.AutoSize = false;
            this.trackBarGamma.Location = new System.Drawing.Point(95, 13);
            this.trackBarGamma.Name = "trackBarGamma";
            this.trackBarGamma.Size = new System.Drawing.Size(473, 22);
            this.trackBarGamma.TabIndex = 18;
            this.trackBarGamma.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarGamma.Scroll += new System.EventHandler(this.trackBarGamma_Scroll);
            // 
            // trackBarLambda
            // 
            this.trackBarLambda.AutoSize = false;
            this.trackBarLambda.Location = new System.Drawing.Point(95, 40);
            this.trackBarLambda.Name = "trackBarLambda";
            this.trackBarLambda.Size = new System.Drawing.Size(473, 22);
            this.trackBarLambda.TabIndex = 20;
            this.trackBarLambda.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarLambda.Scroll += new System.EventHandler(this.trackBarLambda_Scroll);
            // 
            // trackBarPsi
            // 
            this.trackBarPsi.AutoSize = false;
            this.trackBarPsi.Location = new System.Drawing.Point(95, 67);
            this.trackBarPsi.Name = "trackBarPsi";
            this.trackBarPsi.Size = new System.Drawing.Size(473, 22);
            this.trackBarPsi.TabIndex = 21;
            this.trackBarPsi.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarPsi.Scroll += new System.EventHandler(this.trackBarPsi_Scroll);
            // 
            // trackBarTheta
            // 
            this.trackBarTheta.AutoSize = false;
            this.trackBarTheta.Location = new System.Drawing.Point(95, 148);
            this.trackBarTheta.Name = "trackBarTheta";
            this.trackBarTheta.Size = new System.Drawing.Size(473, 22);
            this.trackBarTheta.TabIndex = 24;
            this.trackBarTheta.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarTheta.Scroll += new System.EventHandler(this.trackBarTheta_Scroll);
            // 
            // trackBarSize
            // 
            this.trackBarSize.AutoSize = false;
            this.trackBarSize.Location = new System.Drawing.Point(95, 121);
            this.trackBarSize.Name = "trackBarSize";
            this.trackBarSize.Size = new System.Drawing.Size(473, 22);
            this.trackBarSize.TabIndex = 23;
            this.trackBarSize.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarSize.Scroll += new System.EventHandler(this.trackBarSize_Scroll);
            // 
            // trackBarSigma
            // 
            this.trackBarSigma.AutoSize = false;
            this.trackBarSigma.Location = new System.Drawing.Point(95, 94);
            this.trackBarSigma.Name = "trackBarSigma";
            this.trackBarSigma.Size = new System.Drawing.Size(473, 22);
            this.trackBarSigma.TabIndex = 22;
            this.trackBarSigma.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarSigma.Scroll += new System.EventHandler(this.trackBarSigma_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 126);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 17);
            this.label1.TabIndex = 25;
            this.label1.Text = "Kernel Size";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 153);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 17);
            this.label2.TabIndex = 26;
            this.label2.Text = "Theta";
            // 
            // lblTheta
            // 
            this.lblTheta.AutoSize = true;
            this.lblTheta.Location = new System.Drawing.Point(579, 148);
            this.lblTheta.Name = "lblTheta";
            this.lblTheta.Size = new System.Drawing.Size(46, 17);
            this.lblTheta.TabIndex = 32;
            this.lblTheta.Text = "label4";
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(579, 121);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(46, 17);
            this.lblSize.TabIndex = 31;
            this.lblSize.Text = "label2";
            // 
            // lblLambda
            // 
            this.lblLambda.AutoSize = true;
            this.lblLambda.Location = new System.Drawing.Point(579, 40);
            this.lblLambda.Name = "lblLambda";
            this.lblLambda.Size = new System.Drawing.Size(46, 17);
            this.lblLambda.TabIndex = 30;
            this.lblLambda.Text = "label1";
            // 
            // lblPsi
            // 
            this.lblPsi.AutoSize = true;
            this.lblPsi.Location = new System.Drawing.Point(579, 67);
            this.lblPsi.Name = "lblPsi";
            this.lblPsi.Size = new System.Drawing.Size(46, 17);
            this.lblPsi.TabIndex = 29;
            this.lblPsi.Text = "label1";
            // 
            // lblSigma
            // 
            this.lblSigma.AutoSize = true;
            this.lblSigma.Location = new System.Drawing.Point(579, 94);
            this.lblSigma.Name = "lblSigma";
            this.lblSigma.Size = new System.Drawing.Size(46, 17);
            this.lblSigma.TabIndex = 28;
            this.lblSigma.Text = "label2";
            // 
            // lblGamma
            // 
            this.lblGamma.AutoSize = true;
            this.lblGamma.Location = new System.Drawing.Point(579, 13);
            this.lblGamma.Name = "lblGamma";
            this.lblGamma.Size = new System.Drawing.Size(46, 17);
            this.lblGamma.TabIndex = 27;
            this.lblGamma.Text = "label1";
            // 
            // formGabor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(652, 221);
            this.Controls.Add(this.lblTheta);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.lblLambda);
            this.Controls.Add(this.lblPsi);
            this.Controls.Add(this.lblSigma);
            this.Controls.Add(this.lblGamma);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackBarTheta);
            this.Controls.Add(this.trackBarSize);
            this.Controls.Add(this.trackBarSigma);
            this.Controls.Add(this.trackBarPsi);
            this.Controls.Add(this.trackBarLambda);
            this.Controls.Add(this.trackBarGamma);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblMin);
            this.Controls.Add(this.lblCurrentValue);
            this.Controls.Add(this.button1);
            this.MaximumSize = new System.Drawing.Size(670, 268);
            this.MinimumSize = new System.Drawing.Size(670, 268);
            this.Name = "formGabor";
            this.Text = "Gabor Filter Setting";
            this.Load += new System.EventHandler(this.formGabor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGamma)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarLambda)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPsi)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTheta)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSigma)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblMin;
        private System.Windows.Forms.Label lblCurrentValue;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TrackBar trackBarGamma;
        private System.Windows.Forms.TrackBar trackBarLambda;
        private System.Windows.Forms.TrackBar trackBarPsi;
        private System.Windows.Forms.TrackBar trackBarTheta;
        private System.Windows.Forms.TrackBar trackBarSize;
        private System.Windows.Forms.TrackBar trackBarSigma;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblTheta;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.Label lblLambda;
        private System.Windows.Forms.Label lblPsi;
        private System.Windows.Forms.Label lblSigma;
        private System.Windows.Forms.Label lblGamma;
    }
}