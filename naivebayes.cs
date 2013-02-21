using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkedList
{
    class NavieBayes
    {
        static void Main(string[] args)
        {

            DataTable table = new DataTable();
            Utility u = new Utility();
            table = u.FileToTable(@"D:\Bharat\ML CMSC 678\bw.arff");

            int total = table.Rows.Count;

            var result = from row in table.AsEnumerable()
                         group row by row.Field<string>("Class") into grp
                         select new
                         {
                             Class = grp.Key,
                             ClassCount = grp.Count()
                         };

            DataTable ProbabilityMatrix = new DataTable();
            Dictionary<string, double> PrioriProb = new Dictionary<string, double>();
            Dictionary<string, int> PrioriCount = new Dictionary<string, int>();
            Dictionary<string, Dictionary<string, int>> dict = new Dictionary<string, Dictionary<string, int>>();
            int temp = 0;

            foreach (DataColumn col in table.Columns)
            {
                Dictionary<string, int> subDict = new Dictionary<string, int>();
                var attrTypes = from row in table.AsEnumerable()
                                group row by row.Field<string>(col.ColumnName) into grp
                                select new
                                {
                                    AttrType = grp.Key,
                                    AttrTypeCount = grp.Count()
                                };

                foreach (var a in attrTypes)
                {
                    subDict.Add(a.AttrType, temp++);
                }
                dict.Add(col.ColumnName, subDict);
            }

            foreach (var t in result)
            {
                ProbabilityMatrix.Columns.Add(t.Class, typeof(double));
                PrioriProb.Add(t.Class, (double)t.ClassCount / total);
                PrioriCount.Add(t.Class, t.ClassCount);
            }


            foreach (DataColumn col in table.Columns)
            {
                var attrTypes = from row in table.AsEnumerable()
                                group row by row.Field<string>(col.ColumnName) into grp
                                select new { AttrType = grp.Key, AttrTypeCount = grp.Count() };

                foreach (var a in attrTypes)
                {
                    DataRow dr = ProbabilityMatrix.NewRow();
                    foreach (DataColumn dc in ProbabilityMatrix.Columns)
                    {
                        var count = table.AsEnumerable().
                            Count(row => row.Field<string>(col.ColumnName) == a.AttrType && row.Field<string>("Class") == dc.ColumnName);
                        double p = (double)count / PrioriCount[dc.ColumnName];
                        dr[dc] = p;
                    }
                    ProbabilityMatrix.Rows.Add(dr);
                }
            }

            DataTable testTable = new DataTable();
            testTable = u.FileToTable(@"D:\Bharat\ML CMSC 678\bwtest.arff");
            testTable.Columns.Add("PClass", typeof(string));
            testTable.Columns.Add("Result", typeof(bool));
            testTable.Columns.Add("ClassProb", typeof(double));

            foreach (DataRow row in testTable.Rows)
            {
                Dictionary<string, double> testProb = new Dictionary<string, double>();
                string maxClassLabel = "";
                double maxProb = 0;
                double denom = 0;
                foreach (var t in result)
                {
                    string classLabel = t.Class;

                    double prob = 1;
                    foreach (DataColumn col in testTable.Columns)
                    {
                        if (col.ColumnName == "Class" || col.ColumnName == "PClass")
                            break;
                        string attrValue = row[col].ToString();
                        Dictionary<string, int> tempDict = new Dictionary<string, int>();
                        tempDict = dict[col.ColumnName];
                        if (tempDict.ContainsKey(attrValue))
                        {
                            int index = tempDict[attrValue];
                            prob *= (double)ProbabilityMatrix.Rows[index].Field<double>(classLabel) / PrioriCount[classLabel];
                        }
                        else
                        { }
                    }
                    prob = prob * PrioriProb[classLabel];
                    denom += prob;
                    if (prob > maxProb)
                    {
                        maxProb = prob;
                        maxClassLabel = classLabel;
                    }
                }
                row["ClassProb"] = ((double)maxProb / denom);
                row["PClass"] = maxClassLabel;
                if (row["PClass"].ToString() == row["Class"].ToString())
                    row["Result"] = true;
                else
                    row["Result"] = false;
            }

            for (int i = 0; i < 50; i++)
            {                
                foreach (var item in testTable.Rows[i].ItemArray)
                {
                    Console.Write(" "); 
                    Console.Write(item + " "); 
                }
                Console.WriteLine();
            }
            Console.Read();

            decimal testDataCount = testTable.Rows.Count;
            decimal trueCount = testTable.AsEnumerable().Count(row => row.Field<bool>("Result") == true);
            double accuracy = (double)(trueCount / testDataCount) * 100;
            Console.WriteLine("Accuracy : " + accuracy);
            Console.ReadLine(); Console.ReadLine();

        }// end main

    }

    public class Utility
    {
        public DataTable FileToTable(string filename)
        {
            DataTable table = new DataTable();
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            while ((line = file.ReadLine()) != null)
            {
                if (line.ToLower().StartsWith("@attribute"))
                {
                    table.Columns.Add(line.Split(' ')[1]);
                }
                else if (line.StartsWith("@") == false && line.Length > 1)
                {
                    string[] s = line.Split(',');
                    DataRow r = table.NewRow();
                    int c = 0;
                    foreach (string val in s)
                    {
                        r[c++] = val;
                    }
                    table.Rows.Add(r);
                }
                else
                { }
            }
            file.Close();
            return table;
        }
    }

}
