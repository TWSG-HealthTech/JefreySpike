using System;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using System.Globalization;
using System.Threading;
using System.Configuration;
using Microsoft.ProjectOxford.SpeechRecognition;
using Newtonsoft.Json;

namespace ConsoleSpeech
{
    class ConsoleSpeechProgram
    {
        private static SpeechSynthesizer _ss = new SpeechSynthesizer();
        private static SpeechRecognitionEngine _sre;
        private static bool _done = false;
        private static MicrophoneRecognitionClient _microphoneClient;

        static void Main()
        {
            try
            {
                SetupActiveListener();
                _ss.SetOutputToDefaultAudioDevice();
                Console.WriteLine("(Speaking: I am awake)");
                _ss.Speak("I am awake");
                CultureInfo ci = new CultureInfo(ConfigurationManager.AppSettings["locale"]);
                _sre = new SpeechRecognitionEngine(ci);
                _sre.SetInputToDefaultAudioDevice();
                _sre.SpeechRecognized += sre_SpeechRecognized;
                Choices ch_WakeCommands = new Choices();
                ch_WakeCommands.Add("See Fop");
                ch_WakeCommands.Add("Jefrey");
                ch_WakeCommands.Add("Brenda");
                GrammarBuilder gb_Wake = new GrammarBuilder();
                gb_Wake.Append(ch_WakeCommands);
                Grammar g_Wake = new Grammar(gb_Wake);
                _sre.LoadGrammarAsync(g_Wake);
                _sre.RecognizeAsync(RecognizeMode.Multiple);
                while (_done == false) { Thread.Sleep(1000); }
                Console.WriteLine("\nHit <enter> to close shell\n");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        private static void SetupActiveListener()
        {
            _microphoneClient = SpeechRecognitionServiceFactory.CreateMicrophoneClientWithIntent(
                ConfigurationManager.AppSettings["locale"],
                ConfigurationManager.AppSettings["primaryKey"],
                ConfigurationManager.AppSettings["secondaryKey"],
                ConfigurationManager.AppSettings["luisAppId"],
                ConfigurationManager.AppSettings["luisSubscriptionId"]
            );

            // Event handlers for speech recognition results
            _microphoneClient.OnMicrophoneStatus += OnMicrophoneStatus;
            _microphoneClient.OnPartialResponseReceived += OnPartialResponseReceivedHandler;
            _microphoneClient.OnResponseReceived += OnMicShortPhraseResponseReceivedHandler;
            _microphoneClient.OnConversationError += OnConversationErrorHandler;

            // Event handler for intent result
            _microphoneClient.OnIntent += OnIntentHandler;
        }

        static void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string txt = e.Result.Text;
            float confidence = e.Result.Confidence;
            Console.WriteLine();
            Console.WriteLine($"Recognized: {txt}");
            Console.WriteLine($"Confidence: {confidence}");
            if (confidence < 0.75) return;
            if (txt.Contains("Jefrey"))
            {
                Console.WriteLine("Jefrey got woken!");
                _ss.Speak("Go away, I'm busy!");
            }
            if (txt.Contains("Brenda"))
            {
                Console.WriteLine("Brenda got woken!");
                DoActive();
            }
            if (txt.Contains("See Fop"))
            {
                Console.WriteLine("CFOP got woken!");
                _ss.Speak("Don't worry, you're not that old!");
            }
        }

        static void DoActive()
        {
            _sre.RecognizeAsyncCancel();
            _sre.RecognizeAsyncStop();

            _microphoneClient.StartMicAndRecognition();
        }

        private static void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Console.WriteLine("--- Microphone status change received by OnMicrophoneStatus() ---");
            Console.WriteLine($"********* Microphone status: {e.Recording} *********");
            if (e.Recording)
            {
                Console.WriteLine("Listening...");
            }

            Console.WriteLine();
        }

        private static void OnIntentHandler(object sender, SpeechIntentEventArgs e)
        {
            Console.WriteLine("--- Intent received by OnIntentHandler() ---");
            Console.WriteLine(e.Payload);
            Console.WriteLine();
            HandlePayload(e.Payload);
        }

        private static void HandlePayload(String payload)
        {
            dynamic x = JsonConvert.DeserializeObject(payload);
            dynamic intent = x["intents"][0];
            if (intent["intent"] == "TellJoke")
            {
                _ss.Speak("What wobbles in the sky? A jellycopter!");
            }
            else if (intent["intent"] == "BuyStuff" && intent["actions"][0]["triggered"].Value)
            {
                _ss.Speak($"OK, I'll add {intent["actions"][0]["parameters"][0]["value"][0]["entity"]} to your shopping!");
            }
            else
            {
                _ss.Speak("Sorry, I don't know how to do that.");
            }
        }

        private static void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            Console.WriteLine("--- Error received by OnConversationErrorHandler() ---");
            Console.WriteLine($"Error code: {e.SpeechErrorCode}");
            Console.WriteLine($"Error text: {e.SpeechErrorText}");
            Console.WriteLine();
        }

        private static void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            Console.WriteLine("--- Partial result received by OnPartialResponseReceivedHandler() ---");
            Console.WriteLine(e.PartialResult);
            Console.WriteLine();
        }

        private static void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            Console.WriteLine("--- OnMicShortPhraseResponseReceivedHandler ---");
            Console.WriteLine(e.PhraseResponse.RecognitionStatus);
            if (e.PhraseResponse.Results.Length == 0)
            {
                Console.WriteLine("No phrase response is available.");
            }
            else
            {
                Console.WriteLine("********* Final n-BEST Results *********");
                
                for (int i = 0; i < e.PhraseResponse.Results.Length; i++)
                {
                    Console.WriteLine(
                        $"[{i}] Confidence={e.PhraseResponse.Results[i].Confidence}, Text=\"{e.PhraseResponse.Results[i].DisplayText}\"");
                }

                Console.WriteLine();
            }

            _microphoneClient.EndMicAndRecognition();

            _sre.SetInputToDefaultAudioDevice();
            _sre.RecognizeAsync(RecognizeMode.Multiple);
        }
    }
}
