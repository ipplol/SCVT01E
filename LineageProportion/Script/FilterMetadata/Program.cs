using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FilterMetadata
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader read = new StreamReader("The path to metadata.tsv");
            StreamWriter write = new StreamWriter("*/SCVT01E/LineageProportion/plotdata/China_202301_metadata.tsv");
            string line = read.ReadLine();
            write.Write(line + "\n");
            line = read.ReadLine();
            while (line != null)
            {
                string[] line1 = line.Split('\t');
                if (line1[11].Contains("China") && line1[10].Contains("2023"))
                { 
                    write.Write(line + "\n"); 
                }


                line = read.ReadLine();
            }
            read.Close();
            write.Close();
            return;
        }
    }
}
