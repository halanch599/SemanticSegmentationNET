using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.Plot;
using Emgu.CV;

using Emgu.CV.Structure;
using Emgu.CV.Util;
using ZedGraph;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
using Emgu.CV.CvEnum;
using Emgu.CV.XFeatures2D;
using Emgu.CV.Face;
//using DlibDotNet;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using System.IO;
using System.Globalization;
//using Accord.Imaging;
//using Accord.Imaging.Filters;
using System.Drawing.Drawing2D;
using System.Threading;
using Emgu.CV.Dnn;
using SemanticSegmentationNET.Model;
using Emgu.CV.ML;
using SemanticSegmentationNET.YoloParser;
using Emgu.CV.Text;
using ZXing;
using ZXing.QrCode;
using System.Xml.Schema;
using Emgu.CV.OCR;
using ClosedXML.Excel;
using Microsoft.Office.Interop.Excel;
using Emgu.CV.UI;
using SemanticSegmentationNET.models;
using Emgu.CV.BgSegm;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.EntityFrameworkCore.Internal;
using ClosedXML;

namespace SemanticSegmentationNET
{

    public partial class Form1 : Form
    {
        //Yale face dataset
        List<FaceData> dataset = new List<FaceData>();
        List<FaceData> trainingData = new List<FaceData>();
        List<FaceData> testingData = new List<FaceData>();

        public Image<Bgr, byte> SeamlessMaskImage { get; set; }
        public Point SeamlessMaskLocation { get; set; }

        VectorOfVectorOfPoint contoursGlobal = new VectorOfVectorOfPoint();
        //Install-Package Emgu.CV.runtime.windows -Version 4.2.0.3662
        //Image<Bgr, byte> imgInput;
        Rectangle rect = Rectangle.Empty;
        Dictionary<string, Image<Bgr, byte>> imgList;
        private bool showPixelValue = false;
        int rows, cols;
        private bool IsSelectingGrabCutRectangle;
        private bool IsMouseDown;
        Point MouseDownLocation, MouseUpLocation;
        bool IsSelectingRectangle = false;
        Rectangle rectROI;
        bool SelectROI = false;
        Point StartROI, EndROI;
        bool Isplaying = false;
        bool saveTemplate = false;
        VideoCapture capture;
        VectorOfPoint vp;
        List<Point> Polygon = new List<Point>();
        List<List<Point>> ForegroundPoint;
        List<List<Point>> BackgroundPoints;
        List<Point> currentPolygon;
        //string TrainingFolderPath = "";
        //string TestingFolderPath = "";
        private bool drawFilledPolygon;
        private bool selectForeground;
        private bool selectBackground;
        VideoCapture videoCapture;
        CascadeClassifier cascadeClassifier=null;
        FacemarkLBF facemarkDetector = null;
        private bool IsGrabbingWithLandmarks;
        private bool detectFaces;
        Emgu.CV.Dnn.Net model = null;
        Emgu.CV.ML.ANN_MLP MLPModel =null;
        int[] ActualLabels= null;
        int[] PredictedLabels = null;
        //GeneralConfusionMatrix ConfusionMatrix;
        Emgu.CV.Matrix<float>  x_test= null, x_train =null,Data_Set=null;
        Emgu.CV.Matrix<int> y_test = null, y_train = null, Data_Label = null;
        private SVM SVMModel;
        List<List<Point>> InpaintPoints = null;
        List<Point> InpaintCurrentPoints = null;

        bool InpaintMouseDown = false;
        //Point InpaintMouseLocation = new Point();
        private bool InpaintSelection;

        // FACE RECOGNITION
        List<FaceImageData> trainData = null, testData = null;
        List<int> PredictedLabel;
        List<int> ActualLabel;
        int RandomIndex = 0;

        // ANN Classifier
        ANN_MLP ANN=null;

        //Evaluation Form
        FormConfusionMatrix formConfusionMatrix;
        public Form1()
        {
            InitializeComponent();
            imgList = new Dictionary<string, Image<Bgr, byte>>();
            vp = new VectorOfPoint();
            ForegroundPoint = new List<List<Point>>();
            BackgroundPoints = new List<List<Point>>();
            currentPolygon = new List<Point>();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    imgList.Clear();
                    treeView1.Nodes.Clear();

                    var img = new Image<Bgr, byte>(dialog.FileName);
                    AddImage(img, "Input");
                    pictureBox1.Image = img.AsBitmap();
                    cols = img.Width;
                    rows = img.Height;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void binarizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (imgList.Count == 0)
                {
                    MessageBox.Show("Select and image to process");
                    return;
                }

                var bw = imgList["Input"]
                    .Convert<Gray, byte>()
                    .ThresholdBinary(new Gray(100), new Gray(255));

                AddImage(bw.Convert<Bgr, byte>(), "Binary");
                pictureBox1.Image = bw.AsBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                pictureBox1.Image = imgList[e.Node.Text].AsBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void gaussianBlurToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void gaussianBlurToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    MessageBox.Show("Select an image to process");
                    return;
                }

                var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>().SmoothGaussian(3);
                pictureBox1.Image = img.AsBitmap();
                AddImage(img, "Gaussian");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddImage(Image<Bgr, byte> img, string keyName)
        {
            if (!treeView1.Nodes.ContainsKey(keyName))
            {
                TreeNode node = new TreeNode(keyName);
                node.Name = keyName;
                treeView1.Nodes.Add(node);
                treeView1.SelectedNode = node;
            }

            if (!imgList.ContainsKey(keyName))
            {
                imgList.Add(keyName, img);

            }
            else
            {
                imgList[keyName] = img;
            }
        }
        private void differenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> list = new List<string>();
                foreach (TreeNode item in treeView1.Nodes)
                {
                    if (item.Checked)
                    {
                        list.Add(item.Text);
                    }
                }

                if (list.Count > 1)
                {
                    var img = new Mat();
                    CvInvoke.AbsDiff(imgList[list[0]], imgList[list[1]], img);
                    AddImage(img.ToImage<Bgr, byte>(), "Difference");
                    pictureBox1.Image = img.ToBitmap();

                }
                else
                {
                    MessageBox.Show("Select two images.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void thresholdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                formParameter form = new formParameter();
                form.OnApplied += ApplyThreshold;
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ApplyThreshold(int threshold=100)
        {
            try
            {
                if (imgList["Input"]== null)
                {
                    return;
                }

                var img = imgList["Input"].Convert<Gray, byte>().Clone();
                var output = img.ThresholdBinary(new Gray(threshold), new Gray(255));
                pictureBox1.Image = output.ToBitmap();
                AddImage(output.Convert<Bgr, byte>(), "Thresholding");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void pyUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();
                var imgout = img.PyrUp();

                pictureBox1.Image = imgout.ToBitmap();
                AddImage(imgout, "PyUp");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pyDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();
                var imgout = img.PyrDown();

                pictureBox1.Image = imgout.ToBitmap();
                AddImage(imgout, "PyDown");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void findOrangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //BGR 0 155  245     20  175  265

            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();
                if (img.NumberOfChannels >= 3)
                {
                    var mask = img.InRange(new Bgr(0, 100, 240), new Bgr(180, 220, 255));
                    var imgout = img.Copy(mask);
                    pictureBox1.Image = imgout.ToBitmap();
                    AddImage(imgout, "Oranges");
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void showPixelValueToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {

        }

        private void findAppleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //BGR 20 100  50     220  255  220

            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();
                if (img.NumberOfChannels >= 3)
                {
                    var mask = img.InRange(new Bgr(50, 100, 50), new Bgr(220, 255, 220));
                    var imgout = img.Copy(mask);
                    pictureBox1.Image = imgout.ToBitmap();
                    AddImage(imgout, "Oranges");
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void hSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();
                var hsv = img.Convert<Hsv, byte>();
                int epsilon = 30;
                var mask = hsv.InRange(new Hsv(60 - epsilon, 35, 0), new Hsv(60 + epsilon, 255, 255));
                var imgout = img.Copy(mask);
                pictureBox1.Image = imgout.ToBitmap();

                AddImage(imgout, "Apples");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void hitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Gray, byte>()
                    .ThresholdBinary(new Gray(100), new Gray(255));

                //Mat SE = Mat.Zeros(3, 3, Emgu.CV.CvEnum.DepthType.Cv8S,1);

                int[,] data = {
                    { 0, -1, -1 },
                    { 1, 1, -1 },
                    { 0, 1, 0 } };
                Emgu.CV.Matrix<int> SE = new Emgu.CV.Matrix<int>(data);

                Mat imgout = new Mat();
                CvInvoke.MorphologyEx(img, imgout, Emgu.CV.CvEnum.MorphOp.HitMiss, SE, new Point(0, 2), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(0));

                pictureBox1.Image = imgout.ToBitmap();
                //var SE = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Custom,new Size(3,3),new Point(-1,-1));
                //SE.SetTo(tempStructure);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void filter2DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                float[,] data = {
                    { 0, 1, 0 },
                    { 1, -1, 1},
                    { 0, 1, 0 } };
                Emgu.CV.Matrix<float> SE = new Emgu.CV.Matrix<float>(data);
                SE._Mul(1 / 9.0);

                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Bgr, float>();

                var imgout = img.CopyBlank();
                CvInvoke.Filter2D(img, imgout, SE, new Point(-1, -1));

                pictureBox1.Image = imgout.ToBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void borderPaddingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }
                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Bgr, float>();

                var imgout = new Mat();
                CvInvoke.CopyMakeBorder(img, imgout, 5, 0, 0, 0, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(0));

                pictureBox1.Image = imgout.ToBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void borderRemovalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }
                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Bgr, float>();

                int width = img.Width;
                int height = img.Height;

                img.ROI = new Rectangle(5, 5, width - 10, height - 10);
                var imgout = img.Copy();
                img.ROI = Rectangle.Empty;

                pictureBox1.Image = imgout.ToBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void binaryInverseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }
                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Gray, byte>()
                    .Not();

                pictureBox1.Image = img.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void sobelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                float[,] data = {
                    { -1, 0, 1 },
                    { -2, 0, 2},
                    { -1, 0, 1 } };
                Emgu.CV.Matrix<float> SEx = new Emgu.CV.Matrix<float>(data);

                Emgu.CV.Matrix<float> SEy = SEx.Transpose();

                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Bgr, float>();

                var Gx = new Mat();
                var Gy = new Mat();

                CvInvoke.Sobel(img, Gx, Emgu.CV.CvEnum.DepthType.Cv32F, 1, 0);
                CvInvoke.Sobel(img, Gy, Emgu.CV.CvEnum.DepthType.Cv32F, 0, 1);

                var gx = Gx.ToImage<Gray, float>();
                var gy = Gy.ToImage<Gray, float>();

                var Gxx = new Mat(Gx.Size, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
                var Gyy = new Mat(Gx.Size, Emgu.CV.CvEnum.DepthType.Cv32F, 1);

                CvInvoke.ConvertScaleAbs(Gx, Gxx, 0, 0);
                CvInvoke.ConvertScaleAbs(Gy, Gyy, 0, 0);

                var mag = new Mat(Gx.Size, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                CvInvoke.AddWeighted(Gxx, 0.5, Gyy, 0.5, 0, mag);

                AddImage(mag.ToImage<Bgr, byte>(), "Mag Absolute");


                gx._Mul(gx);
                gy._Mul(gy);

                var M = new Mat(gx.Size, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                CvInvoke.Sqrt(gx + gy, M);
                AddImage(M.ToImage<Bgr, byte>(), "Mag Squared");
                //CvInvoke.Filter2D(img, Gx, SEx, new Point(-1, -1));
                //CvInvoke.Filter2D(img, Gy, SEy, new Point(-1, -1));

                pictureBox1.Image = M.ToBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void showPixelValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showPixelValue = !showPixelValue;
        }

        private void selectingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsSelectingRectangle = !IsSelectingRectangle;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (SeamlessMaskImage!=null)
            {
                SeamlessMaskLocation = e.Location;
            }
            if (InpaintSelection &&  e.Button == MouseButtons.Left)
            {
                InpaintMouseDown = true;
                InpaintCurrentPoints.Add(e.Location);
            }
            if (IsSelectingRectangle == true)
            {
                IsMouseDown = true;
                MouseDownLocation = e.Location;
            }

            if (IsSelectingGrabCutRectangle == true)
            {
                IsMouseDown = true;
                MouseDownLocation = e.Location;
            }

            if (SelectROI)
            {
                IsMouseDown = true;
                StartROI = e.Location;
            }
            if (drawFilledPolygon)
            {
                MouseDownLocation = e.Location;

                // See if we are already drawing a polygon.
                if (Polygon != null)
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        contextMenuStrip1.Show(pictureBox1,e.Location);
                    }
                    else
                    {
                        if (!Polygon.Contains(e.Location))
                        {
                            Polygon.Add(e.Location);
                        }
                    }
                }
                else
                {
                    // Start a new polygon.
                    Polygon = new List<Point>();
                    Polygon.Add(MouseDownLocation);
                }
            }

            if (selectForeground|| selectBackground)
            {
                IsMouseDown = true;
                currentPolygon.Clear();
                currentPolygon.Add(e.Location);
            }
            
            pictureBox1.Invalidate();

        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (InpaintMouseDown && InpaintSelection)
            {
                InpaintMouseDown = false;
                InpaintPoints.Add(InpaintCurrentPoints.ToList());
                InpaintCurrentPoints.Clear();
            }
            if (IsSelectingRectangle)
            {
                IsSelectingRectangle = false;
                IsMouseDown = false;
                MouseUpLocation = e.Location;

                if(saveTemplate && rect!=null)
                {
                    var img = new Bitmap(pictureBox1.Image)
                   .ToImage<Bgr, byte>();

                    img.ROI = rect;
                    var img2 = img.Copy();

                    img.ROI = Rectangle.Empty;

                    pictureBox1.Image = img2.AsBitmap();
                    AddImage(img2, "Template");
                    saveTemplate = false;
                }
            }

            if (IsSelectingGrabCutRectangle)
            {
                IsSelectingGrabCutRectangle = false;
                IsMouseDown = false;
                MouseUpLocation = e.Location;
                var img = new Bitmap(pictureBox1.Image)
               .ToImage<Bgr, byte>();

                if (rect != null)
                {
                    CvInvoke.Rectangle(img, rect, new MCvScalar(0, 0, 255), 2);
                }
                pictureBox1.Image = img.AsBitmap();
            }

            if (SelectROI)
            {
                SelectROI = false;
                IsMouseDown = false;
                EndROI = e.Location;
                int width = Math.Abs(StartROI.Y - EndROI.Y);
                int height = Math.Abs(StartROI.X - EndROI.X);

                rectROI = new Rectangle(StartROI.X, StartROI.Y, width, height);
            }

            //if (selectForeground )
            //{
            //    MouseUpLocation = Point.Empty;
            //    IsMouseDown = false;
            //}
            if (selectForeground && IsMouseDown) 
            {
                IsMouseDown = false;
                currentPolygon.Add(MouseUpLocation);
                if (currentPolygon.Count>1)
                {
                    ForegroundPoint.Add(currentPolygon.ToList());
                    //currentPolygon.Clear();
                }
                MouseUpLocation = Point.Empty;
            }

            if (selectBackground && IsMouseDown)
            {
                IsMouseDown = false;
                currentPolygon.Add(MouseUpLocation);
                if (currentPolygon.Count > 1)
                {
                    BackgroundPoints.Add(currentPolygon.ToList());
                    //currentPolygon.Clear();
                }
                MouseUpLocation = Point.Empty;
            }

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (IsSelectingRectangle)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
            if (IsSelectingGrabCutRectangle)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
            //if (IsMouseDown)
            //{
            //    using (Pen pen = new Pen(Color.Red, 2))
            //    {
            //        e.Graphics.DrawRectangle(pen, rect);
            //    }
            //}



            if (drawFilledPolygon)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
               // e.Graphics.Clear(pictureBox1.BackColor);

                // Draw the new polygon.
                if (Polygon != null)
                {
                    Pen pen = new Pen(Brushes.Blue, 2.0f);
                    // e.Graphics.DrawEllipse(pen, start.X - 2, start.Y - 2, 5, 5);
                    foreach (var point in Polygon)
                    {
                        e.Graphics.DrawEllipse(pen, point.X - 2, point.Y - 2, 5, 5);
                    }

                    // Draw the new polygon.
                    if (Polygon.Count > 1)
                    {
                       
                        e.Graphics.DrawLines(pen, Polygon.ToArray());
                        //if (Polygon[0]==Polygon[Polygon.Count-1])
                        //{
                        //    drawFilledPolygon = false;
                        //}
                    }

                    
                }
                
            }
        }

        private void calculateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                    return;

                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Gray, byte>();
                Mat hist = new Mat();
                float[] ranges = new float[] { 0, 256 };
                int[] channel = { 0 };
                int[] histSize = { 256 };

                VectorOfMat ms = new VectorOfMat();
                ms.Push(img);
                CvInvoke.CalcHist(ms, channel, null, hist, histSize, ranges, false);

                HistogramViewer viewer = new HistogramViewer();
                viewer.Text = "Image Histogram";
                viewer.ShowIcon = false;

                var hi = viewer.HistogramCtrl.GenerateHistogram("Image Histogram", Color.Blue, hist, histSize[0], ranges);
                viewer.HistogramCtrl.Image =hi;
                viewer.HistogramCtrl.BackColor = Color.White;
                viewer.HistogramCtrl.Refresh();
                viewer.Show();
                Mat output = new Mat(hist.Rows, hist.Cols, DepthType.Cv64F, 1);
                hist.ConvertTo(output, DepthType.Cv64F);
                Plot2d plot = new Plot2d(output);


                //plot.SetGridLinesNumber(100);
                plot.SetPlotLineColor(new MCvScalar(0, 0, 255));
                plot.SetMaxX(256);
                plot.SetMinX(-1);
                plot.SetMinY(-1);
                plot.SetInvertOrientation(true);
                plot.SetPlotBackgroundColor(new MCvScalar(255, 255, 255));
                plot.SetPlotGridColor(new MCvScalar(128, 128,128));
                plot.SetShowGrid(true);
                plot.SetShowText(true);
                plot.SetNeedPlotLine(true);
               // plot.SetPlotAxisColor(new MCvScalar(255, 0, 255));
                plot.Render(output);
                // sor the histogram
                int[] histogram = new int[histSize[0]];
                Emgu.CV.Matrix<int> hist1 = new Emgu.CV.Matrix<int>(histSize[0], 1);

                var ar = hist.GetData();
                var list = ar.Cast<Single>().Select(c => (int)c).ToArray();
                var dictionary = list
                                .Select((v, i) => new { Key = i, Value = v })
                                .ToDictionary(o => o.Key, o => o.Value);
                var sorted = dictionary.OrderByDescending(x => x.Value).ToList();
                List<int> selected = new List<int>();
                int N = 50;
                for (int i = 0; i < N; i++)
                {
                    selected.Add(sorted[i].Key);
                }
                Image<Gray, byte> img4 =
                    img.Convert<byte>(delegate (byte b)
                    {
                        return selected.Contains((int)b) ? b : (byte)0;
                    });

                //int threshold = (int)((sorted[0].Key + sorted[1].Key) / 2);
                //var output = img.ThresholdBinary(new Gray(threshold), new Gray(255));
                pictureBox1.Image = output.ToBitmap();

                //pictureBox1.Image = CreateGraph(hist).GetImage();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private ZedGraphControl CreateGraph(Mat data)
        {
            int[] arr = new int[data.Rows];
            data.CopyTo(arr);
            ZedGraphControl zgc = new ZedGraphControl();

            PointPairList list = new PointPairList();
            int x = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                list.Add(arr[i], x);
                x++;
            }
            GraphPane pane = zgc.GraphPane;
            pane.CurveList.Clear();

            //BurlyWood Coral DarkMagenta
            //LineItem myCurve = pane.AddCurve("Гистограмма", list, Color.Coral, SymbolType.None);
            //Color color = Color.FromArgb(100, Color.Coral);
            //myCurve.Line.Fill = new ZedGraph.Fill(color);
            pane.AddCurve("Circle", list, Color.Crimson, SymbolType.Circle);
            //pane.AddBar("Ожидаемый результат", list2, Color.CadetBlue);
            pane.YAxis.Scale.Min = 0;
            pane.YAxis.Scale.Max = arr.Max();

            pane.XAxis.Scale.Min = 0;
            pane.XAxis.Scale.Max = 255;

            pane.Title.Text = "Graph";
            zgc.AxisChange();
            zgc.Invalidate();
            return zgc;
        }

        private void equalizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                    return;

                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Gray, byte>();
                Mat histeq = new Mat();

                CvInvoke.EqualizeHist(img, histeq);
                //CvInvoke.Imshow("Histogram Equalization", histeq);
                AddImage(histeq.ToImage<Bgr, byte>(), "Histogram Equalization");
                pictureBox1.Image = histeq.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void compareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    if (pictureBox1.Image == null)
            //        return;

            //    var img = new Bitmap(pictureBox1.Image)
            //        .ToImage<Gray, byte>();

            //    Image<Gray, byte> img1 = null;
            //    OpenFileDialog dialog = new OpenFileDialog();
            //    if (dialog.ShowDialog() == DialogResult.OK)
            //    {
            //        img1 = new Image<Gray, byte>(dialog.FileName);
            //    }

            //    Mat hist = new Mat();
            //    Mat hist1 = new Mat();

            //    float[] ranges = new float[] { 0, 256 };
            //    int[] channel = { 0 };
            //    int[] histSize = { 256 };

            //    VectorOfMat ms = new VectorOfMat();
            //    ms.Push(img);

            //    VectorOfMat ms1 = new VectorOfMat();
            //    ms1.Push(img1);


            //    CvInvoke.CalcHist(ms, channel, null, hist, histSize, ranges, false);
            //    CvInvoke.CalcHist(ms1, channel, null, hist1, histSize, ranges, false);

            //    CvInvoke.Normalize(hist, hist);
            //    CvInvoke.Normalize(hist1, hist1);

            //    HistogramViewer viewer = new HistogramViewer();
            //    viewer.Text = "Image Histogram";
            //    viewer.ShowIcon = false;
            //    viewer.HistogramCtrl..AddHistogram("Image1 Histogram", Color.Blue, hist, 256, ranges);
            //    viewer.HistogramCtrl.Refresh();
            //    viewer.Show();

            //    HistogramViewer viewer1 = new HistogramViewer();
            //    viewer1.Text = "Image Histogram";
            //    viewer1.ShowIcon = false;
            //    viewer1.HistogramCtrl.AddHistogram("Image2 Histogram", Color.Blue, hist1, 256, ranges);
            //    viewer1.HistogramCtrl.Refresh();
            //    viewer1.Show();


            //    var result1 = CvInvoke.CompareHist(hist, hist, Emgu.CV.CvEnum.HistogramCompMethod.Correl);
            //    var result2 = CvInvoke.CompareHist(hist1, hist1, Emgu.CV.CvEnum.HistogramCompMethod.Correl);
            //    var result3 = CvInvoke.CompareHist(hist, hist1, Emgu.CV.CvEnum.HistogramCompMethod.Correl);

            //    lblBGR.Text = "Hist vs Hist = " + result1.ToString() + "\n" +
            //        "Hist1 vs Hist1 = " + result2.ToString() + "\n" +
            //        "Hist vs Hist1 = " + result3.ToString() + "\n";

            //    //pictureBox1.Image = CreateGraph(hist).GetImage();

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

        private void scharrToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                float[,] data = {
                    { -3,  0, 3 },
                    { -10, 0, 10},
                    { -3,  0, 3 } };
                Emgu.CV.Matrix<float> SEx = new Emgu.CV.Matrix<float>(data);

                Emgu.CV.Matrix<float> SEy = SEx.Transpose();

                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Gray, float>();

                var Gx = new Mat();
                var Gy = new Mat();

                CvInvoke.Scharr(img, Gx, Emgu.CV.CvEnum.DepthType.Cv16S, 1, 0);
                CvInvoke.Scharr(img, Gy, Emgu.CV.CvEnum.DepthType.Cv16S, 0, 1);

                CvInvoke.ConvertScaleAbs(Gx, Gx, 0, 0);
                CvInvoke.ConvertScaleAbs(Gy, Gy, 0, 0);

                CvInvoke.Multiply(Gx, Gx, Gx);
                CvInvoke.Multiply(Gy, Gy, Gy);

                Gx.ConvertTo(Gx, Emgu.CV.CvEnum.DepthType.Cv32F);
                Gy.ConvertTo(Gy, Emgu.CV.CvEnum.DepthType.Cv32F);

                var M = new Mat(Gx.Size, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
                CvInvoke.Sqrt(Gx + Gy, M);
                var imgout = M.ToImage<Bgr, byte>();
                AddImage(imgout, "Scharr");

                pictureBox1.Image = imgout.ToBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void backProjectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                    return;

                if (rect == null) return;

                var imgScene = imgList["Input"].Clone();

                var imgObject = new Bitmap(pictureBox1.Image)
                    .ToImage<Gray, byte>();

                Mat histObject = new Mat();

                float[] ranges = new float[] { 0, 256 };
                int[] channel = { 0 };
                int[] histSize = { 256 };

                var msScene = new VectorOfMat();
                msScene.Push(imgScene);

                VectorOfMat msObject = new VectorOfMat();
                msObject.Push(imgObject);

                CvInvoke.CalcHist(msObject, channel, null, histObject, histSize, ranges, false);
                CvInvoke.Normalize(histObject, histObject, 0, 255, NormType.MinMax);

                Mat proj = new Mat();
                CvInvoke.CalcBackProject(msScene, channel, histObject, proj, ranges);

                var kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(5, 5), new Point(-1, -1));
                CvInvoke.Filter2D(proj, proj, kernel, new Point(-1, -1));

                var binary = proj.ToImage<Gray, byte>().ThresholdBinary(new Gray(50), new Gray(255)).Mat;
                var rgb = imgScene.CopyBlank();

                VectorOfMat vm = new VectorOfMat();
                vm.Push(binary);
                vm.Push(binary);
                vm.Push(binary);
                CvInvoke.Merge(vm, rgb.Mat);
                var output = new Mat();
                CvInvoke.BitwiseAnd(rgb, imgScene, output);
                //HistogramViewer viewer = new HistogramViewer();
                //viewer.Text = "Image Histogram";
                //viewer.ShowIcon = false;
                //viewer.HistogramCtrl.AddHistogram("Image1 Histogram", Color.Blue, histImg, 256, ranges);
                //viewer.HistogramCtrl.Refresh();
                //viewer.Show();

                //HistogramViewer viewer1 = new HistogramViewer();
                //viewer1.Text = "Image Histogram";
                //viewer1.ShowIcon = false;
                //viewer1.HistogramCtrl.AddHistogram("Image2 Histogram", Color.Blue, histTemp, 256, ranges);
                //viewer1.HistogramCtrl.Refresh();
                //viewer1.Show();



                pictureBox1.Image = output.ToBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void convexHullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Bgr, byte>();

                img = img.SmoothGaussian(3);
                var gray = img.Convert<Gray, byte>()
                    .ThresholdBinaryInv(new Gray(225), new Gray(255));
                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat hier = new Mat();

                CvInvoke.FindContours(gray, contours, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                int index = -1;
                double maxarea = -100;
                for (int i = 0; i < contours.Size; i++)
                {
                    double area = CvInvoke.ContourArea(contours[i]);
                    if (area > maxarea)
                    {
                        maxarea = area;
                        index = i;
                    }
                }

                if (index > -1)
                {
                    var biggestcontour = contours[index];
                    //Mat hull = new Mat();

                    VectorOfPoint hull = new VectorOfPoint();
                    CvInvoke.ConvexHull(biggestcontour, hull);

                    //CvInvoke.DrawContours(img, hull, -1, new MCvScalar(0, 0, 255), 3);
                    CvInvoke.Polylines(img, hull.ToArray(), true, new MCvScalar(0, 0.0, 255.0), 3);

                }
                pictureBox1.Image = img.AsBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void watershedSegmentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void logoDetectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        public static VectorOfPoint Process1(Image<Gray,byte> logo, Image<Gray, byte> observedImage)
        {
            VectorOfPoint vp = null;
            Mat homography = null;
           
            Mat mask;
            int k = 2;
            double uniquenessThreshold = 0.80;
            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            var uModelImage = logo.Clone();
            var uObservedImage = observedImage.Clone();
                    KAZE featureDetector = new KAZE();

                    VectorOfKeyPoint logoKeyPoints = new VectorOfKeyPoint();
                    VectorOfKeyPoint observedKeyPoints = new VectorOfKeyPoint();


                    //extract features from the object image
                    Mat modelDescriptors = new Mat();
                    featureDetector.DetectAndCompute(uModelImage, null, logoKeyPoints, modelDescriptors, false);


                    // extract features from the observed image
                    Mat observedDescriptors = new Mat();
                    featureDetector.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);

            // Bruteforce, slower but more accurate
            // You can use KDTree for faster matching with slight loss in accuracy
            //which algorithm to use

            Emgu.CV.Flann.KdTreeIndexParams ip1 =new Emgu.CV.Flann.KdTreeIndexParams(10);
            var index = new Emgu.CV.Flann.AutotunedIndexParams();
            Emgu.CV.Flann.LinearIndexParams ip = new Emgu.CV.Flann.LinearIndexParams();
            Emgu.CV.Flann.SearchParams sp = new SearchParams();
            
            Emgu.CV.Features2D.DescriptorMatcher matcher = new FlannBasedMatcher(index, sp);
                     
                matcher.Add(modelDescriptors);

                        matcher.KnnMatch(observedDescriptors, matches, k, null);
                        mask = new Mat(matches.Size, 1, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                        mask.SetTo(new MCvScalar(255));
                        Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                        int nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(logoKeyPoints, observedKeyPoints,
                            matches, mask, 1.5, 20);
                        if (nonZeroCount >= 4)
                        {
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(logoKeyPoints,
                            observedKeyPoints, matches, mask, 2);
                        }


                if (homography != null)
                {
                    //draw a rectangle along the projected model
                    Rectangle rect = new Rectangle(Point.Empty, logo.Size);
                    PointF[] pts = new PointF[]
                    {
                     new PointF(rect.Left, rect.Bottom),
                     new PointF(rect.Right, rect.Bottom),
                     new PointF(rect.Right, rect.Top),
                     new PointF(rect.Left, rect.Top)
                    };

                    pts = CvInvoke.PerspectiveTransform(pts, homography);
                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
                    vp = new VectorOfPoint(points);
                }
                return vp;
        }


        public static VectorOfPoint Process(Mat logo, Mat observedImage)
        {
            VectorOfPoint vp = null;
            Mat homography = null;
            VectorOfKeyPoint logoKeyPoints = new VectorOfKeyPoint();
            VectorOfKeyPoint observedKeyPoints = new VectorOfKeyPoint();
            Mat mask;
            int k = 2;
            double uniquenessThreshold = 0.80;

            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                using (UMat uModelImage = logo.GetUMat(AccessType.Read))
                using (UMat uObservedImage = observedImage.GetUMat(AccessType.Read))
                {
                    KAZE featureDetector = new KAZE();

                    //extract features from the object image
                    Mat modelDescriptors = new Mat();
                    featureDetector.DetectAndCompute(uModelImage, null, logoKeyPoints, modelDescriptors, false);


                    // extract features from the observed image
                    Mat observedDescriptors = new Mat();
                    featureDetector.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);
                    
                    // Bruteforce, slower but more accurate
                    // You can use KDTree for faster matching with slight loss in accuracy
                    using (Emgu.CV.Flann.LinearIndexParams ip = new Emgu.CV.Flann.LinearIndexParams())
                    using (Emgu.CV.Flann.SearchParams sp = new SearchParams())
                    using (Emgu.CV.Features2D.DescriptorMatcher matcher = new FlannBasedMatcher(ip, sp))
                    {
                        matcher.Add(modelDescriptors);

                        matcher.KnnMatch(observedDescriptors, matches, k, null);
                        mask = new Mat(matches.Size, 1, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                        mask.SetTo(new MCvScalar(255));
                        Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                        int nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(logoKeyPoints, observedKeyPoints,
                            matches, mask, 1.5, 20);
                        if (nonZeroCount >= 4)
                        {
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(logoKeyPoints,
                            observedKeyPoints, matches, mask, 2);
                        }
                    }

                }

                if (homography != null)
                {
                    //draw a rectangle along the projected model
                    Rectangle rect = new Rectangle(Point.Empty, logo.Size);
                    PointF[] pts = new PointF[]
                    {
                     new PointF(rect.Left, rect.Bottom),
                     new PointF(rect.Right, rect.Bottom),
                     new PointF(rect.Right, rect.Top),
                     new PointF(rect.Left, rect.Top)
                    };

                    pts = CvInvoke.PerspectiveTransform(pts, homography);
                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
                    vp = new VectorOfPoint(points);
                }
                return vp;
            }
        }

        public static VectorOfPoint ProcessORB(Image<Gray, byte> template, Image<Gray, byte> sceneImage)
        {
            VectorOfPoint vp = null;
            Mat homography = null;
            VectorOfKeyPoint logoKeyPoints = new VectorOfKeyPoint();
            VectorOfKeyPoint observedKeyPoints = new VectorOfKeyPoint();
            Mat mask;
            int k = 3;
            double uniquenessThreshold = 0.80;
            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();

            //using (UMat uModelImage = template.GetUMat(AccessType.Read))
            //using (UMat uObservedImage = sceneImage.GetUMat(AccessType.Read))
            //{
            Brisk featureDetector = new Brisk();

            //extract features from the object image
            Mat modelDescriptors = new Mat();
            featureDetector.DetectAndCompute(template, null, logoKeyPoints, modelDescriptors, false);


            // extract features from the observed image
            Mat observedDescriptors = new Mat();
            featureDetector.DetectAndCompute(sceneImage, null, observedKeyPoints, observedDescriptors, false);

            //DescriptorMatcher matcher = new BFMatcher(DistanceType.L2);
            BFMatcher matcher = new BFMatcher(DistanceType.Hamming);

            // Bruteforce, slower but more accurate
            // You can use KDTree for faster matching with slight loss in accuracy
            matcher.Add(modelDescriptors);
            matcher.KnnMatch(observedDescriptors, matches, k);
            mask = new Mat(matches.Size, 1, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
            mask.SetTo(new MCvScalar(255));

            Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

            // chekcs if the matched ddescriptor are not rotated and scaled more than the fixed boundaries
            int nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(logoKeyPoints, observedKeyPoints,
                matches, mask, 1.5, 20);
            if (nonZeroCount >= 4)
            {
                homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(logoKeyPoints,
                observedKeyPoints, matches, mask, 2);
            }

            //}

            if (homography != null)
            {
                //draw a rectangle along the projected model
                Rectangle rect = new Rectangle(Point.Empty, template.Size);
                PointF[] pts = new PointF[]
                {
                     new PointF(rect.Left, rect.Bottom),
                     new PointF(rect.Right, rect.Bottom),
                     new PointF(rect.Right, rect.Top),
                     new PointF(rect.Left, rect.Top)
                };

                pts = CvInvoke.PerspectiveTransform(pts, homography);
                Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
                vp = new VectorOfPoint(points);
            }
            return vp;

            /*
             In the next step found descriptors need to be matched between the current view
of the camera and the image of the tracked object. In order to do that the k-nearest
neighbors algorithm was used which is built in the Emgu CV library and is called
bruteForceMatcher.KnnMatch(). Simply it finds the k-nearest matches of the given
descriptor which of course could lead to some mismatches [11]. Nevertheless there
are some countermeasures, for example an Emgu CV function
VoteForSizeAndOrientation which is checking if the matched descriptors are not
rotated and scaled more than the fixed boundaries.
If everything is alright a homography matrix is build which is used to combine
the image of the tracked object and the current view of the camera by using the
corresponding key points. In order to do so is used a function called GetHomographyMatrixFromMatchedFeatures().
             */
        }

        //private static void FindMatch(Image<Gray, Byte> modelImage, Image<Gray, byte> observedImage)
        //{
        //    //https://csharp.hotexamples.com/examples/-/HomographyMatrix/-/php-homographymatrix-class-examples.html
        //    VectorOfKeyPoint modelKeyPoints = new VectorOfKeyPoint();
        //    VectorOfKeyPoint observedKeyPoints= new VectorOfKeyPoint();
        //    Mat modelDescriptors = new Mat();
        //    Mat observedDescriptors = new Mat();

        //    Matrix<int> indices = null;
        //    Matrix<byte> mask = null;
        //    Mat homography = null;

        //    double uniquenessThreshold = 0.8;
        //    int k = 2;


        //    Emgu.CV.Features2D.ORBDetector orb = new ORBDetector();
        //    orb.DetectAndCompute(modelImage, null, modelKeyPoints, modelDescriptors, false);
        //    orb.DetectAndCompute(observedImage, null, observedKeyPoints, observedDescriptors, false);
        //    BFMatcher matcher = new BFMatcher(DistanceType.L2);
        //    matcher.Add(modelDescriptors);
        //    //123
        //    indices = new Matrix<int>(observedDescriptors.Rows, k);
        //    using (Matrix<float> dist = new Matrix<float>(observedDescriptors.Rows, k))
        //    {
        //        matcher.KnnMatch(observedDescriptors, indices, dist, k, null);
        //        mask = new Matrix<byte>(dist.Rows, 1);
        //        mask.SetValue(255);
        //        Features2DToolbox.VoteForUniqueness(dist, uniquenessThreshold, mask);
        //    }

        //    int nonZeroCount = CvInvoke.cvCountNonZero(mask);
        //    if (nonZeroCount >= 4)
        //    {
        //        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints, indices, mask, 1.5, 20);
        //        if (nonZeroCount >= 4)
        //            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints, observedKeyPoints, indices, mask, 2);
        //    }

        //    watch.Stop();

        //    matchTime = watch.ElapsedMilliseconds;
        //}
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void selectROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                SelectROI = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void processToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }
                ProcessROI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void selectTemplateToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            saveTemplate = true;
            IsSelectingRectangle = true;
        }

        private void selectTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void resizeTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!imgList.ContainsKey("Template"))
                {
                    MessageBox.Show("Select a template first");
                    return;
                }

                var img = new Bitmap(pictureBox1.Image)
                        .ToImage<Bgr, byte>();

                img = img.Resize(1.25, Inter.Cubic);
                
                pictureBox1.Image = img.AsBitmap();
                AddImage(img, "Template Resized");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void rotateTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!imgList.ContainsKey("Template"))
                {
                    MessageBox.Show("Select a template first");
                    return;
                }

                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Bgr, byte>();

                img = img.Rotate(90, new Bgr(0,0,0),false);

                pictureBox1.Image = img.AsBitmap();
                AddImage(img, "Template Rotated");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void matchingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                if (rect == null)
                {
                    MessageBox.Show("Select a template");
                    return;
                }

                var imgScene = imgList["Input"].Clone();
                var template = new Bitmap(pictureBox1.Image)
                    .ToImage<Bgr, byte>();

                Mat imgout = new Mat();
                CvInvoke.MatchTemplate(imgScene, 
                    template, 
                    imgout, 
                    Emgu.CV.CvEnum.TemplateMatchingType.Sqdiff);

                double minVal = 0;
                double maxVal = 0;
                Point minLoc = new Point();
                Point maxLoc = new Point();

                CvInvoke.MinMaxLoc(imgout, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                Rectangle r = new Rectangle(minLoc, template.Size);
                CvInvoke.Rectangle(imgScene, r, new MCvScalar(255, 0, 0), 3);
                pictureBox1.Image = imgScene.AsBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void multiScaleTemplateMatchingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                if (rect == null)
                {
                    MessageBox.Show("Select a template");
                    return;
                }

                var imgScene = imgList["Input"].Clone();
                var template = new Bitmap(pictureBox1.Image)
                    .ToImage<Bgr, byte>();
                Rectangle r = Rectangle.Empty;
                double GlobalMin = float.MaxValue;

                for (float scale = 0.5f; scale <=2.0 ; scale+=0.5f)
                {
                    var temp = template.Resize(scale, Inter.Cubic);
                    Mat imgout = new Mat();
                    CvInvoke.MatchTemplate(imgScene,
                        temp,
                        imgout,
                        Emgu.CV.CvEnum.TemplateMatchingType.Sqdiff);

                    double minVal = 0;
                    double maxVal = 0;
                    Point minLoc = new Point();
                    Point maxLoc = new Point();

                    CvInvoke.MinMaxLoc(imgout, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
                    double prob = minVal / (imgout.ToImage<Gray,byte>().GetSum().Intensity);

                    if (prob < GlobalMin)
                    {
                        GlobalMin = prob;
                        r = new Rectangle(minLoc, temp.Size);
                    }

                    //CvInvoke.PutText(imgScene, prob.ToString("##############.##") + ":"+minVal.ToString() + ":" + scale.ToString(), minLoc, FontFace.HersheySimplex, .5, new MCvScalar(0, 0, 255));
                }
                if (r != null)
                {
                    CvInvoke.Rectangle(imgScene, r, new MCvScalar(255, 0, 0), 3);

                    pictureBox1.Image = imgScene.AsBitmap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void detectLogoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }
                if (!imgList.ContainsKey("Input"))
                    return;
                
                var img = imgList["Input"].Clone();
                var logo = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();

                var vp = Process1(logo.Convert<Gray, byte>(), img.Convert<Gray, byte>());
                //var vp = ProcessORB(logo.Convert<Gray, byte>(), img.Convert<Gray, byte>());


                if (vp != null)
                {
                    CvInvoke.Polylines(img, vp, true, new MCvScalar(0, 0, 255), 5);
                }
                pictureBox1.Image = img.AsBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void selectTemplateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveTemplate = true;
            IsSelectingRectangle = true;
        }

        private void addGaussianNoiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                var img = new Bitmap(pictureBox1.Image)
                   .ToImage<Bgr, byte>();
                Bgr avg = new Bgr();
                MCvScalar std = new MCvScalar();

                img.AvgSdv(out avg, out std);

                var output = img.CopyBlank();
                CvInvoke.Randn(output, avg.MCvScalar,std);

                var finaloutput = img.AddWeighted(output, .6, .4, 0);
                pictureBox1.Image = finaloutput.ToBitmap();
                AddImage(finaloutput, "Gaussian Noise");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            }

        private void logoDetectionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (capture == null)
                    return;
                int totalframes = int.Parse(capture.GetCaptureProperty(CapProp.FrameCount).ToString());
                if(totalframes>50)
                {
                    capture.SetCaptureProperty(CapProp.PosFrames, 50);
                    Mat m = capture.QueryFrame();
                    var img = m.ToImage<Bgr, byte>();

                    string logotext = "No logo Found";
                    string logopath1 = @"F:\AJ Data\img\videos\92newslogo.jpg";
                    string logopath2 = @"F:\AJ Data\img\videos\TRTworldlogo.jpg";
                    string logopath3 = @"F:\AJ Data\img\videos\TRTlogo.jpg";
                    var logo1 = new Image<Bgr, byte>(logopath1);
                    var logo2 = new Image<Bgr, byte>(logopath2);
                    var logo3 = new Image<Bgr, byte>(logopath3);

                    // vp = ProcessORB(logo1.Convert<Gray, byte>(), img.Convert<Gray, byte>());
                     vp = ProcessORB(logo2.Convert<Gray, byte>().Canny(100, 50), img.Convert<Gray, byte>().Canny(100, 50));
                     //vp = ProcessORB(logo3.Convert<Gray, byte>().Canny(100,50), img.Convert<Gray, byte>().Canny(100, 50));

                    //if (vp1 != null) vp = vp1;
                    //else if (vp2 != null) vp = vp2;
                    //else vp = vp3;
                    if (vp != null)
                    {
                        CvInvoke.Polylines(img, vp, true, new MCvScalar(0, 0, 255), 5);
                        pictureBox1.Image = img.AsBitmap();
                    }
                }
                Isplaying = true;
                PlayVideo();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void PlayVideo()
        {
            Mat m = null;
            while (Isplaying==true &&  (m= capture.QueryFrame())!=null)
            {
                
                if (vp != null)
                {
                    CvInvoke.Polylines(m, vp, true, new MCvScalar(0, 255, 0), 5);
                }
                pictureBox1.Image = m.ToBitmap();

                await Task.Delay(25);
                
            }
        }

        private void loadVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    capture = new VideoCapture(dialog.FileName);
                    if (capture == null) return;
                    Mat frame = new Mat();
                    capture.Read(frame);
                    pictureBox1.Image = frame.ToBitmap();
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (capture != null && (capture.IsOpened))
                {
                    //   capture.Pause();
                    Isplaying = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (capture != null && (capture.IsOpened))
                {
                    capture.Stop();
                    Isplaying = false;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (capture != null && capture.IsOpened && Isplaying==false)
                {
                    PlayVideo();
                    Isplaying = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void saveFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image!=null)
                {
                    SaveFileDialog dialog = new SaveFileDialog();
                    if (dialog.ShowDialog()==DialogResult.OK)
                    {
                        pictureBox1.Image.Save(dialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void saveLogoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image != null)
                {
                    if(rect!=null)
                    {
                        SaveFileDialog dialog = new SaveFileDialog();
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();
                            img.Save(dialog.FileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void sortObjectsBySizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                var img = new Bitmap(pictureBox1.Image)
                  .ToImage<Bgr, byte>();

                var output = img.CopyBlank();

                var gray = img.Convert<Gray, byte>()
                    .ThresholdBinary(new Gray(50), new Gray(255)).Not();

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat h = new Mat();

                Dictionary<int,double> tringles = new Dictionary<int, double>();

                CvInvoke.FindContours(gray, contours, h, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                VectorOfPoint aprox = new VectorOfPoint();
                for (int i = 0; i < contours.Size; i++)
                {
                    aprox.Clear();
                    double perimeter = CvInvoke.ArcLength(contours[i], true);
                    CvInvoke.ApproxPolyDP(contours[i], aprox, 0.04 * perimeter, true);
                    double area =  CvInvoke.ContourArea(contours[i]);

                    if(aprox.Size>6)
                    {
                        tringles.Add(i, area);
                    }
                }
                if (tringles.Count>0)
                {
                    var sorted = (from item in tringles
                                 orderby item.Value ascending
                                select item).ToList();

                    for (int i = 0; i < sorted.Count; i++)
                    {
                        CvInvoke.DrawContours(output, contours, sorted[i].Key, new MCvScalar(255, 255, 255), -1);
                        var moments = CvInvoke.Moments(contours[sorted[i].Key]);
                        int x = (int)(moments.M10 / moments.M00);
                        int y = (int)(moments.M01 / moments.M00);

                        CvInvoke.PutText(output, (i+1).ToString(), new Point(x, y), FontFace.HersheyPlain, 1.5, new MCvScalar(0, 0, 255), 2);
                        CvInvoke.PutText(output, sorted[i].Value.ToString(), new Point(x, y-30), FontFace.HersheyPlain, 1.5, new MCvScalar(0, 255, 0), 2);
                    }

                }
                pictureBox1.Image = output.AsBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void harrisCornerDetectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyHarisCorners();
                formParameter parameter = new formParameter();
                parameter.OnApplied += ApplyHarisCorners;
                parameter.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ApplyHarisCorners(int threshold =200)
        {
            try
            {
                if (imgList["Input"]== null)
                {
                    return;
                }

                var img = imgList["Input"].Clone();

                var gray = img.Convert<Gray, byte>();
                Mat output = new Mat();
                Mat output1 = new Mat();
                CvInvoke.CornerHarris(gray, output1, 2);

                CvInvoke.Normalize(output1, output, 255, 0, NormType.MinMax);
                Mat scaled = new Mat();
                CvInvoke.ConvertScaleAbs(output, scaled, 1, 0);


                Emgu.CV.Matrix<float> matrix = new Emgu.CV.Matrix<float>(output.Rows, output.Cols, output.NumberOfChannels);
                output.CopyTo(matrix);

               //var out1 = scaled.ToImage<Gray, float>().Convert(delegate (float b) 
               //{
               //    var c = b > threshold ? 255 : 0;
               //    return (byte)(c); 
               
               //});

                for (int rows = 0; rows < scaled.Rows; rows++)
                {
                    for (int cols = 0; cols < scaled.Cols; cols++)
                    {
                        if (matrix[rows, cols] > threshold)
                        {
                            CvInvoke.Circle(img, new Point(cols, rows), 5, new MCvScalar(0, 0, 255), 1);
                        }
                    }
                }
                pictureBox1.Image = img.AsBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void harrisFeatureDetectionAndComputingToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        
        private void shiTomasiToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }
        

        private void computeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyHarisShiTomasi();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ApplyHarisShiTomasi()
        {
            try
            {
                if (imgList["Input"] == null)
                {
                    return;
                }

                var img = imgList["Input"].Clone();
                var gray = img.Convert<Gray, byte>();

                GFTTDetector detector = new GFTTDetector();
                var corners = detector.Detect(gray);

                Mat outimg = new Mat();
                Features2DToolbox.DrawKeypoints(img, new VectorOfKeyPoint(corners), outimg, new Bgr(0, 0, 255));
               

                pictureBox1.Image = outimg.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void detectComputeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                HarisShiTomasiDetectansCompute();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void HarisShiTomasiDetectansCompute()
        {
            try
            {
                if (imgList["Input"] == null)
                {
                    return;
                }

                if (pictureBox1.Image == null)
                {
                    return;
                }


                var img = imgList["Input"].Clone();

                var imgScene = img.Convert<Gray, byte>();
                var imgModel = new Bitmap(pictureBox1.Image)
                    .ToImage<Gray, byte>();


                //Shi- Tomasi
                GFTTDetector detector = new GFTTDetector();
                var cornersScene = detector.Detect(imgScene);
                var cornersModel = detector.Detect(imgModel);

                //detect keypoints
                VectorOfKeyPoint keyPointScene = new VectorOfKeyPoint(cornersScene);
                VectorOfKeyPoint keyPointModel = new VectorOfKeyPoint(cornersModel);

                Mat modelDescriptors = new Mat();
                Mat sceneDescriptors = new Mat();

                VectorOfPoint vp = null;
                Mat homography = null;
                Mat mask;
                int k = 3;
                double uniquenessThreshold = 0.80;
                VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();

                //using (UMat uModelImage = template.GetUMat(AccessType.Read))
                //using (UMat uObservedImage = sceneImage.GetUMat(AccessType.Read))
                //{
                //BriefDescriptorExtractor detector = new BriefDescriptorExtractor();
                //AKAZE detector = new AKAZE();
                //ORBDetector detector = new ORBDetector();
                Brisk featureDetector = new Brisk();
                //Freak detector = new Freak();

                //description
                featureDetector.DetectAndCompute(imgScene, null, keyPointScene, sceneDescriptors, true);
                featureDetector.DetectAndCompute(imgModel, null, keyPointModel, modelDescriptors, true);

                // You can use KDTree for faster matching with slight loss in accuracy
                LinearIndexParams ip = new LinearIndexParams();
                //SearchParams sp = new SearchParams();
                //var ip = new Emgu.CV.Flann.KdTreeIndexParams();
                //var sp = new SearchParams();
                //FlannBasedMatcher matcher = new FlannBasedMatcher(ip, sp);


                Emgu.CV.Matrix<float> mmodelDescriptors = new Emgu.CV.Matrix<float>(modelDescriptors.Size);
                modelDescriptors.CopyTo(mmodelDescriptors);

                Emgu.CV.Matrix<float> msceneDescriptors = new Emgu.CV.Matrix<float>(sceneDescriptors.Size);
                modelDescriptors.CopyTo(msceneDescriptors);

                //var ip = new Emgu.CV.Flann.KdTreeIndexParams();
                //var sp = new SearchParams();
                //FlannBasedMatcher matcher = new FlannBasedMatcher(ip, sp);

                var indices = new Emgu.CV.Matrix<int>(sceneDescriptors.Rows, k);
                var flannMatcher = new Index(modelDescriptors, ip);


                var dist = new Emgu.CV.Matrix<float>(sceneDescriptors.Rows, k);
                flannMatcher.KnnSearch(sceneDescriptors, indices, dist, k);


                //matcher.Add(mmodelDescriptors);
                //matcher.KnnMatch(msceneDescriptors, matches, k, null);
                
                mask = new Mat(matches.Size, 1, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(255));
                Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                int nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(keyPointModel, keyPointScene,
                    matches, mask, 1.5, 20);
                if (nonZeroCount >= 4)
                {
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(keyPointModel,
                    keyPointScene, matches, mask, 2);
                }

                if (homography != null)
                {
                    //draw a rectangle along the projected model
                    Rectangle rect = new Rectangle(Point.Empty, imgModel.Size);
                    PointF[] pts = new PointF[]
                    {
                     new PointF(rect.Left, rect.Bottom),
                     new PointF(rect.Right, rect.Bottom),
                     new PointF(rect.Right, rect.Top),
                     new PointF(rect.Left, rect.Top)
                    };

                    pts = CvInvoke.PerspectiveTransform(pts, homography);
                    var points = pts.Select(a => new Point((int)a.X, (int)a.Y)).ToArray();
                    //Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
                    //vp = new VectorOfPoint(points);
                    CvInvoke.Polylines(img, points, true, new MCvScalar(0, 0, 255), 2);
                }

                pictureBox1.Image = img.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void detectAndComputeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HarisCornersComputeandDetect();
            formParameter parameter = new formParameter();
            parameter.OnApplied += ApplyHarisCorners;
            parameter.ShowDialog();
        }
        private void HarisCornersComputeandDetect(int threshold = 200)
        {
            try
            {
                if (imgList["Input"] == null)
                {
                    return;
                }

                var img = imgList["Input"].Clone();

                var gray = img.Convert<Gray, byte>();
                Mat output = new Mat();
                Mat output1 = new Mat();
                CvInvoke.CornerHarris(gray, output1, 2);

                CvInvoke.Normalize(output1, output, 255, 0, NormType.MinMax);
                Mat scaled = new Mat();
                CvInvoke.ConvertScaleAbs(output, scaled, 1, 0);


                Emgu.CV.Matrix<float> matrix = new Emgu.CV.Matrix<float>(output.Rows, output.Cols, output.NumberOfChannels);
                output.CopyTo(matrix);

                // matrix.Convert<.SetValue(0,matrix.)

                var out1 = scaled.ToImage<Gray, float>().Convert(delegate (float b)
                {
                    var c = b > threshold ? 255 : 0;
                    return (byte)(c);

                });

                for (int rows = 0; rows < scaled.Rows; rows++)
                {
                    for (int cols = 0; cols < scaled.Cols; cols++)
                    {
                        if (matrix[rows, cols] > threshold)
                        {
                            CvInvoke.Circle(img, new Point(cols, rows), 5, new MCvScalar(0, 0, 255), 2);
                        }
                    }
                }
                pictureBox1.Image = img.AsBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void harrisCornerDetectorToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ApplyHarisCorners();
            formParameter parameter = new formParameter();
            parameter.OnApplied += ApplyHarisCorners;
            parameter.ShowDialog();
        }

        private void computeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyFAST();
                formParameter parameter = new formParameter(1,15);
                parameter.OnApplied += ApplyFAST;
                parameter.ShowDialog();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void ApplyFAST(int threshold=10)
        {
            try
            {
                if (imgList["Input"] == null)
                {
                    return;
                }

                var img = imgList["Input"].Clone();
                var gray = img.Convert<Gray, byte>();

                FastFeatureDetector detector = new FastFeatureDetector(threshold);
                var corners = detector.Detect(gray);

                Mat outimg = new Mat();
                Features2DToolbox.DrawKeypoints(img, new VectorOfKeyPoint(corners), outimg, new Bgr(0, 0, 255));


                pictureBox1.Image = outimg.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void computeToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyBRIEF();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void ApplyBRIEF(int threshold = 32)
        {
            // not implemented
            try
            {
                if (imgList["Input"] == null)
                {
                    return;
                }

                var img = imgList["Input"].Clone();
                var gray = img.Convert<Gray, byte>();
                // 16 32 or 64
                BriefDescriptorExtractor detector = new BriefDescriptorExtractor(threshold);
                var corners = detector.Detect(gray);

                Mat outimg = new Mat();
                Features2DToolbox.DrawKeypoints(img, new VectorOfKeyPoint(corners), outimg, new Bgr(0, 0, 255));


                pictureBox1.Image = outimg.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void computeToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            ApplyORB(1000);
        }
        private void ApplyORB(int threshold = 500)
        {
            // not implemented
            try
            {
                if (imgList["Input"] == null)
                {
                    return;
                }

                var img = imgList["Input"].Clone();
                var gray = img.Convert<Gray, byte>();
                // 16 32 or 64
                ORBDetector detector = new ORBDetector();
                var corners = detector.Detect(gray);

                Mat outimg = new Mat();
                Features2DToolbox.DrawKeypoints(img, new VectorOfKeyPoint(corners), outimg, new Bgr(0, 0, 255));


                pictureBox1.Image = outimg.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void computeToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            try
            {

                ApplyAgastFeatureDetector();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ApplyAgastFeatureDetector()
        {
            // not implemented
            try
            {
                if (imgList["Input"] == null)
                {
                    return;
                }

                var img = imgList["Input"].Clone();
                var gray = img.Convert<Gray, byte>();
                // 16 32 or 64
                AgastFeatureDetector detector = new AgastFeatureDetector();
                var corners = detector.Detect(gray);

                Mat outimg = new Mat();
                Features2DToolbox.DrawKeypoints(img, new VectorOfKeyPoint(corners), outimg, new Bgr(0, 0, 255));


                pictureBox1.Image = outimg.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ApplyMSER()
        {
            // not implemented
            try
            {
                if (imgList["Input"] == null)
                {
                    return;
                }

                var img = imgList["Input"].Clone();
                var gray = img.Convert<Gray, byte>();
                // 16 32 or 64
                MSERDetector detector = new MSERDetector();
                var corners = detector.Detect(gray);

                Mat outimg = new Mat();
                Features2DToolbox.DrawKeypoints(img, new VectorOfKeyPoint(corners), outimg, new Bgr(0, 0, 255));


                pictureBox1.Image = outimg.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void computeToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            ApplyMSER();
        }

        private void computeToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            ApplySimpleBlobDetector();
        }
        private void ApplySimpleBlobDetector()
        {
            // not implemented
            try
            {
                if (imgList["Input"] == null)
                {
                    return;
                }

                var img = imgList["Input"].Clone();
                var gray = img.Convert<Gray, byte>();
                // 16 32 or 64
                
                SimpleBlobDetector detector = new SimpleBlobDetector();
                var corners = detector.Detect(gray);

                Mat outimg = new Mat();
                Features2DToolbox.DrawKeypoints(img, new VectorOfKeyPoint(corners), outimg, new Bgr(0, 0, 255));


                pictureBox1.Image = outimg.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void greenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                var img = new Bitmap(pictureBox1.Image)
                  .ToImage<Bgr, byte>();

                int tolerance = 50;

                img = img.SmoothGaussian(5);
                Bgr lower = new Bgr(0, 255 - 150, 0);
                Bgr higher = new Bgr(100, 255, tolerance);
                var mask =  img.InRange(lower, higher).Not();
                var output = img.Clone();
                output.SetValue(new Bgr(0, 0, 0), mask);
                pictureBox1.Image = output.AsBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void redToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                var img = new Bitmap(pictureBox1.Image)
                  .ToImage<Bgr, byte>();

                int tolerance = 50;

                Bgr lower = new Bgr(0, 0, 255 - 100);
                Bgr higher = new Bgr(tolerance, tolerance, 255);
                var mask = img.InRange(lower, higher).Not();
                var output = img.Clone();
                output.SetValue(new Bgr(0, 0, 0), mask);
                pictureBox1.Image = output.AsBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private List<string> OMR(string []files )
        {
            System.IO.StreamWriter file = null;
            List<string> list = new List<string>();
            try
            {
                //string path = @"F:\AJ Data\Projects\emgucv\Exam Marking\data\3.jpg";
                //file = new System.IO.StreamWriter(@"F:\AJ Data\Projects\emgucv\Exam Marking\test\output.txt", true);

                foreach (string path in files)
                {
                    var img = new Image<Bgr, byte>(path);
                    img = img.Rotate(5, new Bgr(255, 255, 255), false);
                    //AddImage(img, "Input");

                    var imgAligned = AlignDocument(img);
                    //AddImage(imgAligned, "Aligned");

                    var gray = imgAligned.Convert<Gray, byte>();
                    var mask = gray.Sub(gray.SmoothGaussian(5));
                    gray = gray.Add(mask).Not().SmoothGaussian(5)
                        .ThresholdBinary(new Gray(20), new Gray(255)).Dilate(2).Erode(2);


                    VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                    Mat h = new Mat();
                    CvInvoke.FindContours(gray, contours, h, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                    if (contours.Size < 0)
                    {
                        return null;
                    }
                    List<KeyValuePair<int, double>> sortedContours = new List<KeyValuePair<int, double>>();

                    for (int i = 0; i < contours.Size; i++)
                    {
                        var area = CvInvoke.ContourArea(contours[i]);
                        sortedContours.Add(new KeyValuePair<int, double>(i, area));
                    }

                    sortedContours = sortedContours.OrderByDescending(x => x.Value).ToList();
                    sortedContours.RemoveRange(2, 1);

                    // extract Roll No.
                    var bboxRollNo = CvInvoke.BoundingRectangle(contours[sortedContours[1].Key]);
                    int TotalRowsRollNo = bboxRollNo.Height / 11;
                    bboxRollNo.Height = bboxRollNo.Height - TotalRowsRollNo;
                    bboxRollNo.Y = bboxRollNo.Y + TotalRowsRollNo;

                    //Process the Roll No extraction
                    imgAligned.ROI = bboxRollNo;
                    var imgRollNo = imgAligned.Copy();
                    imgAligned.ROI = Rectangle.Empty;
                    var grayRollNo = imgRollNo.Convert<Gray, byte>().ThresholdBinaryInv(new Gray(200), new Gray(255));

                    int RollNoColsInSheet = 9;
                    int RollNoRowsInSheet = 10;
                    int RollNOTotalRows = grayRollNo.Rows / RollNoRowsInSheet;
                    int RollTotalCols = grayRollNo.Cols / RollNoColsInSheet;
                    //int Rollchoicewidth = RollTotalCols / RollNoColsInSheet;
                    float RollNoThreshold = 0.40f;
                    string separator = ",";

                    string info = "Id  ";
                    for (int RNCols = 0; RNCols < 9 * RollTotalCols; RNCols += RollTotalCols)
                    {

                        int index = 0;
                        List<KeyValuePair<int, float>> RNPercentage = new List<KeyValuePair<int, float>>();
                        for (int RNRows = 0; RNRows < 10 * RollNOTotalRows; RNRows += RollNOTotalRows)
                        {
                            var rnrect = new Rectangle(RNCols, RNRows, RollNOTotalRows, RollTotalCols);
                            CvInvoke.Rectangle(imgRollNo, rnrect, new MCvScalar(255, 0, 0), 2);
                            grayRollNo.ROI = rnrect;
                            var imgChoice = grayRollNo.Copy();
                            grayRollNo.ROI = Rectangle.Empty;


                            var nonzero = imgChoice.CountNonzero();
                            float fillpercentage = nonzero[0] / (float)(imgChoice.Width * imgChoice.Height);
                            RNPercentage.Add(new KeyValuePair<int, float>(index++, fillpercentage));
                        }
                        if (RNPercentage.Count > 0)
                        {
                            var selected = (from cc in RNPercentage
                                            where cc.Value >= RollNoThreshold
                                            select cc).ToList();
                            //1 choice has been made
                            if (selected != null && selected.Count == 1)
                            {
                                info += selected[0].Key;
                            }
                            else
                            {
                                info += "*";
                            }
                        }
                    }
                    info += separator;

                    // seprate the exam's top row
                    var bboxAnswer = CvInvoke.BoundingRectangle(contours[sortedContours[0].Key]);
                    int TotalRows = bboxAnswer.Height / 31;
                    bboxAnswer.Height = bboxAnswer.Height - TotalRows;
                    bboxAnswer.Y = bboxAnswer.Y + TotalRows;
                    // process exam part
                    imgAligned.ROI = bboxAnswer;
                    var imgAnswer = imgAligned.Copy();
                    imgAligned.ROI = Rectangle.Empty;
                    var grayAnswer = imgAnswer.Convert<Gray, byte>().ThresholdBinaryInv(new Gray(200), new Gray(255));

                    int ExamColsInSheet = 4;
                    int ExamRowsInSheet = 30;
                    int ExamAnswerChoiceCols = 6;
                    TotalRows = grayAnswer.Rows / ExamRowsInSheet;
                    int TotalCols = grayAnswer.Cols / ExamColsInSheet;
                    int choicewidth = TotalCols / ExamAnswerChoiceCols;
                    int questionIndex = 0;
                    float threshold = 0.45f;

                    int colIndex = 0;

                    for (int answersCols = 0; answersCols < 4; answersCols++)
                    {
                        int rowIndex = 0;

                        for (int answersRows = 0; answersRows < 30; answersRows++)
                        {
                            int choiceColIndex = colIndex;
                            questionIndex++;
                            char choicePosition = 'A';
                            List<KeyValuePair<char, float>> choicePercentage = new List<KeyValuePair<char, float>>();
                            for (int k = 0; k < 6; k++)
                            {
                                if (k == 0) continue;
                                var rect2 = new Rectangle(choiceColIndex + choicewidth, rowIndex, choicewidth, choicewidth);
                                CvInvoke.Rectangle(imgAnswer, rect2, new MCvScalar(255, 0, 0), 2);

                                grayAnswer.ROI = rect2;
                                var imgChoice = grayAnswer.Copy();
                                grayAnswer.ROI = Rectangle.Empty;


                                var nonzero = imgChoice.CountNonzero();
                                float fillpercentage = nonzero[0] / (float)(imgChoice.Width * imgChoice.Height);
                                choicePercentage.Add(new KeyValuePair<char, float>(choicePosition, fillpercentage));

                                choicePosition++;
                                choiceColIndex += choicewidth;
                            }
                            if (choicePercentage.Count > 0)
                            {
                                var selected = (from cc in choicePercentage
                                                where cc.Value >= threshold
                                                select cc).ToList();
                                info += questionIndex.ToString();


                                //1 choice has been made
                                if (selected != null && selected.Count == 1)
                                {
                                    info += " " + selected[0].Key;
                                }
                                else
                                {
                                    info += " *";
                                }
                                info += separator;
                                //file.Write(msg);
                                //file.Write(separator);

                            }
                            rowIndex += TotalRows;

                        }

                        colIndex += TotalCols;
                    }
                    info = info.Remove(info.LastIndexOf(','));
                    list.Add(info);
                    //file.Write(info);
                    //file.WriteLine("");
                }
                //file.Close();
                //pictureBox1.Image = imgAnswer.ToBitmap();
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private void examMarkingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = true;
                dialog.Title = "Select Folder to read exam sheets";
                dialog.Filter = "Image Files (*.jpg) |*.jpg";
                if (dialog.ShowDialog()==DialogResult.OK)
                {
                    var list = OMR(dialog.FileNames);
                    if (list!=null)
                    {
                        SaveFileDialog saveFile = new SaveFileDialog();
                        if (saveFile.ShowDialog()==DialogResult.OK)
                        {
                            var file = new System.IO.StreamWriter(saveFile.FileName, false);
                            foreach (var item in list)
                            {
                                file.WriteLine(item);
                            }
                            file.Flush();
                            file.Close();
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Image<Bgr, byte> AlignDocument(Image<Bgr, byte> img)
        {
            try
            {
                //img = img.Rotate(10, new Bgr(255, 255, 255), false);
                var gray = img.Convert<Gray, byte>();
                var mask = gray.Sub(gray.SmoothGaussian(5));
                gray = gray.Add(mask).Not().SmoothGaussian(5)
                    .ThresholdBinary(new Gray(20), new Gray(255)).Dilate(2).Erode(2);


                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat h = new Mat();
                CvInvoke.FindContours(gray, contours, h, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                if (contours.Size < 0)
                {
                    throw new Exception("No contours found.");
                }
                int biggestContourIndex = -1;
                double maxArea = -100;
                for (int i = 0; i < contours.Size; i++)
                {
                    var area = CvInvoke.ContourArea(contours[i]);
                    if (area > maxArea)
                    {
                        maxArea = area;
                        biggestContourIndex = i;
                    }
                }

                var biggestContour = contours[biggestContourIndex];

                var minAreaRectangle = CvInvoke.MinAreaRect(biggestContour);
                //var rect = CvInvoke.FitEllipse(biggestContour);
                //var vertices = CvInvoke.BoxPoints(minAreaRectangle);
                var bbAnswers = CvInvoke.BoundingRectangle(contours[biggestContourIndex]);
                /*
# from https://www.pyimagesearch.com/2017/02/20/text-skew-correction-opencv-python/
# the `cv2.minAreaRect` function returns values in the
# range [-90, 0); as the rectangle rotates clockwise the
# returned angle trends to 0 -- in this special case we
# need to add 90 degrees to the angle
                if angle < -45:
    angle = -(90 + angle)

# otherwise, just take the inverse of the angle to make
# it positive
else:
    angle = -angle
                    */
                double angle = minAreaRectangle.Angle;
                if (angle < -45) angle = -(90 + angle);
                else angle = -angle;

                return img.Rotate(angle, new Bgr(255, 255, 255), false);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void multipleObjectDetectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                if (rect == null)
                {
                    MessageBox.Show("Select a template");
                    return;
                }
                ApplyMultipleObjectDetectionTempalteMatching(0.01f);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ApplyMultipleObjectDetectionTempalteMatching(float threshold=0.1f)
        {
            try
            {
                var imgScene = imgList["Input"].Clone();
                var template = new Bitmap(pictureBox1.Image)
                    .ToImage<Bgr, byte>();

                Mat imgout = new Mat();
                CvInvoke.MatchTemplate(imgScene,
                    template,
                    imgout,
                    Emgu.CV.CvEnum.TemplateMatchingType.Sqdiff);

                
                Mat mm = new Mat();
                CvInvoke.Normalize(imgout, mm, 0, 1, NormType.MinMax,DepthType.Cv64F);
                Emgu.CV.Matrix<double> matches = new Emgu.CV.Matrix<double>(imgout.Size);
                mm.CopyTo(matches);

                double minVal = 0;
                double maxVal = 0;
                Point minLoc = new Point();
                Point maxLoc = new Point();
                do
                {
                    CvInvoke.MinMaxLoc(matches, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
                    matches[minLoc.Y, minLoc.X] = 0.5;
                    matches[maxLoc.Y, maxLoc.X] = 0.5;
                    Rectangle r = new Rectangle(minLoc, template.Size);
                    CvInvoke.Rectangle(imgScene, r, new MCvScalar(255, 0, 0), 3);


                } while (minVal <= threshold);
                pictureBox1.Image = imgScene.AsBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void facemarkAAMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FacemarkAAMParams parameters = new FacemarkAAMParams();
                parameters.M = 1;
                parameters.NIter = 100;
                FacemarkAAM facemarkAAM = new FacemarkAAM(parameters);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void landmarkDetectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    // The contents of this file are in the public domain. See LICENSE_FOR_EXAMPLE_PROGRAMS.txt
            //    // http://dlib.net/face_landmark_detection_ex.cpp.html
            //    /*

            //        This example program shows how to find frontal human faces in an image and
            //        estimate their pose.  The pose takes the form of 68 landmarks.  These are
            //        points on the face such as the corners of the mouth, along the eyebrows, on
            //        the eyes, and so forth.  



            //        The face detector we use is made using the classic Histogram of Oriented
            //        Gradients (HOG) feature combined with a linear classifier, an image pyramid,
            //        and sliding window detection scheme.  The pose estimator was created by
            //        using dlib's implementation of the paper:
            //           One Millisecond Face Alignment with an Ensemble of Regression Trees by
            //           Vahid Kazemi and Josephine Sullivan, CVPR 2014
            //        and was trained on the iBUG 300-W face landmark dataset (see
            //        https://ibug.doc.ic.ac.uk/resources/facial-point-annotations/):  
            //           C. Sagonas, E. Antonakos, G, Tzimiropoulos, S. Zafeiriou, M. Pantic. 
            //           300 faces In-the-wild challenge: Database and results. 
            //           Image and Vision Computing (IMAVIS), Special Issue on Facial Landmark Localisation "In-The-Wild". 2016.
            //        You can get the trained model file from:
            //        http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2.
            //        Note that the license for the iBUG 300-W dataset excludes commercial use.
            //        So you should contact Imperial College London to find out if it's OK for
            //        you to use this model file in a commercial product.


            //        Also, note that you can train your own models using dlib's machine learning
            //        tools.  See train_shape_predictor_ex.cpp to see an example.




            //        Finally, note that the face detector is fastest when compiled with at least
            //        SSE2 instructions enabled.  So if you are using a PC with an Intel or AMD
            //        chip then you should enable at least SSE2 instructions.  If you are using
            //        cmake to compile this program you can enable them by using one of the
            //        following commands when you create the build project:
            //            cmake path_to_dlib_root/examples -DUSE_SSE2_INSTRUCTIONS=ON
            //            cmake path_to_dlib_root/examples -DUSE_SSE4_INSTRUCTIONS=ON
            //            cmake path_to_dlib_root/examples -DUSE_AVX_INSTRUCTIONS=ON
            //        This will set the appropriate compiler options for GCC, clang, Visual
            //        Studio, or the Intel compiler.  If you are using another compiler then you
            //        need to consult your compiler's manual to determine how to enable these
            //        instructions.  Note that AVX is the fastest but requires a CPU from at least
            //        2011.  SSE4 is the next fastest and is supported by most current machines.  

            //    // Make the image larger so we can detect small faces.
            //pyramid_up(img);
            //    */
            //    //http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2
            //    if (pictureBox1.Image == null) return;
            //    Image<Bgr, byte> img1 = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();
            //    var img = img1.Convert<Gray, byte>().ToBitmap().ToArray2D<byte>();
            //    var detector = Dlib.GetFrontalFaceDetector();
                
            //    var faces = detector.Operator(img);

            //    foreach (var face in faces)
            //    {
            //        Dlib.DrawRectangle(img, face,255,2);
            //    }
            //    string rootDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
            //    string path = rootDirectory + "/data/shape_predictor_68_face_landmarks.dat";
            //    var sp = ShapePredictor.Deserialize(path);

            //    var shapes = new List<FullObjectDetection>();
            //    foreach (var rect in faces)
            //    {
            //        var shape = sp.Detect(img, rect);
            //        for (uint i = 0; i < shape.Parts; i++)
            //        {
            //            CvInvoke.Circle(img1, new Point(shape.GetPart(i).X, shape.GetPart(i).Y), 1, new MCvScalar(0, 0, 255), 2);
            //        }
            //    }
            //    pictureBox1.Image = img1.ToBitmap();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

        //public Array2D<RgbPixel> LoadFromSoftwareBitmap(Bitmap Bitmap)
        //{
        //    int BufferSize = (Bitmap.Height * Bitmap.Width * 4);
        //    byte[] DlibImageArray = new byte[BufferSize];
        //    MemoryStream buffer = new MemoryStream(BufferSize);
        //    Bitmap.Save(buffer,Bitmap.RawFormat);
        //    //using (var Reader = new StreamReader(buffer))
        //    //{
        //    //    Reader.re(DlibImageArray);
        //    //}
        //    return Dlib.LoadImageData<RgbPixel>(ImagePixelFormat.Bgra, DlibImageArray, (uint)Bitmap.PixelHeight, (uint)Bitmap.PixelWidth, (uint)Bitmap.PixelWidth * 4);
        //}

        private void landmarkDetectionEmguCVToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // face detector: download https://raw.githubusercontent.com/opencv/opencv/master/data/lbpcascades/lbpcascade_frontalface_improved.xml
            // model: https://raw.githubusercontent.com/kurnianggoro/GSOC2017/master/data/lbfmodel.yaml
            // theory https://www.learnopencv.com/facemark-facial-landmark-detection-using-opencv/
            try
            {
                if (pictureBox1.Image==null)
                {
                    return;
                }
                string rootDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
                string cascadePath = rootDirectory + "/data/lbpcascade_frontalface_improved.xml";
                string modelpath = rootDirectory + "/data/lbfmodel.yaml";
                CascadeClassifier classifier = new CascadeClassifier(cascadePath);

                var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();
                var imgGray = img.Convert<Gray, byte>();
                var faces = classifier.DetectMultiScale(imgGray);

                VectorOfRect rect = new VectorOfRect(faces);

                var  modelParams= new FacemarkLBFParams();
                FacemarkLBF facemarkDetector= new FacemarkLBF(modelParams);
                
                facemarkDetector.LoadModel(modelpath);

                VectorOfVectorOfPointF landmarks = new VectorOfVectorOfPointF();
                bool result = facemarkDetector.Fit(imgGray, rect, landmarks);

               
                if (result)
                {
                    for (int i = 0; i < faces.Length; i++)
                    {
                        CvInvoke.Rectangle(img, faces[i], new MCvScalar(0, 0, 255), 2);
                        FaceInvoke.DrawFacemarks(img, landmarks[i], new MCvScalar(0, 0, 255));
                        var rec = CvInvoke.BoundingRectangle(landmarks[i]);
                    }
                }
                
                pictureBox1.Image = img.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void trainingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //https://docs.opencv.org/master/d5/dd8/tutorial_facemark_aam.html
            /*
             * creating the instance of FacemarkAAM
training the AAM model
Fitting using FacemarkAAM
*/
            try
            {
                FacemarkAAMParams parameters = new FacemarkAAMParams();
                parameters.M = 1;
                parameters.ModelFile= "AAM.yaml";
                FacemarkAAM facemarkAAM = new FacemarkAAM(parameters);
                VectorOfVectorOfPointF landmarks = new VectorOfVectorOfPointF();

                string rootDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
                string cascadePath = rootDirectory + "/data/lbpcascade_frontalface_improved.xml";
                CascadeClassifier classifier = new CascadeClassifier(cascadePath);

                string rootfolder = @"C:\Users\akhtar.jamil\Downloads\lfpw\";

                VectorOfCvString images_train = new VectorOfCvString(Directory.GetFiles(rootfolder+ "/trainset", "*.png"));
                VectorOfCvString landmarks_train = new VectorOfCvString(Directory.GetFiles(rootfolder + "/trainset", "*.pts"));

                for (int i = 0; i < images_train.Size; i++)
                {
                    var img = new Image<Bgr, byte>(images_train[0].ToString()).Resize(300, 300, Inter.Cubic);
                    List<string> lines = File.ReadAllLines(landmarks_train[i].ToString())
                        .ToList<string>();
                    lines.RemoveRange(0, 3);
                    lines.RemoveAt(lines.Count - 1);

                    var lp = lines.ConvertAll(new Converter<string, Point>(StringToPoint)).ToArray();
                    VectorOfPoint lm = new VectorOfPoint(lp);

                    var imgGray = img.Convert<Gray, byte>();
                    var faces = classifier.DetectMultiScale(imgGray);

                    VectorOfRect rect = new VectorOfRect(faces);
                    VectorOfVectorOfPointF points = new VectorOfVectorOfPointF();
                    facemarkAAM.Fit(img, rect, points);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static Point StringToPoint(string pf)
        {
            var points =  pf.Split(' ');
            return new Point((int)float.Parse(points[0],new CultureInfo("en-US")), (int)float.Parse(points[1], new CultureInfo("en-US")));
        }
        private void preprocessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = "Select Location of Training and Testing Folders";
                if (dialog.ShowDialog()==DialogResult.OK)
                {
                    File.WriteAllLines(dialog.SelectedPath  + "/images_train.txt", Directory.GetFiles(dialog.SelectedPath + "/trainset", "*.png"));
                    File.WriteAllLines(dialog.SelectedPath  + "/annotation_train.txt", Directory.GetFiles(dialog.SelectedPath + "/trainset", "*.pts"));
                    File.WriteAllLines(dialog.SelectedPath  + "/images_test.txt", Directory.GetFiles(dialog.SelectedPath + "/testset", "*.png"));
                    File.WriteAllLines(dialog.SelectedPath  + "/annotation_test.txt", Directory.GetFiles(dialog.SelectedPath + "/testset", "*.pts"));
                    MessageBox.Show("Text Files generated at following location\n" + dialog.SelectedPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void facemarkAAMToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void fisherFaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                VectorOfMat images_train = new VectorOfMat();
                VectorOfInt labels_train = new VectorOfInt();
                VectorOfMat images_test = new VectorOfMat();
                VectorOfInt labels_test = new VectorOfInt();

                Dictionary<int, string> mapping_train= new Dictionary<int, string>();
                Dictionary<int, string> mapping_test = new Dictionary<int, string>();

                loadData(@"F:\AJ Data\Data\yalefaces\test", ref images_test, ref labels_test, ref mapping_test);
                loadData(@"F:\AJ Data\Data\yalefaces\train", ref images_train, ref labels_train, ref mapping_train);

                FisherFaceRecognizer fisher = new FisherFaceRecognizer();
                if (File.Exists("fisher.yml"))
                {
                    fisher.Read("fisher.yml");
                }
                else
                {
                    fisher.Train(images_train, labels_train);
                    fisher.Write("fisher.yml");
                }
                int index = 1;
                var result = fisher.Predict(images_train[index]);
                MessageBox.Show("Actual Person " + mapping_train[result.Label].ToString()+  " Predicted Person: " + mapping_test[index].ToString());

                pictureBox1.Image = images_test[index].ToBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void loadData(string path, ref VectorOfMat images, ref VectorOfInt labels, ref Dictionary<int, string> mapping)
        {
            try
            {
                var directoryList = Directory.GetDirectories(path);
                int counter = 0;
                foreach (string directory in directoryList)
                {
                    string name = Path.GetFileName(directory);
                    counter++;
                    mapping.Add(counter, name);

                    foreach (string file  in Directory.GetFiles(directory))
                    {
                        Bitmap map = new Bitmap(file);
                        var img = map.ToImage<Gray, byte>() .Mat;
                        images.Push(img);
                        labels.Push(new int[] { counter });
                    }

                    

                }

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        private void filledObjectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            
            try
            {
                if (pictureBox1.Image == null) return;

                var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();
                var gray = img.Convert<Gray, byte>().ThresholdBinaryInv(new Gray(250),new Gray(255));

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat h = new Mat();
                CvInvoke.FindContours(gray, contours, h, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                var mask = gray.CopyBlank();
                for (int i = 0; i < contours.Size; i++)
                {
                    var area = CvInvoke.ContourArea(contours[i]);
                    if (area > 100)
                    {
                        var bbox = CvInvoke.BoundingRectangle(contours[i]);
                        gray.ROI = bbox;
                        var count = gray.GetSum().Intensity / 255;
                        //var count2 = gray.CountNonzero();
                        mask.ROI = bbox;
                        float prob = (float)count / (bbox.Width * bbox.Height);
                        if (prob<0.5f)
                        {
                            gray.CopyTo(mask);
                        }
                        gray.ROI = Rectangle.Empty;
                        mask.ROI = Rectangle.Empty;

                    }
                }
                img.SetValue(new Bgr(255, 255, 255), mask);
                AddImage(img, "Filtered Image");
                pictureBox1.Image = img.AsBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void lineDetectionWithGaborToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyGabor();
                formGabor gabor = new formGabor();
                gabor.OnApplyGabor += ApplyGabor;
                gabor.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ApplyGabor(double gamma = 0.3, double lambda = 4.0, double psi = 1.0, double theta= 0.6, int size = 3, int sigma= 2)
        {
            //try
            //{
            //    if (pictureBox1.Image == null)
            //    {
            //        return;
            //    }
            //    var img = new Bitmap(pictureBox1.Image).ToImage<Gray, byte>()
            //        .ThresholdBinaryInv(new Gray(240), new Gray(255)).ToBitmap();

            //    GaborFilter filter = new GaborFilter();
            //    filter.Theta = theta;
            //    filter.Lambda = lambda;
            //    filter.Gamma =gamma;
            //    filter.Psi = psi;
            //    filter.Sigma = sigma;
            //    filter.Size = size;

            //    var output = filter.Apply(img);

            //    var img1 = img.ToImage<Gray, byte>();
            //    var img2 = output.ToImage<Gray, byte>().ThresholdBinary(new Gray(200), new Gray(255));
            //    //CvInvoke.Dilate(img2, img2, Mat.Ones(5, 5, DepthType.Cv8U, 1), new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(255));


            //    AddImage(img1.Convert<Bgr, byte>(), "Input");
            //    AddImage(output.ToImage<Bgr, byte>(), "Gabor");


            //    pictureBox1.Image = output;
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message);
            //}
        }

        private void grabCutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null) return;
                if (rect == null) return;

                var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();
                var gray = img.Convert<Gray, byte>();

                var output = img.GrabCut(rect, 1);

                //double min=-1, max=-1;
                //Point minLoc = new Point();
                //Point maxLoc = new Point();

                Image<Gray, byte> img4 = output.Convert<byte>(delegate (byte b) { return  b==3? (byte)255 : (byte)0; });
                //CvInvoke.MinMaxLoc(output, ref min, ref max, ref minLoc, ref maxLoc);

                //img4._SmoothGaussian(3);
                //img4._Dilate(1);

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat m = new Mat();
                CvInvoke.FindContours(img4, contours, m, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                CvInvoke.DrawContours(img, contours, GetBiggestContourID(contours), new MCvScalar(0, 0, 255), 3);
                pictureBox1.Image = img.AsBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int GetBiggestContourID(VectorOfVectorOfPoint contours)
        {
            double maxArea = -100;
            int contourID = 0;
                for (int i = 0; i < contours.Size; i++)
                {
                    double area = CvInvoke.ContourArea(contours[i]);
                    if (area > maxArea)
                    {
                    maxArea = area;
                    contourID = i;
                    }
                }
                return contourID;

        }

        private void grabCutAdvanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void drawFilledPolygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawFilledPolygon = true;
            if (Polygon!=null)
            {
                Polygon.Clear();
            }
        }

        private void completeToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (drawFilledPolygon==true && Polygon !=null)
            {
                Polygon.Add(Polygon[0]);
                pictureBox1.Invalidate();

                //drawFilledPolygon = false;
            }
        }

        private void stopSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void extractMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imgList["Input"]==null)
            {
                return;
            }

            if (Polygon!=null)
            {
                var mask= imgList["Input"].CopyBlank().Convert<Gray, byte>();

                CvInvoke.FillConvexPoly(mask, new VectorOfPoint(Polygon.ToArray()), new MCvScalar(255));
                AddImage(mask.Convert<Bgr, byte>(), "Mask");
                drawFilledPolygon = false;
                pictureBox1.Image = mask.AsBitmap();
            }
        }

        private void selectForeGroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectForeground = true;
        }

        private void selectBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectBackground = true;
        }

        private void faceDetectionFromVideosToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void loadVideoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
               
                if (videoCapture != null)
                {
                    if (videoCapture.IsOpened)
                    {
                        videoCapture.Dispose();
                        videoCapture = null;
                    }
                }
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.Filter = "All Media Files | *.mp4; *.avi;";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        videoCapture = new VideoCapture(dialog.FileName);
                        videoCapture.ImageGrabbed += VideoCapture_ImageGrabbed;
                        
                        Mat frame = new Mat();
                        if (videoCapture.IsOpened)
                        {
                            videoCapture.Read(frame);
                            if (frame != null)
                            {
                                pictureBox1.Image = frame.ToBitmap();
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void VideoCapture_ImageGrabbed(object sender, EventArgs e)
        {
            Mat frame = new Mat();
           
            videoCapture.Retrieve(frame);
            if (frame == null) return;

            if (IsGrabbingWithLandmarks)
            {
                frame = DetectLandmarks(frame);
            }
            else
            {
                if (detectFaces)
                {
                    frame = DetectandCountFaces(frame);
                }
                else
                {
                    Thread.Sleep(25);
                }
            }
            pictureBox1.Invoke(new MethodInvoker(
        delegate ()
        {
            pictureBox1.Image = frame.ToBitmap();
        }));
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoCapture == null) return;
                videoCapture.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pauseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoCapture == null) return;

                videoCapture.Pause();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void stopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoCapture == null) return;
                videoCapture.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void detectLandmarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoCapture == null) return;
                IsGrabbingWithLandmarks = true;
                detectFaces = false;
                videoCapture.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Mat DetectLandmarks(Mat img)
        {

            // face detector: download https://raw.githubusercontent.com/opencv/opencv/master/data/lbpcascades/lbpcascade_frontalface_improved.xml
            // model: https://raw.githubusercontent.com/kurnianggoro/GSOC2017/master/data/lbfmodel.yaml
            // theory https://www.learnopencv.com/facemark-facial-landmark-detection-using-opencv/
            try
            {
                string rootDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
                if (cascadeClassifier==null)
                {
                    string cascadePath = rootDirectory + "/data/lbpcascade_frontalface_improved.xml";
                    cascadeClassifier = new CascadeClassifier(cascadePath);
                }
                if (facemarkDetector==null)
                {
                    string modelpath = rootDirectory + "/data/lbfmodel.yaml";
                    var modelParams = new FacemarkLBFParams();
                    facemarkDetector = new FacemarkLBF(modelParams);
                    facemarkDetector.LoadModel(modelpath);
                }
                var imgGray = img.ToImage<Gray, byte>();
                var faces = cascadeClassifier.DetectMultiScale(imgGray);

                VectorOfRect rect = new VectorOfRect(faces);
                VectorOfVectorOfPointF landmarks = new VectorOfVectorOfPointF();
                
                bool result = facemarkDetector.Fit(imgGray, rect, landmarks);

                if (result)
                {
                    for (int i = 0; i < faces.Length; i++)
                    {
                        //CvInvoke.Rectangle(img, faces[i], new MCvScalar(0, 0, 255), 2);
                        FaceInvoke.DrawFacemarks(img, landmarks[i], new MCvScalar(0, 0, 255));
                        //var rec = CvInvoke.BoundingRectangle(landmarks[i]);
                        // CvInvoke.Rectangle(img, rec, new MCvScalar(0, 0, 255), 2);

                    }
                }
                return img ;
                //pictureBox1.Image = img.ToBitmap();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private Mat DetectandCountFaces(Mat img)
        {

            // face detector: download https://raw.githubusercontent.com/opencv/opencv/master/data/lbpcascades/lbpcascade_frontalface_improved.xml
            // model: https://raw.githubusercontent.com/kurnianggoro/GSOC2017/master/data/lbfmodel.yaml
            // theory https://www.learnopencv.com/facemark-facial-landmark-detection-using-opencv/
            try
            {
                string rootDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
                if (cascadeClassifier == null)
                {
                    string cascadePath = rootDirectory + "/data/lbpcascade_frontalface_improved.xml";
                    cascadeClassifier = new CascadeClassifier(cascadePath);
                }

                var imgGray = img.ToImage<Gray, byte>();
                var faces = cascadeClassifier.DetectMultiScale(imgGray);

                for (int i = 0; i < faces.Length; i++)
                {
                    CvInvoke.Rectangle(img, faces[i], new MCvScalar(0, 0, 255), 2);

                }
                CvInvoke.PutText(img, "Face Count = " + faces.Length.ToString(), new Point(100, 100), FontFace.HersheyPlain, 1.5, new MCvScalar(0, 255, 0));
                return img;
                //pictureBox1.Image = img.ToBitmap();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void humanFaceDetectionFromVideosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoCapture == null) return;
                IsGrabbingWithLandmarks = false;
                detectFaces = true;
                videoCapture.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void objectsWithHolesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null) return;
                int threshold = 10;
                var img = new Bitmap(pictureBox1.Image).ToImage<Gray, byte>()
                    .SmoothGaussian(3);
                var gray = img.ThresholdBinaryInv(new Gray(200), new Gray(255));

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat mat = new Mat();
                CvInvoke.FindContours(gray, contours, mat, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                var mask = gray.CopyBlank();
                var output = img.CopyBlank();

                for (int i = 0; i < contours.Size; i++)
                {
                    var area = CvInvoke.ContourArea(contours[i]);
                    if (area>50)
                    {
                        var bbox = CvInvoke.BoundingRectangle(contours[i]);
                        CvInvoke.DrawContours(mask, contours, i, new MCvScalar(255), -1);
                        gray.ROI = bbox;
                        mask.ROI = bbox;
                        var grayNonZero = gray.CountNonzero();
                        var tempNonZero = mask.CountNonzero();

                        gray.ROI = Rectangle.Empty;
                        mask.ROI = Rectangle.Empty;
                        mask._Dilate(2);

                        int diff = Math.Abs( grayNonZero[0] - tempNonZero[0]);

                        if (diff<=threshold)
                        {
                            img.SetValue(new Gray(255), mask);
                        }
                        mask.SetZero();
                    }
                }
                AddImage(img.Convert<Bgr, byte>(), "Hole Objects");
                pictureBox1.Image = img.ToBitmap();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void countHolesİnObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null) return;
                int threshold = 50;
                var img = new Bitmap(pictureBox1.Image).ToImage<Gray, byte>()
                    .SmoothGaussian(3);
                var gray = img.ThresholdBinaryInv(new Gray(200), new Gray(255));

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat mat = new Mat();
                
                CvInvoke.FindContours(gray, contours, mat, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

                //Emgu.CV.Matrix<int> hierarchy = new Emgu.CV.Matrix<int>(mat.Rows, mat.Cols,mat.NumberOfChannels);
                //mat.CopyTo(hierarchy);
                int []hierarchy = mat.GetData(false).Cast<int>().ToArray();
                var mask = img.CopyBlank();
                //var hierarchy = mat.Split().to.ToImage<.ToImage<Bgra, int>();

                for (int i = 0; i < contours.Size; i++)
                {
                    var area = CvInvoke.ContourArea(contours[i]);
                    if (area > threshold)
                    {
                        var FirstChild = hierarchy[(i * 4) + 2];
                        var Parent = hierarchy[(i * 4) + 3];

                        if (Parent == -1)
                        {
                            var count = hierarchy.Where((p, j) => ((j + 1) % 4 == 0) && p == i).Count();
                            var bbox = CvInvoke.BoundingRectangle(contours[i]);
                            bbox.Y -= 5;
                            CvInvoke.PutText(img, "Holes = " + count.ToString(), bbox.Location, FontFace.HersheyPlain, 1.0,
                                new MCvScalar(0), 1);
                        }
                    }

                }

                //for (int i = 0; i < hierarchy.Length; i++)
                //{
                //    if (hierarchy.GetValue([i])[0]== -1)
                //    {
                //        CvInvoke.DrawContours(mask, contours, i, new MCvScalar(255), -1);
                //    }

                //}
                //img.SetValue(new Gray(255), mask.Dilate(1));
                AddImage(img.Convert<Bgr, byte>(), "Hole Objects");
                pictureBox1.Image = img.ToBitmap();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void removeSmallLargeObjectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null) return;
                int threshold = 10000;
                var img = new Bitmap(pictureBox1.Image).ToImage<Gray, byte>()
                    .SmoothGaussian(3);
                var gray = img.ThresholdBinaryInv(new Gray(200), new Gray(255));

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat mat = new Mat();

                CvInvoke.FindContours(gray, contours, mat, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                var mask = img.CopyBlank();

                for (int i = 0; i < contours.Size; i++)
                {
                    var area = CvInvoke.ContourArea(contours[i]);
                    if (area > threshold)
                    {
                        CvInvoke.DrawContours(mask, contours, i, new MCvScalar(255), -1);
                    }

                }
                img.SetValue(new Gray(255), mask.Not());
                AddImage(img.Convert<Bgr, byte>(), "Filled Holes");
                pictureBox1.Image = img.ToBitmap();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        
        private void fillHolesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //https://docs.opencv.org/master/d9/d8b/tutorial_py_contours_hierarchy.html
            try
            {
                if (pictureBox1.Image == null) return;
                int threshold = 50;
                var img = new Bitmap(pictureBox1.Image).ToImage<Gray, byte>()
                    .SmoothGaussian(3);
                var gray = img.ThresholdBinaryInv(new Gray(200), new Gray(255));

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat mat = new Mat();

                CvInvoke.FindContours(gray, contours, mat, RetrType.Ccomp, ChainApproxMethod.ChainApproxSimple);

                int[] hierarchy = mat.GetData(false).Cast<int>().ToArray();
                var mask = img.CopyBlank();

                for (int i = 0; i < contours.Size; i++)
                {
                    var area = CvInvoke.ContourArea(contours[i]);
                    if (area > threshold)
                    {
                        var FirstChild = hierarchy[(i * 4) + 2];
                        var Parent = hierarchy[(i * 4) + 3];

                        if (Parent == -1)
                        {
                            CvInvoke.DrawContours(mask, contours, i, new MCvScalar(255), -1);
                        }
                    }

                }
                mask._Not();
                AddImage(mask.Convert<Bgr, byte>(), "Filled Holes");
                pictureBox1.Image = mask.ToBitmap();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void yoloObjectDetectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null) return;

                var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();

                List<string> classes = new List<string>();
                
                if (model==null) {
                    string rootDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
                    string configfile = rootDirectory + "data/yolo3.cfg";
                    string namesfile= rootDirectory + "data/coco.names";
                    string modelWeight = rootDirectory + "data/yolo3.weights";

                    model = DnnInvoke.ReadNetFromDarknet(configfile, modelWeight);
                    classes = File.ReadAllLines(namesfile).ToList();
                }
                VectorOfMat output = new VectorOfMat();

                var blob = DnnInvoke.BlobFromImage(img, 1.0/255.0f, new Size(320, 3020), swapRB: true);
                model.SetInput(blob);
                model.Forward(output);

                var outputlayers = model.UnconnectedOutLayers;
                var layernames = model.LayerNames;



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void faceDetectionCaffeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null) return;

                var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();

                List<string> classes = new List<string>();

                if (model == null)
                {
                    string rootDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
                    string modelWeight= rootDirectory + "data/res10_300x300_ssd_iter_140000_fp16.caffemodel";
                    string modelarchiecture = rootDirectory + "data/deploy.prototxt";

                    model = DnnInvoke.ReadNetFromCaffe(modelarchiecture,modelWeight);
                    //classes = File.ReadAllLines(namesfile).ToList();
                }
                VectorOfMat output = new VectorOfMat();

                var blob = DnnInvoke.BlobFromImage(img, 1.0, new Size(300, 300), swapRB: true);
                model.SetInput(blob);
                model.Forward(output);

                //var outputlayers = model.UnconnectedOutLayers;
                //var layernames = model.LayerNames;


                int N= 7;
                for (int i = 0; i < output.Size; i++)
                {
                    var mat = output[i];
                    //double[] hierarchy = mat.GetData(false).Cast<double>().ToArray();
                    var arr = mat.GetData(true).Cast<float>().ToArray();

                    for (int j = 0; j < arr.Length / N; j += N)
                    {
                        var confidence = arr[(j * N) + 2];
                        if (confidence>0.50)
                        {
                            Rectangle rect = new Rectangle();
                            rect.X = (int)(arr[(j * N) + 3] * 320);
                            rect.Y = (int)(arr[(j * N) + 4] * 320);
                            rect.Width = Math.Abs((int)(arr[(j * N) + 5] * 320)- rect.X);
                            rect.Height = Math.Abs((int)(arr[(j * N) + 6] * 320) - rect.Y);
                            CvInvoke.Rectangle(img, rect, new MCvScalar(0, 0, 255), 2);
                        }

                    }


                }

                pictureBox1.Image = img.AsBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void loadDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = true;
                if (dialog.ShowDialog()==DialogResult.OK)
                {
                    var names = dialog.FileNames;
                    foreach (var item in names)
                    {
                        if (item.ToLower().Contains("train"))
                        {
                            UpdateStatus("Loading Train Data...");
                            (x_train,y_train) =  cvsHelperClass.ReadCSV(item, true, LabelIndex: 0);
                        }
                        if (item.ToLower().Contains("test"))
                        {
                            UpdateStatus("Loading Test Data...");
                            (x_test, y_test) = cvsHelperClass.ReadCSV(item, true, LabelIndex: 0);
                        }
                    }
                    UpdateStatus("Data Loaded");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateStatus(string txt)
        {
            try
            {
                lblStatus.Invoke(new System.Action(() => lblStatus.Text = txt));
                lblStatus.Refresh();
            }
            catch (Exception)
            {
                ;
            }
        }
        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var predictions = new Emgu.CV.Matrix<float>(y_test.Rows, 1);
                predictions.SetValue(-1);

                Emgu.CV.Matrix<float> out1 = new Emgu.CV.Matrix<float>(1, 10);
                UpdateStatus("Processing Test Data...");
                if (MLPModel!=null)
                {
                    for (int i = 0; i < x_test.Rows; i++)
                    {
                        var r = x_test.GetRow(i);
                       MLPModel.Predict(r,out1);
                        var p = cvsHelperClass.OneHotEncoding2Matrix(out1);
                        predictions[i, 0] = p[0, 0];
                    }

                    //System.Threading.Thread.Sleep(1);
                    lblStatus.Invoke(new System.Action(() => lblStatus.Text = "Evaluations..."));
                    PredictedLabels = cvsHelperClass.Matrix2Array(predictions);
                    ActualLabels = cvsHelperClass.Matrix2Array(y_test.Convert<float>());

                    ShowRandomImages(PredictedLabels);

                    //ConfusionMatrix = new GeneralConfusionMatrix(ActualLabels, PredictedLabels);
                    //double accuracy = ConfusionMatrix.Accuracy;
                    //double kappa = ConfusionMatrix.Kappa;

                    //lblStatus.Text = string.Format("Accuracy = {0:0.00%} \n Kappa = {1:.00}", accuracy, kappa);
                    ////FormConfusionMatrix form = new FormConfusionMatrix(cvsHelperClass.DrawConfusionMatrix(ConfusionMatrix));
                    ////form.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void evaluateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //if (ConfusionMatrix!=null)
                //{
                //    FormConfusionMatrix form = new FormConfusionMatrix(ConfusionMatrix);
                //    form.Show();
                //}
                
            }
            catch (Exception)
            {

   
                throw;
            }
        }

        private void yolo3LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                    string rootDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
                    string configfile = rootDirectory + "data/yolov3.json";
                    string namesfile = rootDirectory + "data/coco.names";
                    string modelWeight = rootDirectory + "data/person-detection-retail-0013.bin";

                    model = DnnInvoke.ReadNetFromDarknet(configfile, modelWeight);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mNISTSVMClassificationToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void trainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (x_train == null || y_train == null)
                {
                    return;
                }
                int InputDataSize = x_train.Cols;
                string ModelName = "SVM_MNIST";

                // Preprocessing normalize data
                CvInvoke.Normalize(x_train, x_train, 0, 1, NormType.MinMax);
                CvInvoke.Normalize(x_test, x_test, 0, 1, NormType.MinMax);

                // create ANN
                SVMModel = new Emgu.CV.ML.SVM();
                SVMModel.SetKernel(SVM.SvmKernelType.Rbf);
                SVMModel.Type = SVM.SvmType.CSvc;
                SVMModel.TermCriteria = new MCvTermCriteria(500, 0.0001);

                
                TrainData TrainData = new TrainData(x_train, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, y_train.Convert<int>());

                if (File.Exists(ModelName))
                {
                    SVMModel.Load(ModelName);
                    lblStatus.Text = "Model Loaded...";
                }
                else
                {
                    if (SVMModel.Train(TrainData, 2))
                    {
                        if (!File.Exists(ModelName))
                        {
                            SVMModel.Save(ModelName);
                        }
                        lblStatus.Text = "Model Trained...";
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void testToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                //PredictedLabels = new int[y_test.Rows];
                //PredictedLabels  = PredictedLabels.Multiply(-1);

                //Emgu.CV.Matrix<int> out1 = new Emgu.CV.Matrix<float>(1, 10);
                UpdateStatus("Processing Test Data...");
                if (SVMModel != null)
                {
                    for (int i = 0; i < x_test.Rows; i++)
                    {
                        var r = x_test.GetRow(i);
                        PredictedLabels[i] = (int)SVMModel.Predict(r);
                    }

                    //System.Threading.Thread.Sleep(1);
                    lblStatus.Invoke(new System.Action(() => lblStatus.Text = "Evaluations..."));
                    ActualLabels = cvsHelperClass.Matrix2Array(y_test.Convert<float>());

                    ShowRandomImages(PredictedLabels);

                    //double accuracy = ConfusionMatrix.Accuracy;
                    //double kappa = ConfusionMatrix.Kappa;

                    //lblStatus.Text = string.Format("Accuracy = {0:0.00%} \nKappa = {1:.00}", accuracy, kappa);
                    //FormConfusionMatrix form = new FormConfusionMatrix(cvsHelperClass.DrawConfusionMatrix(ConfusionMatrix));
                    //form.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void evaluationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (SVMModel==null)
                {
                    return;
                }
                //ConfusionMatrix = new GeneralConfusionMatrix(ActualLabels, PredictedLabels);
                //FormConfusionMatrix form = new FormConfusionMatrix(ConfusionMatrix);
                //form.Show();

            }
            catch (Exception)
            {


                throw;
            }
        }

        private void watershedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Bgr, byte>();
                var gray= img.Convert<Gray, byte>().Clone();
                var bg = gray.ThresholdBinaryInv(new Gray(200), new Gray(255));
                float[,] data = {
                    { 1, 1, 1 },
                    { 1, -8, 1},
                    { 1, 1, 1 } };

                Emgu.CV.Matrix<float> kernel = new Emgu.CV.Matrix<float>(data);
               
                var imgLaplacian = img.CopyBlank();
                CvInvoke.Filter2D(img, imgLaplacian, kernel, new Point(-1, -1));
                imgLaplacian._Dilate(2);
                
                var mask = gray
                    .ThresholdBinaryInv(new Gray(220), new Gray(255))
                    .Erode(3);

                var markers = (img + imgLaplacian.Convert<Bgr, byte>())
                    .Convert<Gray, byte>().ThresholdBinaryInv(new Gray(200),new Gray(255))
                    .Dilate(1);

                CvInvoke.ConnectedComponents(markers, markers);
                var output = markers.Convert<Gray, Int32>();
               
                CvInvoke.Watershed(img, output);
                Image<Gray, byte> contours = output.Convert<byte>(delegate (Int32 b) { return (byte)(b==-1?255:0); });
                contours = contours.Dilate(1);
                img.SetValue(new Bgr( 0,255, 0), contours);
                AddImage(img, "Watershed");
                pictureBox1.Image = img.AsBitmap();

                //Mat imgLaplacian = new Mat();
                //CvInvoke.Filter2D(img, imgLaplacian, kernel, new Point(-1, -1));
                //Mat sharp = new Mat();
                //img.Mat.ConvertTo(sharp, Emgu.CV.CvEnum.DepthType.Cv32S);

                //Mat imgResult = sharp - imgLaplacian;
                //// convert back to 8bits gray scale
                //imgResult.ConvertTo(imgResult, Emgu.CV.CvEnum.DepthType.Cv8U);
                //imgLaplacian.ConvertTo(imgLaplacian, Emgu.CV.CvEnum.DepthType.Cv8U);


                //Mat dimg = new Mat();
                //Mat labels = new Mat();
                //CvInvoke.DistanceTransform(mask, dimg, labels, Emgu.CV.CvEnum.DistType.L2, 3);

                //CvInvoke.Normalize(dimg, dimg, normType: Emgu.CV.CvEnum.NormType.MinMax);


                //CvInvoke.Threshold(dimg, dimg, 0.8, 1.0, Emgu.CV.CvEnum.ThresholdType.Binary);

                //var markers = new Mat(imgLaplacian.Size, Emgu.CV.CvEnum.DepthType.Cv32S, 1);
                //dimg.ConvertTo(markers, Emgu.CV.CvEnum.DepthType.Cv32S);

                //CvInvoke.Watershed(imgLaplacian, markers);

                //Mat imgout = new Mat();
                //markers.ConvertTo(imgout, Emgu.CV.CvEnum.DepthType.Cv8U);




            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private Image<Gray, byte> BWAreaOpen(Image<Gray, byte>  img, int size)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat h = new Mat();

            CvInvoke.FindContours(img, contours, h, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++)
            {
                var area = CvInvoke.ContourArea(contours[i]);
                if (area<size)
                {
                    img.ROI = CvInvoke.BoundingRectangle(contours[i]);
                    img.SetValue(new Gray(0), img);
                    img.ROI = Rectangle.Empty;
                }
            }

            return img;
        }

        private void cLAHEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                    return;

                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Gray, byte>();
                Mat output = new Mat();

                CvInvoke.CLAHE(img, 50, new Size(8, 8), output);
                //CvInvoke.Imshow("Histogram Equalization", histeq);
                AddImage(output.ToImage<Bgr, byte>(), "CLAHE");
                pictureBox1.Image = output.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void deskewTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null) return;

                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Gray, byte>();
                var gray = img.ThresholdBinaryInv(new Gray(200), new Gray(255))
                    .Dilate(5);

                VectorOfPoint points = new VectorOfPoint();
                CvInvoke.FindNonZero(gray, points);

                var rotatedRect = CvInvoke.MinAreaRect(points);
                Mat rotationMatrix = new Mat(new Size(2,3),DepthType.Cv32F,1);
                var rotated = img.CopyBlank();

                if (rotatedRect.Angle < -45)
                {
                    rotatedRect.Angle = (90 + rotatedRect.Angle);
                }

                CvInvoke.GetRotationMatrix2D(rotatedRect.Center, rotatedRect.Angle, 1.0,rotationMatrix);
                CvInvoke.WarpAffine(img, rotated, rotationMatrix, img.Size, Inter.Cubic, borderMode: BorderType.Replicate); //,borderValue:new MCvScalar(255)
                AddImage(rotated.Convert<Bgr, byte>(), "Translated");
                pictureBox1.Image = rotated.AsBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void loadYoloONNXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // try this
                //https://docs.opencv.org/master/d5/d86/tutorial_dnn_javascript.html
                //https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov2-coco
                //https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov2
                //http://machinethink.net/blog/object-detection-with-yolo/
                //string path = @"C:\Users\akhtar.jamil\Downloads\yolov2-coco-9.onnx";

                string path = @"C:\Users\akhtar.jamil\Downloads\tinyyolov2-1.onnx";

                var net =  Emgu.CV.Dnn.DnnInvoke.ReadNetFromONNX(path);
                if (net.Empty)
                {
                    return;
                }

                Image<Bgr, byte> img = new Bitmap(pictureBox1.Image)
                    .ToImage<Bgr, byte>();

                net.SetPreferableBackend(Emgu.CV.Dnn.Backend.OpenCV);
                net.SetPreferableTarget(Target.Cpu);
                

                var blob = DnnInvoke.BlobFromImage(img,size:new Size(416,416));
                var img2 = img.Resize(416, 416, Inter.Cubic);
                //shape (1x3x416x416)
                
                net.SetInput(blob);

                VectorOfMat output = new VectorOfMat();
                net.Forward(output);

                YoloOutputParser parser = new YoloOutputParser();
                VectorOfFloat vector = new VectorOfFloat();
                //output[0].CopyTo(vector);

                var ar = output[0].GetData(false).Cast<float>().ToArray();
                var boxes = parser.ParseOutputs(ar);
                var filteredBoundingBoxes = parser.FilterBoundingBoxes(boxes, 5, .4F);

                var originalImageHeight = img.Height;
                var originalImageWidth = img.Width;
                var imageWidth = 416;
                var imageHeight = 416;
                foreach (var box in filteredBoundingBoxes)
                {
                    var x = (int)Math.Max(box.Dimensions.X, 0);
                    var y = (int)Math.Max(box.Dimensions.Y, 0);
                    var width = (int)Math.Min(originalImageWidth - x, box.Dimensions.Width);
                    var height = (int)Math.Min(originalImageHeight - y, box.Dimensions.Height);

                    x = (int)(originalImageWidth * x / imageWidth);
                    y = (int)(originalImageHeight * y / imageHeight);
                    width = (int)(originalImageWidth * width / imageWidth);
                    height = (int)(originalImageHeight * height / imageHeight);

                    Rectangle rect = new Rectangle(x, y, width, height);
                    CvInvoke.Rectangle(img, rect, new MCvScalar(0, 255, 0), 2);
                    string text = $"{box.Label} ({(box.Confidence * 100).ToString("0")}%)";
                    CvInvoke.PutText(img, text, new Point(x+10, y + 15), FontFace.HersheyPlain, 1.25, new MCvScalar(0, 0, 255),2);
                }

                pictureBox1.Image = img.AsBitmap();
            }
            catch (Exception  ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void selectRectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsSelectingGrabCutRectangle = true;
            IsMouseDown = false;
            selectForeground = false;
            selectBackground = false;
        }

        private void selectForegroundPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectForeground = true;
            selectBackground = false;
            ForegroundPoint.Clear();
            MouseUpLocation = Point.Empty;
        }

        private void grabCutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null) return;
                if (rect == null) return;

                var img = imgList["Input"];
               // Mat mask = new Mat();
                Mat bgModel = new Mat();
                Mat fgModel = new Mat();

               Image<Gray, byte> mask =new Image<Gray, byte>(img[0].Size);
               // var mask =img.CopyBlank();

                mask.SetValue(new Gray(0));

                if (ForegroundPoint.Count>0 && BackgroundPoints.Count>0)
                {
                    foreach (var poly in ForegroundPoint)
                    {
                        mask.DrawPolyline(poly.ToArray(), false, new Gray(1), 1);
                    }

                    foreach (var poly in BackgroundPoints)
                    {
                        mask.DrawPolyline(poly.ToArray(), false, new Gray(2), 1);
                    }

                    CvInvoke.GrabCut(img, mask, rect, bgModel, fgModel, 10, GrabcutInitType.InitWithMask);
                }
                else
                {
                    CvInvoke.GrabCut(img, mask, rect, bgModel, fgModel, 5, GrabcutInitType.InitWithRect);

                }

                //Image<Gray, byte> img4 = mask.ToImage<Gray, float>().Convert<byte>(delegate (float b) { return b == 3 || b == 1 ? (byte)255 : (byte)0; });
                Image<Gray, byte> img4 = mask.Convert<byte>(delegate (byte b) { return b == 3 || b == 1 ? (byte)255 : (byte)0; });

                img4._SmoothGaussian(3);
                img4._Dilate(1);
                img.SetValue(new Bgr(0, 255, 0), img4);

                //VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                //Mat m = new Mat();
                //CvInvoke.FindContours(img4, contours, m, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                //CvInvoke.DrawContours(img, contours, GetBiggestContourID(contours), new MCvScalar(0, 0, 255), 3);
                pictureBox1.Image = img4.AsBitmap();
                // pictureBox1.Image = mask.AsBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void selectBackgroundToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            selectBackground = true;
            selectForeground = false;
            BackgroundPoints.Clear();
        }

        private float SumOfSquaredDifference(float []a, float []b)
        {
            //var c = a.Select((x, index) => x + b[index]).ToArray();
            return a.Zip(b, (x, y) => x*x - y*y).Sum();
        }
        private void textDetectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

                //https://github.com/argman/EAST#demo
                // for conversion to onxx
                //https://github.com/onnx/tensorflow-onnx

                if (pictureBox1.Image == null) return;
                var img = new Bitmap(pictureBox1.Image)
                    .ToImage<Bgr, byte>();
                    //.ThresholdBinaryInv(new Gray(200), new Gray(255));

                string projectDirectory = Directory.GetParent(Environment.CurrentDirectory)
                    .Parent
                    .Parent
                    .FullName;

                string classifierFile1 = projectDirectory + "/data/trained_classifierNM1.xml";
                string classifierFile2 = projectDirectory + "/data/trained_classifierNM2.xml";
               string tfmodel = projectDirectory+ "/data/frozen_east_text_detection.pb";
                
                //size must be multiple of 32
                var blob = DnnInvoke.BlobFromImage(img,size:new Size(640,320), swapRB:true);


                var detector = DnnInvoke.ReadNetFromTensorflow(tfmodel);
                detector.SetInput(blob);
                detector.SetPreferableBackend(Emgu.CV.Dnn.Backend.OpenCV);
                

                //geometry of the Text-box 
                //feature_fusion/concat_3
                //the confidence score of the detected box
                //feature_fusion / Conv_7 / Sigmoid
                string blobNames = "";
                VectorOfMat geometry = new VectorOfMat();
                VectorOfMat score = new VectorOfMat();
                detector.Forward(geometry, "feature_fusion/concat_3");
                detector.Forward(score, "feature_fusion/Conv_7/Sigmoid");
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void decodeBoundingBoxes(Mat scores, Mat geometry, float scoreThresh)
{
            List<RotatedRect> detections = new List<RotatedRect>();
            List<float> confidences = new List<float>();
    //        scores.Dims 
    //CV_Assert(scores.dims == 4); 
    //        CV_Assert(geometry.dims == 4); 
    //        CV_Assert(scores.size[0] == 1);
    //    CV_Assert(geometry.size[0] == 1); 
    //        CV_Assert(scores.size[1] == 1); 
    //        CV_Assert(geometry.size[1] == 5);
    //    CV_Assert(scores.size[2] == geometry.size[2]); 
    //        CV_Assert(scores.size[3] == geometry.size[3]);

//        int height = scores.SizeOfDimension[2];
//        int width = scores.SizeOfDimension[3];
//    for (int y = 0; y<height; ++y)
//    {
//        float scoresData = scores.GetData([0, 0, y];
//        const float* x0_data = geometry.ptr<float>(0, 0, y);
//        const float* x1_data = geometry.ptr<float>(0, 1, y);
//        const float* x2_data = geometry.ptr<float>(0, 2, y);
//        const float* x3_data = geometry.ptr<float>(0, 3, y);
//        const float* anglesData = geometry.ptr<float>(0, 4, y);
//        for (int x = 0; x<width; ++x)
//        {
//            float score = scoresData[x];
//            if (score<scoreThresh)
//                continue;

//            // Decode a prediction.
//            // Multiple by 4 because feature maps are 4 time less than input image.
//            float offsetX = x * 4.0f, offsetY = y * 4.0f;
//        float angle = anglesData[x];
//        float cosA = std::cos(angle);
//        float sinA = std::sin(angle);
//        float h = x0_data[x] + x2_data[x];
//        float w = x1_data[x] + x3_data[x];

//        Point2f offset(offsetX + cosA* x1_data[x] + sinA* x2_data[x],
//                       offsetY - sinA* x1_data[x] + cosA* x2_data[x]);
//        Point2f p1 = Point2f(-sinA * h, -cosA * h) + offset;
//        Point2f p3 = Point2f(-cosA * w, sinA * w) + offset;
//        RotatedRect r(0.5f * (p1 + p3), Size2f(w, h), -angle* 180.0f / (float) CV_PI);
//        detections.push_back(r);
//            confidences.push_back(score);
//        }
////}
}

        //public object decodeBoundingBoxes(VectorOfMat scores, VectorOfMat geometry, float scoreThresh)
        //{
        //    var detections = new List<object>();
        //    var confidences = new List<object>();
        //    //########### CHECK DIMENSIONS AND SHAPES OF geometry AND scores ############
        //    Debug.Assert(scores.Size..shape.Count == 4);
        //    Debug.Assert("Incorrect dimensions of scores");
        //    Debug.Assert(geometry.shape.Count == 4);
        //    Debug.Assert("Incorrect dimensions of geometry");
        //    Debug.Assert(scores.shape[0] == 1);
        //    Debug.Assert("Invalid dimensions of scores");
        //    Debug.Assert(geometry.shape[0] == 1);
        //    Debug.Assert("Invalid dimensions of geometry");
        //    Debug.Assert(scores.shape[1] == 1);
        //    Debug.Assert("Invalid dimensions of scores");
        //    Debug.Assert(geometry.shape[1] == 5);
        //    Debug.Assert("Invalid dimensions of geometry");
        //    Debug.Assert(scores.shape[2] == geometry.shape[2]);
        //    Debug.Assert("Invalid dimensions of scores and geometry");
        //    Debug.Assert(scores.shape[3] == geometry.shape[3]);
        //    Debug.Assert("Invalid dimensions of scores and geometry");
        //    var height = scores.shape[2];
        //    var width = scores.shape[3];
        //    foreach (var y in Enumerable.Range(0, height - 0))
        //    {
        //        // Extract data from scores
        //        var scoresData = scores[0][0][y];
        //        var x0_data = geometry[0][0][y];
        //        var x1_data = geometry[0][1][y];
        //        var x2_data = geometry[0][2][y];
        //        var x3_data = geometry[0][3][y];
        //        var anglesData = geometry[0][4][y];
        //        foreach (var x in Enumerable.Range(0, width - 0))
        //        {
        //            var score = scoresData[x];
        //            // If score is lower than threshold score, move to next x
        //            if (score < scoreThresh)
        //            {
        //                continue;
        //            }
        //            // Calculate offset
        //            var offsetX = x * 4.0;
        //            var offsetY = y * 4.0;
        //            var angle = anglesData[x];
        //            // Calculate cos and sin of angle
        //            var cosA = math.cos(angle);
        //            var sinA = math.sin(angle);
        //            var h = x0_data[x] + x2_data[x];
        //            var w = x1_data[x] + x3_data[x];
        //            // Calculate offset
        //            var offset = new List<object> {
        //                offsetX + cosA * x1_data[x] + sinA * x2_data[x],
        //                offsetY - sinA * x1_data[x] + cosA * x2_data[x]
        //            };
        //            // Find points for rectangle
        //            var p1 = (-sinA * h + offset[0], -cosA * h + offset[1]);
        //            var p3 = (-cosA * w + offset[0], sinA * w + offset[1]);
        //            var center = (0.5 * (p1[0] + p3[0]), 0.5 * (p1[1] + p3[1]));
        //            detections.append((center, (w, h), -1 * angle * 180.0 / math.pi));
        //            confidences.append(float(score));
        //        }
        //    }

        //    // Return detections and confidences
        //    return new List<object> {
        //        detections,
        //        confidences
        //    };
        //}
        private void oCRToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float[] a = { 2, 3 };
            float[] b = { 1, 2 };

            var result = SumOfSquaredDifference(a, b);
        }

        private void loadDataToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //http://vision.ucsd.edu/content/yale-face-database
            try
            {
                string path = @"F:\AJ Data\Data\yalefaces_Orifinal";
                var traindata = FaceDataSet.LoadFaceData(path).ToList();

                HOGDescriptor hog = new HOGDescriptor();
                List<float[]> HogFeatures = new List<float[]>();

                for (int i = 0; i < traindata.Count(); i++)
                {
                    var feature = hog.Compute(traindata[i].Image);
                    HogFeatures.Add(feature);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void fingerCounting4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private static double EQDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        private List<Point> RemoveClosesPoints(List<Point> pointList, float threshold = 50f)
        {
            var filteredList = new List<Point>();

            for (int j = 0; j < pointList.Count; j++)
            {
                var currentPoint = pointList[j];
                var neighbours = new List<Point>();
                var distances = new List<double>();

                neighbours.Add(currentPoint);

                for (var i = 0; i < pointList.Count; i++)
                {
                    if (pointList[i]!=currentPoint)
                    {
                        var distance = EQDistance(currentPoint, pointList[i]);
                        distances.Add(distance);
                        if (distance < threshold)
                        {
                            neighbours.Add(pointList[i]);
                            
                        }
                    }
                    
                }
                var allneighbours =  neighbours.OrderBy(p => p.Y).ThenBy(p => p.X).ToList();

                if (allneighbours.Count >0 && !filteredList.Contains(allneighbours[0]))
                {
                    filteredList.Add(allneighbours[0]);
                }
            }

            return filteredList;
        }

        private void countFingersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private List<Point> GetPointsFromHull(Mat hull, Point[] points)
        {
            List<Point> list = new List<Point>();
            var data = hull.ToImage<Gray, Int32>();
            for (int i = 0; i < hull.Rows; i++)
            {
                list.Add(points[(int)data[i, 0].Intensity]);
            }
            return list;
        }
        private void objectSizeEstimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //https://www.pyimagesearch.com/2016/03/21/ordering-coordinates-clockwise-with-python-and-opencv/
            //https://www.pyimagesearch.com/2016/03/28/measuring-size-of-objects-in-an-image-with-opencv/
        }

        private void countFingersToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }
                var img = imgList["Input"].Clone();
                var gray = img.Convert<Gray, byte>().SmoothGaussian(5).ThresholdBinaryInv(new Gray(240), new Gray(255));


                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat h = new Mat();

                CvInvoke.FindContours(gray, contours, h, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                if (contours.Size < 1)
                {
                    throw new Exception("No contour is found.");
                }

                int CID = GetBiggestContourID(contours);
                //var points = Array.ConvertAll(contours[CID].ToArray(),x=>(PointF)x);
                var points = contours[CID].ToArray();
                VectorOfPoint biggestContour = new VectorOfPoint(contours[CID].ToArray());

                /*
                // approximate the hull
                var approx = new VectorOfPoint();
                var epsilon = 0.0005 * CvInvoke.ArcLength(contours[CID], true);

                CvInvoke.ApproxPolyDP(contours[CID], approx, epsilon, true);
                */

                var hull = new Mat();

                CvInvoke.ConvexHull(biggestContour, hull, false, false);

                //VectorOfPoint hull2 = new VectorOfPoint();
                //CvInvoke.ConvexHull(approx, hull2, false, false);


                //var hull1 = new Mat();
                ////epsilon = 0.0005 * CvInvoke.ArcLength(hull2, true);
                //CvInvoke.ApproxPolyDP(hull2, hull1, epsilon, true);
                Mat convDefects = new Mat();

                CvInvoke.ConvexityDefects(biggestContour, hull, convDefects);

                var hullPoints = GetPointsFromHull(hull, points);
                
                var convexHullFilterThreshold = int.Parse(cvsHelperClass.ReadConfigParameters("convexHullFilterThreshold"));
                var filteredHull = RemoveClosesPoints(hullPoints, convexHullFilterThreshold);

                var convexHullDistanceThreshold = int.Parse(cvsHelperClass.ReadConfigParameters("convexHullDistanceThreshold"));

                var imgOutput = img.CopyBlank();
                foreach (var p in filteredHull)
                {
                    CvInvoke.Circle(img, p, 5, new MCvScalar(0, 255, 0), -1);
                }
                var data = convDefects.GetData();
                int defects = 0;
                for (int n = 0; n < data.GetLength(0); n++)
                {
                    var idx = (int)data.GetValue(n, 0, 2);
                    var p = points[idx];
                    var dist = double.Parse(data.GetValue(n, 0, 3).ToString());
                    if (dist > convexHullDistanceThreshold)
                    {
                        CvInvoke.Circle(img, p, 5, new MCvScalar(255, 0, 0), -1);
                        defects++;
                    }
                    //CvInvoke.PutText(imgOutput,dist.ToString(), p,FontFace.HersheyPlain,1.0, new MCvScalar(255, 0, 0));

                }


                //CvInvoke.DrawContours(img, contours, CID, new MCvScalar(255, 255, 255));
                CvInvoke.PutText(img, defects.ToString(), new Point(10, 10), FontFace.HersheyPlain, 1.0, new MCvScalar(0, 0, 0));

                pictureBox1.Image = img.AsBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FormConvexHullSettings form = new FormConvexHullSettings();
                form.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void barcodeReaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // create a barcode reader instance
                string srcode = @"F:\AJ Data\img\barcodes\code39-1.png";
                var bitmap = new Bitmap(srcode);
                //var img = bitmap.ToImage<Gray, byte>();
                //pictureBox1.Image = img.AsBitmap();
                //// emgucv qrcode reader
                //var QRreader = new QRCodeReader();
                //VectorOfPoint points = new VectorOfPoint();

                //Emgu.CV.QRCodeDetector detector = new QRCodeDetector();
                //detector.Detect(img, points);

                //var output = new Mat();

                //string text = detector.Decode(img, points);
                ////MessageBox.Show(text);

                var writer = new BarcodeWriter();
                var code = writer.Write("1234567890");
                code.Save(@"F:\AJ Data\img\barcodes\barcode-01.jpg");
                var bitmap1 = new Bitmap(@"F:\AJ Data\img\barcodes\barcode-01.jpg");
                IBarcodeReader reader = new BarcodeReader();
                // detect and decode the barcode inside the bitmap
                var result = reader.Decode(bitmap1);
                // do something with the result
                if (result != null)
                {
                    //txtDecoderType.Text = result.BarcodeFormat.ToString();
                    MessageBox.Show( result.Text + ", Type: " + result.BarcodeFormat.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void blurFacesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Video files (*.mp4,*.avl)|*.mp4;*.avl|All files (*.*)|*.*";
                if (dialog.ShowDialog()==DialogResult.OK)
                {
                    UIVideoPlayer player = UIVideoPlayer.GetInstance(dialog.FileName);
                    player.Dock = DockStyle.Fill;
                    tableLayoutPanel1.Controls.Remove(pictureBox1);
                    tableLayoutPanel1.Controls.Add(player, 1,0);
                    tableLayoutPanel1.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void overlayImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (imgList["Input"] == null) return;
                if (rect == null) return;

                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    //target image
                    var img2 = new Image<Bgr, byte>(dialog.FileName)
                    .Resize(0.75, Inter.Cubic);
                    var img1 = new Bitmap(pictureBox1.Image)
                        .ToImage<Bgr, byte>();

                    var mask = img2.Convert<Gray, byte>()
                        .SmoothGaussian(3)
                        .ThresholdBinaryInv(new Gray(246), new Gray(255))
                        .Erode(1);

                    rect.Width = img2.Width;
                    rect.Height = img2.Height;

                    img1.ROI = rect;
                    img1.SetValue(new Bgr(0, 0, 0), mask);
                    img2.SetValue(new Bgr(0, 0, 0), mask.Not());

                    img1._Or(img2);
                    img1.ROI = Rectangle.Empty;

                    AddImage(img1, "Image Overlay");
                    pictureBox1.Image = img1.AsBitmap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ınpaintToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void selectMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InpaintSelection = !InpaintSelection;
            InpaintCurrentPoints = new List<Point>();
            InpaintPoints = new List<List<Point>>();
        }

        private void applyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //https://docs.opencv.org/3.4/df/d3d/tutorial_py_inpainting.html

            try
            {
                if (pictureBox1.Image== null) return;
                if (InpaintPoints.Count==0) return;

                var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();

                var mask = new Image<Gray, byte>(img.Width,img.Height);
                foreach (var polys in InpaintPoints)
                {
                    mask.DrawPolyline(polys.ToArray(), false, new Gray(255), 5);
                }
                var output = img.CopyBlank();
                CvInvoke.Inpaint(img, mask, output, 3, InpaintType.NS);
                AddImage(mask.Convert<Bgr, byte>(), "Mask");
                AddImage(output, "Inpaint");

                pictureBox1.Image = output.ToBitmap();
                InpaintPoints.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void table2TextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
        private List<Rectangle> Contours2BBox(VectorOfVectorOfPoint contours)
        {
            List < Rectangle >  list= new List<Rectangle>();
            for (int i = 0; i < contours.Size; i++)
            {
                list.Add(CvInvoke.BoundingRectangle(contours[i]));
            }
            return list;
        }

        private VectorOfVectorOfPoint FilterContours(VectorOfVectorOfPoint  contours, double threshold=50)
        {
            VectorOfVectorOfPoint filterContours = new VectorOfVectorOfPoint();
            for (int i = 0; i < contours.Size; i++)
            {
                if (CvInvoke.ContourArea(contours[i])>=threshold)
                {
                    filterContours.Push(contours[i]);
                }
            }
            return filterContours;
        }

        private void convertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //https://towardsdatascience.com/a-table-detection-cell-recognition-and-text-extraction-algorithm-to-convert-tables-to-excel-files-902edcf289ec
                ApplyTable2Excel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ApplyTable2Excel(int NoCols = 4, float MorphThreshold = 30f,int binaryThreshold=200, int offset = 5)
        {
            try
            {
                //https://towardsdatascience.com/a-table-detection-cell-recognition-and-text-extraction-algorithm-to-convert-tables-to-excel-files-902edcf289ec

                if (pictureBox1.Image == null) return;
                
                var img = new Bitmap(pictureBox1.Image).ToImage<Gray, byte>()
                    //.SmoothGaussian(3)
                    .ThresholdBinaryInv(new Gray(binaryThreshold), new Gray(255));

                //pictureBox1.Image = img.AsBitmap();

                int length = (int)(img.Width * MorphThreshold/100);
                Mat vProfile = new Mat();
                Mat hProfile = new Mat();

                var kernelV = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(1, length), new Point(-1, -1));
                var kernelH = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(length, 1), new Point(-1, -1));

                CvInvoke.Erode(img, vProfile, kernelV, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(255));
                CvInvoke.Dilate(vProfile, vProfile, kernelV, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(255));

                CvInvoke.Erode(img, hProfile, kernelH, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(255));
                CvInvoke.Dilate(hProfile, hProfile, kernelH, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(255));

                var mergedImage = vProfile.ToImage<Gray, byte>().Or(hProfile.ToImage<Gray, byte>());
                mergedImage._ThresholdBinary(new Gray(100), new Gray(255));

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat h = new Mat();
                CvInvoke.FindContours(mergedImage, contours, h, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                int bigCID = GetBiggestContourID(contours);
                var bbox = CvInvoke.BoundingRectangle(contours[bigCID]);

                mergedImage.ROI = bbox;
                img.ROI = bbox;
                var temp = mergedImage.Copy();
                temp._Not();

                var imgTable = img.Copy();
                contours.Clear();
                CvInvoke.FindContours(temp, contours, h, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                var filteredContours = FilterContours(contours, 500);
                var bboxList = Contours2BBox(filteredContours);

                var sortedBBoxes = bboxList.OrderBy(x => x.Y).ThenBy(y => y.X).ToList();
                string datapath = @"F:\AJ Data\Data\traineddata.eng\";

                //OCR Part
                Tesseract ocr = new Tesseract();
                ocr.Init(datapath, "eng", OcrEngineMode.TesseractOnly);
                ocr.PageSegMode = PageSegMode.SingleBlock;

                // to write to excel
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Results");

                int rowCounter = 1;
                char colCounter = 'A';
                for (int i = 0; i < sortedBBoxes.Count; i++)
                {
                    var rect = sortedBBoxes[i];
                    rect.X += offset;
                    rect.Y += offset;
                    rect.Width -= offset;
                    rect.Height -= offset;

                    imgTable.ROI = rect;
                    // imgTable.Save(@"F:\test\" +i.ToString()+  ".jpg");
                    ocr.SetImage(imgTable.Copy());
                    //var vv = ocr.GetUTF8Text();
                    string text = ocr.GetUTF8Text().Replace("\r\n", "");

                    if (i % NoCols == 0)
                    {
                        if (i > 0)
                        {
                            rowCounter++;
                        }
                        colCounter = 'A';
                        worksheet.Cell(colCounter.ToString() + rowCounter.ToString()).Value = text;
                    }
                    else
                    {
                        colCounter++;
                        worksheet.Cell(colCounter + rowCounter.ToString()).Value = text;

                    }
                    imgTable.ROI = Rectangle.Empty;
                }
                workbook.SaveAs("f:\\output.xlsx");
                MessageBox.Show("Information extracted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void settingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                FormParamTable2Excel form = new FormParamTable2Excel();
                form.OnTable2Excel += ApplyTable2Excel;
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void shapeMatchingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void ApplyShapeDetection(double threshold = 0.01)
        {
            try
            {
                if (pictureBox1.Image == null) return;
                if (rect == null) return;

                var img = imgList["Input"].Clone();

                var gray = img//.SmoothGaussian(3)
                    .Convert<Gray, byte>()
                    .ThresholdBinaryInv(new Gray(240), new Gray(255));

                gray.ROI = rect;
                var target = gray.Copy();
                gray.ROI = Rectangle.Empty;

                AddImage(target.Convert<Bgr, byte>(), "Template");

                VectorOfVectorOfPoint imgContours = new VectorOfVectorOfPoint();
                VectorOfVectorOfPoint targetContours = new VectorOfVectorOfPoint();
                Mat h = new Mat();
                CvInvoke.FindContours(gray, imgContours, h, RetrType.External, ChainApproxMethod.ChainApproxNone);
                CvInvoke.FindContours(target, targetContours, h, RetrType.External, ChainApproxMethod.ChainApproxNone);

                if (imgContours.Size == 0 || targetContours.Size == 0)
                {
                    throw new Exception("No Contour found to process.");
                }

                for (int i = 0; i < imgContours.Size; i++)
                {
                    var distance = CvInvoke.MatchShapes(imgContours[i], targetContours[0],
                        ContoursMatchType.I2);
                    if (distance <= threshold)
                    {
                        var r = CvInvoke.BoundingRectangle(imgContours[i]);
                        img.Draw(r, new Bgr(0, 0, 255), 2);
                        CvInvoke.PutText(img, distance.ToString(".####")
                            , new Point(r.X + 10, r.Y + 20), FontFace.HersheyPlain, 1.5, new MCvScalar(0, 255, 0));
                    }

                }
                AddImage(gray.Convert<Bgr, byte>(), "Gray");
                pictureBox1.Image = img.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void applyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyShapeDetection();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void settingsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                FormParameterShape form = new FormParameterShape();
                form.OnApplied += ApplyShapeDetection;
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void calcuateExtremePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
             {
                string path = @"F:\AJ Data\img\irregular.jpg";
                var img = new Image<Gray, byte>(path)
                    .ThresholdBinary(new Gray(100), new Gray(255));

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat h = new Mat();

                CvInvoke.FindContours(img, contours, h, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                var ExtremePoints = FindExtremePoints(contours[0].ToArray());

                var rgbImg = img.Convert<Bgr, byte>();
                CvInvoke.DrawMarker(rgbImg, ExtremePoints[0], new MCvScalar(0, 0, 255), MarkerTypes.Diamond,10,5);
                CvInvoke.DrawMarker(rgbImg, ExtremePoints[1], new MCvScalar(0, 0, 255), MarkerTypes.Diamond, 10, 5);
                CvInvoke.DrawMarker(rgbImg, ExtremePoints[2], new MCvScalar(0, 0, 255), MarkerTypes.Diamond, 10, 5);
                CvInvoke.DrawMarker(rgbImg, ExtremePoints[3], new MCvScalar(0, 0, 255), MarkerTypes.Diamond, 10, 5);

                LineSegment2D line1 = new LineSegment2D(ExtremePoints[0], ExtremePoints[2]);
                LineSegment2D line2 = new LineSegment2D(ExtremePoints[1], ExtremePoints[3]);

                var EDx = CalculateEuclideanDistance(ExtremePoints[0], ExtremePoints[2]);
                var EDy = CalculateEuclideanDistance(ExtremePoints[1], ExtremePoints[3]); 

                var midPoint1 = new Point(ExtremePoints[0].X+Math.Abs(ExtremePoints[0].X - ExtremePoints[2].X)/2-80,
                    ExtremePoints[0].Y + Math.Abs(ExtremePoints[0].Y - ExtremePoints[2].Y) / 2-10);

                var midPoint2 = new Point(ExtremePoints[1].X + Math.Abs(ExtremePoints[1].X - ExtremePoints[3].X) / 2  +20,
                   ExtremePoints[1].Y + Math.Abs(ExtremePoints[1].Y - ExtremePoints[3].Y) / 2 - 10);

                Image<Gray, byte> imgTemp1 = img.CopyBlank();
                imgTemp1.Draw(line1, new Gray(255), 1);
                var count1 = imgTemp1.CountNonzero();

                Image<Gray, byte> imgTemp2 = img.CopyBlank();
                imgTemp2.Draw(line2, new Gray(255), 1);
                var count2 = imgTemp2.CountNonzero();

                rgbImg.Draw(line1, new Bgr(0, 0, 255), 2);
                rgbImg.Draw(line2, new Bgr(0, 0, 255), 2);

                CvInvoke.PutText(rgbImg, EDx.ToString(".##"), midPoint1, FontFace.HersheyPlain, 1.5, new MCvScalar(0, 255, 0));
                CvInvoke.PutText(rgbImg, EDy.ToString(".##"), midPoint2, FontFace.HersheyPlain, 1.5, new MCvScalar(0, 255, 0));

                CvInvoke.PutText(rgbImg, count1[0].ToString(".##"),new Point( midPoint1.X,midPoint1.Y+30), FontFace.HersheyPlain, 1.5, new MCvScalar(0, 255, 0));
                CvInvoke.PutText(rgbImg, count2[0].ToString(".##"), new Point(midPoint2.X, midPoint2.Y + 20), FontFace.HersheyPlain, 1.5, new MCvScalar(0, 255, 0));

                pictureBox1.Image = rgbImg.AsBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private double CalculateEuclideanDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
        private List<Point> FindExtremePoints(Point [] points)
        {
            try
            {
                var maxX = points.Max(x => x.X);
                var minX = points.Min(x => x.X);
                var maxY = points.Max(x => x.Y);
                var minY = points.Min(x => x.Y);

                var Left = points.Where(p => p.X == minX).FirstOrDefault();
                var Top = points.Where(p => p.Y == minY).FirstOrDefault();
                var Right = points.Where(p => p.X == maxX).FirstOrDefault();
                var Bottom = points.Where(p => p.Y == maxY).FirstOrDefault();

                return new List<Point> { Left, Top, Right, Bottom };
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        private void orientationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path = @"F:\AJ Data\img\irregular1.jpg";
                var im = new Image<Gray, byte>(path);
                pictureBox1.Image = im.AsBitmap();

                if (pictureBox1.Image == null) return;
                var img = new Bitmap(pictureBox1.Image).ToImage<Gray, byte>()
                    .ThresholdBinaryInv(new Gray(20), new Gray(255));

                Mat m = new Mat();
                CvInvoke.FindContours(img, contoursGlobal, m, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                if (contoursGlobal.Size==0) return;

                var rotrect = CvInvoke.FitEllipse(contoursGlobal[0]);
                var bbox = CvInvoke.BoundingRectangle(contoursGlobal[0]);
                var temp = img.Convert<Bgr, byte>();
                var pointsF = rotrect.GetVertices();
                var points = Array.ConvertAll<PointF, Point>(pointsF, Point.Round); 

                temp.Draw(new CircleF(rotrect.Center, 5), new Bgr(0, 0, 255), -1);
                temp.Draw(bbox, new Bgr(0, 255, 0), 2);
                temp.DrawPolyline(points, true, new Bgr(255, 0, 0), 3);

                Mat line = new Mat();
                CvInvoke.FitLine(new VectorOfPoint(points), line, Emgu.CV.CvEnum.DistType.L2, 0, 0, 0.01);
                pictureBox1.Image = temp.AsBitmap();

                LineSegment2D line1 = new LineSegment2D(points[1], points[2]);
                LineSegment2D line2 = new LineSegment2D(points[0], points[1]);

                var img1 = img.CopyBlank();
                img1.Draw(line1, new Gray(255), 1);
                var w = img1.CountNonzero();
                img1.SetZero();
                img1.Draw(line2, new Gray(255), 1);
                var h = img1.CountNonzero();

                MessageBox.Show(string.Format("Width = {0}, Height = {1}",w[0], h[0]));
                pictureBox1.Image = img1.AsBitmap();

                // mean

                var img2 = img.CopyBlank();
                CvInvoke.DrawContours(img2, contoursGlobal, 0, new MCvScalar(255), -1);
                pictureBox1.Image = img2.AsBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void findMeanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path = @"F:\AJ Data\img\irregular2.jpg";

                var img = new Image<Bgr, byte>(path);
                img._SmoothGaussian(3);

                AddImage(img.Clone(), "Input");
                var gray = img.Convert<Gray, byte>()
                    .ThresholdBinaryInv(new Gray(240), new Gray(255));
                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat h = new Mat();

                CvInvoke.FindContours(gray, contours, h, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                var output = img.CopyBlank();
                var mask = gray.CopyBlank();
                for (int i = 0; i < contours.Size; i++)
                {
                    CvInvoke.DrawContours(mask, contours, i, new MCvScalar(255), -1);
                    var scaler = CvInvoke.Mean(img, mask);
                    img.SetValue(new Bgr(scaler.V0, scaler.V1, scaler.V2), mask);
                    mask.SetZero();
                }
                AddImage(img, "Output");
                pictureBox1.Image = img.AsBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void faceRecognitionHogSVMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
        private void ApplyFaceRecognitionHoG()
        {
            try
            {
                if (testData==null || trainData==null)
                {
                    MessageBox.Show("Please load data...");
                    return;
                }
                //winSize.width - blockSize.width) % blockStride.width == 0 && (winSize.height - blockSize.height) % blockStride.height == 0

                HOGDescriptor hog = new HOGDescriptor(new Size(192, 192 * 2), new Size(32, 32), new Size(8, 8), new Size(16, 16));
                //hog.SetSVMDetector(HOGDescriptor.GetDefaultPeopleDetector());

                List<float[]> hogTrainList = new List<float[]>();
                List<int> labelsTrainList = new List<int>();
                foreach (var item in trainData)
                {
                    var features = hog.Compute(item.Data.Resize(192, 192 * 2, Inter.Linear));
                    hogTrainList.Add(features);
                    labelsTrainList.Add(item.Label);
                }


                x_train = new Emgu.CV.Matrix<float>(helperFunctions.To2D<float>(hogTrainList.ToArray()));
                y_train = new Emgu.CV.Matrix<int>(labelsTrainList.ToArray());


                // Preprocessing normalize data
                CvInvoke.Normalize(x_train, x_train, 0, 1, NormType.MinMax);


                // create ANN
                SVMModel = new Emgu.CV.ML.SVM();
                bool useTrained = true;
                if (File.Exists("face_svm") && useTrained == true)
                {
                    SVMModel.Load("face_svm");
                    MessageBox.Show("Model Loaded.");
                }
                else
                {
                    SVMModel.SetKernel(SVM.SvmKernelType.Rbf);
                    SVMModel.Type = SVM.SvmType.CSvc;
                    SVMModel.TermCriteria = new MCvTermCriteria(1000, 0.0001);
                    SVMModel.C = 250;
                    SVMModel.Gamma = 0.001;

                    TrainData TrainData = new TrainData(x_train, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, y_train);
                    if (SVMModel.Train(TrainData))
                    {
                        SVMModel.Save("face_svm");
                        //MessageBox.Show("Model Trained");
                    }
                    else
                    {
                        MessageBox.Show("Error.");
                    }
                }

                List<float[]> hogTestList = new List<float[]>();
                List<int> labelsTestList = new List<int>();
                foreach (var item in testData)
                {
                    var features = hog.Compute(item.Data.Resize(192, 192 * 2, Inter.Linear));
                    hogTestList.Add(features);
                    labelsTestList.Add(item.Label);
                }
                x_test = new Emgu.CV.Matrix<float>(helperFunctions.To2D<float>(hogTestList.ToArray()));
                y_test = new Emgu.CV.Matrix<int>(labelsTestList.ToArray());
                CvInvoke.Normalize(x_test, x_test, 0, 1, NormType.MinMax);

                PredictedLabel = new List<int>();
                ActualLabel = new List<int>();

                for (int i = 0; i < x_test.Rows; i++)
                {
                    var prediction = SVMModel.Predict(x_test.GetRow(i));
                    //output += "Actual" +testData[i].Label + 
                    //    " Predict:" + prediction.ToString(CultureInfo.InvariantCulture)+"\n";
                    PredictedLabel.Add((int)prediction);
                    ActualLabel.Add(testData[i].Label);
                }

                MessageBox.Show("Model is loaded and generated predictions.");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private void pedestrianDetectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //1. download haarcascade_fullbody.xml
            // https://github.com/opencv/opencv/tree/master/data/haarcascades

        }

        private void haarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //if (videoCapture== null)
                //{
                //    return;
                //}
                ProcessFrames();

                //string rootDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
                //string cascadePath = rootDirectory + "/data/haarcascade_fullbody.xml";
                //cascadeClassifier= new CascadeClassifier(cascadePath);

                //var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();
                //var imgGray = img.Convert<Gray, byte>();

                //videoCapture = new VideoCapture(@"F:\AJ Data\img\videos\walking.avi");
               // videoCapture.ImageGrabbed += ProcessFrames;
                //videoCapture.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ProcessFrames()
        {
            string rootDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
            string cascadePath = rootDirectory + "/data/haarcascade_fullbody.xml";
            cascadeClassifier = new CascadeClassifier(cascadePath);
            ImageViewer viewer = new ImageViewer();
            Mat frame = null;
            using (videoCapture = new VideoCapture(@"F:\AJ Data\img\videos\walking.avi"))
            {
                System.Windows.Forms.Application.Idle += delegate (object sender, EventArgs e)
                {
                    frame = videoCapture.QueryFrame();

                    if (frame != null)
                    {
                        var gray = frame.ToImage<Gray, byte>();
                        var bboxes = cascadeClassifier.DetectMultiScale(gray,2,3);

                        foreach (var rect in bboxes)
                        {
                            CvInvoke.Rectangle(frame, rect, new MCvScalar(0, 0, 255), 2);
                        }
                        viewer.Image = frame;
                        Thread.Sleep(10);
                    }
                };
                viewer.ShowDialog();
            }
        }
        private void carDetectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //https://gist.github.com/199995/37e1e0af2bf8965e8058a9dfa3285bc6
        }

        private void mOG2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyBackgroundSubtraction();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ApplyBackgroundSubtraction(int method=0)
        {
            try
            {
                //https://docs.opencv.org/master/d1/dc5/tutorial_background_subtraction.html
                //BackgroundSubtractorKNN subtractorKNN = new BackgroundSubtractorKNN(500, 1000, false);
                //BackgroundSubtractorMOG2 bgsubtractor = new BackgroundSubtractorMOG2(500, 10, false);
                BackgroundSubtractorGMG bgsubtractor = new BackgroundSubtractorGMG(5,0.8);
                ImageViewer viewer = new ImageViewer();

                Mat frame = null;
                using (videoCapture = new VideoCapture(@"F:\AJ Data\img\videos\walking.avi"))
                {
                    System.Windows.Forms.Application.Idle += delegate (object sender, EventArgs e)
                    {
                        frame = videoCapture.QueryFrame();

                        if (frame != null)
                        {
                            var gray = frame.ToImage<Gray, byte>();
                            var fgMask = new Mat();
                            bgsubtractor.Apply(frame, fgMask);
                            var fMask = fgMask.ToImage<Gray, byte>()
                            .Erode(1)
                            .Dilate(1);

                            var contours = new VectorOfVectorOfPoint();
                            var h = new Mat();
                            CvInvoke.FindContours(fgMask, contours, h, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                            var filteredContours = FilterContours(contours, 500);
                            
                            fMask.SetZero();
                            CvInvoke.DrawContours(fMask, filteredContours, -1, new MCvScalar(255), -1);
                            var merged = new Mat();
                            CvInvoke.HConcat(frame, fMask.Convert<Bgr, byte>(), merged);
                            viewer.Image = merged;
                            Thread.Sleep(10);
                        }
                    };
                    viewer.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void kNNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyBackgroundSubtraction(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void trainModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyFaceRecognitionHoG();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void testModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (ActualLabel == null || PredictedLabel == null)
                {
                    return;
                }
                Random rand = new Random();
                int index = rand.Next(PredictedLabel.Count);
                
                var img = trainData.Where(x=>x.Label== PredictedLabel[index]).FirstOrDefault().Data;
                var testimg = testData[index].Data;
                var output = new Mat();
                CvInvoke.HConcat(img, testimg, output);
                var bgr = output.ToImage<Bgr, byte>();
                CvInvoke.PutText(bgr, "Pred:" + PredictedLabel[index] + " Act:" + ActualLabel[index],
                    new Point(10, 20), FontFace.HersheySimplex, 1.0, new MCvScalar(0, 0, 255), 2);
                pictureBox1.Image = bgr.ToBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void evaluateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyEvaluation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ApplyEvaluation()
        {
            try
            {
                if (ActualLabel == null || PredictedLabel == null)
                {
                    return;
                }

                if (ActualLabel.Count != PredictedLabel.Count)
                {
                    return;
                }

                
                var cm = helperFunctions.ComputeConfusionMatrix(ActualLabel.ToArray(), PredictedLabel.ToArray());
                var metrics = helperFunctions.CalcluateMetrics(cm,ActualLabel.ToArray(), PredictedLabel.ToArray());

                string text = string.Format("Test Samples = {0} \nAccuracy = {1:0.00}%\nPrecision ={2:0.00}%\nRecall = {3:0.00}% \n"
                    ,ActualLabel.Count, metrics[0] * 100, metrics[1] * 100, metrics[2] * 100);
                //MessageBox.Show("Accuracy = " + ((float)count)/ActualLabel.Count*100);

                if (formConfusionMatrix==null || formConfusionMatrix.IsDisposed)
                {
                    formConfusionMatrix = new FormConfusionMatrix(cm, text);
                }
                
                formConfusionMatrix.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void loadDataToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                LoadFaceData();
                this.Cursor = Cursors.Default;
                lblStatus.Text = "Data Loaded.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void LoadFaceData()
        {
            try
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var list = EmguHelper.LoadData(dialog.SelectedPath);
                    (trainData, testData) = EmguHelper.TestTrainSplit(list);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode==Keys.Up)
                {
                    if (ActualLabel == null || PredictedLabel == null)
                    {
                        return;
                    }
                    if (RandomIndex>= PredictedLabel.Count)
                    {
                        RandomIndex = 0;
                    }
                    var img = trainData.Where(x => x.Label == PredictedLabel[RandomIndex]).FirstOrDefault().Data;
                    var testimg = testData[RandomIndex].Data;
                    var output = new Mat();
                    CvInvoke.HConcat(img.Resize(243,320,Inter.Cubic), testimg.Resize(243, 320, Inter.Cubic), output);
                    var bgr = output.ToImage<Bgr, byte>();
                    CvInvoke.PutText(bgr, "Pred:" + PredictedLabel[RandomIndex] + " Act:" + ActualLabel[RandomIndex],
                        new Point(100, 30), FontFace.HersheySimplex, 0.6, new MCvScalar(0, 0, 255), 2);
                    pictureBox1.Image = bgr.ToBitmap();
                    RandomIndex++;
                }
            }
            catch (Exception )
            {
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (x_train == null || y_train == null)
                {
                    return;
                }
                int InputDataSize = x_train.Cols;
                int NoClasses = 10;
                string ModelName = "ANN_MLP_MNIST";

                //lblStatus.Invoke(new Action(() => lblStatus.Text = "Loading Data..."));
                //(x_train, y_train) = cvsHelperClass.ReadCSV(pathTrain, true,LabelIndex:0);
                //(x_test, y_test) = cvsHelperClass.ReadCSV(pathTest, true, LabelIndex: 0);

                // Preprocessing normalize data
                CvInvoke.Normalize(x_train, x_train, 0, 1, NormType.MinMax);
                CvInvoke.Normalize(x_test, x_test, 0, 1, NormType.MinMax);

                var y_train_encoded = cvsHelperClass.Matrix2OneHotEncoding(y_train, NoClasses);

                // create ANN
                MLPModel = new Emgu.CV.ML.ANN_MLP();
                int[] layers = new int[] { InputDataSize, 50, 50, NoClasses };
                Emgu.CV.Matrix<int> LayerSize = new Emgu.CV.Matrix<int>(layers);

                MLPModel.SetLayerSizes(LayerSize);
                MLPModel.SetActivationFunction(ANN_MLP.AnnMlpActivationFunction.SigmoidSym, 0.6, 1);
                MLPModel.SetTrainMethod(ANN_MLP.AnnMlpTrainMethod.Backprop);
                MLPModel.TermCriteria = new MCvTermCriteria(1000, 0.001);

                TrainData TrainData = new TrainData(x_train, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, y_train_encoded);
                bool result = false;

                if (File.Exists(ModelName))
                {
                    MLPModel.Load(ModelName);
                    result = true;
                    lblStatus.Text = "Model Loaded...";
                }
                else
                {
                    if (MLPModel.Train(TrainData, 2))
                    {
                        if (!File.Exists(ModelName))
                        {
                            MLPModel.Save(ModelName);
                        }
                        lblStatus.Text = "Model Trained...";
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void generateTrainingSamplesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FormFaceTraining form = new FormFaceTraining();
                form.ShowDialog ();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void testTrainSplitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (Data_Set==null || Data_Label==null)
                {
                    return;
                }

                (x_train, y_train, x_test, y_test) = cvsHelperClass.TestTrainSplit(Data_Set, Data_Label,0.2f);
                lblStatus.Text = "Data split into test train";
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void trainModelToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (x_train == null || y_train == null)
                {
                    throw new Exception("No data was found for training.");
                }

                FormAnnParameters form = new FormAnnParameters();
                //var screen = Screen.FromPoint(Location);
                //form.Location = new Point( 20, this.Right - form.Width);
                form.OnAnnApply += ApplyANN;
                form.Show();
                //this.Cursor = Cursors.WaitCursor;
                //ApplyANN();
                //lblStatus.Text = "Training completed.";
                //this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //This function perform training and classification of image
        public void ApplyANN(ANN_MLP.AnnMlpActivationFunction ActivationFunction = ANN_MLP.AnnMlpActivationFunction.SigmoidSym,
                            ANN_MLP.AnnMlpTrainMethod TrainMethod = ANN_MLP.AnnMlpTrainMethod.Backprop,
                            float Momentum = 0.005f,
                            int Iterations = 1000,
                            float RMSE = 0.0001f,
                            int []layers=null,
                            bool LoadSavedModel = false)
        {
            try
            {
                
                int NoClasses = cvsHelperClass.GetClassCount(y_train);
                // make hot vector representation for each label
                // 0 1  = 0 = 1 0  
                //        1 = 0 1
                var HotVectors = cvsHelperClass.HotMatrix(y_train, NoClasses);
                //var decoded = cvsHelperClass.DecodeHotMatrix(HotVectors);

                //set input, hidden and output number of  neurons
                if (layers == null)
                {
                    layers = new int[] { x_train.Cols, 50, NoClasses };
                }
                //make sure the input and output classes are correct
                if (layers[0]!= x_train.Cols)
                {
                    throw new Exception("Input neurons must be " + x_train.Cols);
                }
                //make sure the input and output classes are correct
                if (layers[layers.Length-1] != NoClasses)
                {
                    throw new Exception("Output neurons must be " + NoClasses);
                }

                // 
                Emgu.CV.Matrix<int> LayerSize = new Emgu.CV.Matrix<int>(layers);

                //create object for Multilayer perceptron and set different featuers
                ANN= new ANN_MLP();
                ANN.SetTrainMethod(TrainMethod);
                ANN.SetLayerSizes(LayerSize);
                ANN.TermCriteria = new MCvTermCriteria(Iterations, RMSE);
                ANN.SetActivationFunction(ActivationFunction);
                ANN.BackpropMomentumScale = Momentum;
                // MinMax normalization
                //var x_train1 = x_train.CopyBlank();
                //CvInvoke.Normalize(x_train, x_train1, 0, 1, NormType.MinMax);
                // format the data according to TrainData class
                TrainData data = new TrainData(x_train, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample,
                    HotVectors.Convert<float>());

                string type = "ANN";
                bool result = false;
                // if a model is already save then you can load it otherwise train the ANN
                if (LoadSavedModel)
                {
                    if (File.Exists(type.ToString() + ".txt"))
                    {
                        FileStorage file = new FileStorage(type.ToString() + ".txt", FileStorage.Mode.Read);
                        ANN.Read(file.GetNode("opencv_ml_ann_mlp"));
                        lblStatus.Text = "Model loaded.";

                    }
                }
                else
                {
                    result = ANN.Train(data, (int)Emgu.CV.ML.MlEnum.AnnMlpTrainingFlag.Default);
                    if (result)
                    {
                        ANN.Save(type.ToString() + ".txt");
                        lblStatus.Text = "Model trained.";
                    }
                    else
                    {
                        lblStatus.Text = "Model could not be trained.";
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void testModelToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (x_test==null || y_test==null || ANN==null)
                {
                    return;
                }

                // MinMax normalization
                //var x_test1 = x_test.CopyBlank();
                //CvInvoke.Normalize(x_test, x_test1, 0, 1, NormType.MinMax);

                PredictedLabel = new List<int>();
                ActualLabel = new List<int>();

                for (int i = 0; i < x_test.Rows; i++)
                {
                    var pred =  ANN.Predict(x_test.GetRow(i));
                    PredictedLabel.Add((int)pred);
                    ActualLabel.Add(y_test[i, 0]);
                }
                lblStatus.Text = "Testing is completed.";
                
                formConfusionMatrix = null;
                ApplyEvaluation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void evaluateToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyEvaluation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void concatenateImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                List<Image<Bgr, byte>> list = new List<Image<Bgr, byte>>();
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = true;
                dialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"; 
                if (dialog.ShowDialog()==DialogResult.OK)
                {
                    foreach (string file in dialog.FileNames)
                    {
                        list.Add(new Image<Bgr, byte>(file));
                    }
                }
                if (list.Count>1)
                {
                   var img =  cvsHelperClass.HConcatImages(list);
                    pictureBox1.Image = img.AsBitmap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void verticalConcatenationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                List<Image<Bgr, byte>> list = new List<Image<Bgr, byte>>();
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = true;
                dialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in dialog.FileNames)
                    {
                        list.Add(new Image<Bgr, byte>(file));
                    }
                }
                if (list.Count > 1)
                {
                    var img = cvsHelperClass.VConcatImages(list);
                    pictureBox1.Image = img.AsBitmap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void seamlessCloningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path1 = @"F:\AJ Data\Projects\emgucv\data\education.jpeg";
                string path2 = @"F:\AJ Data\Projects\emgucv\data\rabbit.jpg";

                var img1 = new Image<Bgr, byte>(path1);
                var img2 = new Image<Bgr, byte>(path2);

                var img = cvsHelperClass.HConcatImages(img1, img2);
                pictureBox1.Image = img.AsBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void seamlessCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void stitchingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string folder = @"F:\Installedsoftwares\Matlab2020a\toolbox\vision\visiondata\building\";
            }
            catch (Exception ex)
            {

            }
        }

        private void cloneToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            //try
            //{
            //    string path1 = @"F:\AJ Data\Projects\emgucv\data\education.jpeg";
            //    string path2 = @"F:\AJ Data\Projects\emgucv\data\rabbit.jpg";

            //    var img1 = new Image<Bgr, byte>(path1);
            //    var img2 = new Image<Bgr, byte>(path2);

                
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

        private void seamlessCloningToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void selectImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (imgList.Count<1)
                {
                    return;
                }
                formSeamlessClone form = new formSeamlessClone(this);
                form.Show();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SeamlessClone(Image<Bgr, byte> img1, CloningMethod method)
        {
            try
            {
                var img = imgList["Input"].Clone();
                Mat output = new Mat();
                CvInvoke.SeamlessClone(img1, img, this.SeamlessMaskImage, this.SeamlessMaskLocation, output, method);
                pictureBox1.Image = output.ToBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void prepareDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    FolderBrowserDialog dialog = new FolderBrowserDialog();
                    //if (dialog.ShowDialog()==DialogResult.OK)
                    if (true)

                    {
                        //var files = Directory.GetFiles(dialog.SelectedPath);

                        var files = Directory.GetFiles(@"F:\AJ Data\Data\yalefaces_Original");

                        foreach (var file in files)
                        {
                            var img = new Image<Gray, byte>(file).Resize(256,256,Inter.Cubic);
                            var name = Path.GetFileName(file);
                            int label = int.Parse(name.Substring(name.LastIndexOf(".") - 2, 2));
                            var index = dataset.FindIndex(x => x.Label == label);
                            if (index> -1)
                            {
                                dataset[index].Images.Add(img);
                            }
                            else
                            {
                                FaceData face = new FaceData();
                                face.Images = new List<Image<Gray, byte>>();
                                face.Images.Add(img);
                                face.Label = label;
                                dataset.Add(face);
                            }
                        }
                        MessageBox.Show("Data Loaded.");
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void testTrainSplitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataset==null)
                {
                    return;
                }
                

                (trainingData, testingData) =    cvsHelperClass.TestTrainSplit(dataset);
                MessageBox.Show("Split completed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void extractHOGFeaturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (trainingData.Count<1 || testingData.Count<1)
                {
                    MessageBox.Show("Not train test split performed.");
                    return;
                }

                (x_train,y_train) =  CalculateHoGFeatures(trainingData);
                (x_test, y_test) = CalculateHoGFeatures(testingData);
                MessageBox.Show("Hog Features extracted.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private (Matrix<float>, Matrix<int>) CalculateHoGFeatures(List<FaceData> data)
        {
            try
            {
                //HOGDescriptor hog = new HOGDescriptor();
                HOGDescriptor hog = new HOGDescriptor(new Size(256, 256),new Size(32,32),new Size(16,16),new Size(8,8));

                List<float[]> hogTrainList = new List<float[]>();
                List<int> labelsTrainList = new List<int>();
                foreach (var item in data)
                {
                    foreach (var img in item.Images)
                    {
                        var features = hog.Compute(img);
                        hogTrainList.Add(features);
                        labelsTrainList.Add(item.Label);
                    }

                }


                var xtrain = new Emgu.CV.Matrix<float>(helperFunctions.To2D<float>(hogTrainList.ToArray()));
                var ytrain = new Emgu.CV.Matrix<int>(labelsTrainList.ToArray());
                return (xtrain, ytrain);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void trainSVMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (x_train == null || x_train.Rows <1)
                {
                    MessageBox.Show("Please load Training data...");
                    return;
                }

                // Preprocessing normalize data
                //CvInvoke.Normalize(x_train, x_train, 0, 1, NormType.MinMax);


                // create SVM
                SVMModel = new Emgu.CV.ML.SVM();
                bool useTrained = true;
                if (File.Exists("face_svm") && useTrained == true)
                {
                    SVMModel.Load("face_svm");
                    MessageBox.Show("Model Loaded.");
                }
                else
                {
                    SVMModel.SetKernel(SVM.SvmKernelType.Rbf);
                    SVMModel.Type = SVM.SvmType.CSvc;
                    SVMModel.TermCriteria = new MCvTermCriteria(1000, 0.0001);
                    SVMModel.C = 250;
                    SVMModel.Gamma = 0.001;

                    TrainData TrainData = new TrainData(x_train, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, y_train);
                    if (SVMModel.Train(TrainData))
                    {
                        SVMModel.Save("face_svm");
                        MessageBox.Show("Model Trained");
                    }
                    else
                    {
                        MessageBox.Show("Error.");
                    }
                }

                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void evaluationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                PredictedLabel = new List<int>();
                ActualLabel = new List<int>();

                for (int i = 0; i < x_test.Rows; i++)
                {
                    var prediction = SVMModel.Predict(x_test.GetRow(i));
                    PredictedLabel.Add((int)prediction);
                    ActualLabel.Add(y_test[i,0]);
                }

                MessageBox.Show("Model is loaded and generated predictions.");
                ApplyEvaluation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void showResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (PredictedLabel!=null && PredictedLabel.Count>0)
                {
                    Random random = new Random();
                    int index =  random.Next(PredictedLabel.Count - 1);
                    int predLabel = PredictedLabel[index];
                    int actualLabel = ActualLabel[index];

                    var predImage = (from img in testingData
                                     where img.Label == predLabel
                                     select img.Images[random.Next(img.Images.Count)])
                                     .FirstOrDefault().Clone();
                    var actualImage = (from img in testingData
                                     where img.Label == actualLabel
                                     select img.Images[random.Next(img.Images.Count)])
                                     .FirstOrDefault().Clone();

                    CvInvoke.PutText(predImage, "Predicted", new Point(30, 30), FontFace.HersheyPlain, 1.0, new MCvScalar(0, 0, 255));
                    CvInvoke.PutText(actualImage, "Actual", new Point(30, 30), FontFace.HersheyPlain, 1.0, new MCvScalar(0, 0, 255));

                    var output = cvsHelperClass.HConcatImages(predImage.Convert<Bgr, byte>(), actualImage.Convert<Bgr, byte>());
                    pictureBox1.Image = output.AsBitmap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void brightnessContralsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (imgList.Count==0 || imgList["Input"]==null)
                {
                    throw new Exception("Select an input image first.");
                }

                formBrightnessANDContrast form = new formBrightnessANDContrast();
                form.imgInput = imgList["Input"].Clone();
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mINSTDigitRecognitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formMnistRecogntion form = new formMnistRecogntion();
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }

        private void faceSwapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path1 = @"F:\AJ Data\img\faces\aj.jpg";
                string path2 = @"F:\AJ Data\img\faces\imrankhan.jpg";

                var img1 = new Image<Bgr, byte>(path1);
                var img2 = new Image<Bgr, byte>(path2);

                CvInvoke.EqualizeHist(img1[0], img1[0]);
                CvInvoke.EqualizeHist(img1[1], img1[1]);
                CvInvoke.EqualizeHist(img1[2], img1[2]);

                Mat output = new Mat();
                CvInvoke.HConcat(img1, img2, output);

                string rootDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
                string cascadePath = rootDirectory + "/data/lbpcascade_frontalface_improved.xml";
                string modelpath = rootDirectory + "/data/lbfmodel.yaml";
                CascadeClassifier classifier = new CascadeClassifier(cascadePath);

                var faces1 = classifier.DetectMultiScale(img1);
                VectorOfRect rect1 = new VectorOfRect(faces1);

                var faces2 = classifier.DetectMultiScale(img2);
                VectorOfRect rect2 = new VectorOfRect(faces2);

                // load the LBP model
                var modelParams = new FacemarkLBFParams();
                FacemarkLBF facemarkDetector = new FacemarkLBF(modelParams);
                facemarkDetector.LoadModel(modelpath);


                VectorOfVectorOfPointF lm1 = new VectorOfVectorOfPointF();
                bool result1 = facemarkDetector.Fit(img1, rect1, lm1);

                VectorOfVectorOfPointF lm2 = new VectorOfVectorOfPointF();
                bool result2 = facemarkDetector.Fit(img2, rect2, lm2);


                if (result1 && result2)
                {
                    //for (int i = 0; i < faces1.Length; i++)
                    //{
                    //    //CvInvoke.Rectangle(img, faces[i], new MCvScalar(0, 0, 255), 2);
                    //    FaceInvoke.DrawFacemarks(img1, landmarks1[i], new MCvScalar(0, 0, 255));
                    //    //var rec = CvInvoke.BoundingRectangle(landmarks[i]);
                    //    // CvInvoke.Rectangle(img, rec, new MCvScalar(0, 0, 255), 2);

                    //}
                    //for (int i = 0; i < faces2.Length; i++)
                    //{
                    //    //CvInvoke.Rectangle(img, faces[i], new MCvScalar(0, 0, 255), 2);
                    //    FaceInvoke.DrawFacemarks(img2, landmarks2[i], new MCvScalar(0, 255, 0));
                    //    //var rec = CvInvoke.BoundingRectangle(landmarks[i]);
                    //    // CvInvoke.Rectangle(img, rec, new MCvScalar(0, 0, 255), 2);

                    //}

                    //VectorOfPoint hull = new VectorOfPoint();
                    //CvInvoke.ConvexHull(biggestcontour, hull);

                    ////CvInvoke.DrawContours(img, hull, -1, new MCvScalar(0, 0, 255), 3);
                    //CvInvoke.Polylines(img, hull.ToArray(), true, new MCvScalar(0, 0.0, 255.0), 3);

                    // round off the landmarks
                    var LandMarks1 = EmguHelper.VectorOfPointF2RoundedVectorOfPointF(lm1[0]);
                    var LandMarks2 = EmguHelper.VectorOfPointF2RoundedVectorOfPointF(lm2[0]);


                    VectorOfPoint hull1 = new VectorOfPoint();
                    CvInvoke.ConvexHull(EmguHelper.VectorOfPointF2VectorOfPoint(LandMarks1), hull1);

                    VectorOfPoint hull2 = new VectorOfPoint();
                    CvInvoke.ConvexHull(EmguHelper.VectorOfPointF2VectorOfPoint(LandMarks2), hull2);

                    var lmarray1 = LandMarks1.ToArray();
                    //CvInvoke.Polylines(img1, hull1.ToArray(), true, new MCvScalar(0, 0.0, 255.0), 3);
                    //CvInvoke.Polylines(img2, hull2.ToArray(), true, new MCvScalar(0, 0.0, 255.0), 3);

                    var mask = img2.CopyBlank().Convert<Gray, byte>();
                    mask.Draw(hull1.ToArray(), new Gray(255), -1);

                    //Delaunay triangulation
                    //Subdiv2D subdiv2D1 = new Subdiv2D(EmguHelper.VectorOfPoint2PointF(hull1), true);
                    //var triangles1= subdiv2D1.GetDelaunayTriangles();

                    Subdiv2D subdiv2D2 = new Subdiv2D(EmguHelper.VectorOfPoint2PointF(hull2), true);
                    var triangles2 = subdiv2D2.GetDelaunayTriangles();

                    //draw Delauny trangles
                    foreach (var item in triangles2)
                    {
                        Point[] points = Array.ConvertAll<PointF, Point>(item.GetVertices(), Point.Round);
                        CvInvoke.Polylines(img2, points, true, new MCvScalar(0, 0, 255), 1);

                    }
                    // warp the triangles in 2nd image to 1st
                    foreach (var triangle in triangles2)
                    {
                        VectorOfPointF tri1 = new VectorOfPointF();
                        VectorOfPointF  tri2 = new VectorOfPointF();

                        //hull1[triangle.]
                    }
                    //var im = img2.And(mask.Convert<Bgr, byte>(),     mask);
                    pictureBox1.Image = img2.ToBitmap();

                    //List<Triangle2DF> tri2 = new List<Triangle2DF>(); 
                    //for (int i = 0; i < triangles1.Length; i++)
                    //{
                    //    var ind1 = lmarray1.IndexOf(triangles1[i].V0);
                    //    var ind2 = lmarray1.IndexOf(triangles1[i].V1);
                    //    var ind3 = lmarray1.IndexOf(triangles1[i].V2);

                    //    Triangle2DF t = new Triangle2DF(LandMarks2[ind1], LandMarks2[ind2], LandMarks2[ind3]);
                    //    tri2.Add(t);
                    //}


                    //var t1 = EmguHelper.PointF2Point(triangles1[10].GetVertices());
                    //var t2 = EmguHelper.PointF2Point(tri2[10].GetVertices());
                    //var r = CvInvoke.BoundingRectangle(new VectorOfPoint(t1));
                    //CvInvoke.Line(img2, t1[0], t1[1],new MCvScalar(0,0,255), 1);
                    //CvInvoke.Line(img2, t1[1], t1[2], new MCvScalar(0, 0, 255), 1);
                    //CvInvoke.Line(img2, t1[2], t1[0], new MCvScalar(0, 0, 255), 1);
                    //img1.ROI = r;

                    //var temp = img1.Copy();
                    //img1.ROI = Rectangle.Empty;

                    //CvInvoke.FillConvexPoly(img1, new VectorOfPoint(t2), new MCvScalar(255,255,255));
                    //pictureBox1.Image = output.ToBitmap();

                    //var M = CvInvoke.GetAffineTransform(LandMarks1, LandMarks2);
                    //var warped_triangle = new Mat();
                    //CvInvoke.WarpAffine(temp, warped_triangle, M, img2.Size);
                    //CvInvoke.BitwiseAnd(warped_triangle, warped_triangle, warped_triangle);
                    //CvInvoke.SeamlessClone(img1,img2,mask,)
                    //warped_triangle = cv2.warpAffine(cropped_triangle, M, (w, h))
                    //warped_triangle = cv2.bitwise_and(warped_triangle, warped_triangle, mask = cropped_tr2_mask)
                    //img2.ROI = faces2[0];
                    //var temp = img2.Copy();
                    //img2.ROI = Rectangle.Empty;

                    //img1.ROI = faces1[0];
                    //temp = temp.Resize(faces1[0].Width, faces1[0].Height, Inter.Cubic);
                    ////img1.SetValue()
                    //temp.CopyTo(img1);
                    //img1.ROI = Rectangle.Empty;

                }


                //Mat output = new Mat();
                //CvInvoke.HConcat(img1, img2, output);
                //pictureBox1.Image = output.ToBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void loadDataToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                // data set 
                //https://www.kaggle.com/uciml/pima-indians-diabetes-database
                string path = @"F:\AJ Data\Data\diabetes.csv";
               (Data_Set,  Data_Label)  = cvsHelperClass.ReadCSV(path, true, ',', 8);
                lblStatus.Text = "Data loaded.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mLPToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void ShowRandomImages(int[]preds)
        {
            //// show some images

            //int Min = 0;
            //int Max = y_test.Rows - 1;
            //int N = 10; // images to show
            //Random randNum = new Random();

            //var testdata = x_test.Data;

            //var TestIndex = Enumerable
            //    .Repeat(0, N)
            //    .Select(i => randNum.Next(Min, Max))
            //    .ToList();
            //// show some data
            //var img = new Image<Gray, float>(28*N, 28*N);
            //for (int i = 0; i < TestIndex.Count; i++)
            //{
            //    var row = testdata.GetRow(TestIndex[i]);
            //    var temp = new Emgu.CV.Matrix<float>(row)
            //        .Reshape(1, 28)
            //        .Mat.ToImage<Gray, float>()
            //        .Mul(255);
            //    img.ROI = new Rectangle(i * 28, 0, 28, 28);
            //    temp.CopyTo(img);
            //    img.ROI = Rectangle.Empty;
            //    CvInvoke.PutText(img, string.Format("{0}", preds[TestIndex[i]]), new Point(i * 28, 50), FontFace.HersheyPlain, 1.0, new MCvScalar(255));

            //}

            //pictureBox1.Image = img.AsBitmap();
        }
        private void mNISTClassificationToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private async void ProcessROI()
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                if (rect == null)
                {
                    MessageBox.Show("Select an ROI");
                    return;
                }

                var img = new Bitmap(pictureBox1.Image)
                   .ToImage<Bgr, byte>();

                img.ROI = rect;
                var img2 = img.Copy();
                var img3 = img2.SmoothGaussian(25);


                //var blue = img2.SmoothGaussian(15).Canny(150, 50)
                //Image<Bgr, byte> img3 = new Image<Bgr, byte>(new Image<Gray, byte>[] {
                //blue,
                //blue,
                //blue });

                img.SetValue(new Bgr(1, 1, 1));
                img._Mul(img3);

                img.ROI = Rectangle.Empty;
                pictureBox1.Image = img.AsBitmap();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void templateMatchinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    if (pictureBox1.Image == null)
            //    {
            //        return;
            //    }

            //    if (rect == null)
            //    {
            //        MessageBox.Show("Select a template");
            //        return;
            //    }

            //    var img = new Bitmap(pictureBox1.Image)
            //        .ToImage<Bgr, byte>();

            //    img.ROI = rect;
            //    var template = img.Copy();
            //    //template = template.Resize(0.5, Emgu.CV.CvEnum.Inter.Cubic);
            //    template = template.Rotate(90, new Bgr(0, 0, 0));
            //    img.ROI = Rectangle.Empty;

            //    CvInvoke.Imshow("Template", template);

            //    Mat imgout = new Mat();
            //    CvInvoke.MatchTemplate(img, template, imgout, Emgu.CV.CvEnum.TemplateMatchingType.Sqdiff);

            //    double minVal = 0;
            //    double maxVal = 0;
            //    Point minLoc = new Point();
            //    Point maxLoc = new Point();

            //    CvInvoke.MinMaxLoc(imgout, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

            //    Rectangle r = new Rectangle(minLoc, template.Size);
            //    CvInvoke.Rectangle(img, r, new MCvScalar(255, 0, 0), 3);
            //    pictureBox1.Image = img.AsBitmap();

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null)
                {
                    return;
                }

                // for inpaint
                if (InpaintMouseDown==true && InpaintSelection==true)
                {
                    if (InpaintCurrentPoints.Count>=1)
                    {
                        using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                        {
                            Pen p = new Pen(Brushes.Red, 5);
                            g.DrawLine(p, InpaintCurrentPoints.Last(), e.Location);
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            //SmoothingMode.HighQuality;

                        }
                    }
                    InpaintCurrentPoints.Add(e.Location);
                    pictureBox1.Invalidate();
                }
                if (showPixelValue)
                {
                    if (e.X < cols && e.Y < rows)
                    {
                        Bgr bgr = imgList["Input"][e.Y, e.X];
                        lblBGR.Text = "B G R: " + bgr.Blue + " " + bgr.Green + " " + bgr.Red;
                    }
                }
                if (IsSelectingRectangle && IsMouseDown)
                {
                    int x = Math.Min(MouseDownLocation.X, e.X);
                    int y = Math.Min(MouseDownLocation.Y, e.Y);

                    int width = Math.Max(MouseDownLocation.X, e.X) - Math.Min(MouseDownLocation.X, e.X);

                    int height = Math.Max(MouseDownLocation.Y, e.Y) - Math.Min(MouseDownLocation.Y, e.Y);
                    rect = new Rectangle(x, y, width, height);
                    Refresh();
                }

                if (IsSelectingGrabCutRectangle && IsMouseDown)
                {
                    int x = Math.Min(MouseDownLocation.X, e.X);
                    int y = Math.Min(MouseDownLocation.Y, e.Y);

                    int width = Math.Max(MouseDownLocation.X, e.X) - Math.Min(MouseDownLocation.X, e.X);

                    int height = Math.Max(MouseDownLocation.Y, e.Y) - Math.Min(MouseDownLocation.Y, e.Y);
                    rect = new Rectangle(x, y, width, height);
                    Refresh();
                }
                if (SelectROI)
                {
                    int x = Math.Min(StartROI.X, e.X);
                    int y = Math.Min(StartROI.Y, e.Y);

                    int width = Math.Max(StartROI.X, e.X) - Math.Min(StartROI.X, e.X);

                    int height = Math.Max(StartROI.Y, e.Y) - Math.Min(StartROI.Y, e.Y);
                    rect = new Rectangle(x, y, width, height);
                    Refresh();
                }

                if ((selectBackground || selectForeground) && IsMouseDown )
                {
                    Pen pen = null;
                    if (selectBackground)
                    {
                        pen = new Pen(Color.Blue, 5);

                    }
                    if (selectForeground)
                    {
                        pen = new Pen(Color.Red, 5);
                    }

                    if (MouseUpLocation != Point.Empty)
                    {
                        using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                        {
                            g.DrawLine(pen, MouseUpLocation, e.Location);
                            g.SmoothingMode = SmoothingMode.HighQuality;
                        }
                    }

                    pictureBox1.Invalidate();
                    MouseUpLocation = e.Location;
                    currentPolygon.Add(e.Location);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
           
            