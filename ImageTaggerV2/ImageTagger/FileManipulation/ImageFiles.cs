﻿using ImageTagger.DataModels;
using System;
using System.Collections;
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

    public class ImageFiles : IEnumerable<string>
    {
        private ObservableCollection<string> FileNames { get; } = new ObservableCollection<string>();
        public int Count { get { return FileNames.Count; } }
        public TagQueryCriteria currentQuery { get; private set; } = new TagQueryCriteria();

        public List<string> GetAll()
        {
            return new List<string>(FileNames);
        }

        public string Get(int index)
        {
            return FileNames[index];
        }

        public void Set(int index, string newFilePath)
        {
            var args = new ItemChangedEventArgs(index, FileNames[index], newFilePath);
            FileNames[index] = newFilePath;
            ItemChanged(null, args);
        }

        public int IndexOf(string filePath)
        {
            return FileNames.IndexOf(filePath);
        }

        public bool Contains(string filePath)
        {
            return FileNames.Contains(filePath);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return FileNames.GetEnumerator();
        }
        public IEnumerator<string> GetEnumerator()
        {
            return FileNames.GetEnumerator();
        }


        public ImageFiles()
        {
            watcher.Changed += OnFilesChanged;
        }



        public void Load(TagQueryCriteria tagQueryCriteria = null, bool newAdditionsOnly = false)
        {
            FileNames.Clear();

            currentQuery = tagQueryCriteria ?? new TagQueryCriteria();
            var sourcePath = PersistanceUtil.RetreiveSetting(Setting.SourceDirectory);
            if(!Directory.Exists(sourcePath))
            {
                sourcePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                PersistanceUtil.RecordSetting(Setting.SourceDirectory, sourcePath);
            }
            if (newAdditionsOnly)
                FileNames.Add(NewFiles);
            else
                FileNames.Add(ImageFileUtil.GetImageFilenames(sourcePath, currentQuery));

            if (currentQuery.orderDirection == OrderDirection.RANDOM)
                FileNames.Shuffle();
            watcher.Path = sourcePath;
            watcher.EnableRaisingEvents = true;
            FilesLoaded(null, new EventArgs());
        }

        public bool MoveToDestination(ImageInfo imgInfo, string newDirectory)
        {
            string newPath = "";
            var success = MoveFile(imgInfo.ImgPath, newDirectory, out newPath);
            if (success)
            {
                //change file path to new filepath
                var fileNameIndex = IndexOf(imgInfo.ImgPath);
                if (fileNameIndex != -1) Set(fileNameIndex, newPath);
            }
            return success;
        }

        private static bool MoveFile(string imgPath, string newDirectory, out string newPath)
        {
            newPath = imgPath;
            var success = !string.IsNullOrEmpty(imgPath) &&
                !string.IsNullOrWhiteSpace(imgPath) &&
                !string.IsNullOrEmpty(newDirectory) &&
                !string.IsNullOrWhiteSpace(newDirectory);
            if (success)
            {
                //try-catch this
                (new FileInfo(newDirectory)).Directory.Create();
                var possibleNewPath = Path.Combine(newDirectory, Path.GetFileName(imgPath));
                Debug.WriteLine("moved " + imgPath + " to " + possibleNewPath);
                File.Move(imgPath, Path.Combine(imgPath, possibleNewPath));
                newPath = possibleNewPath;
            }
            return success;
        }


        private void OnFilesChanged(object sender, FileSystemEventArgs e)
        {
            var path = e.FullPath;
            var extension = Path.GetExtension(path);
            if (!Contains(path) && (extension.Contains(".jpg") || extension.Contains(".jpeg")))
            {
                FileNames.Add(path);
                NewFiles.Add(path);
                ItemAdded(sender, e);
                Debug.WriteLine("added " + path);
            }
        }

        private HashSet<string> NewFiles { get; } = new HashSet<string>();
        private FileSystemWatcher watcher { get; } = new FileSystemWatcher
        {
            NotifyFilter = NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.FileName |
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Size |
                NotifyFilters.Security,
            Filter = "*.*",
            IncludeSubdirectories = true
        };
        public void MarkNewFilesReceived()
        {
            NewFiles.Clear();
        }


        public event FileSystemEventHandler ItemAdded = delegate { };
        public event ItemChangedEventHandler ItemChanged = delegate { };
        public event FilesLoadedEventHandler FilesLoaded = delegate { };

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
