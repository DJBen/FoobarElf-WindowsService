using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.IO;

namespace FoobarElf
{
    class FoobarPlayer
    {
        private string fireBase;

        private string fireBaseActivity;

        public bool IsPlaying;

        public bool IsPaused;

        public Song currentSong;

        // Note: When IsStopped() is true, the two following var below = 0
        public int currentPosition;

        public int totalLength;

        // Volume: 0~100%
        public int volume;

        // Volume: 0.0dB ~ -100.0dB
        public double volumnDB;

        private bool currentSongShouldUpdate;

        private bool cleaningCommand;

        public bool commandRunning;

        private const string commandURL = "http://localhost:8888/default";

        //private DateTime lastCommandTime;

        public FoobarPlayer()
        {
            IsPlaying = false;
            IsPaused = false;
            currentSong = null;
            currentPosition = 0;
            totalLength = 0;
            currentSongShouldUpdate = false;
            cleaningCommand = false;
            commandRunning = false;
            fireBase = "https://djben.firebaseIO.com/FoobarElf/clients/" + System.Environment.MachineName;
            fireBaseActivity = "https://djben.firebaseIO.com/FoobarElf/activity";
            Console.WriteLine(fireBase);
        }

        public void loadDocument(HtmlAgilityPack.HtmlDocument document)
        {
            HtmlAgilityPack.HtmlNode playStateNode = document.DocumentNode.SelectSingleNode("//tr[@id='state']");
            if (playStateNode == null) return;
            Dictionary<string, string> state = JsonConvert.DeserializeObject<Dictionary<string, string>>(playStateNode.InnerText);
            IsPlaying = Utility.ToBool(state["playing"]);
            IsPaused = Utility.ToBool(state["paused"]);
            Console.WriteLine("Playing = {0}, Paused = {1}", IsPlaying, IsPaused);

            // Load currently playing song
            if (!IsStopped())
            {
                HtmlAgilityPack.HtmlNode currentPlayingNode = document.DocumentNode.SelectSingleNode("//tr[@id='playing']");
                if (currentPlayingNode == null) return;
                Dictionary<string, string> playing = JsonConvert.DeserializeObject<Dictionary<string, string>>(currentPlayingNode.InnerText);
                if (playing != null)
                {
                    Song newSong = new Song(playing);
                    if (currentSong == null || !newSong.Equals(currentSong))
                    {
                        currentSong = newSong;
                        currentSongShouldUpdate = true;
                        Console.WriteLine("Current song should update");
                    }
                }
            }
            else
            {
                currentSong = null;
                Console.WriteLine("No song is currently playing");
            }

            HtmlAgilityPack.HtmlNode progressNode = document.DocumentNode.SelectSingleNode("//tr[@id='progress']");
            if (progressNode == null) return;
            Dictionary<string, string> progress = JsonConvert.DeserializeObject<Dictionary<string, string>>(progressNode.InnerText);
            currentPosition = Utility.ToInt(progress["position"]);
            totalLength = Utility.ToInt(progress["length"]);

            HtmlAgilityPack.HtmlNode volumeNode = document.DocumentNode.SelectSingleNode("//tr[@id='volume']");
            if (volumeNode == null) return;
            Dictionary<string, string> volumeDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(volumeNode.InnerText);
            volume = Utility.ToInt(volumeDict["volume"]);
            volumnDB = -(double)Utility.ToInt(volumeDict["volume_db"]) / 10;
        }

        public void uploadStatus()
        {
            //Console.WriteLine("Updating Status...");

            RestClient client = new RestClient(fireBase);
            RestClient clientActivity = new RestClient(fireBaseActivity);

            RestRequest timeStampRequest = new RestRequest(String.Format("{0}.json", System.Environment.MachineName), Method.PUT);
            timeStampRequest.RequestFormat = DataFormat.Json;
            timeStampRequest.AddBody(new { now = DateTime.Now.ToString("MM-dd-yyyy H:mm:ss zzz") });
            clientActivity.Execute(timeStampRequest);

            //var httpWebRequest = (HttpWebRequest)WebRequest.Create(fireBase + "/status.json");
            //httpWebRequest.Accept = "application/json";
            //httpWebRequest.ContentType = "application/json";
            //httpWebRequest.Method = "PUT";
            //try
            //{
            //    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            //    {
            //        string json = String.Format("{{\"isPlaying\":{0}, \"isPaused\":{1}}}", IsPlaying ? 1 : 0, IsPaused ? 1 : 0);
            //        Console.WriteLine(json);

            //        streamWriter.Write(json);
            //        streamWriter.Flush();
            //        streamWriter.Close();

            //        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            //        {
            //            var result = streamReader.ReadToEnd();
            //            Console.WriteLine(result);
            //        }
            //    }
            //}
            //catch (WebException webException)
            //{
            //    Console.WriteLine(webException.Message);
            //}
            RestRequest statusRequest = new RestRequest("status.json", Method.PUT);
            statusRequest.RequestFormat = DataFormat.Json;
            statusRequest.AddBody(new { isPlaying = IsPlaying ? 1 : 0, isPaused = IsPaused ? 1 : 0, volume = volume, volumeDB = volumnDB });
            client.Execute(statusRequest);

            //client.ExecuteAsync(statusRequest, response =>
            //{
            //    if (response.ErrorException != null)
            //    {
            //        Console.WriteLine(response.ErrorMessage);
            //    }
            //    if (response.ResponseStatus != ResponseStatus.Completed)
            //    {
            //        Console.WriteLine(response.ErrorMessage);
            //    }
            //    else
            //    {
            //        Console.WriteLine(response.Content);
            //    }
            //});

            RestRequest progressRequest = new RestRequest("playing/progress.json", Method.PUT);
            progressRequest.RequestFormat = DataFormat.Json;
            progressRequest.AddBody(new { position = currentPosition, length = totalLength });
            client.Execute(progressRequest);

            if (currentSongShouldUpdate)
            {
                RestRequest songRequest = new RestRequest("playing/song.json", Method.PUT);
                songRequest.RequestFormat = DataFormat.Json;
                songRequest.AddBody(new { title = currentSong.title, album = currentSong.album, albumArtist = currentSong.albumArtist, trackArtist = currentSong.trackArtist, codec = currentSong.codec, CD = currentSong.discNumber, track = currentSong.trackNumber, sampleRate = currentSong.sampleRate, bitRate = currentSong.bitRate, channels = currentSong.channels });
                client.Execute(songRequest);

                currentSongShouldUpdate = false;
            }

            //Console.WriteLine("Status update finished.");
        }

        public void consumeCommand()
        {
            commandRunning = true;
            if (cleaningCommand)
            {
                Console.WriteLine("Wait for cleaning to complete.");
                return;
            }
            RestClient client = new RestClient(fireBase);
            RestRequest commandRequest = new RestRequest("command.json", Method.GET);
            var response = client.Execute(commandRequest);
            var content = response.Content;
            // Console.WriteLine("command: " + content);

            if (content != null && response.ResponseStatus == ResponseStatus.Completed && !cleaningCommand)
            {
                Dictionary<string, string> command = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                if (command != null && command.ContainsKey("cmd"))
                {
                    cleaningCommand = true;
                    string cmd = command["cmd"];
                    string param1 = null;
                    if (command.ContainsKey("param1")) param1 = command["param1"];

                    // Do some clean up
                    RestRequest clearCommandRequest = new RestRequest("command.json", Method.DELETE);
                    var clearResponse = client.Execute(clearCommandRequest);
                    if (clearResponse.ResponseStatus == ResponseStatus.Completed)
                    {
                        Console.WriteLine("Clean up the command successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Fail to clean up command.");
                    }

                    executeCommand(cmd, param1);
                    cleaningCommand = false;
                }
            }
            commandRunning = false;
        }

        private bool executeCommand(string cmd, string param1)
        {
            //Console.WriteLine(DateTime.Now.Subtract(lastCommandTime).Milliseconds);
            //if (lastCommandTime == null || DateTime.Now.Subtract(lastCommandTime).Milliseconds < 300)
            //{
            //    Console.WriteLine("!!!!!!!!");
            //    return false;
            //}
            //lastCommandTime = DateTime.Now;
            RestClient client = new RestClient(commandURL);
            RestRequest commander = new RestRequest("", Method.GET);
            commander.AddParameter("cmd", cmd);
            if (param1 != null) commander.AddParameter("param1", param1);
            var response = client.Execute(commander);
            Console.WriteLine("execute {2}: {0} - {1}", response.StatusCode, response.ErrorMessage, cmd);
            return response.ResponseStatus == ResponseStatus.Completed;
        }

        public bool IsStopped()
        {
            return !(IsPlaying || IsPaused);
        }

        public override string ToString()
        {
            return  String.Format("{0} {{\n\tSong: {1},\n\tIsPlaying: {2},\n\tIsPaused: {3},\n\tprogress: {4}s / {5}s\n}}", base.ToString(), currentSong, IsPlaying, IsPaused, currentPosition, totalLength);
        }
    }
}
