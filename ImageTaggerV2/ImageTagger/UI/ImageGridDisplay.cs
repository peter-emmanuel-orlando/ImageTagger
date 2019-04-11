
using ImageTagger.DataModels;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.OleDb;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Debug = System.Diagnostics.Debug;

namespace ImageTagger
{
    public class ImageGridDisplay
    {
        MainWindow main { get; }
        public ObservableCollection<ImageInfo> Images { get; } = new ObservableCollection<ImageInfo>();
        public ListBox ImageGrid { get { return main.imageGrid; } }

        private int maxLoadedImages = 40;
        private int currentImageOffset = 0;
        private int currentPage = 0;
        private int imagesPerChunk = 25;

        public ImageGridDisplay(MainWindow main)
        {
            this.main = main;
            ImageGrid.ItemsSource = Images;
            ImageGrid.SelectionChanged += ImgGrid_SelectionChanged;
            main.imageGrid_ScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            ImageGrid.Loaded += HandleGridLoaded;
            ImageFiles.FilesLoaded += HandleFilesLoaded;
            ImageFiles.ItemChanged += HandleItemChanged;
            main.loadNextPageButton.Click += HandleLoadNextPageButtonClick;
            Initialize();

            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
            ImageGrid.SelectionChanged -= ImgGrid_SelectionChanged;
            main.imageGrid_ScrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            ImageGrid.Loaded -= HandleGridLoaded;
            ImageFiles.FilesLoaded -= HandleFilesLoaded;
            ImageFiles.ItemChanged -= HandleItemChanged;
            main.loadNextPageButton.Click -= HandleLoadNextPageButtonClick;
        }

        public void Initialize()
        {
            Images.Clear();
            currentImageOffset = 0;
            currentPage = 0;
            RequestMoreImages();
        }

        public static void SetTagFilter()
        {
            // any of these tags match < coded blue
            // AND
            // all of these tags match < coded green
            // AND
            // none of these tags match < coded orange
            var windowsSearchConnection = @"Provider=Search.CollatorDSO;Extended Properties=""Application=Windows""";
            using (OleDbConnection connection = new OleDbConnection(windowsSearchConnection))
            {
                connection.Open();
                var criteria = new TagQueryCriteria(new string[] { "test", "pretty girl" });
                var query = $"SELECT System.ItemPathDisplay FROM SystemIndex WHERE SCOPE='{PersistanceUtil.SourceDirectory}' AND " + criteria.GetQueryClause();
                Debug.WriteLine(query);
                OleDbCommand command = new OleDbCommand(query, connection);

                using (var r = command.ExecuteReader())
                {
                    while (r.Read())
                    {
                        Console.WriteLine(r[0]);
                    }
                }
            }
        }

        private void HandleItemChanged(object sender, ItemChangedEventArgs e)
        {
            var changedImageIndex = Images.IndexOf(new ImageInfo(e.PreviousImagePath));
            Images[changedImageIndex].ImgPath = e.CurrentImagePath;
        }

        private void HandleFilesLoaded(object sender, EventArgs e)
        {
            Initialize();
        }

        private void HandleLoadNextPageButtonClick(object sender, RoutedEventArgs e)
        {
            LoadNextPage();
        }

        private void HandleGridLoaded(object sender, RoutedEventArgs e)
        {
            if (ImageGrid.Items.Count > 0 && ImageGrid.IsLoaded)
            {
                ImageGrid.SelectedIndex = 0;
                ListBoxItem myListBoxItem = (ListBoxItem)(ImageGrid.ItemContainerGenerator.ContainerFromItem(ImageGrid.Items[0]));
                //ImageGrid.ScrollIntoView(myListBoxItem);
                myListBoxItem.Focus();
            }
        }

        private void ImgGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                //var newImageInfo = (e.AddedItems[0] as ImageInfo);
                //Debug.WriteLine("selected: " + newImageInfo.ImgPath);
                //main.ImageDisplay.ChangeImage(newImageInfo);
                //main.ImageTagsDisplay.TagSource = newImageInfo;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Debug.WriteLine(
                " e.VerticalChange:" + e.VerticalChange +
                "e.ViewportHeight:" + e.ViewportHeight +
                " e.VerticalOffset:" + e.VerticalOffset +
                " e.ExtentHeight:" + e.ExtentHeight +
                " e.ExtentHeightChange:" + e.ExtentHeightChange +
                " scrollableHeight:" + (e.OriginalSource as ScrollViewer).ScrollableHeight
            );

            if(IsFullyScrolled())
            {
                RequestMoreImages();
            }
        }

        private bool IsFullyScrolled()
        {
            var result = false;
            var viewer = main.imageGrid_ScrollViewer;
            var scrollableHeight = viewer.ScrollableHeight;
            //if scrolled to bottom, or there is no space to scroll
            var margin = 0.25 * viewer.ViewportHeight;
            result = (Math.Abs(viewer.VerticalOffset + viewer.ViewportHeight - viewer.ExtentHeight) < margin) || scrollableHeight < margin;
            if (result) Debug.WriteLine("At the bottom of the list!");
            return result;
        }

        public void RequestMoreImages()
        {
            if(Images.Count() >= maxLoadedImages)
            {
                main.loadNextPageButton.IsEnabled = true;
                main.loadNextPageButton.Visibility = Visibility.Visible;
            }
            else
            {
                LoadNextChunk();
            }
        }
        private void LoadNextChunk()
        {
            var startIndex = currentImageOffset + (currentPage * imagesPerChunk);
            var loadUpTo = startIndex + imagesPerChunk;
            loadUpTo = Math.Min(loadUpTo, ImageFiles.Count);
            loadUpTo = Math.Min(loadUpTo, startIndex + imagesPerChunk );
            
            for (int i = startIndex; i < loadUpTo; i++)
            {
                var newSquare = new ImageInfo(ImageFiles.Get(i));
                Images.Add(newSquare);
                newSquare.Load();
            }
            currentPage++;

            var selectedIndex = ImageGrid.SelectedIndex;
            if (selectedIndex == -1 && ImageGrid.Items.Count > 0) selectedIndex = 0;
            ImageGrid.SelectedIndex = selectedIndex;
            if (selectedIndex != -1)
            {
                ListBoxItem myListBoxItem = (ListBoxItem)(ImageGrid.ItemContainerGenerator.ContainerFromItem(ImageGrid.Items[selectedIndex]));
                if(myListBoxItem != null) myListBoxItem.Focus();
            }
        }

        private void LoadNextPage()
        {
            main.loadNextPageButton.IsEnabled = false;
            main.loadNextPageButton.Visibility = Visibility.Collapsed;
            currentImageOffset = currentImageOffset + Images.Count();
            currentPage = 0;
            Images.Clear();
            GC.Collect();
            LoadNextChunk();
        }
    }
}
