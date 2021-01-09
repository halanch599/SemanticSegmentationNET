using SemanticSegmentationNET.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SemanticSegmentationNET
{
    public partial class FormConvexHullSettings : Form
    {
        int convexHullFilterThreshold = 0;
        int convexHullDistanceThreshold = 0;

        public FormConvexHullSettings()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                
                var convexHullFilterThreshold = int.Parse(cvsHelperClass.ReadConfigParameters("convexHullFilterThreshold"));
                var convexHullDistanceThreshold = int.Parse(cvsHelperClass.ReadConfigParameters("convexHullDistanceThreshold"));

                if (tbCFThreshold.Text==string.Empty) 
                {
                    lblError.Text = "Please enter text";
                    tbCFThreshold.Focus();
                    return;
                }

                if (tbCDDFThreshold.Text == string.Empty)
                {
                    lblError.Text = "Please enter text";
                    tbCDDFThreshold.Focus();
                    return;
                }

                int.TryParse(tbCFThreshold.Text, out convexHullFilterThreshold);
                int.TryParse(tbCDDFThreshold.Text, out convexHullDistanceThreshold);

                cvsHelperClass.WriteConfigParameters("convexHullFilterThreshold", convexHullFilterThreshold.ToString());
                cvsHelperClass.WriteConfigParameters("convexHullDistanceThreshold", convexHullDistanceThreshold.ToString());

                lblError.Text = "Saved successfully.";
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
            
        }

        private void FormConvexHullSettings_Load(object sender, EventArgs e)
        {
            try
            {
                convexHullFilterThreshold = int.Parse(cvsHelperClass.ReadConfigParameters("convexHullFilterThreshold"));
                convexHullDistanceThreshold = int.Parse(cvsHelperClass.ReadConfigParameters("convexHullDistanceThreshold"));

                tbCFThreshold.Text = convexHullFilterThreshold.ToString();
                tbCDDFThreshold.Text = convexHullDistanceThreshold.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
