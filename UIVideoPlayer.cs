using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu;
using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;

namespace SemanticSegmentationNET
{
    public partial class UIVideoPlayer : UserControl
    {
        VideoCapture videoCapture;
        int CurrentFrame = 0;
        int TotalFrames = 0;
        int skip = 5;
        bool IsPlaying = false;
        CascadeClassifier classifier;
        private static UIVideoPlayer _instance;

        private UIVideoPlayer() { }
        public static UIVideoPlayer GetInstance(string path)
        {
            _instance = null;
            _instance = new UIVideoPlayer();
            _instance.init(path);
            return _instance;
        }

        public void init( string path)
        {
            InitializeComponent();
            try
            {
                videoCapture = new VideoCapture(path);
                Mat frame = new Mat();
                
                    TotalFrames=  int.Parse(videoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount).ToString());
                    if (CurrentFrame<TotalFrames)
                    {
                        if (videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, CurrentFrame))
                        {
                            videoCapture.Read(frame);
                            pictureBox1.Image = frame.ToBitmap();
                            lblCurrent.Text = CurrentFrame.ToString();
                            lblMax.Text = TotalFrames.ToString();
                        }
                    trackBar1.Value = 0;
                    trackBar1.Minimum = 0;
                    trackBar1.Maximum = TotalFrames;
                    }
                    lblMax.Text = TotalFrames.ToString();

                string facePath = Path.GetFullPath(@"../../../data/haarcascade_frontalface_default.xml");
                classifier = new CascadeClassifier(facePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        private Image<Bgr, byte> ProcessFrame(Mat frame)
        {
            var imgRGB = frame.ToImage<Bgr, byte>();
            var imgGray = imgRGB.Convert<Gray, byte>();
            var faces = classifier.DetectMultiScale(imgGray);

            foreach (var rect in faces)
            {
                //imgRGB.Draw(rect, new Bgr(0, 0, 255), 1);
                imgRGB.ROI = rect;
                imgRGB._SmoothGaussian(25);
                imgRGB._Mul(0.3);
                imgRGB.ROI = Rectangle.Empty;
            }
            return imgRGB;
        }
        private async  void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (button1.Text =="Play")
                {
                    Mat frame = new Mat();
                    button1.Text = "Pause";
                    IsPlaying = true;
                    while (IsPlaying  && trackBar1.Value < TotalFrames )
                    {
                        if (videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, trackBar1.Value))
                        {
                            videoCapture.Read(frame);
                            var img = ProcessFrame(frame);
                            lblCurrent.Text = trackBar1.Value.ToString();
                            if (trackBar1.Value + skip<=trackBar1.Maximum)
                            {
                                trackBar1.Value = trackBar1.Value + skip;
                            }
                            else
                            {
                                trackBar1.Value = trackBar1.Maximum;
                            }
                            pictureBox1.Image = img.ToBitmap();
                        }
                        await Task.Delay(1);
                    }
                    if (trackBar1.Value >= TotalFrames)
                    {
                        button1.Text = "Play";
                        IsPlaying = false;
                    }

                }
                else
                {
                    button1.Text = "Play";
                    IsPlaying = false;
                    if (videoCapture!=null)
                    {
                        videoCapture.Pause();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            lblCurrent.Text = trackBar1.Value.ToString();
        }
    }
}
