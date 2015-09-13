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

namespace ProtocolPuppet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            fromCB.SetUpItems(ParticipantID.MCP);
            toCB.SetUpItems(ParticipantID.Reactor_1);
            typeCB.SetUpItems(MessageType.Command);
            (App.Current.Resources["ViewModel"] as ViewModel).PropertyChanged += MainWindow_PropertyChanged;
        }

        void MainWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "MessageLog":
                    logBlock.ScrollToEnd();
                    break;
                default:
                    break;
            }
        }
    }
}
