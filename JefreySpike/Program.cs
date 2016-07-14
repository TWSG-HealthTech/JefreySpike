using System;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using System.Globalization;
namespace ConsoleSpeech
{
    class ConsoleSpeechProgram
    {
        static SpeechSynthesizer ss = new SpeechSynthesizer();
        static SpeechRecognitionEngine sre;
        static bool done = false;
        static bool speechOn = true;
        static void Main(string[] args)
        {
            try
            {
                ss.SetOutputToDefaultAudioDevice();
                Console.WriteLine("\n(Speaking: I am awake)");
                ss.Speak("I am awake");
                CultureInfo ci = new CultureInfo("en-gb");
                sre = new SpeechRecognitionEngine(ci);
                sre.SetInputToDefaultAudioDevice();
                sre.SpeechRecognized += sre_SpeechRecognized;
                Choices ch_StartStopCommands = new Choices();
                ch_StartStopCommands.Add("speech on");
                ch_StartStopCommands.Add("speech off");
                ch_StartStopCommands.Add("klatu barada nikto");
                GrammarBuilder gb_StartStop = new GrammarBuilder();
                gb_StartStop.Append(ch_StartStopCommands);
                Grammar g_StartStop = new Grammar(gb_StartStop);
                Choices ch_Numbers = new Choices();
                ch_Numbers.Add("1");
                ch_Numbers.Add("2");
                ch_Numbers.Add("3");
                ch_Numbers.Add("4");
                ch_Numbers.Add("5");
                ch_Numbers.Add("6");
                ch_Numbers.Add("7");
                ch_Numbers.Add("8");
                ch_Numbers.Add("9");
                GrammarBuilder gb_WhatIsXplusY = new GrammarBuilder();
                gb_WhatIsXplusY.Append("What is");
                gb_WhatIsXplusY.Append(ch_Numbers);
                gb_WhatIsXplusY.Append("plus");
                gb_WhatIsXplusY.Append(ch_Numbers);
                Grammar g_WhatIsXplusY = new Grammar(gb_WhatIsXplusY);
                sre.LoadGrammarAsync(g_WhatIsXplusY);
                sre.LoadGrammarAsync(g_StartStop);
                sre.RecognizeAsync(RecognizeMode.Multiple);
                while (done == false) {; }
                Console.WriteLine("\nHit <enter> to close shell\n");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        } // Main
        static void sre_SpeechRecognized(object sender,
          SpeechRecognizedEventArgs e)
        {
            string txt = e.Result.Text;
            float confidence = e.Result.Confidence;
            Console.WriteLine();
            Console.WriteLine($"Recognized: {txt}");
            Console.WriteLine($"Confidence: {confidence}");
            if (confidence < 0.60) return;
            if (txt.IndexOf("speech on") >= 0)
            {
                Console.WriteLine("Speech is now ON");
                speechOn = true;
            }
            if (txt.IndexOf("speech off") >= 0)
            {
                Console.WriteLine("Speech is now OFF");
                speechOn = false;
            }
            if (speechOn == false) return;
            if (txt.IndexOf("klatu") >= 0 && txt.IndexOf("barada") >= 0)
            {
                ((SpeechRecognitionEngine)sender).RecognizeAsyncCancel();
                done = true;
                Console.WriteLine("(Speaking: Farewell)");
                ss.Speak("Farewell");
            }
            if (txt.IndexOf("What") >= 0 && txt.IndexOf("plus") >= 0)
            {
                string[] words = txt.Split(' ');
                int num1 = int.Parse(words[2]);
                int num2 = int.Parse(words[4]);
                int sum = num1 + num2;
                Console.WriteLine("(Speaking: " + words[2] + " plus " +
                  words[4] + " equals " + sum + ")");
                ss.SpeakAsync(words[2] + " plus " + words[4] +
                  " equals " + sum);
            }
        } // sre_SpeechRecognized
    } // Program
} // ns
