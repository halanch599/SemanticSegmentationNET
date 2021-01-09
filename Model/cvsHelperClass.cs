
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using SemanticSegmentationNET.models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SemanticSegmentationNET.Model
{
    public class cvsHelperClass
    {
        /// <summary>
        /// Concaninteates the images
        /// </summary>
        /// <param name="images">the array of images</param>
        /// <param name="orientation">oriention=0: horizontal concatination, orientation:1:vertical</param>
        /// <returns></returns>

      
    public static Image<Bgr, byte> VConcatImages(List<Image<Bgr, byte>> images)
        {
            try
            {
                int MaxCols = images.Max(x => x.Cols);
                Image<Bgr, byte> imgoutput = null;
                    int totalRows = images.Sum(x => x.Rows);
                    imgoutput = new Image<Bgr, byte>(MaxCols,totalRows, new Bgr(0, 0, 0));
                
                int ycord = 0;
                for (int i = 0; i < images.Count(); i++)
                {
                    imgoutput.ROI = new System.Drawing.Rectangle( 0,ycord, images[i].Width, images[i].Height);
                    images[i].CopyTo(imgoutput);
                    
                    imgoutput.ROI = Rectangle.Empty;
                    ycord += images[i].Height;
                }
                return imgoutput;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static Image<Gray, byte> VConcatImages(List<Image<Gray, byte>> images)
        {
            try
            {
                int MaxCols = images.Max(x => x.Cols);
                Image<Gray, byte> imgoutput = null;
                int totalRows = images.Sum(x => x.Rows);
                imgoutput = new Image<Gray, byte>(MaxCols, totalRows, new Gray(0));

                int ycord = 0;
                for (int i = 0; i < images.Count(); i++)
                {
                    imgoutput.ROI = new System.Drawing.Rectangle(0, ycord, images[i].Width, images[i].Height);
                    images[i].CopyTo(imgoutput);

                    imgoutput.ROI = Rectangle.Empty;
                    ycord += images[i].Height;
                }
                return imgoutput;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static Image<Bgr, byte> VConcatImages(Image<Bgr, byte> img1, Image<Bgr, byte> img2)
        {
            try
            {
                int MaxCols = img1.Cols > img2.Cols ? img1.Cols : img2.Cols;
                Image<Bgr, byte> imgoutput = null;
                int totalRows = img1.Rows + img2.Rows;
                imgoutput = new Image<Bgr, byte>(MaxCols, totalRows, new Bgr(0,0,0));

                imgoutput.ROI = new System.Drawing.Rectangle(0, 0, img1.Width, img1.Height);
                img1.CopyTo(imgoutput);
                imgoutput.ROI = Rectangle.Empty;

                imgoutput.ROI = new System.Drawing.Rectangle(0, img1.Height, img2.Width, img2.Height);
                return imgoutput;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static Image<Bgr, byte> HConcatImages(List<Image<Bgr, byte>> images)
        {
            try
            {
                int MaxRows = images.Max(x => x.Rows);
                Image<Bgr, byte> imgoutput = null;
                int totalCols = images.Sum(x => x.Cols);
                imgoutput = new Image<Bgr, byte>(totalCols, MaxRows, new Bgr(0, 0, 0));

                int xcord = 0;
                for (int i = 0; i < images.Count(); i++)
                {
                    imgoutput.ROI = new System.Drawing.Rectangle(xcord, 0, images[i].Width, images[i].Height);
                    images[i].CopyTo(imgoutput);

                    imgoutput.ROI = Rectangle.Empty;
                    xcord += images[i].Width;
                }
                return imgoutput;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static Image<Bgr, byte> HConcatImages(Image<Bgr, byte> img1, Image<Bgr, byte> img2)
        {
            try
            {
                int MaxRows = img1.Rows > img2.Rows ? img1.Rows : img2.Rows;
                Image<Bgr, byte> imgoutput = null;
                int totalCols = img1.Cols + img2.Cols;
                imgoutput = new Image<Bgr, byte>(totalCols, MaxRows, new Bgr(0, 0, 0));

                imgoutput.ROI = new System.Drawing.Rectangle(0, 0, img1.Width, img1.Height);
                img1.CopyTo(imgoutput);

                imgoutput.ROI = new System.Drawing.Rectangle(img1.Width, 0, img2.Width, img2.Height);
                img2.CopyTo(imgoutput);
                imgoutput.ROI = Rectangle.Empty;
                return imgoutput;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static void WriteConfigParameters(string key, string value)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

                config.AppSettings.Settings[key].Value = value;

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static string ReadConfigParameters(string key)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
                string val = config.AppSettings.Settings[key].Value;
                return val;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static Emgu.CV.Matrix<int> HotMatrix(Emgu.CV.Matrix<int> y_label, int ClassCount)
        {
            var HotVector = new Emgu.CV.Matrix<int>(y_label.Rows, ClassCount);
            HotVector.SetZero();
            for (int row = 0; row < y_label.Rows; row++)
            {
                HotVector[row, y_label[row, 0]] = 1;
            }
            return HotVector;
        }

        public static Emgu.CV.Matrix<int> DecodeHotMatrix(Emgu.CV.Matrix<int> y_label)
        {
            var DecodedMatrix= new Emgu.CV.Matrix<int>(y_label.Rows, 1);
            DecodedMatrix.SetZero();
            for (int row = 0; row < y_label.Rows; row++)
            {
                for (int col = 0; col < y_label.Cols; col++)
                {
                    if (y_label[row,col]==1)
                    {
                        DecodedMatrix[row, 0] = col;
                        break;
                    }
                }
            }
            return DecodedMatrix;
        }

        public static int GetClassCount(Emgu.CV.Matrix<int> y_label)
        {
            try
            {
                var labels = new int[y_label.Rows];
                y_label.Mat.CopyTo(labels);
                return labels.Distinct().Count();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static bool CheckDuplicate(int[] array)
        {
            var query = array.GroupBy(x => x)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();
            return query.Count > 0;
        }
        /// <summary>
        ///  Takes list of Facedata and return x_Train Y-Train, X_Test, Y-Test 
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Labels"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        // public static (Emgu.CV.Matrix<float>, Emgu.CV.Matrix<int>, Emgu.CV.Matrix<float>, Emgu.CV.Matrix<int>)
        public static (List<FaceData>, List<FaceData>) TestTrainSplit(List<FaceData> data, float split = 0.8f)
        {
            if (data.Count<1)
            {
                throw new Exception("Data is not found.");
            }

            int numTrainSamples = (int)Math.Floor(data[0].Images.Count * split);
            int numTestSamples = data[0].Images.Count  - numTrainSamples;

            if (numTrainSamples == 0 || numTestSamples == 0)
            {
                throw new Exception("Insufficient traning or testing dat.");
            }
            List<FaceData> TrainData =new List<FaceData>();
            List<FaceData> TestData = new List<FaceData>();

            TestData = (from d in data
                      select new FaceData{Images =  d.Images.Take(numTestSamples).ToList(), 
                          Label = d.Label }).ToList();

            TrainData = (from d in data
                         select new FaceData
                         {
                             Images = d.Images.Skip(numTestSamples).Take(numTrainSamples).ToList(),
                             Label = d.Label
                         }).ToList();
            //foreach (var item in data)
            //{
            //    var trainImages = item.Images.Take(numTestSamples).ToList();
            //    var testImages = item.Images.Skip(numTestSamples).Take(numTestSamples).ToList();
            //    TrainData.Add(new )
            //}
            return (TrainData, TestData);
            //Emgu.CV.Matrix<float> x_test = new Emgu.CV.Matrix<float>(totalTestExamples, Data.Cols);
            //Emgu.CV.Matrix<int> y_test = new Emgu.CV.Matrix<int>(totalTestExamples, 1);

            //Emgu.CV.Matrix<float> x_train = new Emgu.CV.Matrix<float>(Data.Rows - N, Data.Cols);
            //Emgu.CV.Matrix<int> y_train = new Emgu.CV.Matrix<int>(Data.Rows - N, 1);

            //int Min = 0;
            //int Max = Labels.Rows - 1;
            //var TestIndices = helperFunctions.RandomNumGenerator(Min, Max, N);





            //int testIndex = 0;
            //int trainIndex = 0;
            //for (int row = 0; row < Data.Rows; row++)
            //{
            //    if (TestIndices.Contains(row))
            //    {
            //        for (int j = 0; j < Data.Cols; j++)
            //        {
            //            x_test[testIndex, j] = Data[row, j];
            //        }
            //        y_test[testIndex, 0] = Labels[row, 0];

            //        testIndex++;
            //    }
            //    else
            //    {

            //        for (int j = 0; j < Data.Cols; j++)
            //        {
            //            x_train[trainIndex, j] = Data[row, j];
            //        }
            //        y_train[trainIndex, 0] = Labels[row, 0];
            //        trainIndex++;

            //    }
            //}

            //return (x_train, y_train, x_test, y_test);
        }

        public static (Emgu.CV.Matrix<float>, Emgu.CV.Matrix<int>, Emgu.CV.Matrix<float>, Emgu.CV.Matrix<int>)
            TestTrainSplit(Emgu.CV.Matrix<float> Data, Emgu.CV.Matrix<int> Labels, float split = 0.2f)
        {
            int N = (int)Math.Floor(Labels.Rows * split);
            int Min = 0;
            int Max = Labels.Rows - 1;
            var TestIndices = helperFunctions.RandomNumGenerator(Min, Max, N);

            Emgu.CV.Matrix<float> x_test = new Emgu.CV.Matrix<float>(N, Data.Cols);
            Emgu.CV.Matrix<int> y_test = new Emgu.CV.Matrix<int>(N, 1);

            Emgu.CV.Matrix<float> x_train = new Emgu.CV.Matrix<float>(Data.Rows - N, Data.Cols);
            Emgu.CV.Matrix<int> y_train = new Emgu.CV.Matrix<int>(Data.Rows - N, 1);



            int testIndex = 0;
            int trainIndex = 0;
            for (int row = 0; row < Data.Rows; row++)
            {
                if (TestIndices.Contains(row))
                {
                    for (int j = 0; j < Data.Cols; j++)
                    {
                        x_test[testIndex, j] = Data[row, j];
                    }
                    y_test[testIndex, 0] = Labels[row, 0];

                    testIndex++;
                }
                else
                {

                    for (int j = 0; j < Data.Cols; j++)
                    {
                        x_train[trainIndex, j] = Data[row, j];
                    }
                    y_train[trainIndex, 0] = Labels[row, 0];
                    trainIndex++;

                }
            }

            return (x_train, y_train, x_test, y_test);
        }
        //public static (Emgu.CV.Matrix<float>, Emgu.CV.Matrix<float>) ReadCSV(string path, bool FirstRowHeader = true, char sep=',', int LabelIndex=0)
        //{
        //    List<List<Single>> Data = new List<List<Single>>();
        //    List<float> Label = new List<float>();
        //    using (var reader = new StreamReader(path))
        //    {
        //        if (FirstRowHeader)
        //        {
        //            reader.ReadLine();
        //        }
        //        while (!reader.EndOfStream)
        //        {
        //            var line = reader.ReadLine().Split(',').Select(x => float.Parse(x, CultureInfo.InvariantCulture)).ToList();
        //            var label = int.Parse(line.ElementAt(LabelIndex).ToString());
        //            line.RemoveAt(LabelIndex);
        //            Data.Add(line);
        //            Label.Add(label);
        //        }

        //        var data = Data.Select(a => a.ToArray()).ToArray();
        //        var labels = Label.Select(a => a).ToArray();
 

        //        Emgu.CV.Matrix<float> x_data = new Emgu.CV.Matrix<float>(To2D<Single>(data));
        //        Emgu.CV.Matrix<float> y_labels = new Emgu.CV.Matrix<float>(labels);
        //        return (x_data, y_labels);
        //    }

        //}

        //public static (Emgu.CV.Matrix<float>, Emgu.CV.Matrix<float>) ReadCSV(string path, bool FirstRowHeader = true, char sep = ',', int LabelIndex = 0)
        //{
        //    Mat Data = new Mat();
        //    Mat Labels = new Mat();
        //    List<float> Label = new List<float>();
        //    using (var reader = new StreamReader(path))
        //    {
        //        if (FirstRowHeader)
        //        {
        //            reader.ReadLine();
        //        }
        //        while (!reader.EndOfStream)
        //        {
        //            var line = reader.ReadLine().Split(',').Select(x => float.Parse(x, CultureInfo.InvariantCulture)).ToList();
        //            var label = int.Parse(line.ElementAt(LabelIndex).ToString());
        //            line.RemoveAt(LabelIndex);

        //            IntPtr DataPointer = GCHandle.Alloc(line.ToArray(), GCHandleType.Pinned)
        //                .AddrOfPinnedObject();
        //            int[] sizes =new int[] { 1, line.Count};

        //            var row= new Mat(sizes, Emgu.CV.CvEnum.DepthType.Cv32F, DataPointer);
        //            var mat = Mat.Zeros(1, 1, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
        //            mat.SetTo(new MCvScalar(label));

        //            Data.PushBack(row);
        //            Labels.PushBack(mat);
        //        }
        //        Emgu.CV.Matrix<float> x_data = new Emgu.CV.Matrix<float>(Data.Rows,Data.Cols);
        //        Emgu.CV.Matrix<float> y_labels = new Emgu.CV.Matrix<float>(Labels.Rows,Labels.Cols);

        //        Data.CopyTo(x_data);
        //        Labels.CopyTo(y_labels);

        //        return (x_data, y_labels);
        //    }

        //}


        public static (Emgu.CV.Matrix<float>, Emgu.CV.Matrix<int>) ReadCSV(string path, bool FirstRowHeader = true, char sep = ',', int LabelIndex = 0)
        {
            var list = File.ReadAllLines(path).ToList();
            if (list!=null)
            {
                if (FirstRowHeader)
                {
                    list.RemoveAt(0);
                }
                int ROWS = list.Count;
                int COLS = list[0].Split(sep).Length-1;

                Emgu.CV.Matrix<float> x_data = new Emgu.CV.Matrix<float>(ROWS, COLS);
                Emgu.CV.Matrix<int> y_labels = new Emgu.CV.Matrix<int>(ROWS, 1);

                for(int i=0;i<list.Count;i++)
                {
                    var line = list[i].Split(',').Select(x => float.Parse(x, CultureInfo.InvariantCulture)).ToList();
                    var label = int.Parse(line.ElementAt(LabelIndex).ToString());
                    line.RemoveAt(LabelIndex);

                    var row = line.ToArray();
                    for (int j = 0; j < row.Length; j++)
                    {
                        x_data[i, j] = row[j];
                    }
                    y_labels[i, 0] = label;
                }
                return (x_data, y_labels);
            }
            else
            {
                return (null, null);
            }

        }

        public static int[] Matrix2Array(Emgu.CV.Matrix<float> matrix)
        {
            int[] output = new int[matrix.Rows * matrix.Cols];
            int index = 0;
            for (int row = 0; row < matrix.Rows; row++)
            {
                for (int col = 0; col < matrix.Cols; col++)
                {
                    output[index++] = (int)matrix[row, col];
                }
            }

            return output;
        }
        public static void TrainTestSplitFromFolder(string path, float testPercentage = 0.2f)
        {
            try
            {

                if (!Directory.Exists(path))
                {
                    throw new Exception("File Not Found.");
                }

                string parent = Path.GetDirectoryName(path);


                string trainPath = parent + "\\train\\";
                string testPath = parent + "\\test\\";
                string validPath = parent + "\\valid\\";

                if (!Directory.Exists(trainPath)) Directory.CreateDirectory(trainPath);
                if (!Directory.Exists(testPath)) Directory.CreateDirectory(testPath);
                if (!Directory.Exists(validPath)) Directory.CreateDirectory(validPath);

                var directories = Directory.GetDirectories(path);
                Random rnd = new Random();

                foreach (var directory in directories)
                {
                    var list = Directory.GetFiles(directory).ToList();
                    int testCount = (int)(testPercentage * list.Count);
                    int validCount = (int)(0.1 * list.Count);

                    var testFiles = list.OrderBy(x => rnd.Next()).Take(testCount).ToList();
                    list.RemoveAll(x => testFiles.Contains(x));

                    var validFiles = list.OrderBy(x => rnd.Next()).Take(validCount).ToList();
                    list.RemoveAll(x => validFiles.Contains(x));


                    // create folder at destination and copy files
                    var dir = new DirectoryInfo(directory).Name; ;
                    var destTrainFolder = trainPath + dir;
                    if (!Directory.Exists(destTrainFolder)) Directory.CreateDirectory(destTrainFolder);
                    foreach (var item in list)
                    {
                        var file = destTrainFolder + "\\" + Path.GetFileName(item);
                        if (!File.Exists(file)) File.Copy(item, file);
                    }

                    //test
                    var destTestFolder = testPath + dir;
                    if (!Directory.Exists(destTestFolder)) Directory.CreateDirectory(destTestFolder);
                    foreach (var item in testFiles)
                    {
                        var file = destTestFolder + "\\" + Path.GetFileName(item);
                        if (!File.Exists(file)) File.Copy(item, file);
                    }

                    //valid
                    var destValidFolder = validPath + dir;
                    if (!Directory.Exists(destValidFolder)) Directory.CreateDirectory(destValidFolder);
                    foreach (var item in validFiles)
                    {
                        var file = destValidFolder + "\\" + Path.GetFileName(item);
                        if (!File.Exists(file)) File.Copy(item, file);
                    }
                }

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }
        public static void TrainTestSplitFromFolder(string path, float trainPer=0.7f, float testPer =0.20f, float validPer = 0.10f)
        {
            try
            {

                if (!File.Exists(path))
                {
                    throw new Exception("File Not Found.");
                }

                string parent = Path.GetDirectoryName(path);

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }
        public static Emgu.CV.Matrix<int> Matrix2OneHotEncoding(Emgu.CV.Matrix<int> matrix, int ClassCount)
        {
            Emgu.CV.Matrix<int>  HotVector = new Emgu.CV.Matrix<int>(matrix.Rows, ClassCount);
            HotVector.SetZero();
            for (int row = 0; row < matrix.Height; row++)
            {
                var col = (int)(matrix[row, 0]);
                    HotVector[row, col] = 1;
            }

            return HotVector;
        }
        public static Emgu.CV.Matrix<float> OneHotEncoding2Matrix(Emgu.CV.Matrix<float> matrix)
        {
            Emgu.CV.Matrix<float> DecodeMatrix = new Emgu.CV.Matrix<float>(matrix.Rows, 1);
            //DecodeMatrix.SetZero();
            //var data = matrix.Data;
            //for (int i = 0; i < data.Rows; i++)
            //{
            //    var row = data.GetRow(i);
            //    var maxIndex = row.IndexOf(row.Max());
            //    DecodeMatrix[i, 0] = maxIndex;
            //}

            return DecodeMatrix;
        }

        //https://stackoverflow.com/questions/26291609/converting-jagged-array-to-2d-array-c-sharp
        //public static T[,] To2D<T>(T[][] source)
        //{
        //    try
        //    {
        //        int FirstDim = source.Length;
        //        int SecondDim = source.GroupBy(row => row.Length).Single().Key; // throws InvalidOperationException if source is not rectangular

        //        var result = new T[FirstDim, SecondDim];
        //        for (int i = 0; i < FirstDim; ++i)
        //            for (int j = 0; j < SecondDim; ++j)
        //                result[i, j] = source[i][j];

        //        return result;
        //    }
        //    catch (InvalidOperationException)
        //    {
        //        throw new InvalidOperationException("The given jagged array is not rectangular.");
        //    }
        //}

        //public static string DrawConfusionMatrix(GeneralConfusionMatrix cm)
        //{
        //    string result = "";
        //    var matrix = cm.Matrix;
        //    for (int i = 0; i < matrix.Rows(); i++)
        //    {
        //        if (i==0)
        //        {
        //            result += "\t";

        //            for (int c = 0; c < cm.NumberOfClasses; c++)
        //            {
        //                result += "\t Class" + c.ToString();
        //            }
        //            result += "\n";
        //        }
        //        result += "Class" + i.ToString() + "\t";
        //        for (int j = 0; j < matrix.Columns(); j++)
        //        {
        //            result += "\t"+ matrix[i, j].ToString();
        //        }
        //        result += "\n";
        //    }

        //    return result;
        //}


    }
}
