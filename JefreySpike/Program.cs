﻿using System;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using System.Globalization;
using System.Threading;

namespace ConsoleSpeech
{
    class ConsoleSpeechProgram
    {
        static SpeechSynthesizer ss = new SpeechSynthesizer();
        static SpeechRecognitionEngine sre;
        static bool done = false;
        static void Main()
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
                Choices ch_WakeCommands = new Choices();
                ch_WakeCommands.Add("See Fop");
                ch_WakeCommands.Add("Jefrey");
                ch_WakeCommands.Add("Brenda");
                GrammarBuilder gb_Wake = new GrammarBuilder();
                gb_Wake.Append(ch_WakeCommands);
                Grammar g_Wake = new Grammar(gb_Wake);
                sre.LoadGrammarAsync(g_Wake);
                sre.RecognizeAsync(RecognizeMode.Multiple);
                while (done == false) { Thread.Sleep(1000); }
                Console.WriteLine("\nHit <enter> to close shell\n");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        static void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string txt = e.Result.Text;
            float confidence = e.Result.Confidence;
            Console.WriteLine();
            Console.WriteLine($"Recognized: {txt}");
            Console.WriteLine($"Confidence: {confidence}");
            if (confidence < 0.60) return;
            if (txt.Contains("Jefrey"))
            {
                Console.WriteLine("Jefrey got woken!");
                ss.Speak("What do you want?");
            }
            if (txt.Contains("Brenda"))
            {
                Console.WriteLine("Brenda got woken!");
                ss.Speak("How can I help?");
            }
            if (txt.Contains("See Fop"))
            {
                Console.WriteLine("CFOP got woken!");
                ss.Speak("Don't worry, you're not that old!");
            }
        }
    }
}
