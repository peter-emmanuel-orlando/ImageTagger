using ImageTagger_DataModels;
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

        MainWindow main { get; }
        public ListBox TagsDisplay { get { return main.tagsDisplay; } }
        public MainImageDisplay ImageDisplay { get { return main.ImageDisplay; } }

        private ObservableCollection<ImageTag> mainImageTags { get; } = new ObservableCollection<ImageTag>();
        public IEnumerator<ImageTag> Tags { get { return mainImageTags.GetEnumerator(); } }
        private ImageInfo tagSource;
        public ImageInfo TagSource
        {
            get
            {
                return tagSource;
            }
            set
            {
                tagSource = value;
                mainImageTags.Clear();
                mainImageTags.Add(ImageFileUtil.GetImageTags(value.ImgPath));
            }
        }

        public ImageTagsDisplay(MainWindow main)
        {
            this.main = main;
            TagsDisplay.ItemsSource = mainImageTags;
            TagsDisplay.KeyDown += TagsDisplay_KeyDown;
            TagsDisplay.LostFocus += TagsDisplay_LostFocus;
            mainImageTags.CollectionChanged += HandleCollectionChanged;

            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
            TagsDisplay.KeyDown -= TagsDisplay_KeyDown;
            TagsDisplay.LostFocus -= TagsDisplay_LostFocus;
            mainImageTags.CollectionChanged -= HandleCollectionChanged;
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

        private void TagsDisplay_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplyTagsToMainImage();
        }

        public void ApplyTagsToMainImage()
        {
            var success = ImageFileUtil.ApplyTagsToImage(ImageDisplay.mainImageInfo, Tags);
            if(mainImageTags.Count > 0 && success && main.destinationDirectory != main.sourceDirectory)
            {
                MoveToDestination(ImageDisplay.mainImageInfo, main.destinationDirectory);
            }
        }

        private bool MoveToDestination(ImageInfo imgInfo, string newDirectory)
        {
            string newPath = "";
            var success = ImageFileUtil.MoveImageFile(imgInfo.ImgPath, newDirectory, out newPath);
            if (success)
            {
                //change file path to new filepath
                var fileNameIndex = main.ImageFileNames.IndexOf(imgInfo.ImgPath);
                if(fileNameIndex != -1) main.ImageFileNames[fileNameIndex] = newPath;
                //change imageInfo path to new filepath
                imgInfo.ImgPath = newPath;
                //call changeimage to trigger reload
                ImageDisplay.ChangeImage(ImageDisplay.mainImageInfo);
            }
            return success;
        }

        public bool Add(ImageTag item)
        {
            var success = false;
            if( !item.IsEmpty() && !mainImageTags.Contains(item))
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

        public void Remove(string tagName)
        {
            mainImageTags.Remove(new ImageTag(tagName));
        }

        public void Clear()
        {
            mainImageTags.Clear();
        }

        public void Refresh()
        {
            TagSource = tagSource;
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
