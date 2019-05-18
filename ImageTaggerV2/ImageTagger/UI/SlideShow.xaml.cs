using ImageTagger.DataModels;
using System;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;

namespace ImageTagger.UI
{
    /// <summary>
    /// Interaction logic for SlideShow.xaml
    /// </summary>
    /// features: quick tag hotkey, i.e. pressing 1 taggs "landscape, cololorful, sunset"
    /// and the different numbers can be bound to different sets of tags
    /// //quick move to destination. 
    /// pressing qwerty moves image to one of 6 folders
    /// //favorite searches
    /// watch folder for changes
    /// select multiple images and tag all at once
    /// right click, show in folder <-- done
    /// search google from image
    public partial class SlideShow : Window
    {
        private ImageFiles ImageFiles { get; }
        private ImageInfo mainImageInfo { get; } = new ImageInfo();
        private int currentIndex { get; set; } = 0;


        private bool isPaused = false;
        private bool isClosed = false;
        private bool resetTimer = false;
        private int imgDelay;
        private int ImgDelay
        {
            get => imgDelay;
            set
            {
                SettingsPersistanceUtil.RecordSetting("slideshowImgDelay", "" + value);
                imgDelay = value;
            }
        }
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
            mainSlideshowImagePanel.DataContext = mainImageInfo;
            if (!Int32.TryParse(SettingsPersistanceUtil.RetreiveSetting("slideshowImgDelay"), out imgDelay))
            {
                ImgDelay = 2000;
            }
            slideshowSpeed.Text = ImgDelay + "";
            Enable();
        }

        private void Enable()
        {
            Task.Run(new Action(async () =>
            {
                while (!isClosed)
                {
                    await App.Current.Dispatcher.InvokeAsync(new Action(() =>
                    {
                        ChangeImageIndex();
                    }));
                    resetTimer = true;
                    int timer = 0;
                    while (resetTimer || isPaused || timer < ImgDelay)
                    {
                        if (resetTimer)
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

        private void Pause(bool isPaused)
        {
            this.isPaused = isPaused;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            isClosed = true;
        }

        private void ChangeImageIndex(int deltaIndex = 1)
        {
            if (ImageFiles.Count > 0)
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

        int? prevTextChange = null;
        private void SlideshowSpeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UInt32.TryParse(slideshowSpeed.Text, out uint newDelay))
            {
                slideshowSpeed.Text = newDelay + "";
            }
            else 
                slideshowSpeed.Text = (prevTextChange ?? ImgDelay)+ "";

            prevTextChange = Int32.Parse(slideshowSpeed.Text);
        }

        private void SlideshowSpeed_LostFocus(object sender, RoutedEventArgs e)
        {

            ImgDelay = (int)UInt32.Parse(slideshowSpeed.Text);
            if (ImgDelay < 500)
            {
                ImgDelay = 500;
                slideshowSpeed.Text = ImgDelay + "";
            }
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

        private void ShowInFolder_ContextItem_Click(object sender, RoutedEventArgs e)
        {
            var imgPath = (e.OriginalSource as MenuItem).Tag.ToString();

            //straight up doesnt scroll to selected on the first call, 2nd call works though
            //ExplorerNavigateUtil.GoTo(imgPath);

            //some one elses solution i dont understand external code much
            //files in different folders will cause multiple windows to open
            //doesnt scroll to selected
            //h ttps://gist.github.com/vbfox/551626
            //ShowSelectedInExplorer.FileOrFolder(ImageFiles.Get(currentIndex));

            //uses search-ms
            //opens 1 window
            // items arnt displayed in their actual directories
            ExplorerSearchUtil.ShowInFolder(imgPath);
        }
    }
}
