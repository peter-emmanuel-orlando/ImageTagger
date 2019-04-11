
using ImageTagger.DataModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;

namespace ImageTagger
{

    public class MainImageDisplay
    {
        MainWindow main { get; }
        public Image ImageDisplay { get { return main.mainImageDisplay; } }
        public ImageInfo mainImageInfo { get; private set; } = new ImageInfo();

        public MainImageDisplay(MainWindow main)
        {
            this.main = main;
            ImageDisplay.Source = mainImageInfo;

            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;
            main.imageGrid.SelectionChanged += HandleImageGridSelectionChangeEvent;
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            main.imageGrid.SelectionChanged -= HandleImageGridSelectionChangeEvent;
            // unsubscribe from anything else here
        }

        private void HandleImageGridSelectionChangeEvent(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var newImageInfo = (e.AddedItems[0] as ImageInfo);
                //Debug.WriteLine("selected: " + newImageInfo.ImgPath);

                ChangeImage(newImageInfo);
            }
        }

        private void ChangeImage(ImageInfo newInfo)
        {
            mainImageInfo = newInfo;
            ImageDisplay.Source = mainImageInfo;
        }
    }
}