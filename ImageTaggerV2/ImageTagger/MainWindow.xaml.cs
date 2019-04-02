using ImageTagger_DataModels;
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
        private readonly string TagsDisplayPlaceholder = "[type tags for image]";
        private bool checkForEmptyTags = true;

        private bool randomizeImages = false;
        protected string sourceDirectory = /*@"C:\Users\YumeMura\Downloads"; //*/  @"C:\Users\YumeMura\Desktop\unsamples";

        protected const string locFileName = @"lastSession.loc";
        protected const string genericLocFile = @"SourceDirectory:SourcePlaceHolder|DestinationDirectory:DestinationPlaceHolder";



        public ObservableCollection<string> ImageFileNames { get; } = new ObservableCollection<string>();

        public ImageGridDisplay ImageGridDisplay { get; }
        public ImageTagsDisplay ImageTagsDisplay { get; }
        public MainImageDisplay ImageDisplay { get; }

        public MainWindow()
        { 
            InitializeComponent();
            InitializeSelf();
            ImageDisplay = new MainImageDisplay(this);
            ImageTagsDisplay = new ImageTagsDisplay(this);
            ImageGridDisplay = new ImageGridDisplay(this);
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


        private void TagsDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ImageFileUtil.ApplyTagsToImage(ImageTagsDisplay.TagSource.ImgPath, ImageTagsDisplay.Tags);
                //ImageTagsDisplay.Refresh();
            }
        }

        private void TagsDisplay_LostFocus(object sender, RoutedEventArgs e)
        {
            ImageFileUtil.ApplyTagsToImage(ImageTagsDisplay.TagSource.ImgPath, ImageTagsDisplay.Tags);
            //ImageTagsDisplay.Refresh();
        }


        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Debug.WriteLine("would have added tag: " + addNewTag_TextBox.Text);
                ImageTagsDisplay.Add(new ImageTag(addNewTag_TextBox.Text));
                ImageFileUtil.ApplyTagsToImage(ImageDisplay.mainImageInfo, ImageTagsDisplay.Tags);
                addNewTag_TextBox.Clear();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("would have added tag: " + addNewTag_TextBox.Text);
            ImageTagsDisplay.Add(new ImageTag(addNewTag_TextBox.Text));
            ImageFileUtil.ApplyTagsToImage(ImageDisplay.mainImageInfo, ImageTagsDisplay.Tags);
            addNewTag_TextBox.Clear();
        }

        private void AddNewTag_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if(!addNewTag_AcceptButton.IsFocused)
            {
                addNewTag_TextBox.Clear();
            }
        }
    }
}
