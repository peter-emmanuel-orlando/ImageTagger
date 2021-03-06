﻿
using ImageTagger.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Debug = System.Diagnostics.Debug;

namespace ImageTagger
{
    public class ImageGridDisplay
    {
        ViewSearchWindow main { get; }
        private ObservableCollection<ImageInfo> Images { get; } = new ObservableCollection<ImageInfo>();
        private ListBox ImageGrid { get { return main.imageGrid; } }

        private int maxLoadedSquares = 800;
        private int currentImageOffset = 0;
        private int currentChunk = 0;
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
            main.loadPrevPageButton.Click += HandleLoadPrevPageButtonClick;
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
            currentChunk = 0;
            RequestMoreImages();
        }
        public void SetImage(int imgIndex)
        {
            if (imgIndex > main.ImageFiles.Count)
                imgIndex = main.ImageFiles.Count;
            if (currentImageOffset > imgIndex)
            {
                currentImageOffset = (int)Math.Floor((float)imgIndex / maxLoadedSquares) * maxLoadedSquares;
                currentChunk = 0;
                foreach (var image in Images)
                    image.Unload();
                Images.Clear();
                GC.Collect();
            }
            var index = imgIndex - currentImageOffset;
            var numChunks = (int)Math.Ceiling((float)index / imagesPerChunk);
            for (int i = currentChunk; i < numChunks; i++)
            {
                LoadNextChunk();
            }
            ImageGrid.SelectedIndex = index;
            ListBoxItem myListBoxItem = (ListBoxItem)(ImageGrid.ItemContainerGenerator.ContainerFromItem(ImageGrid.Items[index]));
            if (myListBoxItem != null)
            {
                myListBoxItem.Focus();
                Keyboard.Focus(myListBoxItem);
                myListBoxItem.BringIntoView();
                //main.imageGrid_ScrollViewer.sc.ScrollIntoView(myListBoxItem);
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

        private void HandleLoadPrevPageButtonClick(object sender, RoutedEventArgs e)
        {
            LoadPrevPage();
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
                var newImageInfo = (e.AddedItems.Last() as ImageInfo);
                //newImageInfo.Load();
                //Debug.WriteLine("selected: " + newImageInfo.ImgPath);
                //main.ImageDisplay.ChangeImage(newImageInfo);
                //main.ImageTagsDisplay.TagSource = newImageInfo;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (IsFullyScrolled())
            {
                RequestMoreImages();
            }
            LoadVisible();
            //LoadOnScreen();
        }

        int maxLoadedImages = 333;
        private void LoadOnScreen()
        {
            if (ImageGrid.Items.Count == 0) return;
            var viewer = main.imageGrid_ScrollViewer;
            var itemSize = desiredThumbnailSize + 22;//padding and margin
            var viewerWidth = viewer.ViewportWidth - 8;//scrollbar
            var viewerHeight = viewer.ViewportHeight;
            var columns = Math.Floor(viewerWidth / itemSize);
            var prevRows = Math.Floor(viewer.ContentVerticalOffset / itemSize);
            var visibleRows = Math.Ceiling(viewerHeight / itemSize);
            var visibleItemsStartIndex = (int)(prevRows * columns).Clamp(0, Images.Count - 1);
            var visibleItemsEndIndex = (int)(visibleItemsStartIndex + (visibleRows * columns)).Clamp(0, Images.Count - 1);

            var margin = (int)Math.Ceiling(visibleRows * columns / 2f);
            //visibleItemsStartIndex -= margin;
            //visibleItemsEndIndex += margin;


            var loadedImages = Images.Count(i => i.IsLoaded);
            /*
            for (int i = 0; i < (int)Math.Floor(Images.Count/2f); i++)
            {
                var offset = (int)Math.Ceiling(i / 2f);
                var index = (i % 2 == 0) ? Images.Count - 1 - offset : offset;
                if (loadedImages <= maxLoadedImages)
                    break;
                else if (index >= visibleItemsStartIndex && index <= visibleItemsEndIndex)
                    continue;
                else if (Images[index].IsLoaded)
                {
                    Images[index].Unload();
                    loadedImages--;
                }
            }
            */
            var mem = Process.GetCurrentProcess().PrivateMemorySize64;// GC.GetTotalMemory(false);
            if (mem > 700000000 && loadedImages > maxLoadedImages)//0.7gb
            {
                const int unloadedRez = 15;
                for (int i = 0; i < visibleItemsStartIndex - margin; i++)
                {
                    //Images[i].Unload();
                    if (Images[i].PixelDimensions != unloadedRez)
                        Images[i].RequestLoad(unloadedRez);
                }
                for (int i = visibleItemsEndIndex + 1 + margin; i < Images.Count; i++)
                {
                    //Images[i].Unload();
                    if (Images[i].PixelDimensions != unloadedRez)
                        Images[i].RequestLoad(unloadedRez);
                }
                if (mem > 900000000)
                    GC.Collect();
            }
            //'request load' is a stack data structure, meaning lifo
            //last load preceeding margin
            for (int i = visibleItemsStartIndex; i >= (visibleItemsStartIndex - margin).Clamp(0, Images.Count - 1); i--)
            {
                if (true)//!Images[i].IsLoaded)
                {
                    Images[i].RequestLoad(desiredThumbnailSize + 250);
                }
            }
            //then load proceeding margin
            for (int i = (visibleItemsEndIndex + margin).Clamp(0, Images.Count - 1); i >= visibleItemsEndIndex; i--)
            {
                if (true)//!Images[i].IsLoaded)
                {
                    Images[i].RequestLoad(desiredThumbnailSize + 250);
                }
            }
            //first visible images
            for (int i = visibleItemsEndIndex; i >= visibleItemsStartIndex; i--)
            {
                if (true)//!Images[i].IsLoaded)
                {
                    Images[i].RequestLoad(desiredThumbnailSize + 250);
                }
            }
        }

        private static Process process {get;} = Process.GetCurrentProcess();
        private void LoadVisible()
        {
            var loadedImages = Images.Count(image => image.IsLoaded);
            long mem = process.PrivateMemorySize64;
            var triggerGC = false;
            for (int i = Images.Count - 1; i >= 0 ; i--)
            {
                var item = ImageGrid.Items[i] as ImageInfo;
                var sViewer = main.imageGrid_ScrollViewer;
                var gridItem = (ListBoxItem)ImageGrid.ItemContainerGenerator.ContainerFromItem(item);
                if (gridItem != null)
                {
                    GeneralTransform childTransform = gridItem.TransformToAncestor(sViewer);
                    Rect rectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), gridItem.RenderSize));
                    //Check if the elements Rect intersects with that of the scrollviewer's
                    Rect result = Rect.Intersect(new Rect(new Point(0, 0), sViewer.RenderSize), rectangle);
                    //if result is Empty then the element is not in view
                    if (result == Rect.Empty)
                    {
                        //not visible
                        if (item.IsLoaded && (mem > 700000000 ))//|| loadedImages > maxLoadedImages))//0.7gb
                        {
                            item.Unload();
                            //loadedImages--;
                            triggerGC = true;
                        }
                    }
                    else
                    {
                        //obj is partially Or completely visible
                        if (!item.IsLoaded || item.PixelDimensions != desiredThumbnailSize + 250)
                        {
                            item.RequestLoad(desiredThumbnailSize + 250);
                        }
                    }
                }
            }
            if (triggerGC) GC.Collect();
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
            if (Images.Count() >= maxLoadedSquares)
            {
                //HandleLoadNextPageButtonClick(null, null);
                //main.loadNextPageButton.IsEnabled = true;
                //main.loadNextPageButton.Visibility = Visibility.Visible;
                //Keyboard.Focus(main.loadNextPageButton);
            }
            else
            {
                LoadNextChunk();
            }
        }
        private void LoadNextChunk()
        {
            var startIndex = currentImageOffset + (currentChunk * imagesPerChunk);
            var loadUpTo = startIndex + imagesPerChunk;
            loadUpTo = Math.Min(loadUpTo, main.ImageFiles.Count);
            loadUpTo = Math.Min(loadUpTo, startIndex + imagesPerChunk);

            //for (int i = loadUpTo-1; i >= startIndex; i--)
            for (int i = startIndex; i < loadUpTo; i++)
            {
                var newSquare = new ImageInfo(main.ImageFiles.Get(i));
                Images.Add(newSquare);
                newSquare.Load(desiredThumbnailSize + 250);
            }
            currentChunk++;

            var selectedIndex = ImageGrid.SelectedIndex;
            if (selectedIndex == -1 && ImageGrid.Items.Count > 0) selectedIndex = 0;
            ImageGrid.SelectedIndex = selectedIndex;
            if (selectedIndex != -1)
            {
                ListBoxItem myListBoxItem = (ListBoxItem)(ImageGrid.ItemContainerGenerator.ContainerFromItem(ImageGrid.Items[selectedIndex]));
                if (myListBoxItem != null) myListBoxItem.Focus();// ImageGrid.ScrollIntoView(myListBoxItem);// 
            }
        }


        private void LoadPrevPage()
        {
            if (currentImageOffset != 0)
            {
                currentImageOffset -= maxLoadedSquares;
                if (currentImageOffset < 0) currentImageOffset = 0;
                currentChunk = 0;
                foreach (var image in Images)
                    image.Unload();
                Images.Clear();
                GC.Collect();
                LoadNextChunk();
            }
        }
        private void LoadNextPage()
        {
            //main.loadNextPageButton.IsEnabled = false;
            //main.loadNextPageButton.Visibility = Visibility.Collapsed;
            currentImageOffset = currentImageOffset + Images.Count();
            currentChunk = 0;
            foreach (var image in Images)
                image.Unload();
            Images.Clear();
            GC.Collect();
            LoadNextChunk();
        }
    }
}
