using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Google.Cloud.Vision.V1;
using ImageTagger;
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


        public static void Test(string imageFilePath)
        {
            if (client == null) SetVisionAuthViaDialog();
            if (client == null) return;
            var req = new AnnotateImageRequest() { Image = Image.FromFile(imageFilePath) };
            foreach (var item in Enum.GetValues(typeof(Feature.Types.Type)).Cast<Feature.Types.Type>())
            {
                req.Features.Add(new Feature() { Type = item });
            }

            var res = client.Annotate(req);
            foreach (var annotation in res.FaceAnnotations)
            {
                Debug.WriteLine(annotation);
            }
            foreach (var annotation in res.ImagePropertiesAnnotation.DominantColors.Colors)
            {
                Debug.WriteLine(annotation.Color);
            }
            foreach (var annotation in res.LabelAnnotations)
            {
                Debug.WriteLine(annotation);
            }
            foreach (var annotation in res.LocalizedObjectAnnotations)
            {
                Debug.WriteLine(annotation.Name);
            }
            foreach (var annotation in res.LogoAnnotations)
            {
                Debug.WriteLine(annotation.Description);
            }
            if(res.ProductSearchResults != null)
                foreach (var annotation in res.ProductSearchResults.Results)
                {
                    Debug.WriteLine(annotation.Product.DisplayName);
                }
            Debug.WriteLine(res.SafeSearchAnnotation);
            if (res.WebDetection != null)
                foreach (var annotation in res.WebDetection.BestGuessLabels)
                {
                    Debug.WriteLine(annotation.Label);
                }
        }
    }

    public static class CmdUtil
    {
        public static void ProcessCommand(string command = @"set GOOGLE_APPLICATION_CREDENTIALS=C:\Users\YumeMura\Downloads\TestProject-f23fca2eca3e.private.json")
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + command;
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}
