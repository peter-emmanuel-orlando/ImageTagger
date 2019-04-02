using ImageTagger_DataModels;
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
                mainImageTags.Add(main.GetImageTags(value.ImgPath));
            }
        }
        private readonly ImageTag NoTagsPlaceholder = new ImageTag("[no tags yet...]");

        public ImageTagsDisplay(MainWindow main)
        {
            this.main = main;
            TagsDisplay.ItemsSource = mainImageTags;
            TagsDisplay.KeyDown += TagsDisplay_KeyDown;
            TagsDisplay.LostFocus += TagsDisplay_LostFocus;
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
            ImageFileUtil.ApplyTagsToImage(ImageDisplay.mainImageInfo, Tags);
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
