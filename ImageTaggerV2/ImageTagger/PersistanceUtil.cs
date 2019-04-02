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
        private static string DefaultPersistanceDirectory { get { return Directory.GetCurrentDirectory(); } }


        private static void SaveLocations(string sourcePath = "", string destinationPath = "", string persistanceDirectory = "")
        {
            if (sourcePath == "") sourcePath = DefaultImageDirectory;
            if (destinationPath == "") destinationPath = DefaultImageDirectory;
            if (persistanceDirectory == "") persistanceDirectory = DefaultPersistanceDirectory;

            var newLocFile = genericLocFile.Replace("SourcePlaceHolder", sourcePath).Replace("DestinationPlaceHolder", destinationPath);
            var filename = Path.Combine(persistanceDirectory, locFileName);
            string[] files = Directory.GetFiles(persistanceDirectory, locFileExtension);
            Array.Sort(files);
            if (files.Length > 0) filename = files[0];
            File.WriteAllText(filename, newLocFile);
        }

        private static bool ReadLocations(out string sourcePath, out string destinationPath, string persistanceDirectory = "")
        {
            if(persistanceDirectory == "") persistanceDirectory = Directory.GetCurrentDirectory();
            var success = false;
            sourcePath = destinationPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            FileStream fs = null;
            StreamReader file = null;
            try
            {
                var filename = Path.Combine(persistanceDirectory, locFileName);
                string[] files = Directory.GetFiles(persistanceDirectory, locFileExtension);
                Array.Sort(files);
                if (files.Length > 0) filename = files[0];
                fs = new FileStream(filename, FileMode.OpenOrCreate);
                file = new StreamReader(fs);
                var lines = file.ReadToEnd().Split('|');

                sourcePath = lines[0].Replace("SourceDirectory:", "");
                destinationPath = lines[1].Replace("DestinationDirectory:", "");
                success = true;
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
