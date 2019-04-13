
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

        ///this is the generic, come back and fix
        public event EventHandler PreviewMainWindowUnload;

        public MainImageDisplay ImageDisplay { get; private set; }
        public ImageGridDisplay ImageGridDisplay { get; private set; }
        public ImageTagsDisplay ImageTagsDisplay { get; private set; }
        public AddNewTagComponent AddNewTagComponent { get; private set; }
        public MenuDisplay MenuDisplayComponent { get; private set; }
        public TagSuggestionDisplay TagSuggestionDisplay { get; private set; }


        private bool isDialog = false;
        public MainWindow(HashSet<string> toDisplay = null)
        {
            InitializeComponent();
            isDialog = toDisplay != null;

            if (isDialog)
            {
                setSource_MenuItem.IsEnabled = false;
                setSource_MenuItem.Visibility = Visibility.Collapsed;
                randomize_MenuItem.IsEnabled = false;
                randomize_MenuItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                PersistanceUtil.LoadLocations();
                var randomizeItems = SettingsPersistanceUtil.RetreiveSetting("randomizeItems") == "true";
                randomize_MenuItem.IsChecked = randomizeItems;
                ImageFiles.Load(randomizeItems);
            }


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
    }
}