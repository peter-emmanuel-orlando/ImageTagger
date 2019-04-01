using ImageTagger_Model;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
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
        protected readonly List<string> imageFileNames = new List<string>();
        protected readonly List<ImageInfo> loadedImages = new List<ImageInfo>();

        protected const string locFileName = @"lastSession.loc";
        protected const string genericLocFile = @"SourceDirectory:SourcePlaceHolder|DestinationDirectory:DestinationPlaceHolder";


        public MainWindow()
        { 
            InitializeComponent();
            imageFileNames.Clear();
            imageFileNames = GetImageFilenames(sourceDirectory, true);
            loadedImages.Clear();
            loadedImages = new List<ImageInfo>();
            int count = 0;
            int max = 25;
            foreach (var filename in imageFileNames)
            {
                Debug.WriteLine(filename);
                try
                {
                    loadedImages.Add(new ImageInfo(filename));
                    count++;
                }
                catch (System.NotSupportedException)
                {
                    System.Diagnostics.Debug.WriteLine("was not able to use file: " + filename );
                }
                if (count >= max) break;
            }
            imgGrid.ItemsSource = loadedImages.ToArray();
            if(count > 0)
                imgGrid.SelectedIndex = 0;
        }


        private List<ImageTag> GetImageTags(string imagePath)
        {
            var result = new List<ImageTag>();

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
                            result.Add(new ImageTag(tagText));
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
