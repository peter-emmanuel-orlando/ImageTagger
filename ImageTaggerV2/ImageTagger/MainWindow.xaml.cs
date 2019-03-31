using System;
using System.Collections.Generic;
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

        private string sourceDirectory = @"C:\Users\YumeMura\Downloads";
        private readonly List<string> imageFileNames = new List<string>();
        private readonly List<ImageSquare> loadedImages = new List<ImageSquare>();

        private const string locFileName = @"lastSession.loc";
        private const string genericLocFile = @"SourceDirectory:SourcePlaceHolder|DestinationDirectory:DestinationPlaceHolder";


        public MainWindow()
        { 
            InitializeComponent();
            imageFileNames.Clear();
            imageFileNames = GetImageFilenames(sourceDirectory, true);
            loadedImages.Clear();
            loadedImages = new List<ImageSquare>();
            int count = 0;
            int max = 25;
            foreach (var filename in imageFileNames)
            {
                Debug.WriteLine(filename);
                try
                {
                    loadedImages.Add(new ImageSquare(filename));
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

        private void HandleImageGridSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                Debug.WriteLine("selected: " + (e.AddedItems[0] as ImageSquare).ImgSource.UriSource);
                mainImageDisplay.Source = (e.AddedItems[0] as ImageSquare).ImgSource;
            }
        }

        private void HandleImageGridScrollChange(object sender, ScrollChangedEventArgs e)
        {
            Debug.WriteLine(
                " e.VerticalChange:" +
                e.VerticalChange + 
                "e.ViewportHeight:" +
                e.ViewportHeight +
                " e.VerticalOffset:" +
                e.VerticalOffset +
                " e.ExtentHeight:" +
                e.ExtentHeight +
                " e.ExtentHeightChange:" +
                e.ExtentHeightChange +
                " scrollableHeight:" +
                (e.OriginalSource as ScrollViewer).ScrollableHeight
              );
            var scrollableHeight = (e.OriginalSource as ScrollViewer).ScrollableHeight;
            if (e.VerticalChange > 0 || scrollableHeight == 0)
            {
                if (e.VerticalOffset + e.ViewportHeight == e.ExtentHeight || scrollableHeight == 0)
                {
                    Debug.WriteLine("At the bottom of the list!");
                    if(loadedImages.Count() < imageFileNames.Count())
                    {
                        var loadedCount = loadedImages.Count();
                        var fileNameCount = imageFileNames.Count();
                        for (int i = loadedCount; i < Math.Min(10 + loadedCount, fileNameCount ); i++)
                        {
                            try
                            {
                                //Debug.WriteLine("trying to add file to list:" + imageFileNames[i]);
                                var newSquare = new ImageSquare(imageFileNames[i]);
                                loadedImages.Add(newSquare);
                            }
                            catch (Exception) { }
                        }
                        var selectedIndex = imgGrid.SelectedIndex;
                        imgGrid.ItemsSource = loadedImages.ToArray();
                        ListBoxItem myListBoxItem =
                            (ListBoxItem)(imgGrid.ItemContainerGenerator.ContainerFromItem(imgGrid.Items[selectedIndex]));
                        //myListBoxItem.Focus();
                        return;
                    }
                }
            }
        }

        private void ScrollViewer_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            //e.Handled = true;
        }
    }
}
