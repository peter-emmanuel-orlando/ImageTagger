using ImageTagger_Model;

namespace ImageTagger
{

    public partial class MainWindow
    {
        private ImageInfo mainImageInfo{get; set;}
        private void InitializeMainImageDisplay()
        {
            mainImageDisplay.Source = mainImageInfo;

        }
    }
}
