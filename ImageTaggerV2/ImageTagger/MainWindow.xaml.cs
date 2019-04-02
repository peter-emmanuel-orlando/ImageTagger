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

        protected const string locFileName = @"lastSession.loc";
        protected const string genericLocFile = @"SourceDirectory:SourcePlaceHolder|DestinationDirectory:DestinationPlaceHolder";

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
            List<char> tmp;
            var fixedTag = TagFormatUtil.Fix("ASDFAsrQreF$t#$^H^$WrsTdfGweG WtHRYH3% #5hEY 5H Y", TagCasing.KebabCase, out tmp);
            Debug.WriteLine(fixedTag);
            Thread.Sleep(99999999);
            InitializeComponent();
            PersistanceUtil.LoadLocations();
            ImageFiles.FilesLoaded += HandleFilesReloaded;
            ImageFiles.Load();
        }


        private void HandleFilesReloaded(object sender, EventArgs e)
        {
            OnUnload(this, new EventArgs());

            PersistanceUtil.LoadLocations();
            DirectoryTagUtil.Load();
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
