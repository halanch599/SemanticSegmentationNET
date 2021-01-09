namespace SemanticSegmentationNET
{
    partial class FormAnnParameters
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
            this.btnClose = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.ddlTrainMethod = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbIterations = new System.Windows.Forms.TextBox();
            this.tbRMSE = new System.Windows.Forms.TextBox();
            this.tbLayerSize = new System.Windows.Forms.TextBox();
            this.tbMomentum = new System.Windows.Forms.TextBox();
            this.ddlActivationFunction = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.cbLoadSavedModel = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(258, 282);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(92, 28);
            this.btnClose.TabIndex = 10;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(61, 282);
            this.btnApply.Margin = new System.Windows.Forms.Padding(4);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(92, 28);
            this.btnApply.TabIndex = 9;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(48, 12);
            this.lblMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(0, 17);
            this.lblMessage.TabIndex = 8;
            // 
            // ddlTrainMethod
            // 
            this.ddlTrainMethod.BackColor = System.Drawing.SystemColors.Window;
            this.ddlTrainMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlTrainMethod.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ddlTrainMethod.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.ddlTrainMethod.FormattingEnabled = true;
            this.ddlTrainMethod.Location = new System.Drawing.Point(168, 65);
            this.ddlTrainMethod.Margin = new System.Windows.Forms.Padding(4);
            this.ddlTrainMethod.Name = "ddlTrainMethod";
            this.ddlTrainMethod.Size = new System.Drawing.Size(182, 26);
            this.ddlTrainMethod.TabIndex = 6;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label6.Location = new System.Drawing.Point(44, 65);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(99, 18);
            this.label6.TabIndex = 7;
            this.label6.Text = "Train Method:";
            // 
            // tbIterations
            // 
            this.tbIterations.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.tbIterations.Location = new System.Drawing.Point(172, 143);
            this.tbIterations.Margin = new System.Windows.Forms.Padding(4);
            this.tbIterations.Name = "tbIterations";
            this.tbIterations.Size = new System.Drawing.Size(178, 24);
            this.tbIterations.TabIndex = 2;
            this.tbIterations.Text = "1000";
            // 
            // tbRMSE
            // 
            this.tbRMSE.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.tbRMSE.Location = new System.Drawing.Point(172, 175);
            this.tbRMSE.Margin = new System.Windows.Forms.Padding(4);
            this.tbRMSE.Name = "tbRMSE";
            this.tbRMSE.Size = new System.Drawing.Size(178, 24);
            this.tbRMSE.TabIndex = 3;
            this.tbRMSE.Text = "0.01";
            // 
            // tbLayerSize
            // 
            this.tbLayerSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.tbLayerSize.Location = new System.Drawing.Point(172, 207);
            this.tbLayerSize.Margin = new System.Windows.Forms.Padding(4);
            this.tbLayerSize.Name = "tbLayerSize";
            this.tbLayerSize.Size = new System.Drawing.Size(178, 24);
            this.tbLayerSize.TabIndex = 4;
            this.tbLayerSize.Text = "8,50,2";
            // 
            // tbMomentum
            // 
            this.tbMomentum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.tbMomentum.Location = new System.Drawing.Point(172, 111);
            this.tbMomentum.Margin = new System.Windows.Forms.Padding(4);
            this.tbMomentum.Name = "tbMomentum";
            this.tbMomentum.Size = new System.Drawing.Size(178, 24);
            this.tbMomentum.TabIndex = 1;
            this.tbMomentum.Text = "0.9000";
            // 
            // ddlActivationFunction
            // 
            this.ddlActivationFunction.BackColor = System.Drawing.SystemColors.Window;
            this.ddlActivationFunction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlActivationFunction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ddlActivationFunction.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.ddlActivationFunction.FormattingEnabled = true;
            this.ddlActivationFunction.Location = new System.Drawing.Point(168, 32);
            this.ddlActivationFunction.Margin = new System.Windows.Forms.Padding(4);
            this.ddlActivationFunction.Name = "ddlActivationFunction";
            this.ddlActivationFunction.Size = new System.Drawing.Size(182, 26);
            this.ddlActivationFunction.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label5.Location = new System.Drawing.Point(61, 213);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 18);
            this.label5.TabIndex = 3;
            this.label5.Tag = "{Input, Hidden, Output}";
            this.label5.Text = "Layer Size :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label4.Location = new System.Drawing.Point(73, 143);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 18);
            this.label4.TabIndex = 2;
            this.label4.Text = "Iterations :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label3.Location = new System.Drawing.Point(92, 175);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 18);
            this.label3.TabIndex = 1;
            this.label3.Text = "RMSE :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label2.Location = new System.Drawing.Point(44, 32);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(102, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "Act. Function :";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label7.Location = new System.Drawing.Point(55, 111);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(92, 18);
            this.label7.TabIndex = 5;
            this.label7.Text = "Momentum :";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.cbLoadSavedModel);
            this.panel2.Controls.Add(this.btnClose);
            this.panel2.Controls.Add(this.btnApply);
            this.panel2.Controls.Add(this.lblMessage);
            this.panel2.Controls.Add(this.ddlTrainMethod);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.tbIterations);
            this.panel2.Controls.Add(this.tbRMSE);
            this.panel2.Controls.Add(this.tbLayerSize);
            this.panel2.Controls.Add(this.tbMomentum);
            this.panel2.Controls.Add(this.ddlActivationFunction);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(432, 323);
            this.panel2.TabIndex = 12;
            // 
            // cbLoadSavedModel
            // 
            this.cbLoadSavedModel.AutoSize = true;
            this.cbLoadSavedModel.Location = new System.Drawing.Point(172, 240);
            this.cbLoadSavedModel.Name = "cbLoadSavedModel";
            this.cbLoadSavedModel.Size = new System.Drawing.Size(148, 21);
            this.cbLoadSavedModel.TabIndex = 11;
            this.cbLoadSavedModel.Text = "Load Saved Model";
            this.cbLoadSavedModel.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(158, 282);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(92, 28);
            this.button1.TabIndex = 12;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormAnnParameters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 323);
            this.Controls.Add(this.panel2);
            this.MaximumSize = new System.Drawing.Size(450, 370);
            this.MinimumSize = new System.Drawing.Size(450, 370);
            this.Name = "FormAnnParameters";
            this.Text = "FormAnnParameters";
            this.Load += new System.EventHandler(this.FormAnnParameters_Load);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.ComboBox ddlTrainMethod;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbIterations;
        private System.Windows.Forms.TextBox tbRMSE;
        private System.Windows.Forms.TextBox tbLayerSize;
        private System.Windows.Forms.TextBox tbMomentum;
        private System.Windows.Forms.ComboBox ddlActivationFunction;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox cbLoadSavedModel;
        private System.Windows.Forms.Button button1;
    }
}