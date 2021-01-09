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
    public partial class formGabor : Form
    {
        double gamma= 0.3, lambda= 4.0, psi= 1.0, theta=0.6;
        int size=3, sigma=2;

        public delegate void ApplyGabor(double gamma, double lambda, double psi, double theta, int size, int sigma);
        public event ApplyGabor OnApplyGabor;

        private void trackBarGamma_Scroll(object sender, EventArgs e)
        {
            lblGamma.Text = (trackBarGamma.Value / 100.0).ToString();
        }

        private void trackBarLambda_Scroll(object sender, EventArgs e)
        {
            lblLambda.Text = (trackBarLambda.Value / 100.0).ToString();
        }

        private void trackBarPsi_Scroll(object sender, EventArgs e)
        {
            lblPsi.Text = (trackBarPsi.Value / 100.0).ToString();

        }

        private void trackBarSigma_Scroll(object sender, EventArgs e)
        {
            lblSigma.Text = (trackBarSigma.Value / 100.0).ToString();
        }

        private void trackBarSize_Scroll(object sender, EventArgs e)
        {
            lblSize.Text = (trackBarSize.Value).ToString();
        }

        private void trackBarTheta_Scroll(object sender, EventArgs e)
        {
            lblSigma.Text = (trackBarSigma.Value).ToString();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OnApplyGabor?.Invoke(double.Parse(lblGamma.Text),
                double.Parse(lblLambda.Text),
                double.Parse(lblPsi.Text),
                double.Parse(lblTheta.Text),
                int.Parse(lblSize.Text),
                int.Parse(lblSigma.Text));
        }

        public formGabor()
        {
            InitializeComponent();
            initializeTrackBar();
        }

        private void initializeTrackBar()
        {
            trackBarGamma.Minimum = 1;
            trackBarGamma.Maximum = 100;
            trackBarGamma.Value = (int)(gamma * 100);

            trackBarLambda.Minimum = 1;
            trackBarLambda.Maximum = 100;
            trackBarLambda.Value = (int)lambda;

            trackBarPsi.Minimum = 1;
            trackBarPsi.Maximum = 100;
            trackBarPsi.Value = (int)(psi* 100);

            trackBarTheta.Minimum = 1;
            trackBarTheta.Maximum = 100;
            trackBarTheta.Value = (int)(gamma * 100);

            trackBarSize.Minimum = 1;
            trackBarSize.Maximum = 100;
            trackBarSize.Value = size;

            trackBarSigma.Minimum = 1;
            trackBarSigma.Maximum = 100;
            trackBarSigma.Value = sigma;

            lblGamma.Text = trackBarGamma.Value.ToString();
            lblLambda.Text = trackBarLambda.Value.ToString();
            lblPsi.Text = trackBarPsi.Value.ToString();
            lblTheta.Text = trackBarTheta.Value.ToString();
            lblSize.Text = trackBarSize.Value.ToString();
            lblSigma.Text = trackBarSigma.Value.ToString();
        }

        private void formGabor_Load(object sender, EventArgs e)
        {
        }
    }
}
