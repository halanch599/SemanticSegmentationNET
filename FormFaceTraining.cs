using Emgu.CV;
using Emgu.CV.Structure;
using SemanticSegmentationNET.models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SemanticSegmentationNET
{
    public partial class FormFaceTraining : Form
    {
        List<Image<Gray, byte>> Images= null;
        VideoCapture cameraCapture = null;
        CascadeClassifier cascadeClassifier = null;
        Mat frame = null;
        Rectangle[] bboxes = null;
        public FormFaceTraining()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = true;
                if (dialog.ShowDialog()==DialogResult.OK)
                {
                    tbImage.Text = Path.GetFullPath(dialog.FileName);
                    Images = new List<Image<Gray, byte>>();
                    foreach (var file in dialog.FileNames)
                    {
                        Images.Add(new Image<Gray, byte>(file));
                    }
                    pictureBox1.Image = Images[0].AsBitmap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            tbEmail.Text = "";
            tbFirstName.Text = "";
            tbImage.Text = "";
            tbLastName.Text = "";
            Images.Clear();
            lblMessage.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnCapture.Text == "Turn On Camera")
                {
                    btnCapture.Text = "Turn Off Camera";
                    if (cameraCapture == null)
                    {
                        cameraCapture = new VideoCapture(0);
                        cameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 243);
                        cameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 320);

                        cameraCapture.ImageGrabbed += CameraCapture_ImageGrabbed;
                    }
                    cameraCapture.ImageGrabbed -= CameraCapture_ImageGrabbed;
                    cameraCapture.ImageGrabbed += CameraCapture_ImageGrabbed;

                    cameraCapture.Start();
                    frame = new Mat();
                    if (cascadeClassifier == null)
                    {
                        cascadeClassifier = EmguHelper.GetInstanceCascadeClassifier();
                    }
                }
                else
                {
                    btnCapture.Text = "Turn On Camera";
                    cameraCapture.ImageGrabbed -= CameraCapture_ImageGrabbed;
                    //cameraCapture = null;
                    //cascadeClassifier = null;
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CameraCapture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                if (cameraCapture.IsOpened)
                {
                    cameraCapture.Retrieve(frame);
                    if (frame != null)
                    {
                        var gray = frame.ToImage<Gray, byte>();
                            //.Resize(0.5,Emgu.CV.CvEnum.Inter.Cubic);
                        
                        bboxes = cascadeClassifier.DetectMultiScale(gray.Convert<Gray, byte>());
                        //bboxes = bboxes.Select(b =>
                        //{
                        //    b.X = b.X - 50;
                        //    b.Y = b.Y - 50;
                        //    b.Width = b.Width + 100;
                        //    b.Height = b.Height + 100; return b;
                        //}).ToArray();

                        //foreach (var rect in bboxes)
                        //{
                        //    CvInvoke.Rectangle(frame, rect, new MCvScalar(0, 0, 255), 2);
                        //}
                        pictureBox1.Image = frame.ToBitmap();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (cameraCapture==null)
                {
                    return;
                }
                if (Images==null)
                {
                    Images = new List<Image<Gray, byte>>();
                }

                cameraCapture.Pause();
                //pictureBox1.SizeMode = PictureBoxSizeMode.Normal;

                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Gray, byte>();
                if (bboxes.Length>0)
                {
                    //img.ROI = bboxes[0];
                    Images.Add(img.Copy());
                    //img.ROI = Rectangle.Empty;
                    lblMessage.Text = "Image Captured: " + Images.Count.ToString();
                }
                cameraCapture.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (tbFirstName.Text.Trim()=="")
                {
                    lblError.Text = "Please enter first name";
                    lblError.ForeColor = Color.Red;
                    tbFirstName.Focus();
                    return;
                }

                string fname = tbFirstName.Text.Trim();
                string lname = tbLastName.Text.Trim();
                string email = tbEmail.Text.Trim();

                string path = @"F:\AJ Data\Data\yalefaces_Original\";

                if (Images.Count==11)
                {
                    for (int i = 0; i < Images.Count; i++)
                    {
                        //var img = Images[i];

                        Images[i].Save(path + (i + 1) + tbFirstName.Text +  ".jpg");
                        //img.Resize(243,320,Emgu.CV.CvEnum.Inter.Cubic)
                        //.Save(path + tbFirstName.Text + (i + 1) + ".jpg");
                    }

                }

            }
            catch (Exception ex)
            {

                lblError.Text = ex.Message;
            }
        }

        private void FormFaceTraining_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (cameraCapture!=null)
                {
                    cameraCapture.Dispose();
                    cascadeClassifier = null;
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
