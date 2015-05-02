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

namespace MCP.Equipment
{
    /// <summary>
    /// Interaction logic for ReactorInformationWindow.xaml
    /// </summary>
    public partial class ReactorInformationWindow : Window
    {
        public Task WaitTask = new Task(async delegate { await Task.Delay(1); });
        public bool Confirmed = false;

        public ReactorInformationWindow(string title, bool canEditID, IEnumerable<string> availablePumps)
        {
            InitializeComponent();
            this.Title = title;
            reactorIDbox.IsEnabled = canEditID;
            var pids = (Enum.GetValues(typeof(ParticipantID)) as ParticipantID[]).Skip(2);
            reactorIDbox.ItemsSource = pids;
            feedID.ItemsSource = availablePumps;
            aerationID.ItemsSource = availablePumps;
            harvestID.ItemsSource = availablePumps;
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
