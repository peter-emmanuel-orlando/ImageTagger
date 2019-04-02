﻿using ImageTagger_DataModels;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Debug = System.Diagnostics.Debug;
using Path = System.IO.Path;

namespace ImageTagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {



        private string programDirectory = "";
        private string destinationDirectory = "";
        private bool checkForEmptyTags = true;

        private bool randomizeImages = false;
        protected string sourceDirectory = /*@"C:\Users\YumeMura\Downloads"; //*/  @"C:\Users\YumeMura\Desktop\unsamples";

        protected const string locFileName = @"lastSession.loc";
        protected const string genericLocFile = @"SourceDirectory:SourcePlaceHolder|DestinationDirectory:DestinationPlaceHolder";



        public ObservableCollection<string> ImageFileNames { get; } = new ObservableCollection<string>();

        public MainImageDisplay ImageDisplay { get; }
        public ImageGridDisplay ImageGridDisplay { get; }
        public ImageTagsDisplay ImageTagsDisplay { get; }
        public AddNewTagComponent AddNewTagComponent { get; }

        public MainWindow()
        { 
            InitializeComponent();
            InitializeSelf();
            ImageDisplay = new MainImageDisplay(this);
            ImageTagsDisplay = new ImageTagsDisplay(this);
            ImageGridDisplay = new ImageGridDisplay(this);
            AddNewTagComponent = new AddNewTagComponent(this);
        }

        private void InitializeSelf()
        {
            ImageFileNames.Clear();
            GetImageFilenames(sourceDirectory, randomizeImages).ForEach((item) => { ImageFileNames.Add(item); });
        }



        public HashSet<ImageTag> GetImageTags(string imagePath)
        {
            var result = new HashSet<ImageTag>();

            Debug.WriteLine("tags at: " + imagePath);
            if (File.Exists(imagePath))
            {
                try
                {
                    var sFile = ShellFile.FromParsingName(imagePath);
                    var tagsList = sFile.Properties.System.Keywords.Value;
                    if (tagsList != null)
                    {
                        foreach (var tagText in tagsList)
                        {
                            var newTag = new ImageTag(tagText);//perhaps automatically do to lower?
                            if (!result.Contains(newTag))
                                result.Add(newTag);
                        }
                    }
                }
                catch (Exception e) {throw e; }
            }

            return result;
        }

        /*
        public enum FileSortMethod
        {
            Name = 0,
            Date,
            Random
        }

        public enum FileSortDirection
        {
            Ascending = 0,
            Descending
        }
        */

        private List<string> GetImageFilenames(string sourcePath, bool randomize = false)
        {
            var result = new List<string>();
            var fileTypes = new string[] { ".jpg" };//, "jpeg", "gif", "png", };
            foreach (var fileName in Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                if (fileTypes.Contains( Path.GetExtension(fileName)))
                    result.Add(fileName);
            }
            if (randomize) result.Shuffle();
            return result;
        }
    }
}
