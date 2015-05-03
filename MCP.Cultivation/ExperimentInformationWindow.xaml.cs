using MCP.Equipment;
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
using System.Windows.Shapes;

namespace MCP.Cultivation
{
    /// <summary>
    /// Interaction logic for ExperimentInformationWindow.xaml
    /// </summary>
    public partial class ExperimentInformationWindow : Window
    {
        public Task WaitTask = new Task(async delegate { await Task.Delay(1); });
        public bool Confirmed = false;

        public ExperimentInformationWindow(string title, bool canEdit)
        {
            InitializeComponent();
            this.Title = title;
            titleBox.IsEnabled = canEdit;
            datePicker.IsEnabled = canEdit;
            reactorsSelector.ItemsSource = Inventory.Current.Reactors.Keys;
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
