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
using MCP.Calibration;
using Microsoft.Research.DynamicDataDisplay.Markers2;

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
            plotter.Legend.HorizontalAlignment = HorizontalAlignment.Left;
            fluidBox.SetUpItems(CalibrationFluid.Water);
            targetBox.UseEnumItemTemplate();
            targetBox.ItemsSource = (new CalibrationTarget[] { CalibrationTarget.Pump, CalibrationTarget.Stirrer });
            modeBox.SetUpItems(CalibrationMode.Standard);
            baudrateBox.SetUpItems(BaudRate._9600);
            ViewModel.Current.Calibrator.Subcalibrations.CollectionChanged += Subcalibrations_CollectionChanged;
        }

        private void Subcalibrations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //remove old graphs
            for (int i = 0; i < plotter.Children.Count; i++)
            {
                if (plotter.Children[i] is LineChart)
                {
                    plotter.Children.RemoveAt(i);
                    i--;
                }
            }
            //add new graphs
            foreach (Subcalibration sub in ViewModel.Current.Calibrator.Subcalibrations)
            {
                plotter.AddLineChart(sub.DataSource).WithDescription(sub.Setpoint.ToString()).WithStroke(new SolidColorBrush(GetRandomColor())).WithStrokeThickness(2);
            }
        }

        private Random rnd = new Random();
        private Color GetRandomColor()
        {
            return Color.FromArgb((byte)255, (byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255));
        }
    }
}
