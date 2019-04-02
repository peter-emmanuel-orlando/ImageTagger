using ImageTagger_DataModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ImageTagger
{

    public class ImageTagsDisplay
    {

        MainWindow main { get; }
        ListBox TagsDisplay
        {
            get
            {
                return main.tagsDisplay;
            }
        }

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

        public ImageTagsDisplay(MainWindow main)
        {
            this.main = main;
            TagsDisplay.ItemsSource = mainImageTags;
        }

        public void Add(ImageTag item)
        {
            mainImageTags.Add(item);
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
