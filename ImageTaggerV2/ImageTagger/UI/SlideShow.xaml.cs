using ImageTagger.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ImageTagger.UI
{
    /// <summary>
    /// Interaction logic for SlideShow.xaml
    /// </summary>
    public partial class SlideShow : Window
    {
        private ImageFiles ImageFiles { get; }
        private ImageInfo mainImageInfo { get; } = new ImageInfo();
        private int currentIndex { get; set; } = 0;
        public static int PlaySlideshow(ImageFiles imageFiles, int startIndex = 0)
        {
            var slideShow = new SlideShow(imageFiles, startIndex);
            slideShow.ShowDialog();
            return slideShow.currentIndex;
        }
        private SlideShow(ImageFiles imageFiles, int startIndex)
        {
            InitializeComponent();
            ImageFiles = imageFiles;
            currentIndex = startIndex - 1;
            mainSlideshowImageDisplay.DataContext = mainImageInfo;
            Play();
        }

        private void Play()
        {
            Task.Run(new Action(async () =>
            {
                while(true)
                {
                    await App.Current.Dispatcher.InvokeAsync(new Action(() =>
                    {
                        NextImage();
                    }));
                    Thread.Sleep(2000);
                }
            }));
        }

        private void NextImage()
        {
            if(ImageFiles.Count > 0)
            {
                currentIndex = (currentIndex +1) % ImageFiles.Count;
                mainImageInfo.ImgPath = ImageFiles.Get(currentIndex);
                mainImageInfo.Load(-1, DispatcherPriority.Send);
                //ImageDisplay.Source = mainImageInfo;
            }
        }
    }
}
