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
using TCD;
using MCP.Protocol;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using MCP.Calibration;

namespace ODCalibrator
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
            //chart
            plotter.AddLineChart(ViewModel.Current.DataSource).WithDescription("Sensor Value").WithStroke(new SolidColorBrush(DesignColors.Red)).WithStrokeThickness(2);
        }
    }
}
