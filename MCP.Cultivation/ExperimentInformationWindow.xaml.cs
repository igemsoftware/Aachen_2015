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
using TCD;

namespace MCP.Cultivation
{
    /// <summary>
    /// Interaction logic for ExperimentInformationWindow.xaml
    /// </summary>
    public partial class ExperimentInformationWindow : Window
    {
        private Task WaitTask = new Task(async delegate { await Task.Delay(1); });
        public bool Confirmed = false;
        private List<ReactorAssociationInfo> reactorAssociations = new List<ReactorAssociationInfo>();
        
        public ExperimentInformationWindow(string title, bool canEdit)
        {
            InitializeComponent();
            this.Title = title;
            titleBox.IsEnabled = canEdit;
            datePicker.IsEnabled = canEdit;
            reactorsSelector.IsEnabled = canEdit;
            Initialize();
        }
        private async void Initialize()
        {
            await Task.Delay(5);
            //populate the items control for reactor selection
            if ((this.DataContext as Experiment).Cultivations.Count == 0)
                foreach (ParticipantID pid in Inventory.Current.Reactors.Keys)
                    reactorAssociations.Add(new ReactorAssociationInfo(pid, false));
            else
                foreach (Cultivation c in (this.DataContext as Experiment).Cultivations)
                    reactorAssociations.Add(new ReactorAssociationInfo(c.Reactor.ParticipantID, true));
            reactorsSelector.DataContext = reactorAssociations;
        }
        public async Task<List<ParticipantID>> ShowAsync()
        {
            this.Show();
            await WaitTask;
            List<ParticipantID> selectedReactors = new List<ParticipantID>();
            foreach (ReactorAssociationInfo rai in reactorAssociations)
                if (rai.IsAssociated)
                    selectedReactors.Add(rai.ReactorID);
            return selectedReactors;
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
    public class ReactorAssociationInfo : PropertyChangedBase
    {
        private ParticipantID _ReactorID;
        public ParticipantID ReactorID { get { return _ReactorID; } set { _ReactorID = value; OnPropertyChanged(); } }

        private bool _IsAssociated;
        public bool IsAssociated { get { return _IsAssociated; } set { _IsAssociated = value; OnPropertyChanged(); } }

        public ReactorAssociationInfo(ParticipantID id, bool value)
        {
            this.ReactorID = id;
            this.IsAssociated = value;
        }
    }
}
