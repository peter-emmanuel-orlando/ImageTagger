
using Microsoft.WindowsAPICodePack.Shell;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Concurrent;
using ImageTagger.DataModels;

namespace ImageTagger
{
    public enum LoadState
    {
        Unloaded = 0,
        Loading,
        Loaded
    }
    public static class DirectoryTagUtil
    {
        public static LoadState loadState { get; private set; } = LoadState.Unloaded;
        private static readonly ConcurrentDictionary<string, HashSet<string>> tags = new ConcurrentDictionary<string, HashSet<string>>();


        public static bool HasCategory( string category)
        {
            return tags.ContainsKey(category);
        }

        public static HashSet<string> GetTagCategories()
        {
            return new HashSet<string>(tags.Keys);
        }

        public static HashSet<string> GetTags(string category)
        {
            if (tags.ContainsKey(category))
                return new HashSet<string>(tags[category]);
            else
                return new HashSet<string>();
        }

        public static List<TagSuggestion> GetSuggestedTags(string imgFilePath, string category = "")
        {
            var r = new Random(DateTime.UtcNow.Millisecond);
            if (category == "" || !HasCategory(category)) category = "loaded";
            var tmp = GetTags(category).Select((tagText) =>
            {
                return new TagSuggestion(new ImageTag(tagText), r.NextDouble());
            });
            var result = new List<TagSuggestion>(tmp);
            result.Shuffle();
            return result;
        }

        static DirectoryTagUtil()
        {
            tags.TryAdd("quickList", new HashSet<string>());
            tags.TryAdd("all", new HashSet<string>());
            tags.TryAdd("loaded", new HashSet<string>());
            OnProcessedNewCategory?.Invoke(null, new AddedCategoryEventArgs("quickList"));
            OnProcessedNewCategory?.Invoke(null, new AddedCategoryEventArgs("all"));
            OnProcessedNewCategory?.Invoke(null, new AddedCategoryEventArgs("loaded"));
        }
        

        public static async void Load()
        {
            PreviewTagsLoaded?.Invoke(null, new EventArgs());
            loadState = LoadState.Loading;
            await LoadTagsFromFileAsynch(PersistanceUtil.SourceDirectory);
            await LoadTagsFromListAsynch(PersistanceUtil.ResourcePersistanceDirectory + @"\AllPossibleTags.txt");
            loadState = LoadState.Loaded;
            TagsLoaded?.Invoke(null, new EventArgs());
        }


        private static async Task LoadTagsFromListAsynch(string filename)
        {
            await Task.Run(delegate () { LoadTagsFromList(filename); });
        }

        private static void LoadTagsFromList(string filename)
        {
            // Read the file line by line.
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
                        tags.TryAdd(line, new HashSet<string>());
                        OnProcessedNewCategory?.Invoke(null, new AddedCategoryEventArgs(line));
                        currentKey = line;
                    }
                    else
                    {
                        if (currentKey != "" && tags.ContainsKey(currentKey))
                        {
                            tags[currentKey].Add(line);
                            OnProcessedNewTag?.Invoke(null, new AddedTagEventArgs(currentKey, line));
                        }
                        tags["all"].Add(line);//<- inefficient
                        OnProcessedNewTag?.Invoke(null, new AddedTagEventArgs("all", line));
                    }
                }
            }
            fs.Close();
            file.Close();
        }

        private static async Task LoadTagsFromFileAsynch(string filename)
        {
            await Task.Run(delegate () { LoadTagsFromDirectory(filename); });
        }

        private static void LoadTagsFromDirectory(string directory)
        {
            tags["loaded"].Clear();
            var ImageFilenames = new List<string>(Directory.EnumerateFiles(directory, "*jpg", SearchOption.AllDirectories));
            ImageFilenames.AddRange(Directory.EnumerateFiles(directory, "*jpeg", SearchOption.AllDirectories));

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
                            OnProcessedNewTag?.Invoke(null, new AddedTagEventArgs(null, tag));
                        }
                    }
                }
            }
        }

        public static event PreviewTagsLoadedEventHandler PreviewTagsLoaded = delegate { };
        public static event ProcessedNewCategoryEventHandler OnProcessedNewCategory = delegate { };
        public static event ProcessedNewTagEventHandler OnProcessedNewTag = delegate { };
        public static event TagsLoadedEventHandler TagsLoaded = delegate { };

    }
    public delegate void PreviewTagsLoadedEventHandler(object sender, EventArgs e);
    public delegate void ProcessedNewCategoryEventHandler(object sender, AddedCategoryEventArgs e);
    public delegate void ProcessedNewTagEventHandler(object sender, AddedTagEventArgs e);
    public delegate void TagsLoadedEventHandler(object sender, EventArgs e);

    public class AddedCategoryEventArgs : EventArgs
    {
        public AddedCategoryEventArgs(string category)
        {
            this.category = category;
        }

        public string category { get; }
    }
    public class AddedTagEventArgs : EventArgs
    {
        public AddedTagEventArgs(string category, string newTagText)
        {
            this.category = category;
            this.newTagText = newTagText;
        }

        public string category { get; }
        public string newTagText { get; }
    }
    public class TagSuggestion
    {

        public ImageTag tag { get; }
        public double confidenceLevel { get; }

        public TagSuggestion(ImageTag tag, double confidenceLevel)
        {
            this.confidenceLevel = confidenceLevel;
            this.tag = tag;
        }
    }
}



