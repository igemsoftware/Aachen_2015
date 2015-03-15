using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SerialInterface;
using TCD;
using TCD.Controls;

namespace PumpController
{
    public class ViewModel : PropertyChangedBase
    {
        private SerialIO _PrimarySerial = new SerialIO();
        public SerialIO PrimarySerial { get { return _PrimarySerial; } set { _PrimarySerial = value; OnPropertyChanged(); } }

        private int _StepsPerMinute = 60;
        public int StepsPerMinute { get { return _StepsPerMinute; } set { _StepsPerMinute = value; OnPropertyChanged(); } }

        private RelayCommand _SetSteppingSpeed;
        public RelayCommand SetSteppingSpeed { get { return _SetSteppingSpeed; } set { _SetSteppingSpeed = value; OnPropertyChanged(); } }
        
			
			

        public ViewModel()
        {
            SetSteppingSpeed = new RelayCommand(delegate
            {
                PrimarySerial.SendString(string.Format("STEPS_PER_MINUTE\t{0}", StepsPerMinute));
            });
        }
    }
}
