using Emgu.CV.Features2D;
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
    public partial class formParameter : Form
    {
        public delegate void ApplyFunction(int x);
        public event ApplyFunction OnApplied;

        int Min, Max;
        public formParameter(int min=0, int max=255)
        {
            InitializeComponent();
            Min =min;
            Max = max;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            lblCurrentValue.Text = trackBar1.Value.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OnApplied?.Invoke(trackBar1.Value);
        }

        private void formParameter_Load(object sender, EventArgs e)
        {
            trackBar1.Minimum = Min;
            trackBar1.Maximum = Max;
            lblCurrentValue.Text = trackBar1.Value.ToString();
            lblMin.Text = Min.ToString();
            lblMax.Text = Max.ToString();

        }
    }
}
