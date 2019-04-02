using ImageTagger_DataModels;
using Microsoft.WindowsAPICodePack.Shell;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageTagger
{
    public static class DirectoryTagUtil
    {
        public static readonly Dictionary<string, HashSet<string>> tags = new Dictionary<string, HashSet<string>>();

        public static Dictionary<string, HashSet<string>> Tags => tags; //<-- needs to return a new dict

        static DirectoryTagUtil()
        {
            tags.Add("quickList", new HashSet<string>());
            tags.Add("all", new HashSet<string>());
            tags.Add("loaded", new HashSet<string>());
        }

        public static void Load()
        {
            //load tags from list
            LoadTagsFromFileAsynch(PersistanceUtil.SourceDirectory);
        }

        private static void LoadTagsFromList()
        {
            tags.Clear();

            // Read the file line by line.
            var filename = PersistanceUtil.ResourcePersistanceDirectory + @"\AllPossibleTags.txt";
            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate);
            StreamReader file = new StreamReader(fs);
            string line;
            string currentKey = "";
            while ((line = file.ReadLine()) != null)
            {
                line = line.Replace("\n", "");
                line = line.Replace("\r", "");
                line = line.Replace("\t", "");
                line = line.Replace(";", "");
                if (line != "")
                {
                    if (line[0] == '*' && !tags.ContainsKey(line))
                    {
                        tags.Add(line, new HashSet<string>());
                        currentKey = line;
                    }
                    else
                    {
                        if (currentKey != "" && tags.ContainsKey(currentKey))
                        {
                            tags[currentKey].Add(line);
                        }
                        tags["all"].Add(line);//<- inefficient
                    }
                }
            }
            fs.Close();
            file.Close();
        }

        private static async void LoadTagsFromFileAsynch(string filename)
        {
            Task loadTask;
            loadTask = Task.Run(delegate () { LoadTagsFromFile(filename); });
            await loadTask;
        }

        private static void LoadTagsFromFile(string filename)
        {
            tags["loaded"].Clear();
            var ImageFilenames = new List<string>(Directory.EnumerateFiles(filename, "*jpg", SearchOption.AllDirectories));
            ImageFilenames.AddRange(Directory.EnumerateFiles(filename, "*jpeg", SearchOption.AllDirectories));

            foreach (var imageFilename in ImageFilenames)
            {
                ShellObject sf = ShellFile.FromParsingName(imageFilename);
                var tagsRaw = sf.Properties.System.Keywords.Value;
                if (tagsRaw != null)
                {
                    foreach (var tag in tagsRaw)
                    {
                        if (!tags["loaded"].Contains(tag))
                        {
                            tags["loaded"].Add(tag);
                        }
                    }
                }
            }
        }
    }
}
