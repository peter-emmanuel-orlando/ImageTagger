using ImageTagger.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace ImageTagger
{
    /*
    public enum FileSortMethod
    {
        Name = 0,
        Date,
        Size,
    }

    public enum FileSortDirection
    {
        Ascending = 0,
        Descending
    }
    */

    public static class ImageFiles
    {
        private static ObservableCollection<string> FileNames { get; } = new ObservableCollection<string>();

        public static int Count { get { return FileNames.Count; } }

        public static string Get(int index)
        {
            return FileNames[index];
        }

        public static void Set(int index, string newFilePath)
        {
            var args = new ItemChangedEventArgs(index, FileNames[index], newFilePath);
            FileNames[index] = newFilePath;
            ItemChanged(null, args);
        }

        public static int IndexOf(string filePath)
        {
            return FileNames.IndexOf(filePath);
        }

        public static bool Contains(string filePath)
        {
            return FileNames.Contains(filePath);
        }

        public static IEnumerator<string> GetEnumerator()
        {
            return FileNames.GetEnumerator();
        }





        private static FileSystemWatcher watcher = new FileSystemWatcher();


        static ImageFiles()
        {
            watcher = new FileSystemWatcher();
            watcher.NotifyFilter = NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.FileName |
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Size |
                NotifyFilters.Security;
            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            watcher.Changed += OnFilesChanged;
        }



        /// <summary>
        /// loads with the default randomise setting
        /// </summary>
        /// <param name="tagQueryCriteria"></param>
        public static void Load(TagQueryCriteria tagQueryCriteria = null)
        {
            var randomize = SettingsPersistanceUtil.RetreiveSetting("randomizeItems") == "true";
            Load(randomize, tagQueryCriteria);
        }

        public static void Load(bool randomize, TagQueryCriteria tagQueryCriteria = null)
        {
            FileNames.Clear();
            
            //tagQueryCriteria = new TagQueryCriteria(new string[] { "female", }, null, new string[] { "hispanic", });
            var persistancePath = PersistanceUtil.SourceDirectory;
            FileNames.Add(ImageFileUtil.GetImageFilenames(persistancePath, tagQueryCriteria ));
            if (randomize) FileNames.Shuffle();
            watcher.Path = persistancePath;
            watcher.EnableRaisingEvents = true;
            FilesLoaded(null, new EventArgs());
        }
        
        private static void OnFilesChanged(object sender, FileSystemEventArgs e)
        {
            var path = e.FullPath;
            var extension = Path.GetExtension(path);
            if (!Contains(path) && (extension.Contains(".jpg") || extension.Contains(".jpeg")))
            {
                FileNames.Add(path);
                ItemAdded(sender, e);
                Debug.WriteLine("added " + path);
            }
        }



        public static event FileSystemEventHandler ItemAdded = delegate { };
        public static event ItemChangedEventHandler ItemChanged = delegate { };
        public static event FilesLoadedEventHandler FilesLoaded = delegate { };

    }

    public delegate void FilesLoadedEventHandler(object sender, EventArgs e);
    public delegate void ItemChangedEventHandler(object sender, ItemChangedEventArgs e);
    public class ItemChangedEventArgs : EventArgs
    {
        public ItemChangedEventArgs(int imageFilesIndex, string previousImagePath, string currentImagePath)
        {
            ImageFilesIndex = imageFilesIndex;
            PreviousImagePath = previousImagePath;
            CurrentImagePath = currentImagePath;
        }

        public int ImageFilesIndex { get; }
        public string PreviousImagePath { get; }
        public string CurrentImagePath { get; }
    }
}
