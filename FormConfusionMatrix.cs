using SemanticSegmentationNET.Model;
using SemanticSegmentationNET.models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SemanticSegmentationNET
{
    public partial class FormConfusionMatrix : Form
    {
        public FormConfusionMatrix()
        {

        }
        public FormConfusionMatrix(int[,] ConfusionMatrix, string txt="")
        {
            InitializeComponent();
            richTextBox2.Text = txt;

            dataGridView1.DataSource =helperFunctions.Array2Datatable(ConfusionMatrix);
        }

        private void FormConfusionMatrix_Load(object sender, EventArgs e)
        {

        }
    }
}
