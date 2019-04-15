using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Google.Cloud.Vision.V1;
using Google.ColorUtil;
using ImageTagger;
using ImageTagger.DataModels;
using ImageTagger.UI;

namespace VisionAPISuggestions
{
    public static class VisionApi
    {
        private static ImageAnnotatorClient client;
        private static string authPath { get; } = $"{Directory.GetCurrentDirectory()}/visionAPI.auth.json";
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
                    client = ImageAnnotatorClient.Create();
                }
                catch (Exception)
                {
                    MessageBox.Show("the provided json file is not correct. Vision API will not work until a valid authentication json is provided");
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


        public static List<ImageTag> RequestVisionAnalysis(string imageFilePath)
        {
            if (client == null) SetVisionAuthViaDialog();
            if (client == null) return new List<ImageTag>();

            var req = new AnnotateImageRequest() { Image = Image.FromFile(imageFilePath) };
            req.AddAllFeatures();

            var res = client.Annotate(req);
            return ParseAnnotations(res);
        }

        private static List<ImageTag> ParseAnnotations(AnnotateImageResponse res)
        {
            var result = new HashSet<ImageTag>();
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
                        result.Add(new ImageTag("angry"));
                    if (annotation.BlurredLikelihood.MeetsThreshold(threshold))
                        result.Add(new ImageTag("blurry"));
                    if (annotation.HeadwearLikelihood.MeetsThreshold(threshold))
                        result.Add(new ImageTag("headwear"));
                    if (annotation.JoyLikelihood.MeetsThreshold(threshold))
                        result.Add(new ImageTag("joyful"));
                    if (annotation.SorrowLikelihood.MeetsThreshold(threshold))
                        result.Add(new ImageTag("sad"));
                    if (annotation.SurpriseLikelihood.MeetsThreshold(threshold))
                        result.Add(new ImageTag("suprised"));
                    if (annotation.UnderExposedLikelihood.MeetsThreshold(threshold))
                        result.Add(new ImageTag("underExposed"));
                }
            }
            #endregion

            // parse dominant color annotations
            #region
            if (res.ImagePropertiesAnnotation != null)
            {
                var colorArray = "[";
                foreach (var annotation in res.ImagePropertiesAnnotation.DominantColors.Colors)
                {
                    double hue, saturation, value;
                    ColorConverter.RGB2HSL(annotation.Color,out hue, out saturation,out value);
                    if(annotation.Score > 0.05)
                        colorArray += $"{{hue:{hue.ToString("0.#")},_saturation:{saturation.ToString("0.#")},_value:{value.ToString("0.#")}}},";
                }
                colorArray += "]";
                result.Add(new ImageTag(colorArray));
            }
            #endregion

            // parse label annotations
            #region
            if (res.LabelAnnotations != null)
            {
                foreach (var annotation in res.LabelAnnotations)
                {
                    result.Add(new ImageTag(annotation.Description));
                }
            }
            #endregion

            // parse object annotations
            #region
            if (res.LocalizedObjectAnnotations != null)
            {
                foreach (var annotation in res.LocalizedObjectAnnotations)
                {
                    result.Add(new ImageTag(annotation.Name));
                }
            }
            #endregion

            // parse logo annotations
            #region
            if (res.LogoAnnotations != null)
            {
                foreach (var annotation in res.LogoAnnotations)
                {
                    result.Add(new ImageTag(annotation.Description));
                }
            }
            #endregion

            // parse product search annotations
            #region
            if (res.ProductSearchResults != null)
            {
                foreach (var annotation in res.ProductSearchResults.Results)
                {
                    result.Add(new ImageTag(annotation.Product.DisplayName));
                }
            }
            #endregion

            // parse safe search annotations
            #region
            if (res.SafeSearchAnnotation != null)
            {
                var isNSFW = false;
                if (res.SafeSearchAnnotation.Adult.MeetsThreshold(threshold))
                { result.Add(new ImageTag("Explicit")); isNSFW = true; }
                if (res.SafeSearchAnnotation.Medical.MeetsThreshold(threshold))
                { result.Add(new ImageTag("Medical")); }
                if (res.SafeSearchAnnotation.Racy.MeetsThreshold(threshold))
                { result.Add(new ImageTag("Suggestive")); isNSFW = true; }
                if (res.SafeSearchAnnotation.Spoof.MeetsThreshold(threshold))
                { result.Add(new ImageTag("Spoof")); }
                if (res.SafeSearchAnnotation.Violence.MeetsThreshold(threshold))
                { result.Add(new ImageTag("Violence")); isNSFW = true; }
                if (isNSFW)
                    result.Add(new ImageTag("nsfw"));
            }
            #endregion

            // parse web tags annotations
            #region
            if (res.WebDetection != null)
            {
                foreach (var annotation in res.WebDetection.BestGuessLabels)
                {
                    result.Add(new ImageTag(annotation.Label));
                }
            }
            #endregion

            //end
            #endregion

            result.Add(new ImageTag("autoTagged"));
            return new List<ImageTag>(result);
        }
    }
}
