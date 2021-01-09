using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace SemanticSegmentationNET.Model
{
   public class FaceDataSet
    {
        public Image<Gray, byte> Image;
        public string Label;
        public float LabelInt;
        public static IEnumerable<FaceDataSet> LoadFaceData(string path)
        {
            try
            {
                List<FaceDataSet> TestData = new List<FaceDataSet>();
                List<FaceDataSet> TrainData = new List<FaceDataSet>();

                List<string> list = new List<string>();
                int counter = 0;
                var files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    var FileName = Path.GetFileName(file);
                    var NameWithoutExtension = Path.GetFileNameWithoutExtension(FileName);
                    var obj = new FaceDataSet();
                    if (!list.Contains(NameWithoutExtension))
                    {
                        list.Add(NameWithoutExtension);
                        counter++;
                    }
                    obj.Label = FileName;
                    obj.LabelInt = counter;
                    obj.Image = new Image<Gray, byte>(file);
                    TrainData.Add(obj);
                    
                }
                return TrainData;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
