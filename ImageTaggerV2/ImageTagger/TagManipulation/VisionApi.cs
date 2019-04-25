using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Google.Cloud.Vision.V1;
using Google.ColorUtil;
using ImageTagger;
using ImageTagger.UI;

namespace VisionAPISuggestions
{
    public static class VisionApi
    {
        private static ImageAnnotatorClient client;
        private static string authPath { get; } = $"{Directory.GetCurrentDirectory()}\\visionAPI.auth.json";
        static VisionApi()
        {
            RefreshAuthToken();
        }

        public static void RefreshAuthToken()
        {
            if (File.Exists(authPath))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", authPath);
                try
                {
                    MessageBox.Show(File.ReadAllText(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")));
                    client = ImageAnnotatorClient.Create();
                }
                catch (Exception e)
                {
                    MessageBox.Show("the provided json file is not correct. Vision API will not work until a valid authentication json is provided\n\n" + e);
                    client = null;
                }
            }
            else 
                SetVisionAuthViaDialog();
        }

        public static void SetVisionAuthViaDialog()
        {
            var authJson = (File.Exists(authPath))? File.ReadAllText(authPath) : "";
            authJson = RequestStringDialog.StartDialog(authJson, "provide vision auth json for suggestions. An authentication token can be retreived from cloud.google.com/vision", "auth json", "paste json here");
            File.WriteAllText(authPath, authJson);
            RefreshAuthToken();
        }


        public static Dictionary<string, List<TagSuggestion>> RequestBatchVisionAnalysis(IEnumerable<string> paths)
        {
            if (client == null) SetVisionAuthViaDialog();
            if (client == null) return new Dictionary<string, List<TagSuggestion>>();
            var result = new Dictionary<string, List<TagSuggestion>>();

            var req = new List<AnnotateImageRequest>();
            var imageFilePaths = new List<string>(paths);
            for (int i = 0; i < imageFilePaths.Count; i++)
            {
                var reqItem = new AnnotateImageRequest();
                reqItem.Image = Image.FromFile(imageFilePaths[i]);
                reqItem.AddAllFeatures();
                req.Add(reqItem);
            }
            try
            {
                var batchResponse = client.BatchAnnotateImages(req);
                for (int i = 0; i < imageFilePaths.Count; i++)
                {
                    var res = batchResponse.Responses[i];
                    result.Add(imageFilePaths[i], ParseAnnotations(res));
                }
            }
            catch(Exception e)
            {
                MessageBox.Show($"vision API has encountered an error: \n\n {e}");
            }
            return result;
        }


        public static List<TagSuggestion> RequestVisionAnalysis(string imageFilePath)
        {
            if (client == null) SetVisionAuthViaDialog();
            if (client == null) return new List<TagSuggestion>();

            var req = new AnnotateImageRequest() { Image = Image.FromFile(imageFilePath) };
            req.AddAllFeatures();

            var res = client.Annotate(req);
            return ParseAnnotations(res);
        }

        private static List<TagSuggestion> ParseAnnotations(AnnotateImageResponse res)
        {
            var result = new HashSet<TagSuggestion>();
            var threshold = Likelihood.Possible;
            //parse all annotations
            #region
            //start

            // parse face annotations
            #region
            if (res.FaceAnnotations != null)
            {
                foreach (var annotation in res.FaceAnnotations)
                {
                    if (annotation.AngerLikelihood.MeetsThreshold(threshold))
                        result.Add(new TagSuggestion("angry", 1, "FaceAnnotation"));
                    if (annotation.BlurredLikelihood.MeetsThreshold(threshold))
                        result.Add(new TagSuggestion("blurry", 1, "FaceAnnotation"));
                    if (annotation.HeadwearLikelihood.MeetsThreshold(threshold))
                        result.Add(new TagSuggestion("headwear", 1, "FaceAnnotation"));
                    if (annotation.JoyLikelihood.MeetsThreshold(threshold))
                        result.Add(new TagSuggestion("joyful", 1, "FaceAnnotation"));
                    if (annotation.SorrowLikelihood.MeetsThreshold(threshold))
                        result.Add(new TagSuggestion("sad", 1, "FaceAnnotation"));
                    if (annotation.SurpriseLikelihood.MeetsThreshold(threshold))
                        result.Add(new TagSuggestion("suprised", 1, "FaceAnnotation"));
                    if (annotation.UnderExposedLikelihood.MeetsThreshold(threshold))
                        result.Add(new TagSuggestion("underExposed", 1, "FaceAnnotation"));
                }
            }
            #endregion

            // parse dominant color annotations
            #region
            if (res.ImagePropertiesAnnotation != null)
            {
                //var colorArray = "[";
                foreach (var annotation in res.ImagePropertiesAnnotation.DominantColors.Colors)
                {
                    //color
                    //pastel, normal, vibrant
                    //dark, light
                    //if value is below 0.25, color is black
                    var colors = new string[]{ "red", "orange", "yellow", "green", "blue", "indigo", "violet" };
                    var vibrancies = new string[] { "pastel", "", "vibrant", };
                    var darknessValues = new string[] { "dark", "", "bright" };
                    
                    double hue, saturation, value;
                    ColorConverter.RGB2HSL(annotation.Color,out hue, out saturation,out value);
                    if(annotation.Score > 0.05)
                    {
                        var color = colors[(int)Math.Round(hue * 6)];

                        if (value > 0.9) color = darknessValues[2] + color;
                        else if (value > 0.5) color = darknessValues[1] + color;
                        else if (value > 0.3) color = darknessValues[0] + color;

                        if (saturation > 0.8) color = vibrancies[2] + color;
                        else if (saturation > 0.4) color = vibrancies[1] + color;
                        else if (saturation > 0.1) color = vibrancies[0] + color;

                        if (saturation < 0.1) {

                            if (value > 0.9) color = "white";
                            else if (value > 0.7) color = "lightgrey";
                            else if (value > 0.3) color = "darkgrey";
                        }
                        if (value < 0.3) color = "black";

                        result.Add(new TagSuggestion(color, 1, "color"));
                        //colorArray += $"{{hue:{hue.ToString("0.#")},_saturation:{saturation.ToString("0.#")},_value:{value.ToString("0.#")}}},";
                    }
                }
                //colorArray += "]";
            }
            #endregion

            // parse label annotations
            #region
            if (res.LabelAnnotations != null)
            {
                foreach (var annotation in res.LabelAnnotations)
                {
                    result.Add(new TagSuggestion(annotation.Description, annotation.Score, "labelAnnotations"));
                }
            }
            #endregion

            // parse object annotations
            #region
            if (res.LocalizedObjectAnnotations != null)
            {
                foreach (var annotation in res.LocalizedObjectAnnotations)
                {
                    result.Add(new TagSuggestion(annotation.Name, annotation.Score, "objectAnnotations"));
                }
            }
            #endregion

            // parse logo annotations
            #region
            if (res.LogoAnnotations != null)
            {
                foreach (var annotation in res.LogoAnnotations)
                {
                    result.Add(new TagSuggestion(annotation.Description, annotation.Score, "logoAnnotations"));
                }
            }
            #endregion

            // parse product search annotations
            #region
            if (res.ProductSearchResults != null)
            {
                foreach (var annotation in res.ProductSearchResults.Results)
                {
                    result.Add(new TagSuggestion(annotation.Product.DisplayName, annotation.Score, "productAnnotations"));
                }
            }
            #endregion

            // parse safe search annotations
            #region
            if (res.SafeSearchAnnotation != null)
            {
                var isNSFW = false;
                if (res.SafeSearchAnnotation.Adult.MeetsThreshold(threshold))
                { result.Add(new TagSuggestion("Explicit",1, "moderationAnnotations")); isNSFW = true; }
                if (res.SafeSearchAnnotation.Medical.MeetsThreshold(threshold))
                { result.Add(new TagSuggestion("Medical", 1, "moderationAnnotations")); }
                if (res.SafeSearchAnnotation.Racy.MeetsThreshold(threshold))
                { result.Add(new TagSuggestion("Suggestive", 1, "moderationAnnotations")); isNSFW = true; }
                if (res.SafeSearchAnnotation.Spoof.MeetsThreshold(threshold))
                { result.Add(new TagSuggestion("Spoof", 1, "moderationAnnotations")); }
                if (res.SafeSearchAnnotation.Violence.MeetsThreshold(threshold))
                { result.Add(new TagSuggestion("Violence", 1, "moderationAnnotations")); isNSFW = true; }
                if (isNSFW)
                    result.Add(new TagSuggestion("nsfw", 1, "moderationAnnotations"));
            }
            #endregion

            // parse web tags annotations
            #region
            if (res.WebDetection != null)
            {
                foreach (var annotation in res.WebDetection.BestGuessLabels)
                {
                    result.Add(new TagSuggestion(annotation.Label, 1, "webAnnotations"));
                }
            }
            #endregion

            //end
            #endregion

            result.Add(new TagSuggestion("autoTagged", 1, "General"));
            return new List<TagSuggestion>(result);
        }
    }
}
