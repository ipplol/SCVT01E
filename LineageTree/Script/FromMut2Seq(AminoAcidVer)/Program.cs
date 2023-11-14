using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FromMut2Seq_AminoAcidVer_
{
    class Program
    {
        static void Main(string[] args)//考虑indel
        {
            StreamReader readref = new StreamReader("./SCVT01E/LineageTree/Data/SpikeAA.faa");
            string refAA = readref.ReadLine();
            readref.Close();

            StreamReader readmut = new StreamReader("./SCVT01E/LineageTree/Data/Lineage mutlist.txt");
            StreamWriter write = new StreamWriter("./SCVT01E/LineageTree/Data/LineageSpikeSeq.fa");

            int i, j, k;
            string line = readmut.ReadLine();
            while (line != null)
            {
                string[] line1 = line.Split('\t');
                string output = ">" + line1[0];
                char[] seqChar = refAA.ToCharArray();
                List<string> seq = new List<string>();
                for (i = 0; i < seqChar.Length; i++)
                    seq.Add(seqChar[i].ToString());

                if (line1[1] != "")
                {
                    string[] mut = line1[1].Split(' ');
                    for (i = 0; i < mut.Count(); i++)
                    {
                        if (!mut[i].Contains("DEL") && !mut[i].Contains("INS"))
                        {
                            //seq[Convert.ToInt32(mut[i].Substring(0, mut[i].Length - 1))-1] = mut[i][mut[i].Length - 1].ToString();//241T
                            seq[Convert.ToInt32(mut[i].Substring(1, mut[i].Length - 2)) - 1] = mut[i][mut[i].Length - 1].ToString();//C241T
                        }
                        else
                        {
                            if (mut[i].Contains("DEL"))
                            {
                                for (j = 0; j < mut[i].Length; j++)
                                    if (mut[i][j] == 'D' && mut[i][j + 1] == 'E' && mut[i][j + 2] == 'L')
                                        break;
                                string[] del1 = mut[i].Substring(j + 3, mut[i].Length - j - 3).Split('/');
                                for (j = Convert.ToInt32(del1[0]); j <= Convert.ToInt32(del1[1]); j++)
                                    seq[j - 1] = "";
                            }

                            if (mut[i].Contains("INS"))
                            {
                                string[] ins1 = mut[i].Split(':');
                                seq[Convert.ToInt32(ins1[0].Substring(0, ins1[0].Length - 3)) - 1] += ins1[1];
                            }
                        }
                    }
                }

                string seqsequence = "";
                for (i = 0; i < seq.Count; i++)
                    seqsequence += seq[i];
                write.Write(output + "\n");
                write.Write(seqsequence + "\n");
                line = readmut.ReadLine();
            }

            write.Close();
            readmut.Close();
        }
    }
}
