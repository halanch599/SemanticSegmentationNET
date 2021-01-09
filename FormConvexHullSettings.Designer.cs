namespace SemanticSegmentationNET
{
    partial class FormConvexHullSettings
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
            this.lblError = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.tbCFThreshold = new System.Windows.Forms.TextBox();
            this.tbCDDFThreshold = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Location = new System.Drawing.Point(311, 49);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(0, 17);
            this.lblError.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(314, 135);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(104, 32);
            this.button1.TabIndex = 1;
            this.button1.Text = "Apply";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tbCFThreshold
            // 
            this.tbCFThreshold.Location = new System.Drawing.Point(314, 79);
            this.tbCFThreshold.Name = "tbCFThreshold";
            this.tbCFThreshold.Size = new System.Drawing.Size(104, 22);
            this.tbCFThreshold.TabIndex = 2;
            // 
            // tbCDDFThreshold
            // 
            this.tbCDDFThreshold.Location = new System.Drawing.Point(314, 107);
            this.tbCDDFThreshold.Name = "tbCDDFThreshold";
            this.tbCDDFThreshold.Size = new System.Drawing.Size(104, 22);
            this.tbCDDFThreshold.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(248, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Convexity Defect Distance Threshold :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(95, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(187, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Convexhull Filter Threshold :";
            // 
            // FormConvexHullSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(665, 216);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbCDDFThreshold);
            this.Controls.Add(this.tbCFThreshold);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblError);
            this.MaximumSize = new System.Drawing.Size(683, 263);
            this.Name = "FormConvexHullSettings";
            this.Text = "Form Convexhull Settings";
            this.Load += new System.EventHandler(this.FormConvexHullSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox tbCFThreshold;
        private System.Windows.Forms.TextBox tbCDDFThreshold;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}