# SCVT01E

The site includes custom scripts and files used in the study of “**Enhanced neutralization of SARS-CoV-2 XBB sub-lineages and BA.2.86 by a tetravalent COVID-19 vaccine booster**”.

The code includes multiple language. We recommend running the scripts on a Windows working station. In this documentation, unless otherwise noted, all steps are run under the Windows system.

Some of the scripts are written in Microsoft C# language based on the .net framework, the original code and project files are provided. To run the scripts, [Microsoft Visual Studio](https://visualstudio.microsoft.com/) is needed. The directory of the input and output should be named in the script before hitting the Compile and run button.

# Sequence Data collection

The metadata of high-quality open-access SARS-CoV-2 sequences file was downloaded from the [RCoV19](https://ngdc.cncb.ac.cn/ncov/release_genome?lang=en) as the input. The following file was downloaded and unzipped:

_metadata.tsv_

# Calculate lineage proportion

**2.1** **Filter metadata by country and collection date**

Filter the metadata file using **FilterMetadata** (C#).

*This will generate a file for each country in ./SCTV01E/LineageProportion/plotdata:

e.g. *"China_202301_metadata.tsv"*

**2.2** **Calculate lineage proportion for each country**

Based on the filtered metadata files, we calculate the lineage proportion over time using **LineageDistributionFromMetadata** (C#).

*This will generate 3 files for each filtered metadata in ./SCTV01E/LineageProportion/plotdata:

e.g.

*"China_LineageDistribution_barplot.txt"*
*"China_LineageDistribution_matrix.txt"*
*"China_RaisedLineage.txt"*

The *"_barplot.txt"* can be directly visualized as a bar plot using [R ggplot](https://r-charts.com/ggplot2/) geom_bar function.

# Construction lineage tree

**3.1** **Obtain common mutations of each lineage**

We obtained the mutations of selected lineages from the outbreak.info website using the [outbreak API](https://outbreak-info.github.io/R-outbreak-info/). The frequency was set to 0.75. Mutations downloaded were futher reformed in Excel into the file *"Lineage mutlist.txt"*.

**3.2** **Generating common sequence for each lineage**

The mutations were transferred to artificial sequences by replacing mutations on the reference sequence using **FromMut2Seq(AminoAcidVer)** (C#)

*This will output a *"LineageSpikeSeq.fa"* file in ./SCTV01E/LineageTree/Data.

**3.2** **Tree build**

Phylogenetic reconstruction was performed with [MEGA-X](https://www.megasoftware.net/dload_win_gui). 

Visualization was performed with [iTol](https://itol.embl.de/). 
Annotation files were prepared manually and can be found in  ./SCTV01E/LineageTree/Data.


# Antigenic map

To obtain the antigenic map of this project, run the R script **Antigenic_map.R** in ./SCVT01E/Antigenic_map. 

The output pdf file can be found at the working directory.
Make sure the input file is in the working directory.
