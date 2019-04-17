using Clarifai.API;
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
using System.Collections.Async;
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
using System.Threading;

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
        quality
    }

    public static class ImageAnalysis
    {
        private static HttpClient httpClient;
        private static ClarifaiClient clarifaiClient; 
        public const int maxItemsPerBatchRequest = 8;

        static ImageAnalysis()
        {
            RefreshAPIKey();
        }

        public static void RefreshAPIKey()
        { 
            var apiKey = SettingsPersistanceUtil.RetreiveSetting("apiKey");

            if (apiKey == "")
                SetAPIKeyViaDialog();
            else
            {
                httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(@"https://api.clarifai.com/v2/models/");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", apiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                clarifaiClient = new ClarifaiClient(apiKey);
            }
        }

        public static void SetAPIKeyViaDialog()
        {
            var apiKey = SettingsPersistanceUtil.RetreiveSetting("apiKey");
            apiKey = RequestStringDialog.StartDialog(apiKey, "provide clarifai api key for suggestions", "clarifai key", "input key here");
            SettingsPersistanceUtil.RecordSetting("apiKey", apiKey);
            if(apiKey != "")
                RefreshAPIKey();
        }

        public static bool IsPerformingBatchOperation { get; private set; } = false;
        public static System.Collections.Async.IAsyncEnumerable<Dictionary<string, List<TagSuggestion>>> RequestBatchAnalysis(IEnumerable<string> imageFilePaths, bool skipAutoTagged = true)
        {
            return new AsyncEnumerable<Dictionary<string, List<TagSuggestion>>>( async yield =>
            {
                var result = new Dictionary<string, List<TagSuggestion>>();
                if (IsPerformingBatchOperation)
                {
                    MessageBox.Show("only one batch operation may be performed at a time!");
                    yield.Break();
                }

                IsPerformingBatchOperation = true;
                var tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                var cancelContext = new CancelDialogDataContext()
                {
                    MaxValue = imageFilePaths.Count(),
                    OnCancel = (s, e) => { tokenSource.Cancel(); },
                    OnClosed = (s, e) => { tokenSource.Cancel(); }
                };
                CancelDialog cancelWindow = null;
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    cancelWindow = new CancelDialog(cancelContext);
                    cancelWindow.Show();
                });
                var splitPaths = new List<List<string>>();
                splitPaths.Add(new List<string>());
                foreach (var imageFilePath in imageFilePaths)
                {
                    var i = splitPaths.Count - 1;
                    if (splitPaths[i].Count >= 8)
                    {
                        splitPaths.Add(new List<string>());
                        i++;
                    }

                    if (skipAutoTagged && ImageFileUtil.GetImageTags(imageFilePath).Contains(new ImageTag("autoTagged")))
                    {
                        App.Current.Dispatcher.Invoke(() => cancelContext.CurrentValue++);
                        continue;
                    }
                    splitPaths[i].Add(imageFilePath);
                }
                foreach (var paths in splitPaths)
                {
                    try
                    {

                        result = VisionAPISuggestions.VisionApi.RequestBatchVisionAnalysis(paths);

                        var input = paths.Select((path) => new ClarifaiFileImage(File.ReadAllBytes(path), path));
                        if (token.IsCancellationRequested) break;
                        var res = await clarifaiClient.WorkflowPredict("workflow", input).ExecuteAsync();
                        if (res.IsSuccessful)
                        {
                            foreach (var workflow in res.Get().WorkflowResults)
                            {
                                var imageFilePath = workflow.Input.ID;
                                var suggestions = ParsePredictions(imageFilePath, workflow.Predictions);
                                if (result.ContainsKey(imageFilePath))
                                    result[imageFilePath].AddRange(suggestions);
                                else
                                    result.Add(imageFilePath, suggestions);
                            }
                        }
                        else Debug.WriteLine("batch clarifai analysis was unsuccessful");
                        foreach (var path in paths)
                        {
                            if (result.ContainsKey(path))
                                result[path].Add(new TagSuggestion(new ImageTag("autoTagged"), 1, "general"));
                            else
                                result.Add(path, new List<TagSuggestion>(new TagSuggestion[] { new TagSuggestion(new ImageTag("autoTagged"), 1, "general") }));
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("error thrown in batch conversion\n\n" + e);
                    }
                    finally
                    {
                        await yield.ReturnAsync(result);
                    }
                    Thread.Sleep(1000);//can only make a call every 1 sec
                    App.Current.Dispatcher.Invoke(() => cancelContext.CurrentValue += 8);

                }
                App.Current.Dispatcher.Invoke(() => cancelWindow.Close());
                IsPerformingBatchOperation = false;
            });
        }

        public static async Task<List<TagSuggestion>> RequestWorkflowAnalysis(string imageFilePath, params ImageAnalysisType[] categories)
        {
            HashSet<TagSuggestion> result = new HashSet<TagSuggestion>();
            var bytes = File.ReadAllBytes(imageFilePath);
            var input = new ClarifaiFileImage(bytes);
            var res = await clarifaiClient.WorkflowPredict("workflow", input).ExecuteAsync();
            if (res.IsSuccessful)
            {
                var predictions = res.Get().WorkflowResult.Predictions;
                result.AddRange( ParsePredictions(imageFilePath, predictions));
            }
            else
                MessageBox.Show("Clarifai Analysis was not successful! Check your internet connection and api key, and you have a workflow named 'workflow'");

            result.AddRange(VisionAPISuggestions.VisionApi.RequestVisionAnalysis(imageFilePath));
            return new List<TagSuggestion>(result);
        }

        private static List<TagSuggestion> ParsePredictions(string imageFilePath, IEnumerable<ClarifaiOutput> predictions)
        {
            var cacheWorkflowPath = imageFilePath + "/" + "workflow";
            var result = new List<TagSuggestion>();
            if(!RequestCachedResults(cacheWorkflowPath, out result))
            {
                Debug.WriteLine("cache miss! api request made");
                var tmpResult = new HashSet<TagSuggestion>();
                foreach (var prediction in predictions)
                {
                    if (prediction.Model.ModelID == clarifaiClient.PublicModels.GeneralModel.ModelID)
                    {
                        var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.general);
                        var cachePath = imageFilePath + "/" + category;
                        var parsed = ParseGeneralData(prediction.Data.Cast<Concept>());
                        CacheAnalyticsResults(cachePath, parsed);
                        tmpResult.AddRange(parsed);
                    }
                    else if (prediction.Model.ModelID == clarifaiClient.PublicModels.DemographicsModel.ModelID)
                    {
                        var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.demographics);
                        var cachePath = imageFilePath + "/" + category;
                        var parsed = ParseDemographicsData(prediction.Data.Cast<Demographics>());
                        CacheAnalyticsResults(cachePath, parsed);
                        tmpResult.AddRange(parsed);
                    }
                    else if (prediction.Model.ModelID == clarifaiClient.PublicModels.ModerationModel.ModelID)
                    {
                        var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.moderation);
                        var cachePath = imageFilePath + "/" + category;
                        var parsed = ParseModerationData(prediction.Data.Cast<Concept>());
                        CacheAnalyticsResults(cachePath, parsed);
                        tmpResult.AddRange(parsed);
                    }
                    else if (prediction.Model.ModelID == clarifaiClient.PublicModels.PortraitQualityModel.ModelID || prediction.Model.ModelID == clarifaiClient.PublicModels.LandscapeQualityModel.ModelID )
                    {
                        var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.quality);
                        var cachePath = imageFilePath + "/" + category;
                        var parsed = ParseQualityData(prediction.Data.Cast<Concept>());
                        CacheAnalyticsResults(cachePath, parsed);
                        tmpResult.AddRange(parsed);
                    }
                    else
                    {
                        var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.general);
                        var cachePath = imageFilePath + "/" + category;
                        var parsed = ParseGeneralData(prediction.Data);
                        CacheAnalyticsResults(cachePath, parsed);
                        tmpResult.AddRange(parsed);
                    }
                }
                result.AddRange(tmpResult);
                CacheAnalyticsResults(cacheWorkflowPath, result);
            }
            else Debug.WriteLine("cache hit! avoided a number of api requests");

            return result;
        }

        private static List<TagSuggestion> ParseQualityData(IEnumerable<Concept> data)
        {
            var category = Enum.GetName(typeof(ImageAnalysisType), ImageAnalysisType.quality);
            var result = new List<TagSuggestion>();
            foreach (var concept in data)
            {
                var name = concept.Name;
                var certainty = (double)concept.Value.Value;
                var threshold = 0.6;
                if (certainty > threshold)
                {
                    result.Add(new TagSuggestion(new ImageTag(concept.Name), certainty, category));
                }
            }
            return result;
        }

        /* non workflow analysis
        public static async Task<List<TagSuggestion>> RequestAnalysis(string imageFilePath, params ImageAnalysisType[] categories)
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
        */

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

