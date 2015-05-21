using System;
using System.Collections.Generic;
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
using TCD.Controls;

using Microsoft.Research.DynamicDataDisplay.DataSources;
using MCP.Protocol;

namespace PumpCalibrator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            speedBox.SetUpItems(PumpingSpeed.Medium);
            baudrateBox.SetUpItems(BaudRate._9600);
            plotter.AddLineGraph(ViewModel.Current.Calibrator.DataSource, Colors.Blue, 2, "weight");    
        }
    }
}
