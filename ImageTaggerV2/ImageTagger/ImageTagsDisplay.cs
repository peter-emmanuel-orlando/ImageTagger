using ImageTagger_DataModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<ImageTag> mainImageTags { get; } = new ObservableCollection<ImageTag>();
        public ImageTagsDisplay(MainWindow main)
        {
            this.main = main;
            TagsDisplay.ItemsSource = mainImageTags;
        }
    }

    public partial class MainWindow
    {
        public void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            string tagName = (e.OriginalSource as CheckBox).Content + "";
            Debug.WriteLine(tagName);
            ImageTagsDisplay.mainImageTags.Remove(new ImageTag(tagName));
        }
    }
}
