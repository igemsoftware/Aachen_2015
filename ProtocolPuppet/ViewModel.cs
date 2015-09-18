using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD.Controls;
using MCP.Protocol;
using TCD;

namespace ProtocolPuppet
{
    public class ViewModel : PropertyChangedBase
    {
        private SerialIO _PrimarySerial = new SerialIO();
        public SerialIO PrimarySerial { get { return _PrimarySerial; } set { _PrimarySerial = value; OnPropertyChanged(); } }
        
        private RelayCommand _SendMessageCommand;
        public RelayCommand SendMessageCommand { get { return _SendMessageCommand; } set { _SendMessageCommand = value; OnPropertyChanged(); } }

        private ParticipantID _From;
        public ParticipantID From { get { return _From; } set { _From = value; OnPropertyChanged(); } }

        private ParticipantID _To;
        public ParticipantID To { get { return _To; } set { _To = value; OnPropertyChanged(); } }

        private MessageType _Type;
        public MessageType Type { get { return _Type; } set { _Type = value; OnPropertyChanged(); } }
        
			

        private string _Contents;
        public string Contents { get { return _Contents; } set { _Contents = value; OnPropertyChanged(); } }

        private string _MessageLog;
        public string MessageLog { get { return _MessageLog; } set { _MessageLog = value; OnPropertyChanged(); } }
        
			

        public ViewModel()
        {
            PrimarySerial.NewMessageReceived += PrimarySerial_NewMessageReceived;
            SendMessageCommand = new RelayCommand(delegate
            {
                SendMessage();
            });
        }
        private void PrimarySerial_NewMessageReceived(object sender, Message message)
        {
            try
            {
                MessageLog += message.ToString() + "\r\n";
            }
            catch
            {

            }

        }

        private void SendMessage()
        {
            try
            {
                Message msg = new Message(From, To, Type, Contents.Split(new string[] { "\\t" }, StringSplitOptions.None));
                PrimarySerial.SendMessage(msg);
                MessageLog += msg.ToString() + "\r\n";
            }
            catch
            {

            }
        }
    }
}
