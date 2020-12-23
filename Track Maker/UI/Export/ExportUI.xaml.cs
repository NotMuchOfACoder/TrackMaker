﻿using Starfrost.UL5.Logging;
using Starfrost.UL5.ScaleUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Track_Maker
{

    public enum FormatType { Export, Import }
    /// <summary>
    /// Interaction logic for ExportUI.xaml
    /// </summary>
    public partial class ExportUI : Window
    {
        public IExportFormat ExpFormat { get; set; }
        public MainWindow MnWindow { get; set; }
        public FormatType Type { get; set; }
        public List<Storm> StormsToExport { get; set; }
        public ExportUI(FormatType FType, IExportFormat ExportFormat)
        {
            InitializeComponent();
            Export_Init(FType, ExportFormat);
   

        }

        private void Export_Init(FormatType FType, IExportFormat ExportFormat)
        {
            Logging.Log("ExportUI Initialising...");
            Logging.Log($"Format type: {FType}, export format: {ExportFormat.Name}");
            MnWindow = (MainWindow)Application.Current.MainWindow;
            ExpFormat = ExportFormat;
            StormsToExport = MnWindow.CurrentProject.SelectedBasin.GetFlatListOfStorms(); // feature pushed back to Dano, maybe even 3.0/"Aurora"
            Type = FType;

            if (ExportFormat is IImageExportFormat)
            {
                IImageExportFormat ExpFI = (IImageExportFormat)ExportFormat;
                if (!ExpFI.DisplayQualityControl) HideQualityControl(); 
            }
            else // not an IImageExportFormat
            {
                HideQualityControl();
            }

            GeneratePreview(); 

            //completely different in Dano
            //ExportFormat.GeneratePreview(ExportPlatform_Preview);
            //ExportPlatform_Preview.UpdateLayout(); 
        }

        // Set the header using the Export Platform. 
        private void Setup()
        {

            // Set the "Export to..." text based on the ExportFormat's Name and the Type supplied to us in the constructor. 
            switch (Type)
            {
                case FormatType.Import:
                    ExportPlatform_ExportBtn.Content = "Import";
                    ExportPlatform.Text = $"Import from {ExpFormat.GetName()}";
                    Title = $"Import from {ExpFormat.GetName()}";
                    return;
                case FormatType.Export:
                    ExportPlatform.Text = $"Export to {ExpFormat.GetName()}";
                    Title = $"Export to {ExpFormat.GetName()}";
                    return;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Setup();
        }

        private void ExportPlatform_ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            // Call RunIEX to handle the import or export.
            if (!RunIEX())
            {
                // last error global thing in error probably needed
                Error.Throw("Warning", "191: The export failed", ErrorSeverity.Warning, 191); 
            }
            else
            {
                Close(); 
            }

            return;
        }

        private void HideQualityControl()
        {
            Height = 474;
            Width = 918;
            ExportPlatform_PreviewTextBlock.Margin = new Thickness(10, 89, 0, 0); 
            ExportPlatform_PreviewBorder.Margin = new Thickness(10, 118, 0, 0);
            ExportPlatform_ExportBtn.Margin = new Thickness(782, 381, 0, 0);

            // Make the quality control uninteractable and invisible
            QualityControl.Height = 0;
            QualityControl.Width = 0;
            QualityControl.IsEnabled = false;

        }

        /// <summary>
        /// Method name: RunIEX()
        /// 
        /// Runs the import/export (Version 515)
        /// </summary>
        private bool RunIEX()
        {

            // Old code - dano move when we can do easy previews due to multitab
            // Stop the ticktimer while importing or exporting because we need to do this stuff.
            MnWindow.TickTimer.Stop();

            switch (Type)
            {
                case FormatType.Import:
                    ImportResult IResult = ExpFormat.Import();

                    switch (IResult.Status)
                    {
                        case ExportResults.Cancelled:
                            MnWindow.TickTimer.Start();
                            return true;
                        case ExportResults.Error:
                            MnWindow.TickTimer.Start();
                            return false;
                        case ExportResults.OK:
                            if (IResult.Project.FileName == null)
                            {
                                Error.Throw("Fatal Error!", "Failed to set current file name", ErrorSeverity.Error, 190);
                                return false; // possibly extend subsequent else 
                            }
                            else
                            {
                                GlobalStateP.SetCurrentOpenFile(IResult.Project.FileName);
                            }

                            //may bindings work?
                            MnWindow.Title = $"Track Maker 2.0 - {GlobalStateP.GetCurrentOpenFile()}";

                            // we are not setting current project before, usually you wouldn't need to do this hack
                            MnWindow.CurrentProject = IResult.Project;

                            MnWindow.TickTimer.Start();
                            MnWindow.UpdateLayout();


                            return true;
                    }
                    // this shouldn't run
                    return true; 
                    
                case FormatType.Export:

                    if (ExpFormat is IImageExportFormat)
                    {
                        IImageExportFormat IIEF = (IImageExportFormat)ExpFormat;

                        // surely allowing (bool == bool?) would be more intuitive for users
                        bool IsFullQuality = (bool)QualityControl.QualityControl_FullQuality.IsChecked;
                        bool IsHalfQuality = (bool)QualityControl.QualityControl_HalfQuality.IsChecked;
                        bool IsQuarterQuality = (bool)QualityControl.QualityControl_QuarterQuality.IsChecked;
                        bool IsEighthQuality = (bool)QualityControl.QualityControl_EighthQuality.IsChecked;

                        if (IsFullQuality) IIEF.Quality = ImageQuality.Full;
                        if (IsHalfQuality) IIEF.Quality = ImageQuality.Half;
                        if (IsQuarterQuality) IIEF.Quality = ImageQuality.Quarter;
                        if (IsEighthQuality) IIEF.Quality = ImageQuality.Eighth;

                        // need to make this priscilla-specific
                        return ExpFormat.Export(MnWindow.CurrentProject); 

                    }
                    else
                    {
                        ExpFormat.Export(MnWindow.CurrentProject);
                    }


                    // wish VS allowed the samE var names under different code paths
                    Project CurProject = MnWindow.CurrentProject;



                    if (CurProject.FileName != null && CurProject.FileName != "") MnWindow.Title = $"Track Maker 2.0 - {CurProject.FileName}";
                    MnWindow.UpdateLayout();
                    MnWindow.TickTimer.Start();
                    Close();
                    return true;
            }

            return false;
        }

        // previewgenerator
        private bool GeneratePreview()
        {
            // in iris we will have proper temporary file management
            string TempFileName = $"tmm_preview_{DateTime.Now.ToString("dddd-mm-yy HHmmss")}.png.tmp";

            // there's gotta be better ways to do this
            ExportImage EI = new ExportImage();
            EI.ExportCore(MnWindow.CurrentProject, TempFileName);

            // load it again
            BitmapImage TempImage = new BitmapImage();
            TempImage.BeginInit();
            TempImage.UriSource = new Uri(TempFileName);
            TempImage.EndInit();

            ExportPlatform_Preview.Background = TempImage;

            File.Delete(TempFileName);

        }
    }
}
