using ImageTagger.DataModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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


        private bool isPlaying = true;
        private bool resetTimer = false;
        private int imgDelay = 2000;
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
            currentIndex = startIndex - 2;//because textchanged will be called
            mainSlideshowImageDisplay.DataContext = mainImageInfo;
            slideshowSpeed.Text = imgDelay + "";
            Play();
        }

        private void Play()
        {
            Task.Run(new Action(async () =>
            {
                while(isPlaying)
                {
                    await App.Current.Dispatcher.InvokeAsync(new Action(() =>
                    {
                        ChangeImageIndex();
                    }));
                    resetTimer = true;
                    while (resetTimer)
                    {
                        resetTimer = false;
                        Thread.Sleep(imgDelay);
                    }
                }
            }));
        }

        private void Stop()
        {
            isPlaying = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Stop();
        }

        private void ChangeImageIndex(int deltaIndex = 1)
        {
            if(ImageFiles.Count > 0)
            {
                currentIndex = (currentIndex + deltaIndex) % ImageFiles.Count;
                while (currentIndex < 0)
                {
                    currentIndex = ImageFiles.Count + currentIndex;
                }
                mainImageInfo.ImgPath = ImageFiles.Get(currentIndex);
                mainImageInfo.Load(-1, DispatcherPriority.Send);
                //ImageDisplay.Source = mainImageInfo;
            }
        }

        private void HandleMouseOverMenuPanel(object sender, MouseEventArgs e)
        {
            menu.Visibility = Visibility.Visible;
        }

        private void HandleMouseOffMenuPanel(object sender, MouseEventArgs e)
        {
            menu.Visibility = Visibility.Hidden;
        }

        private void SlideshowSpeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(slideshowSpeed.Text, out float newDelay))
            {
                imgDelay = (int)newDelay;
                if (imgDelay < 500)
                    imgDelay = 500;
                ChangeImageIndex();
            }
            slideshowSpeed.Text = imgDelay + "";
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                ChangeImageIndex();
                e.Handled = true;
                resetTimer = true;
            }
            else if (e.Key == Key.Left)
            {
                ChangeImageIndex(-1);
                e.Handled = true;
                resetTimer = true;
            }
        }
    }
}
