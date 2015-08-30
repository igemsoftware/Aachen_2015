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
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using MCP.Protocol;

namespace MCP.Equipment
{
    /// <summary>
    /// Interaction logic for SensorInformationWindow.xaml
    /// </summary>
    public partial class GasSensorInformationWindow : Window
    {
        public Task WaitTask = new Task(async delegate { await Task.Delay(1); });
        public bool Confirmed = false;

        public GasSensorInformationWindow(string title, bool canEditID, GasSensorInformation context)
        {
            InitializeComponent();
            this.Title = title;
            sensorIDbox.IsEnabled = canEditID;
            saveButton.IsEnabled = canEditID;
            this.DataContext = context;
            if ((from rd in context.ResponseCurve where double.IsNaN(rd.Percent) select rd).Count() == 0)
            {
                plotter.AddLineChart(context.DataSource).WithDescription("Response Curve").WithStroke(new SolidColorBrush(DesignColors.Blue)).WithStrokeThickness(2);
            }
            context.LoadResponseCurve();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            WaitTask.Start();
            base.OnClosing(e);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            this.Confirmed = true;
            this.Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
