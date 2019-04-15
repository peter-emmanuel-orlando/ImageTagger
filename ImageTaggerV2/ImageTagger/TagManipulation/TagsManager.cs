
using Microsoft.WindowsAPICodePack.Shell;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Concurrent;
using ImageTagger.DataModels;
using System.Diagnostics;
using ImageAnalysisAPI;

namespace ImageTagger
{
    public enum LoadState
    {
        Unloaded = 0,
        Loading,
        Loaded
    }
    public static class TagsManager
    {
        public static LoadState loadState { get; private set; } = LoadState.Unloaded;
        private static ConcurrentDictionary<string, HashSet<string>> TagsRecord = new ConcurrentDictionary<string, HashSet<string>>();


        public static string[] RetreiveTags(string category)
        {
            if (HasCategory(category))
                return TagsRecord[category].ToArray();
            else
                return new string[] { };
        }

        public static void RecordTag(string category, string tagText)
        {
            tagText = FormatUtil.FixTag(tagText);
            category = FormatUtil.FixCategory(category);
            if (tagText == "" || tagText == null)
                return;
            else if (HasCategory(category))
                TagsRecord[category].Add(tagText);
            else
            {
                AddCategory(category);
                TagsRecord[category].Add(tagText);
            }

            Persist();
        }

        public static void AddCategory(string category)
        {
            category = FormatUtil.FixCategory(category);
            if (ReservedTagCategories.Contains(category)) throw new ArgumentException(string.Join(", ", ReservedTagCategories.Names) + " are reserved categories");
            if (!HasCategory(category))
                TagsRecord.TryAdd(category, new HashSet<string>());
        }



        public static void RemoveTagFromRecord(string tag)
        {
            foreach (var category in TagsRecord.Keys)
            {
                if (TagsRecord[category].Contains(tag))
                {
                    TagsRecord[category].Remove(tag);
                }
            }

            Persist();
        }

        private static void Persist()
        {
            var cleaned = new ConcurrentDictionary<string, HashSet<string>>();
            foreach (var category in TagsRecord.Keys)
            {
                if (category == "loaded") continue;
                cleaned.TryAdd(category, TagsRecord[category]);
            }
            ImageTagger.TagManipulation.Internal.MyTagsRecord.Persist(cleaned);
        }


        public static HashSet<string> GetTagCategories()
        {
            var result = new HashSet<string> { "insight" };
            foreach (var category in TagsRecord.Keys)
            {
                result.Add(category);
            }
            return result;
        }

        public static void RemoveTagFromCategory(string category, string tag)
        {
            if (TagsRecord[category].Contains(tag))
            {
                TagsRecord[category].Remove(tag);
            }

            Persist();
        }

        public static bool HasCategory(string setting)
        {
            return TagsRecord.ContainsKey(setting);
        }

        public static bool HasTag(string tag)
        {
            foreach (var category in TagsRecord.Keys)
            {
                if (TagsRecord[category].Contains(tag))
                {
                    return true;
                }
            }
            return false;
        }
        
        public static List<TagSuggestion> GetTagSuggestions(string imgFilePath, string category = "")
        {
            if (category == "" || !HasCategory(category)) category = "loaded";
            var tmp = RetreiveTags(category).Select((tagText) =>
            {
                return new TagSuggestion(new ImageTag(tagText), 0, category);
            });
            var result = new List<TagSuggestion>(tmp);
            result.Shuffle();
            return result;
        }

        public static async void GetImageAnalysisTags( string imagePath, Action<List<TagSuggestion>> callback, params ImageAnalysisType[] analysisTypes)
        {
            var result = await ImageAnalysisAPI.ImageAnalysis.RequestWorkflowAnalysis(imagePath, analysisTypes);
            App.Current.Dispatcher.Invoke(() => callback(result));
        }

        static TagsManager()
        {
            TagsRecord.TryAdd("loaded", new HashSet<string>());
            OnProcessedNewCategory(null, new AddedCategoryEventArgs("loaded"));
        }



        public static async void Load()
        {
            foreach (var category in TagsRecord.Keys)
            {
                TagsRecord[category].Clear();
            }
            ImageTagger.TagManipulation.Internal.MyTagsRecord.Load(ref TagsRecord);
            foreach (var category in TagsRecord.Keys)
            {
                OnProcessedNewCategory(null, new AddedCategoryEventArgs(category));
                foreach (var tag in TagsRecord[category])
                {
                    OnProcessedNewTag(null, new AddedTagEventArgs(category, tag));
                }
            }
            OnPreviewTagsLoaded(null, new EventArgs());
            loadState = LoadState.Loading;
            await LoadTagsFromFileAsynch(PersistanceUtil.SourceDirectory);
            loadState = LoadState.Loaded;
            OnTagsLoaded(null, new EventArgs());
        }

        private static async Task LoadTagsFromFileAsynch(string filename)
        {
            await Task.Run(delegate () { LoadTagsFromDirectory(filename); });
        }

        private static void LoadTagsFromDirectory(string directory)
        {
            TagsRecord["loaded"].Clear();
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
                        if (!TagsRecord["loaded"].Contains(tag))
                        {
                            TagsRecord["loaded"].Add(tag);
                            OnProcessedNewTag(null, new AddedTagEventArgs(null, tag));
                        }
                    }
                }
            }
        }

        //needed to put events back on the ui thread
        private static void OnPreviewTagsLoaded(object sender, EventArgs e) { App.Current.Dispatcher.Invoke(()=> PreviewTagsLoaded?.Invoke(sender, e)); }
        private static void OnProcessedNewCategory(object sender, AddedCategoryEventArgs e) { App.Current.Dispatcher.Invoke(() => ProcessedNewCategory?.Invoke(sender, e)); }
        private static void OnProcessedNewTag(object sender, AddedTagEventArgs e) { App.Current.Dispatcher.Invoke(() => ProcessedNewTag?.Invoke(sender, e)); }
        private static void OnTagsLoaded(object sender, EventArgs e) { App.Current.Dispatcher.Invoke(() => TagsLoaded?.Invoke(sender, e)); }

        public static event PreviewTagsLoadedEventHandler PreviewTagsLoaded = delegate { };
        public static event ProcessedNewCategoryEventHandler ProcessedNewCategory = delegate { };
        public static event ProcessedNewTagEventHandler ProcessedNewTag = delegate { };
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

}



