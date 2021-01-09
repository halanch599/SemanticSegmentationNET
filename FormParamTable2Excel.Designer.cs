namespace SemanticSegmentationNET
{
    partial class FormParamTable2Excel
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbNoCols = new System.Windows.Forms.TextBox();
            this.tbMorphThreshold = new System.Windows.Forms.TextBox();
            this.tboffset = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.tbBinaryThreshold = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(93, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "NoCols";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "MorphThreshold";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(103, 102);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "offset";
            // 
            // tbNoCols
            // 
            this.tbNoCols.Location = new System.Drawing.Point(165, 43);
            this.tbNoCols.Name = "tbNoCols";
            this.tbNoCols.Size = new System.Drawing.Size(131, 22);
            this.tbNoCols.TabIndex = 3;
            // 
            // tbMorphThreshold
            // 
            this.tbMorphThreshold.Location = new System.Drawing.Point(165, 71);
            this.tbMorphThreshold.Name = "tbMorphThreshold";
            this.tbMorphThreshold.Size = new System.Drawing.Size(131, 22);
            this.tbMorphThreshold.TabIndex = 4;
            // 
            // tboffset
            // 
            this.tboffset.Location = new System.Drawing.Point(165, 99);
            this.tboffset.Name = "tboffset";
            this.tboffset.Size = new System.Drawing.Size(131, 22);
            this.tboffset.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(165, 155);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(131, 29);
            this.button1.TabIndex = 6;
            this.button1.Text = "Apply";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(162, 23);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(0, 17);
            this.lblMessage.TabIndex = 7;
            // 
            // tbBinaryThreshold
            // 
            this.tbBinaryThreshold.Location = new System.Drawing.Point(165, 127);
            this.tbBinaryThreshold.Name = "tbBinaryThreshold";
            this.tbBinaryThreshold.Size = new System.Drawing.Size(131, 22);
            this.tbBinaryThreshold.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 130);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 17);
            this.label4.TabIndex = 8;
            this.label4.Text = "Binary Threshold";
            // 
            // FormParamTable2Excel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 204);
            this.Controls.Add(this.tbBinaryThreshold);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tboffset);
            this.Controls.Add(this.tbMorphThreshold);
            this.Controls.Add(this.tbNoCols);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "FormParamTable2Excel";
            this.Text = "Form Param Table2Excel";
            this.Load += new System.EventHandler(this.FormParamTable2Excel_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbNoCols;
        private System.Windows.Forms.TextBox tbMorphThreshold;
        private System.Windows.Forms.TextBox tboffset;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.TextBox tbBinaryThreshold;
        private System.Windows.Forms.Label label4;
    }
}