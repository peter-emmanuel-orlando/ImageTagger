
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Debug = System.Diagnostics.Debug;
using Path = System.IO.Path;
using System.ComponentModel;
using ImageTagger.UI;
using ImageTagger.DataModels;
using System.Collections.Async;

namespace ImageTagger
{
    public partial class ViewSearchWindow : Window
    {
        private string programDirectory = Directory.GetCurrentDirectory();
        private RoutedEventHandler batchEvent;
        ///this is the generic, come back and fix
        public event EventHandler PreviewMainWindowUnload;

        public MainImageDisplay ImageDisplay { get; private set; }
        public ImageGridDisplay ImageGridDisplay { get; private set; }
        public ImageTagsDisplay ImageTagsDisplay { get; private set; }
        public AddNewTagComponent AddNewTagComponent { get; private set; }
        public MenuDisplay MenuDisplayComponent { get; private set; }
        public TagSuggestionDisplay TagSuggestionDisplay { get; private set; }
        public ImageFiles ImageFiles{get;} = new ImageFiles();

        public bool isModal { get; } = false;
        

        public ViewSearchWindow(bool isModal = false)
        {
            InitializeComponent();
            
            this.isModal = isModal;

            if (isModal)
            {
            }

            //todo: currently doesnt work
            this.Closing += HandleWindowClosingEvent;

            setDestination_MenuItem.IsEnabled = false;
            setDestination_MenuItem.Visibility = Visibility.Collapsed;

            ImageDisplay = new MainImageDisplay(this);
            ImageTagsDisplay = new ImageTagsDisplay(this);
            ImageGridDisplay = new ImageGridDisplay(this);
            AddNewTagComponent = new AddNewTagComponent(this);
            MenuDisplayComponent = new MenuDisplay(this);
            TagSuggestionDisplay = new TagSuggestionDisplay(this);

            batchTag_MenuItem.Click += batchEvent = BatchTag_MenuItem_Click;
        }
        public void SetSearch(TagQueryCriteria tagQueryCriteria = null, bool newAdditionsOnly = false)
        {
            ImageFiles.Load( tagQueryCriteria, newAdditionsOnly);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            PreviewMainWindowUnload?.Invoke(this, new EventArgs());
        }

        private void EditTagsRecord_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var tmp = new EditMyTags();
            tmp.ShowDialog();
        }

        private void SetAPIKeysRecord_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImageAnalysisAPI.ImageAnalysis.SetAPIKeyViaDialog();
            VisionAPISuggestions.VisionApi.SetVisionAuthViaDialog();
        }

        private void BatchTag_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var files = ImageFiles.GetAll();
            if (files.Count > ImageAnalysisAPI.ImageAnalysis.maxItemsPerBatchRequest)
            {
                if (MessageBoxResult.No == MessageBox.Show(files.Count + " files are queued for process, do you want to continue?", "Warning:", MessageBoxButton.YesNo))
                    return;
            }
            Task.Run( async () =>
            {
                var asyncEnumerable = ImageAnalysisAPI.ImageAnalysis.RequestBatchAnalysis(files);
                await asyncEnumerable.ForEachAsync( toCombine =>
                {
                    ImageFileUtil.BatchApplyTagsToImages(toCombine.ToDictionary(v => v.Key, v => v.Value.Cast<ImageTag>()));
                });
            });
        }

        private void NewWindow_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Launcher.OpenNewWindow();
        }

        private void HandleWindowClosingEvent(object sender, CancelEventArgs e)
        {
            var result = MessageBox.Show("Would you like to keep ImageTagger open in the background?", "Confirm exit", MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void SlideShow_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            var startIndex = ImageFiles.IndexOf(ImageDisplay.mainImageInfo.ImgPath);
            var newIndex = SlideShow.PlaySlideshow(ImageFiles, startIndex);
            ImageGridDisplay.SetImage(newIndex);
            this.Show();
        }

        private void ShowInFolder_ContextItem_Click(object sender, RoutedEventArgs e)
        {
            var imgPath = (e.OriginalSource as MenuItem).Tag.ToString();
            ExplorerSearchUtil.ShowInFolder(imgPath);
        }
    }
}