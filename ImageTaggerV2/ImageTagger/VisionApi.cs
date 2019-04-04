using System.Diagnostics;
using Google.Cloud.Vision.V1;

namespace VisionAPISuggestions
{
    public static class VisionApi
    {

        public static void Test()
        {
            //CmdUtil.ProcessCommand();
            // Instantiates a client
            var client = ImageAnnotatorClient.Create();
            // Load the image file into memory
            var image = Image.FromFile(@"C:\Users\YumeMura\Downloads\New folder\1cb088c7f8e9635b6154b5d40cdf014b.jpg");
            // Performs label detection on the image file
            var response = client.DetectWebInformation(image);
            foreach (var annotation in response.WebEntities)
            {
                if (annotation.Description != null)
                    Debug.WriteLine(annotation.Description);
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
