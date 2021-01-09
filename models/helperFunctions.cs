using DocumentFormat.OpenXml.Drawing.Charts;
using Emgu.CV;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SemanticSegmentationNET.models
{
    public class helperFunctions
    {
        public static int[] RandomNumGenerator(int Min,int Max, int N)
        {
            Random randNum = new Random();
            List<int> list = new List<int>();
            while (list.Count<N)
            {
                int num = randNum.Next(Min, Max);
                if (!list.Contains(num))
                {
                    list.Add(num);
                }
            }
            return list.ToArray();
        }
        public static T[,] To2D<T>(T[][] source)
        {
            try
            {
                int FirstDim = source.Length;
                int SecondDim = source.GroupBy(row => row.Length).Single().Key; // throws InvalidOperationException if source is not rectangular

                var result = new T[FirstDim, SecondDim];
                for (int i = 0; i < FirstDim; ++i)
                    for (int j = 0; j < SecondDim; ++j)
                        result[i, j] = source[i][j];

                return result;
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("The given jagged array is not rectangular.");
            }
        }
        public static int[,] ComputeConfusionMatrix(int[] actual, int[] predicted)
        {
            int classes = actual.Distinct().Count();
            int [,] CM = new int[classes, classes];
            if (actual.Length != predicted.Length)
                throw new Exception("Vectors does not match");

            // For each classification,
            for (int k = 0; k < actual.Length; k++)
            {
                int p = predicted[k]-1; // cols contain expected values for  classes starting from 1 int p = predicted[k]-1;
                int a = actual[k]-1;  // rows contain predicted values

                CM[p, a]++;
            }
            return CM;
        }

        public static double[] CalcluateMetrics(int[,] CM, int[] actual, int[] predicted)
        {
            double[] metrics =new double[3];
            int samples = actual.Length;
            int classes = (int)CM.GetLongLength(0);
            var diagonal = GetDiagonal(CM);

            var diagnolSum = GetDiagonal(CM).Sum();
            int[] colTtotals = GetSumCol(CM);


            /// <remarks>
            ///   The accuracy is the sum of the diagonal elements
            ///    divided by the number of samples.
            /// </remarks>
            /// 

            // accuracy
            double accuracy = diagnolSum / (double)samples;

            // Precision
            var precision = new double[classes];
            
            for (int i = 0; i < precision.Length; i++)
                precision[i] = diagonal[i] == 0 ? 0 : diagonal[i] / (double)colTtotals[i];

            // Recall
            var rowTotals = GetSumRow(CM);
            var recall = new double[classes];
            for (int i = 0; i < recall.Length; i++)
                recall[i] = diagonal[i] == 0 ? 0 : diagonal[i] / (double)rowTotals[i];

            metrics[0] = accuracy;
            metrics[1] = precision.Average();
            metrics[2] = recall.Average();
            return metrics;
        }
        private static int[] GetDiagonal(int [,] matrix)
        {
            try
            {
                var diagonal = Enumerable.Range(0, matrix.GetLength(0)).Select(i => matrix[i, i]).ToArray();

                //int rowlength = matrix.GetLength(0);
                //int columnlength = matrix.GetLength(1);
                //if (rowlength != columnlength)
                //{
                //    throw new Exception("Matrix is not Square");
                //}

                //int []diagonal= new int[rowlength];
                //for (int row = 0; row < rowlength; row++)
                //{
                //            diagonal[row]= matrix[row, row];
                //}
                return diagonal;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        private static int[] GetSumRow(int[,] matrix)
        {
            try
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                int[] rowSum = new int[rows];
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col< cols; col++)
                    {
                        rowSum[row] += matrix[row, col];
                    }
                }
                return rowSum;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        private static int[] GetSumCol(int[,] matrix)
        {
            try
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                int[] colSum = new int[cols];
                for (int col = 0; col < cols; col++)
                {
                    for (int row = 0; row < rows; row++)
                    {
                        colSum[col] += matrix[row, col];
                    }
                }
                return colSum;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
       
        public static System.Data.DataTable Array2Datatable(int[,] numbers)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("Classes");
            for (int i = 0; i < numbers.GetLength(1); i++)
            {
                dt.Columns.Add("Class" + (i + 1));
            }

            for (var i = 0; i < numbers.GetLength(0); ++i)
            {
                DataRow row = dt.NewRow();
                row[0] = "Class" + (i + 1);
                for (var j = 0; j < numbers.GetLength(1); ++j)
                {
                    row[j+1] = numbers[i, j];
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
  
        //Similarity measure 
        public double Similarity(double[] x, double[] y)
        {
            try
            {
                if (x.Length!=y.Length)
                {
                    throw new Exception("Length of both vectors not same.");
                }
                double sum = 0.0;

                for (int i = 0; i < x.Length; i++)
                {
                    double u = x[i] - y[i];
                    sum += u * u;
                }

                return 1.0 / (1.0 + Math.Sqrt(sum));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        // Euclidean distance between two vectors
        public double Distance(double[] x, double[] y)
        {
            try
            {
                if (x.Length != y.Length)
                {
                    throw new Exception("Length of both vectors are not same.");
                }
                double sum = 0.0;
                for (int i = 0; i < x.Length; i++)
                {
                    double u = x[i] - y[i];
                    sum += u * u;
                }
                return Math.Sqrt(sum);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
