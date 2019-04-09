
using ImageTagger.DataModels;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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

        public ImageGridDisplay(MainWindow main)
        {
            this.main = main;
            ImageGrid.ItemsSource = Images;
            ImageGrid.SelectionChanged += ImgGrid_SelectionChanged;
            main.imageGrid_ScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            ImageGrid.Loaded += HandleGridLoaded;
            ImageFiles.FilesLoaded += HandleFilesLoaded;
            ImageFiles.ItemChanged += HandleItemChanged;
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
        }

        public void Initialize()
        {
            int count = 0;
            int max = 25;
            Images.Clear();
            var e = ImageFiles.GetEnumerator();
            while( e.MoveNext())
            {
                var filename = e.Current;
                Debug.WriteLine(filename);
                try
                {
                    Images.Add(new ImageInfo(filename));
                    count++;
                }
                catch (System.NotSupportedException)
                {
                    System.Diagnostics.Debug.WriteLine("was not able to use file: " + filename);
                }
                if (count >= max) break;
            }
            if (count > 0)
            {
                ImageGrid.SelectedIndex = 0;
                HandleGridLoaded(null, null);
            }
        }

        private void SetTagFilter()
        {
            // any of these tags match < coded blue
            // AND
            // all of these tags match < coded green
            // AND
            // none of these tags match < coded orange
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
                var newImageInfo = (e.AddedItems[0] as ImageInfo);
                Debug.WriteLine("selected: " + newImageInfo.ImgPath);

                main.ImageDisplay.ChangeImage(newImageInfo);

                main.ImageTagsDisplay.TagSource = newImageInfo;
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
            LoadMoreImages();
        }

        private bool IsFullyScrolled()
        {
            var result = false;
            var viewer = main.imageGrid_ScrollViewer;
            var scrollableHeight = viewer.ScrollableHeight;
            //if scrolled to bottom, or there is no space to scroll
            var margin = 0.25 * viewer.ViewportHeight;
            result = (Math.Abs(viewer.VerticalOffset + viewer.ViewportHeight - viewer.ExtentHeight) < margin) || scrollableHeight < margin;
            if(result) Debug.WriteLine("At the bottom of the list!");
            return result;
        }


        private void LoadMoreImages()
        {
            if (Images.Count() < ImageFiles.Count)
            {
                var loadedCount = Images.Count();
                var fileNameCount = ImageFiles.Count;
                if (loadedCount < fileNameCount && IsFullyScrolled())
                {
                    for (int i = loadedCount; i < Math.Min(1 + loadedCount, fileNameCount); i++)
                    {
                        try
                        {
                            //Debug.WriteLine("trying to add file to list:" + imageFileNames[i]);
                            var newSquare = new ImageInfo(ImageFiles.Get(loadedCount + i));
                            Images.Add(newSquare);
                            ManageMemory();
                        }
                        catch (Exception e) { Debug.WriteLine(e); }
                        //wait for UI thread to complete in order to get accurate results
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                        {
                            LoadMoreImages();
                        }));
                    }
                }
                //calculate if scrolled to bottom

                var selectedIndex = ImageGrid.SelectedIndex;
                ListBoxItem myListBoxItem = (ListBoxItem)(ImageGrid.ItemContainerGenerator.ContainerFromItem(ImageGrid.Items[selectedIndex]));
                myListBoxItem.Focus();

                //myListBoxItem.KeyDown += LastImage_KeyDown;
            }
        }

        int loadStartIndex = 0;
        int loadEndIndex = 0;
        const int imageSizeMin = 120;
        const int imageSizeMax = 1000;
        int imageSizeCurrent = imageSizeMin;

        private void ManageMemory()
        {
            var viewer = main.imageGrid_ScrollViewer;
            var picsInColumn = (int)Math.Floor(viewer.ActualHeight / imageSizeCurrent);
            var picsInRow = (int)Math.Floor(viewer.ActualWidth / imageSizeCurrent);
            var desiredLoadCount = picsInColumn * picsInRow;
            var desiredStartIndex = (int)(picsInRow * (main.imageGrid_ScrollViewer.VerticalOffset / imageSizeCurrent));

            Images[loadStartIndex].Load();
            loadStartIndex++;
        }

        private void LastImage_KeyDown(object sender, KeyEventArgs e)
        {
            //var container = e.OriginalSource as ListBoxItem;
            //container.Focus();
            //e.Handled = true;
            //var item = imgGrid.ItemContainerGenerator.ItemFromContainer(container);
            //var index = imgGrid.Items.IndexOf(item);
            //imgGrid.SelectedIndex = index + 1;
            //Debug.WriteLine(index);
        }
    }
}
