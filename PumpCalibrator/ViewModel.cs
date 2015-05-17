using MCP.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD;

namespace PumpCalibrator
{
    public class ViewModel : PropertyChangedBase
    {
        private SerialIO _PrimarySerial = new SerialIO();
        public SerialIO PrimarySerial { get { return _PrimarySerial; } set { _PrimarySerial = value; OnPropertyChanged(); } }

        private RelayCommand _StartCalibrationCommand;
        public RelayCommand StartCalibrationCommand { get { return _StartCalibrationCommand; } set { _StartCalibrationCommand = value; OnPropertyChanged(); } }

			

        public ViewModel()
        {
            
        }
    }
}
