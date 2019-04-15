
using ImageTagger.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageTagger
{
    public class ImageTagsDisplay
    {
        //need to make rules for tags
        MainWindow main { get; }
        public ListBox TagsDisplay { get { return main.tagsDisplay; } }
        public MainImageDisplay ImageDisplay { get { return main.ImageDisplay; } }

        private ObservableCollection<ImageTag> mainImageTags { get; } = new ObservableCollection<ImageTag>();
        public IEnumerable<ImageTag> Tags { get { return mainImageTags; } }
        public ImageInfo TagSource { get; private set; }

        public ImageTagsDisplay(MainWindow main)
        {
            this.main = main;
            TagsDisplay.ItemsSource = mainImageTags;
            TagsDisplay.KeyDown += TagsDisplay_KeyDown;
            TagsDisplay.LostFocus += TagsDisplay_LostFocus;
            mainImageTags.CollectionChanged += HandleCollectionChanged;
            main.imageGrid.SelectionChanged += HandleImageGridSelectionChangeEvent;

            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
            TagsDisplay.KeyDown -= TagsDisplay_KeyDown;
            TagsDisplay.LostFocus -= TagsDisplay_LostFocus;
            mainImageTags.CollectionChanged -= HandleCollectionChanged;
            main.imageGrid.SelectionChanged -= HandleImageGridSelectionChangeEvent;
        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (mainImageTags.Count == 0)
                main.noTagsMessage.Opacity = 1;
            else
                main.noTagsMessage.Opacity = 0;
        }

        private void TagsDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyTagsToMainImage();
            }
        }

        private void HandleImageGridSelectionChangeEvent(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var newImageInfo = (e.AddedItems[0] as ImageInfo);
                //Debug.WriteLine("selected: " + newImageInfo.ImgPath);

                ChangeImageTags(newImageInfo);
            }
        }

        private void ChangeImageTags(ImageInfo newInfo)
        {
            TagSource = newInfo;
            mainImageTags.Clear();
            mainImageTags.Add(ImageFileUtil.GetImageTags(newInfo.ImgPath));
        }

        private void TagsDisplay_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplyTagsToMainImage();
        }

        public void ApplyTagsToMainImage()
        {
            var success = ImageFileUtil.ApplyTagsToImage(ImageDisplay.mainImageInfo, Tags);
            if(mainImageTags.Count > 0 && success && PersistanceUtil.DestinationDirectory != PersistanceUtil.SourceDirectory)
            {
                main.ImageFiles.MoveToDestination(ImageDisplay.mainImageInfo, PersistanceUtil.DestinationDirectory);
            }
        }

        public void Add(IEnumerable<ImageTag> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
        public bool Add(ImageTag item)
        {
            var success = false;
            if (!item.IsEmpty() && !mainImageTags.Contains(item))
            {
                success = true;
                mainImageTags.Add(item);
            }
            return success;
        }
        
        public bool Contains(string tagName)
        {
            return Contains(new ImageTag(tagName));
        }

        public bool Contains(ImageTag tag)
        {
            return mainImageTags.Contains(tag);
        }

        public void Remove(IEnumerable<ImageTag> items)
        {
            foreach (var item in items)
            {
                Remove(item);
            }
        }
        public void Remove(string tagName)
        {
            Remove(new ImageTag(tagName));
        }
        public void Remove(ImageTag tag)
        {
            mainImageTags.Remove(tag);
        }

        public void Clear()
        {
            mainImageTags.Clear();
        }

        public void Refresh()
        {
            TagSource = TagSource;
        }

    }

    public partial class MainWindow
    {
        public void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            string tagName = (e.OriginalSource as CheckBox).Content + "";
            Debug.WriteLine(tagName);
            ImageTagsDisplay.Remove(tagName);
        }
    }
}
