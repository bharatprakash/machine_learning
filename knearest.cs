using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkedList
{
    class knearest
    {
        static void Main(string[] args)
        {
            DataTable table = new DataTable();
            KUtility util = new KUtility();
            table = util.FileToTable(@"D:\Bharat\ML CMSC 678\bw.arff");
            table.Columns.Add("Distance");

            DataTable testTable = new DataTable();
            testTable = util.FileToTable(@"D:\Bharat\ML CMSC 678\bwtest.arff");
            testTable.Columns.Add("PClass", typeof(string));
            testTable.Columns.Add("Result", typeof(bool));

            for (int k = 1; k < 21; k++)
            {
                foreach (DataRow row in testTable.Rows)
                {
                    foreach (DataRow r in table.Rows)
                    {
                        int dist = 0;
                        foreach (DataColumn attr in testTable.Columns)
                        {
                            if (attr.ColumnName.ToLower() == "class")
                                break;

                            if (row[attr].ToString() != r[attr.ColumnName].ToString())
                                dist++;
                        }
                        r["Distance"] = dist;


                    }// distances calculated for nth test instance

                    DataTable tempTable = table.Clone();
                    DataRow[] results = table.Select("", "Distance");
                    //populate new destination table
                    for (var i = 0; i < k; i++)
                        tempTable.ImportRow(results[i]);

                    var groupedClass = from b in tempTable.AsEnumerable()
                                       group b by b.Field<string>("Class") into g
                                       select new
                                       {
                                           ClassLabel = g.Key,
                                           Count = g.Count()
                                       };

                    var maxZ = groupedClass.Max(obj => obj.Count);
                    var maxObj = groupedClass.Where(obj => obj.Count == maxZ);
                    string MaxClassLabel = "";
                    foreach (var v in maxObj)
                    {
                        MaxClassLabel = v.ClassLabel;
                    }

                    row["PClass"] = MaxClassLabel;
                    if (row["PClass"].ToString() == row["Class"].ToString())
                        row["Result"] = true;
                    else
                        row["Result"] = false;

                }
                decimal testDataCount = testTable.Rows.Count;
                decimal trueCount = testTable.AsEnumerable().Count(row => row.Field<bool>("Result") == true);
                double accuracy = (double)(trueCount / testDataCount) * 100;
                Console.WriteLine("Accuracy at k = " + k + ": " + accuracy.ToString("n3") + "%");
            }
            Console.ReadLine();
            Console.Read();

        }
    }

    public class KUtility
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
