using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        public static void Load(bool randomize = false, TagQueryCriteria tagQueryCriteria = null)
        {
            FileNames.Clear();
            tagQueryCriteria = new TagQueryCriteria(new string[] { "female", }, null, new string[] { "hispanic", });
            FileNames.Add(ImageFileUtil.GetImageFilenames(PersistanceUtil.SourceDirectory, tagQueryCriteria ));
            if (randomize) FileNames.Shuffle();
            FilesLoaded(null, new EventArgs());
        }
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
