﻿using ImageTagger.DataModels;
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


        private bool isPaused = false;
        private bool isEnabled = true;
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
                while(isEnabled)
                {
                    await App.Current.Dispatcher.InvokeAsync(new Action(() =>
                    {
                        ChangeImageIndex();
                    }));
                    resetTimer = true;
                    int timer = 0;
                    while (resetTimer || isPaused || timer < imgDelay)
                    {
                        if(resetTimer)
                        {
                            resetTimer = false;
                            timer = 0;
                        }
                        Thread.Sleep(1);
                        timer++;
                    }
                }
            }));
        }

        private void Pause( bool isPaused)
        {
            this.isPaused = isPaused;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            isEnabled = false;
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
            if (e.Key == Key.Right || e.Key == Key.L)
            {
                ChangeImageIndex();
                e.Handled = true;
                resetTimer = true;
            }
            else if (e.Key == Key.Left || e.Key == Key.J)
            {
                ChangeImageIndex(-1);
                e.Handled = true;
                resetTimer = true;
            }
            else if (e.Key == Key.K)
            {
                e.Handled = true;
                resetTimer = true;
                Pause(!isPaused);
            }
            else if (e.Key == Key.Escape)
            {
                this.Close();
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
        private void HandleMouseOverControlPanel(object sender, MouseEventArgs e)
        {
            controlPanel.Visibility = Visibility.Visible;
        }

        private void HandleMouseOffControlPanel(object sender, MouseEventArgs e)
        {
            controlPanel.Visibility = Visibility.Hidden;
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeImageIndex(-1);
        }
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Pause(!isPaused);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeImageIndex();
        }
    }
}