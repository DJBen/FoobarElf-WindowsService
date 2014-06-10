using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;

namespace FoobarElf
{
    public partial class FoobarElf : ServiceBase
    {
        const string jsonURL = "http://localhost:8888/djben";

        static bool retrieving;

        static System.Timers.Timer timer;

        static FoobarPlayer player;

        public FoobarElf()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("FoobarElfService"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "FoobarElfService", "FoobarElfLog");
            }
            eventLog.Source = "FoobarElfService";
            eventLog.Log = "FoobarElfLog";
        }

        protected override void OnStart(string[] args)
        {
            eventLog.WriteEntry("FoobarElf OnStart");
            Console.WriteLine("Hello World!");
            retrieving = false;
            timer = new System.Timers.Timer(250);
            timer.Elapsed += timer_Elapsed;
            timer.Enabled = true;
            player = new FoobarPlayer();

            // Test
            Song testEmptySong = new Song(new Dictionary<string, string>());
            Console.WriteLine(testEmptySong);
        }

        protected override void OnStop()
        {
            eventLog.WriteEntry("FoobarElf onStop.");
            timer.Enabled = false;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (retrieving)
            {
                //Console.WriteLine("Already retrieving!");
                return;
            }
            HttpResponseMessage message = null;
            if ((message = FoobarElf.RetrievePage().Result) != null)
            {
                HtmlAgilityPack.HtmlDocument document = GetDocument(message).Result;
                player.loadDocument(document);
                player.uploadStatus();
                if (!player.commandRunning) player.consumeCommand();
                // Console.WriteLine(player);
            }
        }

        private async Task<HtmlAgilityPack.HtmlDocument> GetDocument(HttpResponseMessage responseMessage)
        {
            string result = await responseMessage.Content.ReadAsStringAsync();
            if (!responseMessage.IsSuccessStatusCode)
                throw new Exception("Unable to retrieve document");

            var document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(result);
            return document;
        }

        private async static Task<HttpResponseMessage> RetrievePage()
        {
            //Console.WriteLine("Begin retrieving...");
            retrieving = true;
            var client = new System.Net.Http.HttpClient();
            try
            {
                var result = await client.GetAsync(jsonURL);
                //Console.WriteLine("Finish retrieving.");
                retrieving = false;
                timer.Interval = 250;
                return result;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("An error occurred. Reduce timer interval to 3s.");
                retrieving = false;
                timer.Interval = 3000;
                return null;
            }
        }
    }
}
