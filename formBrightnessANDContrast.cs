using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using SemanticSegmentationNET.Model;

namespace SemanticSegmentationNET
{
    public partial class formBrightnessANDContrast : Form
    {
        public Image<Bgr, byte> imgInput { get; set; }
        Image<Bgr, byte> imgOutput = null;
        public formBrightnessANDContrast()
        {
            InitializeComponent();
        }

        private void formBrightnessANDContrast_Load(object sender, EventArgs e)
        {
            try
            {
                if (imgInput == null)
                {
                    throw new Exception("Select and image");
                }

                pictureBox1.Image = imgInput.AsBitmap();

                lblMinBrightness.Text = trackBar1.Minimum.ToString();
                lblMaxBrightness.Text = ((float)trackBar1.Maximum / 100).ToString();
                lblCurrentBrightness.Text = ((float)trackBar1.Value/100).ToString();

                lblMinContrast.Text = trackBar2.Minimum.ToString();
                lblMaxContrast.Text = (trackBar2.Maximum ).ToString();
                lblCurrentContrast.Text = (trackBar2.Value ).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            lblCurrentBrightness.Text = ((float)trackBar1.Value / 100).ToString();

            imgOutput = imgInput.Mul(double.Parse(lblCurrentBrightness.Text)) + trackBar2.Value;
            pictureBox1.Image = cvsHelperClass.HConcatImages(imgInput, imgOutput).AsBitmap();

        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            //MessageBox.Show("Test");
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            lblCurrentContrast.Text = (trackBar2.Value ).ToString();

            imgOutput = imgInput.Mul(double.Parse(lblCurrentBrightness.Text)) + trackBar2.Value;
            pictureBox1.Image = cvsHelperClass.HConcatImages(imgInput, imgOutput).AsBitmap();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (imgOutput!=null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Images (*.jpg;)|*.jpg;|Images (*.bmp;)|*.bmp;|Images (*.png;)|*.png;";

                if (dialog.ShowDialog()==DialogResult.OK)
                {
                    imgOutput.Save(dialog.FileName);
                    MessageBox.Show("File save successfully.");
                }
            }
        }
    }
}
