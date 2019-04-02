using System;
using System.Diagnostics;
using System.IO;

namespace ImageTagger
{
    public static class PersistanceUtil
    {

        private const string locFileExtension = @".loc";
        private const string locFileName = @"lastSession" + locFileExtension;
        private const string genericLocFile = @"SourceDirectory:SourcePlaceHolder|DestinationDirectory:DestinationPlaceHolder";
        private static string DefaultImageDirectory { get { return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); } }
        public static string ResourcePersistanceDirectory { get { return Directory.GetCurrentDirectory(); } }

        public static string SourceDirectory { get; private set; }
        public static string DestinationDirectory { get; private set; }

        static PersistanceUtil()
        {
            LoadLocations();
        }

        public static void ChangeSource(string sourceDirectory = null)
        {
            if (String.IsNullOrEmpty(sourceDirectory) || String.IsNullOrWhiteSpace(sourceDirectory))
                sourceDirectory = DefaultImageDirectory;
            SaveLocations(sourceDirectory, DestinationDirectory);
        }

        public static void ChangeDestination(string destinationDirectory)
        {
            if (String.IsNullOrEmpty(destinationDirectory) || String.IsNullOrWhiteSpace(destinationDirectory))
                destinationDirectory = DefaultImageDirectory;
            SaveLocations(SourceDirectory, destinationDirectory);
        }

        private static void SaveLocations(string sourceDirectory, string destinationDirectory)
        {
            var newLocFile = genericLocFile.Replace("SourcePlaceHolder", sourceDirectory).Replace("DestinationPlaceHolder", destinationDirectory);
            var filename = Path.Combine(ResourcePersistanceDirectory, locFileName);
            string[] files = Directory.GetFiles(ResourcePersistanceDirectory, locFileExtension);
            Array.Sort(files);
            if (files.Length > 0) filename = files[0];
            File.WriteAllText(filename, newLocFile);
            LoadLocations();
        }

        public static void LoadLocations()
        {
            string source;
            string destination;
            ReadLocations(out source, out destination);
            SourceDirectory = source;
            DestinationDirectory = destination;
        }
        private static bool ReadLocations(out string sourcePath, out string destinationPath)
        { 
            var success = false;

            sourcePath = DefaultImageDirectory;
            destinationPath = DefaultImageDirectory;

            FileStream fs = null;
            StreamReader file = null;
            try
            {
                var filename = Path.Combine(ResourcePersistanceDirectory, locFileName);
                string[] files = Directory.GetFiles(ResourcePersistanceDirectory, locFileExtension);
                Array.Sort(files);
                if (files.Length > 0) filename = files[0];
                fs = new FileStream(filename, FileMode.OpenOrCreate);
                file = new StreamReader(fs);
                var lines = file.ReadToEnd().Split('|');
                if(lines.Length == 2)
                {
                    sourcePath = lines[0].Replace("SourceDirectory:", "");
                    destinationPath = lines[1].Replace("DestinationDirectory:", "");
                    success = true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                file?.Close();
                fs?.Close();
            }
            return success;
        }
    }
}
