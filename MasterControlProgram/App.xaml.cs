using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MasterControlProgram
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //TODO: Menu Bar with "Settings" item
        //TODO: SettingsWindow
        //TODO: Setting for Home Directory
        //TODO: system.windows.forms.datavisualization

        public static ViewModel ViewModel { get { return App.Current.Resources["ViewModel"] as ViewModel; } }
        
			
    }
}
