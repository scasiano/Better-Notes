﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;

namespace BetterNotes
{
    public class TextToSpeech
    {
            public static void GetSpeech(string input)
            {
                SpeechSynthesizer reader = new SpeechSynthesizer();
                reader.SetOutputToDefaultAudioDevice();
                reader.Speak(input);
                reader.Dispose();
            }//getspeech

            public static void PutSpeechInFile(string input, string path)
            {
                SpeechSynthesizer reader = new SpeechSynthesizer();
                reader.SetOutputToWaveFile(path);
                reader.Speak(input);
                reader.Dispose();
            }//file
     }//class 
    
}
