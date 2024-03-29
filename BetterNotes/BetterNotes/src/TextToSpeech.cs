﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;

namespace BetterNotes {
    public static class TextToSpeech {
            public static void GetSpeech(string input) {
                var reader = new SpeechSynthesizer();
                reader.SetOutputToDefaultAudioDevice();
                reader.Speak(input);
                reader.Dispose();
            }//getspeech
            public static void PutSpeechInFile(string input, string path) {
                var reader = new SpeechSynthesizer();
                if (File.Exists(path)) File.Delete(path); 
                reader.SetOutputToWaveFile(path);
                reader.Speak(input);
                reader.Dispose();
            }//file
     }//class 
}
