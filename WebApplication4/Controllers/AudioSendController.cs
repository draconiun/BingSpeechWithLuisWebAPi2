using Microsoft.CognitiveServices.SpeechRecognition;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class AudioSendController : ApiController
    {
        private List<int> NumberLenght=new List<int>();
        private string abc = "";
        private string textAudioGlobal = "";
        private string textIntentionGlobal = "";
        private string subscriptionKey = "9142ff4250c44c7eb22a9908076a4b1c";
        private DataRecognitionClient dataClient;
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

        [HttpPost]
        [Route("/api/ImportAudio")]
        public async Task<IHttpActionResult> ImportAudio(string nameAudio)
        {
            string root = System.Web.HttpContext.Current.Server.MapPath("~/Content/Audios/");
            var provider = new MultipartFormDataStreamProvider(root);
            var streamImage = await Request.Content.ReadAsMultipartAsync(provider);

            foreach (MultipartFileData fileData in streamImage.FileData)
            {
                File.Move(fileData.LocalFileName,
                  Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/Content/Audios/"), nameAudio));
            }

            string URLAudio = System.Web.HttpContext.Current.Server.MapPath("~/Content/Audios/")+ nameAudio;

            this.IsMicrophoneClientShortPhrase = false;
            this.IsMicrophoneClientWithIntent = false;
            this.IsMicrophoneClientDictation = false;
            this.IsDataClientShortPhrase = false;
            this.IsDataClientWithIntent = false;
            this.IsDataClientDictation = false;

            //this.CreateDataRecoClient();
            //this.CreateDataRecoClientWithIntent();

            //Read Audio
            UnionMethodsAsync(URLAudio);
            //SendAudioHelper(URLAudio);
            await Task.Delay(15000);
            string replitTextAudioGlobal = textAudioGlobal;
            string replitTextIntentionGlobal = textIntentionGlobal;
            //NumberLenght = NumberLenght;

            var model = JsonConvert.DeserializeObject<BingLuisModel>(replitTextIntentionGlobal);

            //File.Delete(URLAudio);

            return Ok(model);
            //return StatusCode(HttpStatusCode.NoContent);
        }

        private void UnionMethodsAsync(string vawAudio)
        {
            string PointURL = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/75a1f980-aa8e-42e5-927c-eef62286b24c?subscription-key=3441bb92f501414e8bcb7013517a20f1&verbose=true&timezoneOffset=0&q=";

            this.dataClient = SpeechRecognitionServiceFactory.CreateDataClientWithIntentUsingEndpointUrl(
                this.DefaultLocale,
                this.SubscriptionKey,
                PointURL);
            this.dataClient.AuthenticationUri = this.AuthenticationUri;

            this.dataClient.OnResponseReceived += this.OnDataShortPhraseResponseReceivedHandler;
            this.dataClient.OnPartialResponseReceived += this.OnPartialResponseReceivedHandler;
            this.dataClient.OnConversationError += this.OnConversationErrorHandler;
            this.dataClient.OnIntent += this.OnIntentHandler;

            using (FileStream fileStream = new FileStream(vawAudio, FileMode.Open, FileAccess.Read))
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

        private void SendAudioHelper(string vawAudio)
        {
            using (FileStream fileStream = new FileStream(vawAudio, FileMode.Open, FileAccess.Read))
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
            string PointURL = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/75a1f980-aa8e-42e5-927c-eef62286b24c?subscription-key=3441bb92f501414e8bcb7013517a20f1&verbose=true&timezoneOffset=0&q=";

            this.dataClient = SpeechRecognitionServiceFactory.CreateDataClientWithIntentUsingEndpointUrl(
                this.DefaultLocale,
                this.SubscriptionKey,
                PointURL);
            this.dataClient.AuthenticationUri = this.AuthenticationUri;

            this.dataClient.OnResponseReceived += this.OnDataShortPhraseResponseReceivedHandler;
            this.dataClient.OnPartialResponseReceived += this.OnPartialResponseReceivedHandler;
            this.dataClient.OnConversationError += this.OnConversationErrorHandler;
            this.dataClient.OnIntent += this.OnIntentHandler;

        }

        private void OnIntentHandler(object sender, SpeechIntentEventArgs e)
        {
            string intentionsLUIS = e.Payload;

            textIntentionGlobal += intentionsLUIS;

            JObject json = JObject.Parse(intentionsLUIS);
        }

        private void CreateDataRecoClient()
        {
            this.dataClient = SpeechRecognitionServiceFactory.CreateDataClient(
                this.Mode,
                this.DefaultLocale,
                this.SubscriptionKey);
            this.dataClient.AuthenticationUri = this.AuthenticationUri;

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
                int tamText = textAudio.Length;
                NumberLenght.Add(tamText);
            }
        }

        private void OnDataShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            if (e.PhraseResponse.Results.Length < 1) return;

            for (int i = 0; i < 1; i++)
            {
                Confidence a = e.PhraseResponse.Results[i].Confidence;

                string textAudio = e.PhraseResponse.Results[i].DisplayText;

                textAudioGlobal += textAudio;
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
