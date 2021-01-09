using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SemanticSegmentationNET
{
    public partial class formSeamlessClone : Form
    {
        Form1 form=null;
        Image<Bgr, byte> imgInput = null;
        Image<Bgr, byte> mask= null;

        Point start, end;
        List<Point> points = new List<Point>();
        bool isSelecting = false;
        bool MouseDown = false;
        public formSeamlessClone(Form1 f)
        {
            InitializeComponent();
            form = f;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                isSelecting = false;
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Image Files |*.jpg;*.png;*.bmp;";
                if (dialog.ShowDialog()==DialogResult.OK)
                {
                    imgInput = new Image<Bgr, byte>(dialog.FileName);
                    pictureBox1.Image = imgInput.AsBitmap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (imgInput==null || mask==null)
            {
                return;
            }

            if (form==null)
            {
                return;
            }

            form.SeamlessMaskImage = mask;
            if (form.SeamlessMaskLocation==Point.Empty)
            {
                MessageBox.Show("Click on a location in original image to clone at the position.");
                form.Focus();
                return;
            }

            if (comboBox1.SelectedIndex==-1)
            {
                MessageBox.Show("Select cloning method.");
                return;
            }
            var mode =  CloningMethod.Mixed;
            if (comboBox1.SelectedIndex==1)
            {
                mode = CloningMethod.MonochromeTransfer;
            }
            else
            {
                if (comboBox1.SelectedIndex==2)
                {
                    mode = CloningMethod.MonochromeTransfer;
                }
            }
            
            form.SeamlessClone(imgInput, mode);
        }

        private void formSeamlessClone_Load(object sender, EventArgs e)
        {
            try
            {
                isSelecting = false;
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Image Files |*.jpg;*.png;*.bmp;";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    imgInput = new Image<Bgr, byte>(dialog.FileName);
                    pictureBox1.Image = imgInput.AsBitmap();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                start = e.Location;
                MouseDown = true;
                points.Add(start);
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            points.Add(e.Location);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            end = e.Location;
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (isSelecting)
                {
                    //e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    //e.Graphics.Clear(Color.White);

                    // Draw the new polygon.
                    if (points != null)
                    {
                        // Draw the new polygon.
                        if (points.Count > 1)
                        {
                            e.Graphics.DrawLines(Pens.Red, points.ToArray());
                        }

                        // Draw the newest edge.
                        if (points.Count > 0)
                        {
                            using (Pen dashed_pen = new Pen(Color.Red))
                            {
                                dashed_pen.DashPattern = new float[] { 3, 3 };
                                e.Graphics.DrawLine(dashed_pen,
                                    points[points.Count - 1],
                                    end);
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {

                
            }
        }

        private void completeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (points.Count<2)
                {
                    return;
                }
                isSelecting = false;
                mask = imgInput.CopyBlank();
                points.Add(points[0]);
                //img.DrawPolyline(points.ToArray(),true,new Bgr(255,255,255),1);
                mask.Draw(points.ToArray(), new Bgr(255, 255, 255), -1);
                pictureBox1.Image = mask.AsBitmap();
                points.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            points.Clear();
            pictureBox1.Invalidate();
            if (form!=null)
            {
                form.SeamlessMaskImage = null;
                form.SeamlessMaskLocation = Point.Empty;
            }
        }

        private void resizeUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (imgInput!=null)
                {
                    imgInput =  imgInput.Resize(1.1, Emgu.CV.CvEnum.Inter.Cubic);
                    pictureBox1.Image = imgInput.AsBitmap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void resizeDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (imgInput != null)
                {
                    imgInput = imgInput.Resize(0.90, Emgu.CV.CvEnum.Inter.Cubic);
                    pictureBox1.Image = imgInput.AsBitmap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pictureBox1_DragOver(object sender, DragEventArgs e)
        {
           
        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
           
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            points.Clear();
            pictureBox1.Invalidate();
            if (form != null)
            {
                form.SeamlessMaskImage = null;
                form.SeamlessMaskLocation = Point.Empty;
            }
        }

        private void formSeamlessClone_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[]; // get all files droppeds  
            if (files != null && files.Count()>0)
            {
                var path = files.First();
                var ext = Path.GetExtension(path);
                if (ext.Contains(".jpg")|| ext.Contains(".jpeg"))
                {
                    Bitmap bitmap = new Bitmap(path);
                    pictureBox1.Image = bitmap;
                }
                
            }
        }

        private void formSeamlessClone_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void selectMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (imgInput==null)
                {
                    return;
                }
                isSelecting = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
           
        }
    }
}
