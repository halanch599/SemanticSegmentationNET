using DocumentFormat.OpenXml.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticSegmentationNET.models
{
   public class FaceImageData
    {
        public int Label { get; set; }
        public string Name { get; set; }
        public Image<Emgu.CV.Structure.Gray, byte> Data{ get; set; }
    }
}
