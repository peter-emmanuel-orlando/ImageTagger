using ImageTagger_Model;
using System.Collections.ObjectModel;

namespace ImageTagger
{

    public partial class MainWindow
    {
        private ObservableCollection<ImageTag> mainImageTags { get; } = new ObservableCollection<ImageTag>();
        private void InitializeImageTagsDisplay()
        {
            tagsDisplay.ItemsSource = mainImageTags;
        }
    }
}