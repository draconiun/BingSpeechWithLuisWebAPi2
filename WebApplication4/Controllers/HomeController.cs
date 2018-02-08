using Microsoft.CognitiveServices.SpeechRecognition;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication4.Controllers
{
    public class HomeController : Controller
    {
        private  string abc = "";
        //private const string IsolatedStorageSubscriptionKeyFileName = "Subscription.txt";
        //private const string DefaultSubscriptionKeyPromptMessage = "Paste your subscription key here to start";
        private string textAudioGlobal = "";
        private string textAudio = "";
        private string subscriptionKey = "9142ff4250c44c7eb22a9908076a4b1c";
        private DataRecognitionClient dataClient;
        //private MicrophoneRecognitionClient micClient;
        public bool IsMicrophoneClientShortPhrase { get; set; }
        public bool IsMicrophoneClientDictation { get; set; }
        public bool IsMicrophoneClientWithIntent { get; set; }
        public bool IsDataClientShortPhrase { get; set; }
        public bool IsDataClientWithIntent { get; set; }
        public bool IsDataClientDictation { get; set; }
        public string SubscriptionKey
        {
            get
            {
                return this.subscriptionKey;
            }

            set
            {
                this.subscriptionKey = value;
            }
        }

        private string LuisEndpointUrl
        {
            get { return ConfigurationManager.AppSettings["LuisEndpointUrl"]; }
        }

        private bool UseMicrophone
        {
            get
            {
                return this.IsMicrophoneClientWithIntent ||
                    this.IsMicrophoneClientDictation ||
                    this.IsMicrophoneClientShortPhrase;
            }
        }
        private bool WantIntent
        {
            get
            {
                return !string.IsNullOrEmpty(this.LuisEndpointUrl) &&
                    (this.IsMicrophoneClientWithIntent || this.IsDataClientWithIntent);
            }
        }

        private SpeechRecognitionMode Mode
        {
            get
            {
                if (this.IsMicrophoneClientDictation ||
                    this.IsDataClientDictation)
                {
                    return SpeechRecognitionMode.LongDictation;
                }

                return SpeechRecognitionMode.ShortPhrase;
            }
        }

        private string DefaultLocale
        {
            get { return "es-ES"; }
        }

        private string ShortWaveFile
        {
            get
            {
                return ConfigurationManager.AppSettings["ShortWaveFile"];
            }
        }

        private string LongWaveFile
        {
            get
            {
                return ConfigurationManager.AppSettings["LongWaveFile"];
            }
        }

        private string AuthenticationUri
        {
            get
            {
                return ConfigurationManager.AppSettings["AuthenticationUri"];
            }
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            //this.IsMicrophoneClientShortPhrase = false;
            //this.IsMicrophoneClientWithIntent = false;
            //this.IsMicrophoneClientDictation = false;
            //this.IsDataClientShortPhrase = false;
            //this.IsDataClientWithIntent = false;
            //this.IsDataClientDictation = true;

            ////this.CreateDataRecoClientWithIntent();
            //this.CreateDataRecoClient();

            //SendAudioHelper("c:/users/aaron.mejia/source/repos/WebApplication4/WebApplication4/Content/Audios/test-micro.wav");


            ////string testAudio = abc;

            return View();
        }

        private void SendAudioHelper(string wavFileName)
        {
            using (FileStream fileStream = new FileStream(wavFileName, FileMode.Open, FileAccess.Read))
            {
                int bytesRead = 0;
                byte[] buffer = new byte[1024];

                try
                {
                    do
                    {
                        bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                        this.dataClient.SendAudio(buffer, bytesRead);
                    }
                    while (bytesRead > 0);
                }
                finally
                {
                    this.dataClient.EndAudio();
                }
            }
        }

        private void CreateDataRecoClientWithIntent()
        {
            this.dataClient = SpeechRecognitionServiceFactory.CreateDataClientWithIntentUsingEndpointUrl(
                this.DefaultLocale,
                this.SubscriptionKey,
                "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/75a1f980-aa8e-42e5-927c-eef62286b24c?subscription-key=3441bb92f501414e8bcb7013517a20f1&verbose=true&timezoneOffset=0&q=");
            this.dataClient.AuthenticationUri = this.AuthenticationUri;

            // Event handlers for speech recognition results
            this.dataClient.OnResponseReceived += this.OnDataShortPhraseResponseReceivedHandler;
            this.dataClient.OnPartialResponseReceived += this.OnPartialResponseReceivedHandler;
            this.dataClient.OnConversationError += this.OnConversationErrorHandler;

            // Event handler for intent result
            this.dataClient.OnIntent += this.OnIntentHandler;


            //string apiKey = "75a1f980-aa8e-42e5-927c-eef62286b24c";
            //string SubscriptionKeyLUIS = "3441bb92f501414e8bcb7013517a20f1";
            //string PointURL = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/75a1f980-aa8e-42e5-927c-eef62286b24c?subscription-key=3441bb92f501414e8bcb7013517a20f1&verbose=true&timezoneOffset=0&q=";

        }

        private void OnIntentHandler(object sender, SpeechIntentEventArgs e)
        {
            string xxxxx = e.Payload;

            JObject json = JObject.Parse(xxxxx);

            //this.WriteLine("--- Intent received by OnIntentHandler() ---");
            //this.WriteLine("{0}", e.Payload);
            //this.WriteLine();
        }

        private void CreateDataRecoClient()
        {
            this.dataClient = SpeechRecognitionServiceFactory.CreateDataClient(
                this.Mode,
                this.DefaultLocale,
                this.SubscriptionKey);
            this.dataClient.AuthenticationUri = this.AuthenticationUri;

            // Event handlers for speech recognition results
            if (this.Mode == SpeechRecognitionMode.ShortPhrase)
            {
                this.dataClient.OnResponseReceived += this.OnDataShortPhraseResponseReceivedHandler;
            }
            else
            {
                this.dataClient.OnResponseReceived += this.OnDataDictationResponseReceivedHandler;
            }

            this.dataClient.OnPartialResponseReceived += this.OnPartialResponseReceivedHandler;
            this.dataClient.OnConversationError += this.OnConversationErrorHandler;
        }

        private void OnDataDictationResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            if (e.PhraseResponse.Results.Length < 1) return;

            for (int i = 0; i < 1; i++)
            {
                Confidence a = e.PhraseResponse.Results[i].Confidence;

                string textAudio = e.PhraseResponse.Results[i].DisplayText;

                textAudioGlobal += textAudio;

                int tam = textAudio.Length;
            }
            //if (e.PhraseResponse.RecognitionStatus == RecognitionStatus.EndOfDictation ||
            //    e.PhraseResponse.RecognitionStatus == RecognitionStatus.DictationEndSilenceTimeout)
            //{
            //    string xx = "";
            //}

            //for (int i = 0; i < e.PhraseResponse.Results.Length; i++)
            //{
            //    Confidence a = e.PhraseResponse.Results[i].Confidence;
            //    string xxyy = e.PhraseResponse.Results[i].DisplayText;
            //    ViewBag.MensajeFinal += xxyy;
            //    abc += xxyy;
            //}
        }

        private void OnDataShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            for (int i = 0; i < e.PhraseResponse.Results.Length; i++)
            {
                Confidence a = e.PhraseResponse.Results[i].Confidence;

                string xxyy = e.PhraseResponse.Results[i].DisplayText;

                ViewBag.MensajeFinal += (" " + xxyy);

            }
        }

        private void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            string x = e.PartialResult;
        }

        private void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {

            string x = e.SpeechErrorCode.ToString();
            string y = e.SpeechErrorText;

        }


    }
}
