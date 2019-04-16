
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

namespace ImageTagger
{
    public partial class MainWindow : Window
    {
        private string programDirectory = Directory.GetCurrentDirectory();

        ///this is the generic, come back and fix
        public event EventHandler PreviewMainWindowUnload;

        public MainImageDisplay ImageDisplay { get; private set; }
        public ImageGridDisplay ImageGridDisplay { get; private set; }
        public ImageTagsDisplay ImageTagsDisplay { get; private set; }
        public AddNewTagComponent AddNewTagComponent { get; private set; }
        public MenuDisplay MenuDisplayComponent { get; private set; }
        public TagSuggestionDisplay TagSuggestionDisplay { get; private set; }
        public ImageFiles ImageFiles{get;} = new ImageFiles();

        public bool isDialog { get; } = false;
        

        public MainWindow(bool isDialog = false, TagQueryCriteria tagQueryCriteria = null, bool newAdditionsOnly = false)
        {
            InitializeComponent();
            this.isDialog = isDialog;

            if (isDialog)
            {
                setSource_MenuItem.IsEnabled = false;
                setSource_MenuItem.Visibility = Visibility.Collapsed;
                randomize_MenuItem.IsEnabled = false;
                randomize_MenuItem.Visibility = Visibility.Collapsed;
            }

            var randomizeItems = SettingsPersistanceUtil.RetreiveSetting("randomizeItems") == "true";
            randomize_MenuItem.IsChecked = randomizeItems;
            ImageFiles.Load(randomizeItems, tagQueryCriteria, newAdditionsOnly);

            ImageDisplay = new MainImageDisplay(this);
            ImageTagsDisplay = new ImageTagsDisplay(this);
            ImageGridDisplay = new ImageGridDisplay(this);
            AddNewTagComponent = new AddNewTagComponent(this);
            MenuDisplayComponent = new MenuDisplay(this);
            TagSuggestionDisplay = new TagSuggestionDisplay(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            PreviewMainWindowUnload?.Invoke(this, new EventArgs());
            if(this.isDialog)
                this.DialogResult = true;
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
                var toCombine = await ImageAnalysisAPI.ImageAnalysis.RequestBatchAnalysis(files);
                ImageFileUtil.BatchApplyTagsToImages(toCombine.ToDictionary(v => v.Key, v => v.Value.Cast<ImageTag>()));
            });
        }

        /*
        private void TestVisionAnalysis_Click(object sender, RoutedEventArgs e)
        {
            var result = VisionAPISuggestions.VisionApi.RequestVisionAnalysis(ImageDisplay.mainImageInfo.ImgPath);
            foreach (var tag in result)
            {
                Debug.WriteLine(tag.TagName);
            }
        }
        */
    }
}