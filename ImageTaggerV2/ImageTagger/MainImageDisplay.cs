using ImageTagger_DataModels;
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
        public Image ImageDisplay
        {
            get
            {
                return main.mainImageDisplay;
            }
        }
        private ImageInfo mainImageInfo { get; set; } = new ImageInfo(@"C:\Users\YumeMura\Desktop\unsamples\1zBMSv4xEA4.jpg");

        public MainImageDisplay(MainWindow main)
        {
            this.main = main;
            ImageDisplay.Source = mainImageInfo;
        }

        public void ChangeImage(ImageInfo newInfo)
        {
            mainImageInfo = newInfo;
            ImageDisplay.Source = mainImageInfo;
        }
    }
}