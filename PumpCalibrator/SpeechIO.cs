using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Speech.Recognition;

namespace PumpCalibrator
{
    public static class SpeechIO
    {
        private static SpeechSynthesizer Synthesizer = new SpeechSynthesizer();
        //private static SpeechRecognizer _Recognizer = new SpeechRecognizer();
        //public static SpeechRecognizer Recognizer { get { return _Recognizer; } set { _Recognizer = value; } }

			
        private static Choices Commands = new Choices("Start calibration", "Finish calibration");

        static SpeechIO()
        {
          //  InitializeSpeechRecogonition();
        }
        private static void InitializeSpeechRecogonition()
        {
            LoadGrammar();
        }
        private static void LoadGrammar()
        {
            //Load up the Choices object with the contents of the Color list, populate the GrammarBuilder, 
            //create a Grammar with the Grammar builder helper and load it up into the SpeechRecognizer 
            //GrammarBuilder grammarBuilder = new GrammarBuilder(Commands);
            //Grammar testGrammar = new Grammar(grammarBuilder);
            //Recognizer.LoadGrammar(testGrammar);
        }

        public static void Speak(string text)
        {
            Synthesizer.SpeakAsync(text);
            
        }

    }
}
