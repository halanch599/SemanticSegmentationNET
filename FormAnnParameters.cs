using Emgu.CV.ML;
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
    public partial class FormAnnParameters : Form
    {
        public delegate void DelegateANNApply(ANN_MLP.AnnMlpActivationFunction ActivationFunction,
                            ANN_MLP.AnnMlpTrainMethod TrainMethod,
                            float Momentum,
                            int Iterations,
                            float RMSE,
                            int[] layers,
                            bool LoadSavedModel);
        public event DelegateANNApply OnAnnApply;

        public FormAnnParameters()
        {
            InitializeComponent();
        }

        private void FormAnnParameters_Load(object sender, EventArgs e)
        {
            try
            {
                FillDropDownList();
                LoadConfiguration();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                ddlActivationFunction.SelectedIndex = int.Parse(cvsHelperClass.ReadConfigParameters("ActivationFunction"));
                ddlTrainMethod.SelectedIndex = int.Parse(cvsHelperClass.ReadConfigParameters("TrainMethod"));
                tbMomentum.Text = cvsHelperClass.ReadConfigParameters("Momentum");
                tbIterations.Text = cvsHelperClass.ReadConfigParameters("Iterations");
                tbRMSE.Text = cvsHelperClass.ReadConfigParameters("RMSE");
                tbLayerSize.Text = cvsHelperClass.ReadConfigParameters("LayerSize");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }
        private void FillDropDownList()
        {
            List<string> list = new List<string>() { "SigmoidSym", "Gaussian", "Identity", "LeakyRelu", "Relu" };
            ddlActivationFunction.DataSource = list;
            ddlActivationFunction.SelectedIndex = 0;

            list = new List<string>() { "Backprop", "Rprop", "Anneal" };
            ddlTrainMethod.DataSource = list;
            ddlTrainMethod.SelectedIndex = 0;

        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            try
            {
                this.Cursor = Cursors.WaitCursor;
                var ActivationFunction = ANN_MLP.AnnMlpActivationFunction.SigmoidSym;
                var TrainMethod = ANN_MLP.AnnMlpTrainMethod.Backprop;
                float Momentum = 0.5000f;
                int iterations = 1000;
                float RMSE = 0.001F;
                switch (ddlActivationFunction.SelectedIndex)
                {
                    case 0:
                        ActivationFunction = ANN_MLP.AnnMlpActivationFunction.SigmoidSym;
                        break;
                    case 1:
                        ActivationFunction = ANN_MLP.AnnMlpActivationFunction.Gaussian;
                        break;
                    case 2:
                        ActivationFunction = ANN_MLP.AnnMlpActivationFunction.Identity;
                        break;
                    case 3:
                        ActivationFunction = ANN_MLP.AnnMlpActivationFunction.LeakyRelu;
                        break;
                    case 4:
                        ActivationFunction = ANN_MLP.AnnMlpActivationFunction.Relu;
                        break;
                    default:
                        ActivationFunction = ANN_MLP.AnnMlpActivationFunction.SigmoidSym;
                        break;
                }

                switch (ddlTrainMethod.SelectedIndex)
                {
                    case 0:
                        TrainMethod = ANN_MLP.AnnMlpTrainMethod.Backprop;
                        break;
                    case 1:
                        TrainMethod = ANN_MLP.AnnMlpTrainMethod.Rprop;
                        break;
                    case 2:
                        TrainMethod = ANN_MLP.AnnMlpTrainMethod.Anneal;
                        break;
                    default:
                        TrainMethod = ANN_MLP.AnnMlpTrainMethod.Rprop;
                        break;
                }

                int []LayerSize = Array.ConvertAll(tbLayerSize.Text.Split(','), s => int.Parse(s));
                //Matrix<int> LayerSize = new Matrix<int>(new int[] { layersize[0], layersize[1], layersize[2] });
                //Matrix<int> LayerSize = new Matrix<int>(layersize);

                int.TryParse(tbIterations.Text, out iterations);
                float.TryParse(tbRMSE.Text, out RMSE);
                float.TryParse(tbMomentum.Text, out Momentum);

                this.Cursor = Cursors.WaitCursor;

                OnAnnApply?.Invoke(ActivationFunction, TrainMethod, Momentum, iterations, RMSE, LayerSize,cbLoadSavedModel.Checked);
                this.Cursor = Cursors.Default; 

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Cursor = Cursors.Default;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                cvsHelperClass.WriteConfigParameters("ActivationFunction", ddlActivationFunction.SelectedIndex.ToString());
                cvsHelperClass.WriteConfigParameters("TrainMethod", ddlTrainMethod.SelectedIndex.ToString());
                cvsHelperClass.WriteConfigParameters("Momentum", tbMomentum.Text);
                cvsHelperClass.WriteConfigParameters("Iterations", tbIterations.Text);
                cvsHelperClass.WriteConfigParameters("RMSE", tbRMSE.Text);
                cvsHelperClass.WriteConfigParameters("LayerSize", tbLayerSize.Text);
                lblMessage.Text = "Configuration Information Saved.";
                lblMessage.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
