using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LineageDistributionFromMetadata
{
    /* Display the distribution of dominant Lineages at each time point,
     * If a lineage has never dominated,
     * Then merge it to the previous level,
     * If the previous level does not exist, it will be attributed to others */
    public class Lineage
    {
        public string LineageName;
        public string ShortName;
        public List<string> Ancestors = new List<string>(); //所有直系祖先，按近到远排序
        public List<string> Children = new List<string>(); //所有孩子，孙子
        public bool ChildMeetThreshold = false;//是否有孩子节点达到了主导标准，如果是，则不能与其孩子节点合并，必须归类到others
    }
    public class TimeWindow
    {
        public string WindowTime;
        public int TotalSeqNumber = 0;
        public Dictionary<string, int> LineageCountDic = new Dictionary<string, int>();
        public Dictionary<string, int> MergedLineageCountDic = new Dictionary<string, int>();//把子分支merge到大分支下
    }
    class Program
    {
        static Dictionary<string, string> ShortName2LineageName = new Dictionary<string, string>();
        static Dictionary<string, Lineage> LineageDic = new Dictionary<string, Lineage>();
        static Dictionary<string, TimeWindow> TimeWindowDic = new Dictionary<string, TimeWindow>();
        static List<string> TotalLineageList = new List<string>();
        static List<string> LineagePassed = new List<string>();//达到绘图标准的Lineage

        static string Country = "United States";
        static void ReadAlias()//读入Lineage列表 里面有对应的准确序号 如 EG.5.1	对应 XBB.1.9.2.5.1
        {
            int i, j, k;
            StreamReader read = new StreamReader("*/LineageProportion/plotdata/lineage_aliasList.txt");
            string line = read.ReadLine();
            line = read.ReadLine();
            while (line != null)
            {
                string[] line1 = line.Split('\t');
                ShortName2LineageName.Add(line1[0], line1[1]);
                if (LineageDic.ContainsKey(line1[1]))
                {
                    LineageDic[line1[1]].LineageName = line1[1];
                    LineageDic[line1[1]].ShortName = line1[0];
                }
                else
                {
                    Lineage newl = new Lineage();
                    newl.LineageName = line1[1];
                    newl.ShortName = line1[0];
                    LineageDic.Add(line1[1], newl);
                }
                string[] line2 = line1[1].Split('.');
                for (i = line2.Length - 2; i >= 0; i--)
                {
                    string ancestor = line2[0];
                    for (j = 1; j <= i; j++)
                        ancestor += "." + line2[j];
                    LineageDic[line1[1]].Ancestors.Add(ancestor);
                    if (!LineageDic.ContainsKey(ancestor))
                    {
                        Lineage newla = new Lineage();
                        newla.LineageName = ancestor;
                        LineageDic.Add(ancestor, newla);
                    }
                    LineageDic[ancestor].Children.Add(line1[1]);
                }
                line = read.ReadLine();
            }
            return;
        }
        static void ReadMetadata()//读入metadata表 NGDC风格 这里的metadata已经按时间和国家过滤了
        {
            int i, j, k;
            StreamReader read = new StreamReader("*/LineageProportion/plotdata/" + Country + "_202301_metadata.tsv");
            string line = read.ReadLine();
            line = read.ReadLine();
            while (line != null)
            {
                string[] line1 = line.Split('\t');
                if (line1[10].Length < 7)//日期不符合要求
                {
                    line = read.ReadLine();
                    continue;
                }

                /*if (!line1[1].Contains("EPI_ISL") && !line1[3].Contains("EPI_ISL"))//非GISAID序列
                {
                    line = read.ReadLine();
                    continue;
                }*/

                /*if (!line1[11].Contains("China"))//国家不符合要求
                {
                    line = read.ReadLine();
                    continue;
                }*/

                string date = line1[10].Substring(0, 7); //10

                //该月总序列数
                if (TimeWindowDic.ContainsKey(date))//该月份存在，总数+1
                {
                    TimeWindowDic[date].TotalSeqNumber++;
                }
                else//添加新月份
                {
                    TimeWindow b = new TimeWindow();
                    b.WindowTime = date;
                    b.TotalSeqNumber = 1;
                    TimeWindowDic.Add(date, b);
                }

                //该lineage序列数
                string seqlineage;
                if (ShortName2LineageName.ContainsKey(line1[4]))
                {
                    seqlineage = ShortName2LineageName[line1[4]];
                    if (!TotalLineageList.Contains(seqlineage))
                        TotalLineageList.Add(seqlineage);
                }
                else
                {
                    line = read.ReadLine();
                    continue;
                }

                if (TimeWindowDic[date].LineageCountDic.ContainsKey(seqlineage))
                    TimeWindowDic[date].LineageCountDic[seqlineage]++;
                else
                    TimeWindowDic[date].LineageCountDic.Add(seqlineage, 1);

                line = read.ReadLine();
            }
        }
        static void PickLineageForPlot()//选择频率高于阈值的Lineage来画图
        {
            int i, j, k;
            foreach (string val in TimeWindowDic.Keys)
            {
                foreach (string vall in TimeWindowDic[val].LineageCountDic.Keys)
                {
                    //if (vall == "XBB.1.9.2.5.1.1.3")
                    //    Console.WriteLine("?");
                    if ((double)TimeWindowDic[val].LineageCountDic[vall] / TimeWindowDic[val].TotalSeqNumber >= 0.01)//set threashold
                        if (!LineagePassed.Contains(vall))
                        {
                            LineagePassed.Add(vall);
                            for (i = 0; i < LineageDic[vall].Ancestors.Count; i++)
                                LineageDic[LineageDic[vall].Ancestors[i]].ChildMeetThreshold = true;
                        }
                }
            }
            return;
        }
        static void LineageCountMerge()//把未满足要求的子分支merge到大分支下面
        {
            int i, j, k;
            for (i = 0; i < LineagePassed.Count; i++)
            {
                foreach (string valT in TimeWindowDic.Keys)
                {
                    TimeWindowDic[valT].MergedLineageCountDic.Add(LineagePassed[i], 0);
                    if (TimeWindowDic[valT].LineageCountDic.ContainsKey(LineagePassed[i]))
                        TimeWindowDic[valT].MergedLineageCountDic[LineagePassed[i]] += TimeWindowDic[valT].LineageCountDic[LineagePassed[i]];

                    //merge
                    /*if(LineageDic[LineagePassed[i]].ChildMeetThreshold = false)
                    {
                        foreach (string valL in TimeWindowDic[valT].LineageCountDic.Keys)
                            if(LineageDic[LineagePassed[i]].Children.Contains(valL))
                                TimeWindowDic[valT].MergedLineageCountDic[LineagePassed[i]] += TimeWindowDic[valT].LineageCountDic[valL];
                    }*/
                }
            }
            return;
        }
        static void OutputResult()
        {
            int i, j, k;
            StreamWriter write = new StreamWriter("*/LineageProportion/plotdata/" + Country + "_LineageDistribution_matrix.txt");
            StreamWriter writeBarplot = new StreamWriter("*/LineageProportion/plotdata/" + Country + "_LineageDistribution_barplot.txt");
            StreamWriter writerise = new StreamWriter("*/LineageProportion/plotdata/" + Country + "_RaisedLineage.txt");
            string line = "Country\tLineage";
            foreach (string valT in TimeWindowDic.Keys)
                line += "\t" + valT;
            //for (i = 0; i < LineagePassed.Count; i++)
            //line += "\t" + LineagePassed[i] + "(" + LineageDic[LineagePassed[i]].ShortName + ")";
            //line += "\tOthers";
            write.WriteLine(line);
            writeBarplot.WriteLine("Date\tLineage\tProportion");

            for (i = 0; i < LineagePassed.Count; i++)
            {
                line = Country + "\t" + LineagePassed[i] + "(" + LineageDic[LineagePassed[i]].ShortName + ")";
                List<double> proportion = new List<double>();
                foreach (string valT in TimeWindowDic.Keys)
                {
                    line += "\t" + Convert.ToString((double)TimeWindowDic[valT].MergedLineageCountDic[LineagePassed[i]] / TimeWindowDic[valT].TotalSeqNumber);
                    proportion.Add((double)TimeWindowDic[valT].MergedLineageCountDic[LineagePassed[i]] / TimeWindowDic[valT].TotalSeqNumber);
                }
                write.WriteLine(line);
                if (proportion[proportion.Count - 1] > 0 && proportion[proportion.Count - 1] >= proportion[proportion.Count - 2] && proportion[proportion.Count - 2] >= proportion[proportion.Count - 3])
                    writerise.WriteLine(line);
            }

            foreach (string valT in TimeWindowDic.Keys)
            {
                line = valT;
                int TotalPickedCount = 0;
                for (i = 0; i < LineagePassed.Count; i++)
                {
                    line += "\t" + Convert.ToString((double)TimeWindowDic[valT].MergedLineageCountDic[LineagePassed[i]] / TimeWindowDic[valT].TotalSeqNumber);
                    TotalPickedCount += TimeWindowDic[valT].MergedLineageCountDic[LineagePassed[i]];
                    writeBarplot.WriteLine(valT + "\t" + LineagePassed[i] + "(" + LineageDic[LineagePassed[i]].ShortName + ")" + "\t" + Convert.ToString((double)TimeWindowDic[valT].MergedLineageCountDic[LineagePassed[i]] / TimeWindowDic[valT].TotalSeqNumber));
                }
                line += "\t" + Convert.ToString(1 - (double)TotalPickedCount / TimeWindowDic[valT].TotalSeqNumber);
                //write.WriteLine(line);
                writeBarplot.WriteLine(valT + "\t0thers\t" + Convert.ToString(1 - (double)TotalPickedCount / TimeWindowDic[valT].TotalSeqNumber));
            }
            write.Close();
            writeBarplot.Close();
            writerise.Close();
            return;
        }
        static void OutputLineageRelation()
        {
            LineagePassed.Sort();

            return;
        }
        static void Main(string[] args)
        {
            ReadAlias();
            ReadMetadata();
            PickLineageForPlot();
            LineageCountMerge();
            OutputResult();
            return;
        }
    }
}

