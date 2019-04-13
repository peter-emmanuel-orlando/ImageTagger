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






        public static void Load(bool randomize = false, TagQueryCriteria tagQueryCriteria = null)
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

        private static string ignoreNext = "";
        private static void OnFilesChanged(object sender, FileSystemEventArgs e)
        {
            var fileName = Path.GetFileName(e.FullPath);
            var extension = Path.GetExtension(e.FullPath);
            if (fileName != ignoreNext && (extension.Contains(".jpg") || extension.Contains(".jpeg")))
                Debug.WriteLine("added " + e.FullPath);
            if (fileName == ignoreNext) ignoreNext = "";
        }


        public static bool MoveToDestination(ImageInfo imgInfo, string newDirectory)
        {
            string newPath = "";
            ignoreNext = Path.GetFileName(imgInfo.ImgPath);
            var success = ImageFileUtil.MoveFile(imgInfo.ImgPath, newDirectory, out newPath);
            if (success)
            {
                //change file path to new filepath
                var fileNameIndex = ImageFiles.IndexOf(imgInfo.ImgPath);
                if (fileNameIndex != -1) ImageFiles.Set(fileNameIndex, newPath);
            }
            return success;
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
