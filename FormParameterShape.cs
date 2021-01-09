using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.SqlServer.Server;
using SemanticSegmentationNET.Model;

namespace SemanticSegmentationNET
{
    public partial class FormParameterShape : Form
    {
        public delegate void ApplyFunction(double param);
        public event ApplyFunction OnApplied;
        public FormParameterShape()
        {
            InitializeComponent();
            var threshold = double.Parse(cvsHelperClass.ReadConfigParameters("shapeThreshold"), CultureInfo.InvariantCulture);
            tbThreshold.Text = threshold.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                if (tbThreshold.Text == string.Empty)
                {
                    MessageBox.Show("Please enter threshold value");
                    return;
                }
                var threshold = float.Parse(tbThreshold.Text, CultureInfo.InvariantCulture);
                cvsHelperClass.WriteConfigParameters("shapeThreshold", threshold.ToString());

                OnApplied?.Invoke(threshold);
            }
            catch (Exception)
            {
                MessageBox.Show("Please enter threshold value");
            }
        }
    }
}
