﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// <summary>
    /// Interaction logic for SeasonManager.xaml
    /// </summary>
    public partial class SeasonManager : Window
    {
        public MainWindow MnWindow { get; set; }
        public SeasonManager()
        {
            InitializeComponent();
            MnWindow = (MainWindow)Application.Current.MainWindow; 
        }

        private void Setup()
        {
            if (MnWindow.CurrentProject.SelectedBasin.GetFlatListOfStorms().Count == 0)
            {
                MessageBox.Show("Please add a storm to use this functionality.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close(); 
            }

            StormList.DataContext = MnWindow.CurrentProject.SelectedBasin.GetFlatListOfStorms(); 
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Setup();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close(); 
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // The user wants to delete a storm.
            if (StormList.SelectedIndex == -1)
            {
                // -1 means no option selected, so alert the user and then do nothing
                MessageBox.Show("No storm selected to delete. [Error Code: ESE1]", "starfrost's Track Maker", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; 
            }

            if (MnWindow.CurrentProject.SelectedBasin.GetFlatListOfStorms()[StormList.SelectedIndex].Id == MnWindow.CurrentProject.SelectedBasin.CurrentLayer.CurrentStorm.Id)
            {
                // if we want to delete the current storm, make the currentstorm null
                MnWindow.CurrentProject.SelectedBasin.CurrentLayer.CurrentStorm = null;
            }

            Storm Sto = (Storm)StormList.SelectedItem; 

            bool R = MnWindow.CurrentProject.SelectedBasin.RemoveStormWithName(Sto.Name);

            // assert if it failed...
            Debug.Assert(R); 
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (StormList.SelectedIndex == -1)
            {
                MessageBox.Show("No storm selected to edit. [Error Code: ESE2]", "starfrost's Track Maker", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // initialise the edit ui

            List<Storm> FlatList = MnWindow.CurrentProject.SelectedBasin.GetFlatListOfStorms();

            EditUI Eui = new EditUI(FlatList[StormList.SelectedIndex]);
            Eui.Owner = this;
            Eui.Show();
            StormList.DataContext = MnWindow.CurrentProject.SelectedBasin.GetFlatListOfStorms();
            StormList.UpdateLayout();
        }

        private void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (StormList.SelectedIndex == -1)
            {
                MessageBox.Show("No storm selected to edit. [Error Code: ESE3]", "starfrost's Track Maker", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<Storm> FlatList = MnWindow.CurrentProject.SelectedBasin.GetFlatListOfStorms();

            // deal with this when it gets ported to Dano
            for (int i = 0; i < FlatList.Count; i++)
            {
                Storm _ = FlatList[i];

                if (StormList.SelectedIndex == i)
                {
                    MnWindow.CurrentProject.SelectedBasin.CurrentLayer.CurrentStorm = _;
                    MessageBox.Show($"The storm {_.Name} is now selected.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                    return; 
                }
            }
        }
    }
}
