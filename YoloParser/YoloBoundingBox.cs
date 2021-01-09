using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SemanticSegmentationNET.YoloParser
{
    public class BoundingBoxDimensions : DimensionsBase { }
    public class YoloBoundingBox
    {
        public BoundingBoxDimensions Dimensions { get; set; }

        public string Label { get; set; }

        public float Confidence { get; set; }

        public RectangleF Rect
        {
            get { return new RectangleF(Dimensions.X, Dimensions.Y, Dimensions.Width, Dimensions.Height); }
        }

        public Color BoxColor { get; set; }
    }
}
