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
using WinForms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Debug = System.Diagnostics.Debug;
using Path = System.IO.Path;

namespace ImageTagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {



        private string programDirectory = Directory.GetCurrentDirectory();
        protected string sourceDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        private string destinationDirectory = "";
        private bool checkForEmptyTags = true;

        private bool randomizeImages = false;

        protected const string locFileName = @"lastSession.loc";
        protected const string genericLocFile = @"SourceDirectory:SourcePlaceHolder|DestinationDirectory:DestinationPlaceHolder";

        ///this is the generic, come back and fix
        public event EventHandler PreviewMainWindowInitialized;

        public ObservableCollection<string> ImageFileNames { get; } = new ObservableCollection<string>();

        public MainImageDisplay ImageDisplay { get; private set; }
        public ImageGridDisplay ImageGridDisplay { get; private set; }
        public ImageTagsDisplay ImageTagsDisplay { get; private set; }
        public AddNewTagComponent AddNewTagComponent { get; private set; }

        public MainWindow()
        { 
            InitializeComponent();
            InitializeSelf();
        }

        protected void OnPreviewMainWindowInitialized(object sender, EventArgs e)
        {
            PreviewMainWindowInitialized?.Invoke(sender, e);
        }

        private void InitializeSelf()
        {
            OnPreviewMainWindowInitialized(this, new EventArgs());

            ImageFileNames.Clear();
            ImageFileUtil.GetImageFilenames(sourceDirectory, randomizeImages).ForEach((item) => { ImageFileNames.Add(item); });

            ImageDisplay = new MainImageDisplay(this);
            ImageTagsDisplay = new ImageTagsDisplay(this);
            ImageGridDisplay = new ImageGridDisplay(this);
            AddNewTagComponent = new AddNewTagComponent(this);
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog();
            fbd.SelectedPath = sourceDirectory;
            var result = fbd.ShowDialog();
            if ( result == WinForms.DialogResult.OK )
            {
                sourceDirectory = fbd.SelectedPath;
                InitializeSelf();
            }
        }
    }
}
