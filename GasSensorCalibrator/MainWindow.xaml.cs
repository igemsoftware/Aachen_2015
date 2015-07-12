using MCP.Calibration;
using MCP.Protocol;
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
using TCD.Controls;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace GasSensorCalibrator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            baudrateBox.SetUpItems(BaudRate._9600);
            targetBox.UseEnumItemTemplate();
            targetBox.ItemsSource = (new CalibrationTarget[] { CalibrationTarget.Oxygen, CalibrationTarget.Carbon_Dioxide, CalibrationTarget.CHx });
            modeBox.SetUpItems(CalibrationMode.Standard);
            //chart
            plotter.AddLineGraph(ViewModel.Current.DataSource, DesignColors.Red, 2, "Sensor Value");
        }
    }
}
