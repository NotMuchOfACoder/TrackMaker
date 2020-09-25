﻿using Microsoft.Win32; 
using System;
using System.Collections.Generic;
using System.IO; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// EasyTimeline export. Created for 1.0
/// 
/// This is a giant, no-good, shitty, evil, bad, do not do this, definition of what not to do, but oh well, I have to finish this shit. i said it'd be out 2 days ago!
/// 
/// Created: 2020-05-13
/// 
/// Modified: 2020-05-17
/// 
/// TODO: Abbreviation for category names, replace godawful center tag 
/// </summary>
namespace Track_Maker
{
    public class ExportEasyTimeline : IExportFormat
    {
        public bool AutoStart { get; set; }
        internal MainWindow MnWindow { get; set; }
        public string Name { get; set; }

        public ExportEasyTimeline()
        {
            AutoStart = true;
            MnWindow = (MainWindow)Application.Current.MainWindow;
            Name = "EasyTimeline";
        }

        public Project Import()
        {
            throw new NotImplementedException();  //EasyTimeline does not support import.

        }

        public string GetName()
        {
            return Name;
        }

        public bool Export(Project Project)
        {
            try
            {
                SaveFileDialog SFD = new SaveFileDialog();

                SFD.Filter = "Text documents|*.txt";

                SFD.ShowDialog();

                if (SFD.FileName == "") return true; // The user did not select a file. 

                //utilfunc v2
                if (File.Exists(SFD.FileName))
                {
                    File.Delete(SFD.FileName);

                    FileStream fc = File.Create(SFD.FileName);
                    fc.Close(); 
                }

                ExportCore(Project, SFD.FileName);

                return true;
            }
            catch (PathTooLongException err)
            {
                MessageBox.Show($"Error: Windows sucks. [Error Code: EE2]\n\n{err}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (IOException err)
            {
                MessageBox.Show($"An error occurred writing to EasyTimeline format. [Error Code: EE1]\n\n{err}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool ExportCore(Project Project, string FileName)
        {
            using (StreamWriter SW = new StreamWriter(new FileStream(FileName, FileMode.Create)))
            {
                // maybe remove this
                SW.WriteLine("<!-- Generated by Track Maker 2.x © 2019-2020 starfrost. -->"); 
                SW.WriteLine("<div style=\"overflow:auto\">");
                SW.WriteLine("<center>");
                SW.WriteLine("<timeline>");
                SW.WriteLine("ImageSize = width:950 height:200"); // the image size of the timeline
                SW.WriteLine("PlotArea  = top:10 bottom:80 right:20 left:20"); // the plot area of the timeline
                SW.WriteLine("Legend    = columns:3 left:30 top:58 columnwidth:300"); // the plot area of the timeline
                SW.WriteLine("AlignBars = early"); // bar alignment

                // section 2: definition part 2

                // This code is a mess

                List<Storm> FlatList = Project.SelectedBasin.GetFlatListOfStorms();

                DateTime _startdate = FlatList[0].FormationDate; // The formation date of the first storm. 
                DateTime _startdate_period =  _startdate.AddDays(-_startdate.Day).AddMonths(-1);
                DateTime _enddate = FlatList[FlatList.Count - 1].GetDissipationDate().AddDays(1); // The dissipation date of the last storm. 

                SW.WriteLine("DateFormat = dd/mm/yyyy\n"); // date format
                SW.WriteLine($"Period = from:{_startdate_period.ToString("dd'/'MM'/'yyyy")} till:{_enddate.ToString("dd'/'MM'/'yyyy")}"); // the period. generated dynamically.
                SW.WriteLine("TimeAxis = orientation:horizontal"); // the time axis - x or y
                SW.WriteLine($"ScaleMinor = grid:black unit:month increment:1 start:{_startdate_period.ToString("dd'/'MM'/'yyyy")}");

                // section 3: defintion part 3: C O L O U R S

                SW.WriteLine("Colors = ");

                // Dynamically generate the colour list.
                SW.WriteLine("  id:canvas value:gray(0.88)");
                SW.WriteLine("  id:GP     value:red");

                // Dynamically generate the colours - conversion system for 1.0/2.0?

                foreach (Category Cat in MnWindow.Catman.CurrentCategorySystem.Categories)
                {
                    if (Cat.HigherBound < 999)
                    {
                        SW.WriteLine($"  id:{Cat.Name.Replace(" ", "")} value:rgb({Cat.Color.ScR}, {Cat.Color.ScG}, {Cat.Color.ScB}) legend:{Cat.Name.Replace(" ", "_")}_-_{Cat.LowerBound}-{Cat.HigherBound}_mph_({Utilities.RoundNearest(Cat.LowerBound * 1.61, 1)}-{Utilities.RoundNearest(Cat.HigherBound * 1.61, 1)}_km/h)");
                    }
                    else
                    {
                        SW.WriteLine($"  id:{Cat.Name.Replace(" ", "")} value:rgb({Cat.Color.ScR}, {Cat.Color.ScG}, {Cat.Color.ScB}) legend:{Cat.Name.Replace(" ", "_")}_-_>{Cat.LowerBound}_mph_(>{Utilities.RoundNearest(Cat.LowerBound * 1.61, 1)}_km/h)");
                    }

                }

                // section 4: definition part 4: Background C O L O U R S
                SW.WriteLine("Backgroundcolors = canvas:canvas");

                // section 5: bar data
                SW.WriteLine("BarData = ");
                SW.WriteLine("  barset:Hurricane");
                SW.WriteLine("  bar:Month");

                // section 6: plot data
                SW.WriteLine("PlotData = ");
                SW.WriteLine("  barset:Hurricane width:10 align:left fontsize:S shift:(4,-4) anchor:till"); // write the basic information

                
                // Write the storm information for EasyTimeline - section 7
                for (int i = 0; i < FlatList.Count; i++)
                {
                    // Get the peak storm category. 
                    Storm St = FlatList[i];
                    Category Cat = St.GetPeakCategory(St, MnWindow.Catman.CurrentCategorySystem);

                    // write the storm info in ddmmyyyy format
                    SW.WriteLine($"  from:{St.FormationDate.ToString("dd'/'MM'/'yyyy")} till:{St.GetDissipationDate().ToString("dd'/'MM'/'yyyy")} color:{Cat.Name.Replace(" ", "")} text:{St.Name} ({Cat.Name})");

                    if (i % 6 == 5) // every 6, do this
                    {
                        SW.WriteLine("  barset:break");
                    }
                }

                // section 8: months
                SW.WriteLine("  bar:Month width:5 align:center fontsize:S shift:(0,-20) anchor:middle color:canvas");

                for (int i = 0; i <= Utilities.GetMonthsBetweenTwoDates(_startdate_period, _enddate); i++)
                {
                    DateTime TD = new DateTime();
                    DateTime TD_next = new DateTime();

                    // Prevent goin before the startdate and causing EasyTimeline to generate an error
                    if (i == 0)
                    {
                        TD = _startdate.AddDays(-_startdate.Day).AddMonths(-1);
                        // don't bother with i+1
                        TD_next = TD.AddMonths(1);
                    }
                    else
                    {
                        TD = _startdate_period.AddMonths(i);
                        TD_next = TD.AddMonths(1); // 7es

                        // Add this. 
                        if (TD_next > _enddate)
                        {
                            continue; // force continue so we don't have dupes
                        }

                    }

                    SW.WriteLine($"  from:{TD.ToString("dd'/'MM'/'yyyy")} till:{TD_next.ToString("dd'/'MM'/'yyyy")} text:{TD_next.ToString("MMMM")}");
                }

                // section 9: end
                SW.WriteLine("</timeline>");
                SW.WriteLine("</center>");
                SW.WriteLine("</div>");

            }

            string[] Lines = File.ReadAllLines(FileName); //V2 LineArray

            Clipboard.SetText(Lines.ConvertArrayToString());

            MessageBox.Show("Saving successful. The EasyTimeline syntax has also been copied to the clipboard.", "Information", MessageBoxButton.OK, MessageBoxImage.Information); 
            return true;
        }
        public void GeneratePreview(Canvas canvas)
        {
            return;
        }
    }
}
