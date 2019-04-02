using ImageTagger_DataModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Debug = System.Diagnostics.Debug;

namespace ImageTagger
{

    public class ImageGridDisplay
    {
        MainWindow main { get; }
        public ObservableCollection<ImageInfo> Images { get; } = new ObservableCollection<ImageInfo>();
        public ListBox ImageGrid
        {
            get
            {
                return main.imageGrid;
            }
        }

        public ImageGridDisplay(MainWindow main)
        {
            this.main = main;
            ImageGrid.ItemsSource = Images;
            ImageGrid.SelectionChanged += ImgGrid_SelectionChanged;
            main.imageGrid_ScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            ImageGrid.Loaded += HandleGridLoaded;
            Initialize();
        }

        private void Initialize()
        {

            int count = 0;
            int max = 25;
            Images.Clear();
            foreach (var filename in main.ImageFileNames)
            {
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
            }
        }

        private void HandleGridLoaded(object sender, RoutedEventArgs e)
        {
            if (ImageGrid.Items.Count > 0)
            {
                ImageGrid.SelectedIndex = 0;
                ListBoxItem myListBoxItem = (ListBoxItem)(ImageGrid.ItemContainerGenerator.ContainerFromItem(ImageGrid.Items[0]));
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
                    LoadMoreImages();
                }
            }
        }

        private void LoadMoreImages()
        {
            if (Images.Count() < main.ImageFileNames.Count())
            {
                var loadedCount = Images.Count();
                var fileNameCount = main.ImageFileNames.Count();
                for (int i = loadedCount; i < Math.Min(10 + loadedCount, fileNameCount); i++)
                {
                    try
                    {
                        //Debug.WriteLine("trying to add file to list:" + imageFileNames[i]);
                        var newSquare = new ImageInfo(main.ImageFileNames[i]);
                        Images.Add(newSquare);
                    }
                    catch (Exception) { }
                }
                var selectedIndex = ImageGrid.SelectedIndex;
                //this is a kinda janky workaround. what i need to do is handle arrow keypresses and move focus manually
                ListBoxItem myListBoxItem = (ListBoxItem)(ImageGrid.ItemContainerGenerator.ContainerFromItem(ImageGrid.Items[selectedIndex]));
                //imgGrid.ScrollIntoView(myListBoxItem);
                myListBoxItem.Focus();

                myListBoxItem.KeyDown += LastImage_KeyDown;
                return;
            }
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
