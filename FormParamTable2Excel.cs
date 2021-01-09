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
    public partial class FormParamTable2Excel : Form
    {
        public delegate void DelegateTable2Excel(int NoCols, float MorphThreshold,int BinaryThreshold, int offset);
        public event DelegateTable2Excel OnTable2Excel;
        public FormParamTable2Excel()
        {
            InitializeComponent();
        }

        private void FormParamTable2Excel_Load(object sender, EventArgs e)
        {
            //int NoCols = 4, float MorphThreshold = 0.30f, int offset = 5
            tbNoCols.Text = cvsHelperClass.ReadConfigParameters("NoCols");
            tbMorphThreshold.Text = cvsHelperClass.ReadConfigParameters("MorphThreshold");
            tboffset.Text = cvsHelperClass.ReadConfigParameters("offset");
            tbBinaryThreshold.Text = cvsHelperClass.ReadConfigParameters("binaryThreshold");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                lblMessage.Text = "";
                int NoCols = int.Parse(tbNoCols.Text);
                float MorphThreshold = float.Parse(tbMorphThreshold.Text);
                int offset = int.Parse(tboffset.Text);
                int binaryThreshold = int.Parse(tbBinaryThreshold.Text);
                
                cvsHelperClass.WriteConfigParameters("NoCols", NoCols.ToString());
                cvsHelperClass.WriteConfigParameters("MorphThreshold", MorphThreshold.ToString());
                cvsHelperClass.WriteConfigParameters("offset", offset.ToString());
                cvsHelperClass.WriteConfigParameters("binaryThreshold", binaryThreshold.ToString());

                lblMessage.Text = "Updated successfully.";
                lblMessage.ForeColor = Color.Green;
                    
                OnTable2Excel?.Invoke(NoCols, MorphThreshold, binaryThreshold, offset);
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
                lblMessage.ForeColor = Color.Green;
            }
        }
    }
}
