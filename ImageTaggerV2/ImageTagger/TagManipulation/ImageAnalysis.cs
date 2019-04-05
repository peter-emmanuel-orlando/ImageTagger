using Clarifai.API;
using Clarifai.DTOs.Inputs;
using Vision = Google.Cloud.Vision.V1;
using ImageTagger;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Clarifai.API.Requests.Models;
using ImageModels = Clarifai.DTOs.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using Newtonsoft.Json;
using System.Text;
using System.Web.Script.Serialization;
using Clarifai.API.Responses;
using Clarifai.DTOs.Models.Outputs;
using Clarifai.DTOs.Predictions;
using Newtonsoft.Json.Linq;
using Clarifai.DTOs;
using Clarifai.DTOs.Models;

namespace ImageAnalysisAPI
{
    public static class ImageAnalysis
    {
        private static readonly HttpClient httpClient;

        static ImageAnalysis()
        {
            var apiKey = SettingsPersistanceUtil.RetreiveSetting("apiKey");

            if (apiKey == "")
                MessageBox.Show("Without a Clarifai API key, suggestions are limited.");
            else
            {
                httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(@"https://api.clarifai.com/v2/models/");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", apiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }


        public enum AnalysisCategories
        {
            None = 0,
            Demographics,
            Apparel,
            Appropriate,
            Color,
        }


        public class Suggestion : IPrediction
        {
            public Suggestion(string iD, string name, DateTime? createdAt, string appID, decimal? value, string language)
            {
                ID = iD;
                Name = name;
                CreatedAt = createdAt;
                AppID = appID;
                Value = value;
                Language = language;
            }

            public string TYPE => "concept";
            public string ID { get; }
            public string Name { get; }
            public DateTime? CreatedAt { get; }
            public string AppID { get; }
            public decimal? Value { get; }
            public string Language { get; }
        }









        public static async Task<List<Tuple<string, double>>> RequestAnalysis(string imageFilePath)
        {
            List<Tuple<string, double>> result;
            var cacheHit = RequestCachedResults(imageFilePath, out result);

            if (!cacheHit && httpClient != null)
            {
                await Task.Run(async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "aaa03c23b3724a16a56b629203edc62c/outputs");
                    //var v = new Clarifai.DTOs.Predictions.Demographics("d", "d");
                    var bytes = File.ReadAllBytes(imageFilePath);
                    var reqJSON = new
                    {
                        inputs = new[] {
                            new {data = new{ image = new{ base64 = bytes } } }
                        }
                    };
                    var stringContent = new StringContent(JsonConvert.SerializeObject(reqJSON), Encoding.UTF8, "application/json");
                    request.Content = stringContent;
                    var res = await httpClient.SendAsync(request);
                    Debug.WriteLine("server response: " + res.StatusCode);
                    var resString = await res.Content.ReadAsStringAsync();
                    var parsedJson = JToken.Parse(resString);
                    resString = parsedJson.ToString(Formatting.Indented);
                    //Debug.WriteLine(resString);
                    var obj = JsonConvert.DeserializeAnonymousType(resString, new { status = new { }, outputs = new[] { new { data = new { concepts = new Suggestion[] { } } } } }, new JsonSerializerSettings() { ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor });
                    var concepts = obj.outputs[0].data.concepts;

                    foreach (var concept in concepts)
                    {
                        result.Add(new Tuple<string, double>(concept.Name, (concept.Value.HasValue) ? (double)concept.Value.Value : 0d));
                    }

                });
            }
            CacheAnalyticsResults(imageFilePath, result);
            return result;
        }

        private static int maxCached = 6;
        private static Dictionary<string, Tuple<long, List<Tuple<string, double>>>> CachedResponses = new Dictionary<string, Tuple<long, List<Tuple<string, double>>>>();
        private static bool RequestCachedResults(string imageFilePath, out List<Tuple<string, double>> results)
        {
            var success = false;
            if (CachedResponses.ContainsKey(imageFilePath))
            {
                Debug.WriteLine("cacheHit! avoided uneccessary API call");
                results = CachedResponses[imageFilePath].Item2;
                CachedResponses[imageFilePath] = new Tuple<long, List<Tuple<string, double>>>(DateTime.Now.Ticks, results);
                success = true;
            }
            else { results = new List<Tuple<string, double>>(); Debug.WriteLine("cacheMiss! API call made");
            }
            return success;
        }

        private static void CacheAnalyticsResults(string imageFilePath, List<Tuple<string, double>> toCache )
        {
            CachedResponses.Remove(imageFilePath);
            if (CachedResponses.Count >= maxCached)
            {
                string oldest = "";
                long time = 0;
                foreach (var cachedKey in CachedResponses.Keys)
                {
                    var current = CachedResponses[cachedKey];
                    if (current.Item1 > time)
                    {
                        oldest = cachedKey;
                        time = current.Item1;
                    }
                }
                Debug.WriteLine("dropped " + imageFilePath + " from the cache!");
                CachedResponses.Remove(oldest);
            }
            CachedResponses.Add(imageFilePath, new Tuple<long, List<Tuple<string, double>>> (DateTime.Now.Ticks, toCache));
        }

        public static async Task<Dictionary<string, List<string>>> GetSuggestionsAsync(string imageFilePath)
        {
            var apiKey = SettingsPersistanceUtil.RetreiveSetting("apiKey");
            var result = new Dictionary<string, List<string>>();
            if (apiKey != "")
            {
                var client = new ClarifaiClient(apiKey);
                await Task.Run(async () =>
                {
                    // When using async/await
                    var bytes = File.ReadAllBytes(imageFilePath);
                    //var model = new ImageModels.DemographicsModel(client, ImageModels.)
                    var res = await client.PublicModels.DemographicsModel
                        .Predict(new ClarifaiFileImage(bytes))
                        .ExecuteAsync();



                    // Print the concepts
                    foreach (var concept in res.Get().Data)
                    {

                        foreach (var innerConcept in concept.AgeAppearanceConcepts)
                            Debug.WriteLine($"{innerConcept.Name}: {innerConcept.Value}");
                        foreach (var innerConcept in concept.GenderAppearanceConcepts)
                            Debug.WriteLine($"{innerConcept.Name}: {innerConcept.Value}");
                        foreach (var innerConcept in concept.MulticulturalAppearanceConcepts)
                            Debug.WriteLine($"{innerConcept.Name}: {innerConcept.Value}");

                        Debug.WriteLine($"TYPE: {concept.TYPE}");
                    }
                });
            }
            return result;
        }


    }

}
