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
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using MCP.Protocol;

namespace MCP.Equipment
{
    /// <summary>
    /// Interaction logic for PumpInformationWindow.xaml
    /// </summary>
    public partial class PumpInformationWindow : Window
    {
        public Task WaitTask = new Task(async delegate { await Task.Delay(1); });
        public bool Confirmed = false;

        public PumpInformationWindow(string title, bool canEditID, PumpInformation context)
        {
            InitializeComponent();
            this.Title = title;
            pumpIDbox.IsEnabled = canEditID;
            saveButton.IsEnabled = canEditID;
            this.DataContext = context;
            var chart = plotter.AddLineGraph(context.DataSource, DesignColors.Blue, 2, "Response Curve");
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
