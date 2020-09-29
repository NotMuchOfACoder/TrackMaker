﻿using Microsoft.Win32; 
using System;
using System.Collections.Generic;
using System.IO; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Track_Maker
{
    /// <summary>
    /// BestTrack export services
    /// 
    /// This is crap because I don't understand what half of this shitty format does. 
    /// 
    /// Created: 2020-05-17
    /// 
    /// Modified: 2020-09-25 (v2.0.462)
    /// </summary>
    public class ExportBestTrack : IExportFormat
    {
        public bool AutoStart { get; set; }
        public MainWindow MnWindow { get; set; }
        public string Name { get; set; }

        public ExportBestTrack()
        {
            AutoStart = true;
            Name = "Best Track (ATCF)";
            MnWindow = (MainWindow)Application.Current.MainWindow;
        }

        public string GetName()
        {
            return Name; 
        }

        public Project Import()
        {
            throw new NotImplementedException(); 
        }

        /// <summary>
        /// Needs to be majorly updated and refactored for v2
        /// </summary>
        /// <param name="basin"></param>
        /// <returns></returns>
        public bool Export(Project Project)
        {
            try
            {
                SaveFileDialog SFD = new SaveFileDialog();
                SFD.Title = "Save to best track - enter folder name";
                SFD.Filter = "Folders|*.";
                SFD.ShowDialog();

                if (SFD.FileName == "") return true;

                //utilfunc v2
                if (File.Exists(SFD.FileName))
                {
                    File.Delete(SFD.FileName);
                    FileStream FS = File.Create(SFD.FileName);
                    FS.Close();
                }

                // Run the export and if we failed clean up 
                if (!ExportCore(Project, SFD.FileName))
                {
                    string _ = SFD.FileName.Replace(".", "");
                    foreach (string FileName in Directory.EnumerateFiles(_))
                    {
                        File.Delete(FileName);
                    }

                    Directory.Delete(_); 
                }
            }
            catch (PathTooLongException err)
            {
                MessageBox.Show($"Error: Windows sucks. [Error Code: EB2]\n\n{err}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (IOException err)
            {
                MessageBox.Show($"An error occurred writing to EasyTimeline format. [Error Code: EB1]\n\n{err}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Needs to be majorly updated and refactored for v2
        /// </summary>
        /// <param name="basin"></param>
        /// <returns></returns>
        public bool ExportCore(Project Project, string FileName)
        {
            Directory.CreateDirectory(FileName);
            Directory.SetCurrentDirectory(FileName.Replace(".","")); 

            // create a file for each storm
            foreach (Storm Storm in Project.SelectedBasin.GetFlatListOfStorms())
            {
                if (Storm.Name.Length > 12)
                {   
                    // Woah, calm it (Priscilla 442)
                    MessageBox.Show("This format, the ATCF BestTrack format, has several limitations (and is obsolete to begin with). Exporting to this format is not recommended. \n\nLimitations:\n\n- It does not support storms with names with a length above twelve characters.\n-It does not support abbreviated character names of lengths more than three characters.\n- It uses weird different spacing.\n\n- There is no documentation on the Internet, despite my efforts.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Create a new file for each storm.
                using (StreamWriter SW = new StreamWriter(new FileStream($@"{Storm.Name}", FileMode.Create)))
                {
                    // For each node. 
                    foreach (Node Node in Storm.NodeList)
                    {
                        SW.Write("AL, ");

                        // Pad with a zero if <10 and write storm id.
                        SW.Write($"{Utilities.PadZero(Storm.Id + 1)}, ");

                        // Get the node date
                        DateTime NodeDate = Storm.GetNodeDate(Node.Id); 

                        SW.Write($"{NodeDate.ToString("yyyyMMdd")}{Utilities.PadZero(NodeDate.Hour)}");

                        // Some weird padding shit
                        SW.Write(",   , ");

                        // Best marker, more useless padding
                        SW.Write("BEST,   ");

                        // God knows what this does
                        SW.Write("0, ");

                        // Node position.

                        Coordinate X = Project.SelectedBasin.FromNodePositionToCoordinate(Node.Position); 

                        SW.Write($"{X.Coordinates.X.ToString()}{X.Directions[0].ToString()},  {X.Coordinates.Y.ToString()}{X.Directions[1].ToString()},  ");

                        // Intensity.

                        for (int i = 0; i < 4 - Node.Intensity.ToString().Length; i++)
                        {
                            SW.Write(" ");
                        }

                        // Convert to KT (DANO: MOVE TO DEDICATED HurricaneConversions CLASS)

                        double Intensity = Utilities.RoundNearest(Node.Intensity / 1.151, 5);

                        SW.Write($"{Intensity}, ");

                        // Pressure. We don't do this until dano and even then it will be optional. Just put 1000mbars
                        SW.Write("1000, ");

                        // Write the category and a WHOLE bunch of information that we don't need or use yet - environmental pressure etc - I don't know what most of these are tbh

                        string Cat = Utilities.AbbreviateCategory(Storm.GetNodeCategory(Node, MnWindow.Catman.CurrentCategorySystem).Name);

                        if (Cat.Length > 3)
                        {
                            MessageBox.Show("Due to the limitations of the ATCF format, category names cannot be longer than 3 characters abbreviated.");
                            return false; 
                        }

                        // Placeholder information
                        for (int i = 0; i < 3 - Cat.Length; i++)
                        {
                            SW.Write(" ");
                        }

                        SW.Write($"{Cat},  "); // todo: abbreviate (dano)

                        // Aforementioned information
                        SW.Write($"x,   0,    ,    0,    0,    0,    0, 1014,   90,  60,   0,   0,   L,   0,    ,   0,   0, ");

                        // Shit Format
                        for (int i = 0; i < (12 - Storm.Name.Length); i++)
                        {
                            SW.Write(" ");
                        }

                        SW.Write(Storm.Name);

                        // More pointless and irrelevant data.
                        SW.Write(", M,  0,    ,    0,    0,    0,    0, ");

                        // Why does this exist
                        SW.Write("genesis-num, ");
                        SW.Write(Utilities.PadZero(Storm.Id + 1, 2));
                        SW.Write(", \n"); // Write newline
                    }
                }
            }

            
            return true;
        }

        public void GeneratePreview(Canvas canvas)
        {
            throw new NotImplementedException(); 
        }

    }
}
