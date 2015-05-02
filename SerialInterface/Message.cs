using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialInterface
{
    public class Message
    {
        public ParticipantID Sender { get; private set; }
        public ParticipantID Receiver { get; private set; }
        public MessageType MessageType { get; private set; }
        public string[] Contents { get; private set; }

        public string Raw
        {
            get
            {
                return string.Format("{0}{1}{2}{3}", (char)Sender, (char)Receiver, (char)MessageType, string.Join("\t", Contents));
            }
        }

        public Message()
        {

        }
        public Message(string raw)
        {
            Sender = (ParticipantID)(byte)raw[0];
            Receiver = (ParticipantID)(byte)raw[1];
            MessageType = (MessageType)(byte)raw[2];
            Contents = raw.Substring(3).Split('\t');
        }
        public Message(ParticipantID sender, ParticipantID receiver, MessageType type, params string[] contents)
        {
            this.Sender = sender;
            this.Receiver = receiver;
            this.MessageType = type;
            this.Contents = contents;
        }

        public override string ToString()
        {
            return this.Raw;
        }
    }
}
