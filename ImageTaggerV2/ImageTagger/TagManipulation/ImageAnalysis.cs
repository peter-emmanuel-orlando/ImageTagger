﻿using Clarifai.API;
using Clarifai.DTOs.Inputs;
using Vision = Google.Cloud.Vision.V1;
using ImageTagger;
using System;
using System.Linq;
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
using ImageTagger.DataModels;
using ImageTagger.UI;
using System.Text.RegularExpressions;

namespace ImageAnalysisAPI
{


    public enum ImageAnalysisType
    {
        all = 0,
        general,
        demographics,
        moderation,
        //apparel,
        //color,
        //quality
    }

    public static class ImageAnalysis
    {
        private static HttpClient httpClient;
        private static ClarifaiClient clarifaiClient;

        static ImageAnalysis()
        {
            RefreshAPIKey();
        }

        public static void RefreshAPIKey()
        { 
            var apiKey = SettingsPersistanceUtil.RetreiveSetting("apiKey");

            if (apiKey == "")
            {
                 apiKey = RequestClarafaiAPIDialog.GetAPIKeyViaDialog();
            }
                
            else
            {
                httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(@"https://api.clarifai.com/v2/models/");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", apiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                clarifaiClient = new ClarifaiClient(apiKey);
            }
        }

        public static async Task<List<TagSuggestion>> RequestAnalysis(string imageFilePath, params ImageAnalysisType[] categories)
        {
            var bytes = File.ReadAllBytes(imageFilePath);
            var input = new ClarifaiFileImage(bytes);
            var res = await clarifaiClient.WorkflowPredict("workflow", input).ExecuteAsync();
            var predictions = res.Get().WorkflowResult.Predictions;
            var result = new HashSet<TagSuggestion>();
            foreach (var prediction in predictions)
            {
                if (prediction.Model.ModelID == clarifaiClient.PublicModels.GeneralModel.ModelID)
                {
                    var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.general);
                    var cachePath = imageFilePath + "/" + category;
                    var parsed = ParseGeneralData(prediction.Data.Cast<Concept>());
                    CacheAnalyticsResults(cachePath, parsed);
                    result.AddRange(parsed);
                }
                else if(prediction.Model.ModelID == clarifaiClient.PublicModels.DemographicsModel.ModelID)
                {
                    var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.demographics);
                    var cachePath = imageFilePath + "/" + category;
                    ParseGeneralData(prediction.Data);
                    var parsed = ParseDemographicsData(prediction.Data.Cast<Demographics>());
                    CacheAnalyticsResults(cachePath, parsed);
                    result.AddRange(parsed);
                }
                else if (prediction.Model.ModelID == clarifaiClient.PublicModels.ModerationModel.ModelID)
                {
                    var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.moderation);
                    var cachePath = imageFilePath + "/" + category;
                    var parsed = ParseModerationData(prediction.Data.Cast<Concept>());
                    CacheAnalyticsResults(cachePath, parsed);
                    result.AddRange(parsed);
                }
                else
                {
                    var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.general);
                    var cachePath = imageFilePath + "/" + category;
                    var parsed = ParseGeneralData(prediction.Data);
                    CacheAnalyticsResults(cachePath, parsed);
                    result.AddRange(parsed);
                }
            }
            return new List<TagSuggestion>(result);
        }

        public static async Task<List<TagSuggestion>> RequestAnalysiss(string imageFilePath, params ImageAnalysisType[] categories)
        {
            if (categories.Length == 0 || categories.Contains(ImageAnalysisType.all))
                categories = (ImageAnalysisType[])Enum.GetValues(typeof(ImageAnalysisType));

            var result = new List<TagSuggestion>();
            if (categories.Contains(ImageAnalysisType.general))
                result.AddRange(await RequestGeneralAnalysisAsync(imageFilePath));
            if (categories.Contains(ImageAnalysisType.demographics))
                result.AddRange(await RequestDemographicsAnalysisAsync(imageFilePath));
            if (categories.Contains(ImageAnalysisType.moderation))
                result.AddRange(await RequestModerationAnalysisAsync(imageFilePath));

            return result;
        }

        private static async Task<List<TagSuggestion>> RequestGeneralAnalysisAsync(string imageFilePath)
        {
            List<TagSuggestion> result;
            var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.general);
            var cachePath = imageFilePath + "/" + category;
            var cacheHit = RequestCachedResults(cachePath, out result);

            if (!cacheHit && httpClient != null)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "aaa03c23b3724a16a56b629203edc62c/outputs");
                var bytes = File.ReadAllBytes(imageFilePath);
                var res = await clarifaiClient.PublicModels.GeneralModel.Predict(new ClarifaiFileImage(bytes)).ExecuteAsync();
                if(res.IsSuccessful)
                {
                    result = ParseGeneralData(res.Get().Data);
                }
            }
            CacheAnalyticsResults(cachePath, result);
            return result;
        }

        private static List<TagSuggestion> ParseGeneralData(IEnumerable<IPrediction> data)
        {
            var parsedJson = JToken.Parse(JsonConvert.SerializeObject(data));
            var s = parsedJson.ToString(Formatting.Indented);
            s = Regex.Replace(s, @"\]([\s\S]*?)\[", ",");
            s = "[" + s.Split('[').Last();//Regex.Replace(s, @"[\s\S]*\[", "[");
            s = s.Split(']').First() + "]"; //Regex.Replace(s, @"\]([\s\S]*)", "]");
            Debug.WriteLine(s);
            var concepts = JsonConvert.DeserializeObject<List<Concept>>(s);//s.Split(new string[] { "~break~" }, StringSplitOptions.RemoveEmptyEntries);
            var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.general);
            var result = new List<TagSuggestion>();
            foreach (var concept in concepts)
            {
                result.Add(new TagSuggestion(new ImageTag(concept.Name), (concept.Value.HasValue) ? (double)concept.Value.Value : 0d, category));
            }
            return result;
        }


        private static async Task<List<TagSuggestion>> RequestDemographicsAnalysisAsync(string imageFilePath)
        {
            List<TagSuggestion> result;
            var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.demographics);
            var cachePath = imageFilePath + "/" + category;
            var cacheHit = RequestCachedResults(cachePath, out result);

            if (!cacheHit && clarifaiClient != null)
            {
                var bytes = File.ReadAllBytes(imageFilePath);
                var modelId = clarifaiClient.PublicModels.DemographicsModel.ModelID;
                var res = await clarifaiClient.Predict<Demographics>(modelId, new ClarifaiFileImage(bytes)).ExecuteAsync();
                if(res.IsSuccessful)
                    result = ParseDemographicsData(res.Get().Data);

            }
            CacheAnalyticsResults(cachePath, result);
            return result;
        }

        private static List<TagSuggestion> ParseDemographicsData(IEnumerable<Demographics> data)
        {

            List<TagSuggestion> result = new List<TagSuggestion>();
            var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.demographics);
            Func<string, string> fixName = (string name) =>
            {
                if (name == "middle eastern or north african") return "arab";
                else if (name == "american indian or alaska native") return "nativeAmerican";
                else if (name == "black or african american") return "africanOrigin";
                else if (name == "native hawaiian or pacific islander") return "pacificIslander";
                else if (name == "hispanic, latino, or spanish origin") return "hispanic";
                else if (name == "white") return "caucasian";
                else return name;
            };
            foreach (var person in data)
            {
                //not using age right now
                /*foreach (var innerConcept in person.AgeAppearanceConcepts)
                {
                    Debug.WriteLine($"{innerConcept.Name}: {innerConcept.Value}");
                }*/
                var gender = "unsure";
                var certainty = 0m;
                foreach (var innerConcept in person.GenderAppearanceConcepts)
                {
                    if (innerConcept.Name == "feminine" && innerConcept.Value.Value > 0.6m) { gender = "female"; certainty = innerConcept.Value.Value; }
                    else if (innerConcept.Name == "masculine" && innerConcept.Value.Value > 0.6m) { gender = "male"; certainty = innerConcept.Value.Value; }
                }
                result.Add(new TagSuggestion(new ImageTag(gender), (double)certainty, category));

                foreach (var innerConcept in person.MulticulturalAppearanceConcepts)
                {
                    var threshold = 0.099m;
                    if (innerConcept.Value.Value > threshold)
                        result.Add(new TagSuggestion(new ImageTag(fixName(innerConcept.Name)), (double)innerConcept.Value.Value, category));
                }
            }
            return result;
        }

        private static async Task<List<TagSuggestion>> RequestModerationAnalysisAsync(string imageFilePath)
        {
            List<TagSuggestion> result;
            var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.moderation);
            var cachePath = imageFilePath + "/" + category;
            var cacheHit = RequestCachedResults(cachePath, out result);

            if (!cacheHit && clarifaiClient != null)
            {
                // When using async/await
                var bytes = File.ReadAllBytes(imageFilePath);
                //var model = new ImageModels.DemographicsModel(client, ImageModels.)
                var res = await clarifaiClient.PublicModels.ModerationModel
                    .Predict(new ClarifaiFileImage(bytes))
                    .ExecuteAsync();
                // Print the concepts
                ParseModerationData(res.Get().Data);
            }
            CacheAnalyticsResults(cachePath, result);
            return result;
        }

        private static List<TagSuggestion> ParseModerationData(IEnumerable<Concept> data)
        {

            var isSafe = false;
            var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.moderation);
            var result = new List<TagSuggestion>();
            foreach (var concept in data)
            {
                var name = concept.Name;
                var certainty = (double)concept.Value.Value;
                var threshold = 0.3;
                if (certainty > threshold)
                {
                    result.Add(new TagSuggestion(new ImageTag(concept.Name), certainty, category));
                    if (name == "safe") isSafe = true;
                }
            }
            if (!isSafe)
                result.Add(new TagSuggestion(new ImageTag("nsfw"), 1, category));
            return result;
        }

        /*
        private static async Task<List<TagSuggestion>> RequestCustomAnalysisAsync(string imageFilePath)
        {
            List<TagSuggestion> result;
            var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.moderation);
            var cachePath = imageFilePath + "/" + category;
            var cacheHit = RequestCachedResults(cachePath, out result);

            if (!cacheHit && clarifaiClient != null)
            { }
        }
        */














        private static int maxCached = 26;
        private static Dictionary<string, Tuple<long, List<TagSuggestion>>> CachedResponses = new Dictionary<string, Tuple<long, List<TagSuggestion>>>();
        private static bool RequestCachedResults(string imageFilePath, out List<TagSuggestion> results)
        {
            var success = false;
            if (CachedResponses.ContainsKey(imageFilePath))
            {
                Debug.WriteLine("cacheHit! avoided uneccessary API call");
                results = CachedResponses[imageFilePath].Item2;
                CachedResponses[imageFilePath] = new Tuple<long, List<TagSuggestion>>(DateTime.Now.Ticks, results);
                success = true;
            }
            else
            {
                results = new List<TagSuggestion>(); Debug.WriteLine("cacheMiss! API call made");
            }
            return success;
        }

        private static void CacheAnalyticsResults(string imageFilePath, List<TagSuggestion> toCache)
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
            CachedResponses.Add(imageFilePath, new Tuple<long, List<TagSuggestion>>(DateTime.Now.Ticks, toCache));
        }


    }

}
/* general model via http
private class Suggestion : IPrediction
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

private static async Task<List<TagSuggestion>> RequestGeneralAnalysisAsync(string imageFilePath)
{
    List<TagSuggestion> result;
    var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.general);
    var cachePath = imageFilePath + "/" + category;
    var cacheHit = RequestCachedResults(cachePath, out result);

    if (!cacheHit && httpClient != null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "aaa03c23b3724a16a56b629203edc62c/outputs");
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
            result.Add(new TagSuggestion(new ImageTag(concept.Name), (concept.Value.HasValue) ? (double)concept.Value.Value : 0d, category));
        }
    }
    CacheAnalyticsResults(cachePath, result);
    return result;
}
*/

