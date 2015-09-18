using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD;
using TCD.Controls;
using MCP.Protocol;

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
        
        //TODO: in Visual Studio go to View/Task List and then select "Comments" - this shows a list of all comments starting with TODO - like this one for example
        //TODO: suggestions what to build:
        //TODO: first implement a simple "message composer". Imaging dropdown-menus to select the ParticipantIDs and MessageType of the first three bytes and then a textbox where you can enter the rest of the message (using ~ as a placeholder for \t and then string.Replace('~', '\t') before sending the message
        //      this first component is useful for Sayantan to test the software on the Arduino
        //TODO: next implement to receive data/readings from the scale
        //TODO: then implement automatic calibration stuff...
			

        public ViewModel()
        {
            SetSteppingSpeed = new RelayCommand(delegate
            {
                PrimarySerial.SendString(string.Format("STEPS_PER_MINUTE\t{0}", StepsPerMinute));
            });
        }
    }
}
