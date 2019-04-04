using ImageTagger_DataModels;
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

namespace ImageTagger
{
    public partial class MainWindow : Window
    {
        private string programDirectory = Directory.GetCurrentDirectory();
        private bool checkForEmptyTags = true;

        private bool randomizeImages = false;

        ///this is the generic, come back and fix
        public event EventHandler PreviewMainWindowUnload;

        public MainImageDisplay ImageDisplay { get; private set; }
        public ImageGridDisplay ImageGridDisplay { get; private set; }
        public ImageTagsDisplay ImageTagsDisplay { get; private set; }
        public AddNewTagComponent AddNewTagComponent { get; private set; }
        public MenuDisplay MenuDisplayComponent { get; private set; }
        public TagSuggestionDisplay TagSuggestionDisplay { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            //testing out some stuff
            SettingsPersistanceUtil.RecordSetting("artStyle", "abstract");
            SettingsPersistanceUtil.RecordSetting("artStyle", "impressionist");
            SettingsPersistanceUtil.RecordSetting("landscape", "evergreen");
            AllTagsRecord.RecordTag("artStyle", "abstract");
            AllTagsRecord.RecordTag("artStyle", "impressionist");
            AllTagsRecord.RecordTag("landscape", "evergreen");

            Debug.WriteLine("retreived setting for sexy: " + SettingsPersistanceUtil.RetreiveSetting("sexy"));
            Debug.WriteLine("retreived tags for sexy: " + String.Join(", ", AllTagsRecord.RetreiveTags("sexy")));
            Debug.WriteLine("retreived tags for landscape: " + String.Join(", ", AllTagsRecord.RetreiveTags("landscape")));
            //Thread.Sleep(99999999);
            ///end test section

            PersistanceUtil.LoadLocations();
            ImageFiles.FilesLoaded += HandleFilesReloaded;
            ImageFiles.Load();
        }


        private void HandleFilesReloaded(object sender, EventArgs e)
        {
            OnUnload(this, new EventArgs());

            PersistanceUtil.LoadLocations();
            ImageDisplay = new MainImageDisplay(this);
            ImageTagsDisplay = new ImageTagsDisplay(this);
            ImageGridDisplay = new ImageGridDisplay(this);
            AddNewTagComponent = new AddNewTagComponent(this);
            MenuDisplayComponent = new MenuDisplay(this);
            TagSuggestionDisplay = new TagSuggestionDisplay(this);
        }

        protected void OnUnload(object sender, EventArgs e)
        {
            PreviewMainWindowUnload?.Invoke(sender, e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }
    }
}
