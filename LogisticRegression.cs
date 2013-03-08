
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkedList
{
    class LogisticReg
    {
        static void Main(string[] args)
        {
            DataTable table = new DataTable();
            Util u = new Util();
            const double ETA = 0.0001;

            table = u.FileToTable(@"D:\Bharat\ML CMSC 678\HW2\ionosphere.arff");
                        
            table = u.Normalize(table);
                       
            DataTable dataSet = table.Clone();
            DataView view = new DataView(table);
            dataSet = view.ToTable();
            dataSet.Columns.Remove("Class");

            int total = table.Rows.Count;
            int attrCount = table.Columns.Count - 1;

            double[] wVector = new double[attrCount];
            double[] gVector;

            double dotProduct;
            double denom;
            double pi;
            double error;

            int cnt = 0;

            while (true)
            { 
                gVector = new double[attrCount];
                for(int i = 0; i< total;i++)
                {
                    double[] xi = dataSet.Rows[i].ItemArray.ToArray().Select(x => double.Parse(x.ToString())).ToArray<double>();
                    dotProduct = wVector.Zip(xi, (d1, d2) => d1 * d2).Sum();
                    denom = (double)(1 + Math.Exp(-dotProduct));
                    pi = (double)1/denom;
                    error = double.Parse(table.Rows[i]["Class"].ToString()) - pi;
                    for (int j = 0; j < gVector.Length; j++)
                    {
                        gVector[j] += error * double.Parse(dataSet.Rows[i][j].ToString());
                    }
                }
                double[] wVectorOld = wVector;                
                wVector = wVector.Zip(gVector, (d1, d2) => Math.Round(d1 + d2 * ETA,3)).ToArray<double>();
                
                if (wVector.SequenceEqual(wVectorOld))
                    break;
                cnt++;
            }
            
            DataTable testTable = new DataTable();
            testTable = u.FileToTable(@"D:\Bharat\ML CMSC 678\HW2\ionoTest.arff");
            
            DataTable testDataSet = testTable.Clone();
            DataView testView = new DataView(testTable);
            testDataSet = testView.ToTable();
            testDataSet.Columns.Remove("Class");
            int testTotal = testDataSet.Rows.Count;

            testTable.Columns.Add("PClass", typeof(double));
            testTable.Columns.Add("Result", typeof(bool));

            double vProduct;
            for(int i = 0; i < testTotal;i++)
            {
                double[] xi = testDataSet.Rows[i].ItemArray.ToArray().Select(x => double.Parse(x.ToString())).ToArray<double>();
                vProduct = wVector.Zip(xi, (d1, d2) => d1 * d2).Sum();
                if (vProduct > 0)
                    testTable.Rows[i]["PClass"] = 1;
                else
                    testTable.Rows[i]["PClass"] = 0;

                if (testTable.Rows[i]["PClass"].ToString() == testTable.Rows[i]["Class"].ToString())
                    testTable.Rows[i]["Result"] = true;
                else
                    testTable.Rows[i]["Result"] = false;
            }
                        
            decimal testDataCount = testTable.Rows.Count;
            decimal trueCount = testTable.AsEnumerable().Count(row => row.Field<bool>("Result") == true);
                        
            double accuracy = (double)(trueCount / testDataCount) * 100;
            Console.WriteLine("Accuracy : " + accuracy);
            Console.ReadLine(); Console.ReadLine();
        }
    }



    public class Util
    {
        public DataTable FileToTable(string filename)
        {
            DataTable table = new DataTable();
            table.Columns.Add("C1", typeof(double));
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            while ((line = file.ReadLine()) != null)
            {
                if (line.ToLower().StartsWith("@attribute"))
                {
                    table.Columns.Add(line.Split(' ')[1],typeof(double));
                }
                else if (line.StartsWith("@") == false && line.Length > 1)
                {
                    string[] s = line.Split(',');
                    DataRow r = table.NewRow();
                    r[0] = 1.0;
                    int c = 1;
                    foreach (string val in s)
                    {
                        r[c++] = double.Parse(val);
                    }
                    table.Rows.Add(r);
                }
                else
                { }
            }
            file.Close();
            
            return table;
        }

        public DataTable Normalize(DataTable table)
        {
        
            foreach(DataColumn col in table.Columns)
            {
                double max = (double)table.Compute("MAX(" + col.ColumnName + ")","");
                double min = (double)table.Compute("MIN(" + col.ColumnName + ")", "");
                foreach (DataRow row in table.Rows)
                {
                    double a = double.Parse(row[col].ToString()) - min;
                    double b = max - min;
                    if (b != 0 )
                        row[col] = Math.Round(a/b,3);                    
                }
            }

            return table;
        }
        


    }
}
