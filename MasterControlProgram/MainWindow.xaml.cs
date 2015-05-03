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
using TCD;
using TCD.Controls;

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
            for (int i = 0; i < plotter.Children.Count; i++)
            {
                if (plotter.Children[i] is LineGraph)
                {
                    plotter.Children.RemoveAt(i);
                    i--;
                }
            }
            plotTitle.Content = string.Empty;
            foreach (Cultivation cultivation in (sender as ListBox).SelectedItems)
            {
                plotTitle.Content += cultivation.Reactor.ParticipantID.GetValueName() + " ";
                plotter.AddLineGraph(cultivation.DataSource, Colors.Blue, 2, "Values");
            }
        }

    }
}
