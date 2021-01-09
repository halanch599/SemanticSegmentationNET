using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Dnn;

namespace SemanticSegmentationNET
{
    public partial class formMnistRecogntion : Form
    {

        Net model= null;
        Point lastPoint = Point.Empty;//Point.Empty represents null for a Point object

        bool isMouseDown = false, IsDrawing=false; 

        public formMnistRecogntion()
        {
            InitializeComponent();
        }

        private void formMnistRecogntion_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = e.Location;//we assign the lastPoint to the current mouse position: e.Location ('e' is from the MouseEventArgs passed into the MouseDown event)

            isMouseDown = true;//we set to true because our mouse button is down (clicked)

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown == true)//check to see if the mouse button is down

            {

                if (lastPoint != null)//if our last point is not null, which in this case we have assigned above

                {

                    if (pictureBox1.Image == null)//if no available bitmap exists on the picturebox to draw on

                    {
                        //create a new bitmap
                        //assign the picturebox.Image property to the bitmap created
                        Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);

                        pictureBox1.Image = bmp;
                    }

                    using (Graphics g = Graphics.FromImage(pictureBox1.Image))

                    {//we need to create a Graphics object to draw on the picture box, its our main tool

                         g.FillEllipse(Brushes.Black, e.X, e.Y, 15, 15);
                        //g.DrawLine(new Pen(Color.Black, 10), e.Location, new Point(e.X+2,e.Y+2));
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        //this is to give the drawing a more smoother, less sharper look

                    }

                    pictureBox1.Invalidate();//refreshes the picturebox

                    lastPoint = e.Location;//keep assigning the lastPoint to the current mouse position

                }

            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)

            {
                pictureBox1.Image = null;
                Invalidate();
                lblResult.Text = "";
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;

            lastPoint = Point.Empty;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //https://github.com/onnx

            try
            {

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "ONNX model (*.onnx)|*.onnx;";
                if (dialog.ShowDialog()==DialogResult.OK)
                {
                    model =  DnnInvoke.ReadNetFromONNX(dialog.FileName);
                    MessageBox.Show("Model loaded.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save(@"C:\Users\akhtar.jamil\Downloads\out.jpg");
        }

        private float[] SoftMax(float[] arr)
        {
            var exp = (from a in arr
                       select (float)Math.Exp(a))
                      .ToArray();
            var sum= exp.Sum();
            return exp.Select(x => x / sum).ToArray();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                    throw new Exception("Draw a  digit.");
                if (model==null)
                {
                    throw new Exception("Load the ONNX model.");
                }

                Bitmap bmp = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
                pictureBox1.DrawToBitmap(bmp, pictureBox1.ClientRectangle);

                var img = bmp
                .ToImage<Gray, byte>()
                .Not()
                .SmoothGaussian(3)
                 .Resize(28, 28, Emgu.CV.CvEnum.Inter.Cubic)
                 .Mul(1/255.0f);

                var intput = DnnInvoke.BlobFromImage(img);
                model.SetInput(intput);

                float[] array = new float[10];
                var output = model.Forward();

                output.CopyTo(array);
                var prob = SoftMax(array);
                var index = Array.IndexOf(prob, prob.Max());
                lblResult.Text = index.ToString();
               
                chart1.Series.Clear();
                chart1.Titles.Clear();
                chart1.Series.Add("Hist");
                chart1.Titles.Add("Probabilities");

                for (int i = 0; i < prob.Length; i++)
                {
                    chart1.Series["Hist"].Points.AddXY(i , prob[i]);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
