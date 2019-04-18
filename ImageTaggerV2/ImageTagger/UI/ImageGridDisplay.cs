﻿
using ImageTagger.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.OleDb;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Debug = System.Diagnostics.Debug;

namespace ImageTagger
{
    public class ImageGridDisplay
    {
        ViewSearchWindow main { get; }
        private ObservableCollection<ImageInfo> Images { get; } = new ObservableCollection<ImageInfo>();
        private ListBox ImageGrid { get { return main.imageGrid; } }

        private int maxLoadedImages = 40;
        private int currentImageOffset = 0;
        private int currentPage = 0;
        private int imagesPerChunk = 25;

        private const int minThumbnailSize = 100;
        private const int maxThumbnailSize = 1000;
        private int desiredThumbnailSize = minThumbnailSize;

        public ImageGridDisplay(ViewSearchWindow main)
        {
            this.main = main;
            ImageGrid.ItemsSource = Images;
            ImageGrid.SelectionChanged += ImgGrid_SelectionChanged;
            main.imageGrid_ScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            ImageGrid.Loaded += HandleGridLoaded;
            main.ImageFiles.FilesLoaded += HandleFilesLoaded;
            //ImageFiles.ItemChanged += HandleItemChanged;
            main.loadNextPageButton.Click += HandleLoadNextPageButtonClick;
            main.changeThumbnailSizeSlider.ValueChanged += HandleThumbnailSizeSliderChangeClick;
            Initialize();

            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;
        }
        
        private void HandleThumbnailSizeSliderChangeClick(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider s = e.OriginalSource as Slider;
            var newVal = (int)(minThumbnailSize + (maxThumbnailSize - minThumbnailSize) * (s.Value / 100));
            desiredThumbnailSize = newVal;
            foreach (var image in Images)
            {
                image.DesiredDimensions = newVal;
            }
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
            ImageGrid.SelectionChanged -= ImgGrid_SelectionChanged;
            main.imageGrid_ScrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            ImageGrid.Loaded -= HandleGridLoaded;
            main.ImageFiles.FilesLoaded -= HandleFilesLoaded;
            //ImageFiles.ItemChanged -= HandleItemChanged;
            main.loadNextPageButton.Click -= HandleLoadNextPageButtonClick;
        }

        public void Initialize()
        {
            foreach (var image in Images)
            {
                image.Unload();
            }
            Images.Clear();
            GC.Collect();
            currentImageOffset = 0;
            currentPage = 0;
            RequestMoreImages();
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
                ImageGrid.ScrollIntoView(myListBoxItem);
                myListBoxItem.Focus();
            }
        }

        private void ImgGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var newImageInfo = (e.AddedItems[0] as ImageInfo);
                newImageInfo.Load();
                //Debug.WriteLine("selected: " + newImageInfo.ImgPath);
                //main.ImageDisplay.ChangeImage(newImageInfo);
                //main.ImageTagsDisplay.TagSource = newImageInfo;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
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
            loadUpTo = Math.Min(loadUpTo, main.ImageFiles.Count);
            loadUpTo = Math.Min(loadUpTo, startIndex + imagesPerChunk );
            
            for (int i = startIndex; i < loadUpTo; i++)
            {
                var newSquare = new ImageInfo(main.ImageFiles.Get(i));
                Images.Add(newSquare);
                newSquare.Load(desiredThumbnailSize + 250);
            }
            currentPage++;

            var selectedIndex = ImageGrid.SelectedIndex;
            if (selectedIndex == -1 && ImageGrid.Items.Count > 0) selectedIndex = 0;
            ImageGrid.SelectedIndex = selectedIndex;
            if (selectedIndex != -1)
            {
                ListBoxItem myListBoxItem = (ListBoxItem)(ImageGrid.ItemContainerGenerator.ContainerFromItem(ImageGrid.Items[selectedIndex]));
                if (myListBoxItem != null) myListBoxItem.Focus();// ImageGrid.ScrollIntoView(myListBoxItem);// 
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
