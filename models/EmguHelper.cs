using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticSegmentationNET.models
{
   public class EmguHelper
    {
        public static VectorOfPoint VectorOfPointF2VectorOfPoint(VectorOfPointF vectorOfPointF)
        {
            return new VectorOfPoint(Array.ConvertAll<PointF, Point>(vectorOfPointF.ToArray(), Point.Round));
        }
        public static VectorOfPointF VectorOfPointF2RoundedVectorOfPointF(VectorOfPointF vectorOfPointF)
        {
            return new VectorOfPointF(
                (from p in vectorOfPointF.ToArray()
                 select new PointF((float)Math.Round(p.X), (float)Math.Round(p.Y))).ToArray()
                );
        }
        public static PointF[] VectorOfPoint2PointF(VectorOfPoint points)
        {
            return Point2PointF(points.ToArray());
        }
        public static Point[] PointF2Point(PointF []points)
        {
            return Array.ConvertAll<PointF, Point>(points, Point.Round);
        }
        public static PointF[] Point2PointF(Point[] points)
        {
            return Array.ConvertAll(points, new Converter<Point, PointF>(PointToPointF));

        }
        private static PointF PointToPointF(Point pf)
        {
            return new PointF((pf.X), (pf.Y));
        }
        // for rounding
        private static PointF PointF2PointF(PointF pf)
        {
            var p = Point.Round(pf);
            return new PointF((float)p.X, (float)p.Y);
        }
        public static CascadeClassifier GetInstanceCascadeClassifier()
        {
            string rootDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
            string cascadePath = rootDirectory + "/data/haarcascade_frontalface_default.xml";
            return new CascadeClassifier(cascadePath);
        }

        public static List<FaceImageData> LoadData(string path)
        {
            List<FaceImageData> list = new List<FaceImageData>();
            var files =  Directory.GetFiles(path, "*.*");
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var label = int.Parse(name.Substring(name.Length - 2));
                var data = new FaceImageData();
                data.Name = name;
                data.Label = label;
                data.Data = new Emgu.CV.Image<Emgu.CV.Structure.Gray, byte>(file);
                list.Add(data);
            }
            return list;
        }
        public static Matrix<double> Image2Matrix(Image<Gray, byte> img)
        {
            Matrix<double> matrix = new Matrix<double>(img.Rows, img.Cols, img.NumberOfChannels);
            img.Convert<Gray, double>().CopyTo(matrix);
            return matrix;
        }
        public static Image<Gray, double> Matrix2Image(Matrix<double> matrix)
        {
            Image<Gray, double> output = new Image<Gray, double>(matrix.Rows,matrix.Cols);
            matrix.CopyTo(output);
            return output;
        }
        public static (List<FaceImageData>, List<FaceImageData>) TestTrainSplit(List<FaceImageData> list)
        {
            try
            {
                var TrainData = new List<FaceImageData>();
                var TestData = new List<FaceImageData>();
                for (int i = 0; i < list.Count; i++)
                {
                    if (i%11==0)
                    {
                        TestData.Add(list[i]);
                    }
                    else
                    {
                        TrainData.Add(list[i]);
                    }
                }

                return (TrainData, TestData);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
