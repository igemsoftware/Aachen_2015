using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using MCP.Cultivation;

namespace MasterControlProgram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectedCultivation_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (plotter.Children.Count > 3)
                plotter.Children[3].Remove();
            plotter.AddLineGraph((e.AddedItems[0] as Experiment).DataSource, Colors.Blue, 2, "Values");

        }

    }
}
