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
        private static ObservableCollection<string> ImageFileNames { get; } = new ObservableCollection<string>();

        public static int Count { get { return ImageFileNames.Count; } }

        public static string Get(int index)
        {
            return ImageFileNames[index];
        }

        public static void Set(int index, string newFilePath)
        {
            var args = new ItemChangedEventArgs(index, ImageFileNames[index], newFilePath);
            ImageFileNames[index] = newFilePath;
            ItemChanged(null, args);
        }

        public static int IndexOf(string filePath)
        {
            return ImageFileNames.IndexOf(filePath);
        }

        public static bool Contains(string filePath)
        {
            return ImageFileNames.Contains(filePath);
        }

        public static IEnumerator<string> GetEnumerator()
        {
            return ImageFileNames.GetEnumerator();
        }

        public static void Load(bool randomize = false)
        {
            ImageFileNames.Clear();
            ImageFileNames.Add(ImageFileUtil.GetImageFilenames(PersistanceUtil.SourceDirectory));
            if (randomize) ImageFileNames.Shuffle();
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
