
using ImageTagger.DataModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace ImageTagger
{

    public class MainImageDisplay
    {
        ViewSearchWindow main { get; }
        public Image ImageDisplay { get { return main.mainImageDisplay; } }
        public ImageInfo mainImageInfo { get;} = new ImageInfo();

        public MainImageDisplay(ViewSearchWindow main)
        {
            this.main = main;
            //ImageDisplay.DataContext = mainImageInfo;
            main.mainImagePanel.DataContext = mainImageInfo;

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
                var newImageInfo = (e.AddedItems.Last() as ImageInfo);
                //Debug.WriteLine("selected: " + newImageInfo.ImgPath);
                //if (!newImageInfo.IsLoaded) newImageInfo.Load();// App.Current.Dispatcher.BeginInvoke( new Action( newImageInfo.Load), DispatcherPriority.Loaded);
                ChangeImage(newImageInfo);
            }
            else if(main.imageGrid.SelectedItems.Count > 0)
            {
                var newImageInfo = (main.imageGrid.SelectedItems.Last() as ImageInfo);
                ChangeImage(newImageInfo);
            }

            if(main.imageGrid.SelectedItems.Count > 1)
                main.multipleImagesIndicator.Visibility = Visibility.Visible;
            else
                main.multipleImagesIndicator.Visibility = Visibility.Hidden;
        }

        private void ChangeImage(ImageInfo newInfo)
        {
            mainImageInfo.CloneFrom(newInfo, -1, DispatcherPriority.Send);
            //ImageDisplay.Source = mainImageInfo;
        }
    }
}