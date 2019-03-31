using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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



        private string programDirectory = "";
        private string sourceDirectory = "";
        private string destinationDirectory = "";
        private readonly string TagsDisplayPlaceholder = "[type tags for image]";
        private bool checkForEmptyTags = true;
        private bool randomizeImages = false;
        private readonly List<string> imageFileNames = new List<string>();

        private const string locFileName = @"lastSession.loc";
        private const string genericLocFile = @"SourceDirectory:SourcePlaceHolder|DestinationDirectory:DestinationPlaceHolder";


        public MainWindow()
        { 
            InitializeComponent();
            var tmp = GetImageFilenames(@"C:\Users\YumeMura\Downloads");
            var imageSquares = new List<ImageSquare>();
            int count = 0;
            int max = 25;
            foreach (var filename in tmp)
            {
                Debug.WriteLine(filename);
                try
                {
                    imageSquares.Add(new ImageSquare(filename));
                    count++;
                }
                catch (System.NotSupportedException)
                {
                    System.Diagnostics.Debug.WriteLine("was not able to use file: " + filename );
                }
                if (count >= max) break;
            }
            imgGrid.ItemsSource = imageSquares;
        }

        /*
        private void Initialize()
        {


            programDirectory = Directory.GetCurrentDirectory();
            try
            {
                // Read the file and display it line by line.
                var filename = programDirectory + @"\" + locFileName;
                System.IO.FileStream fs = new FileStream(filename, FileMode.OpenOrCreate);
                System.IO.StreamReader file = new System.IO.StreamReader(fs);
                var lines = file.ReadToEnd().Split('|');
                file.Close();
                fs.Close();

                sourceDirectory = lines[0].Replace("SourceDirectory:", "");
                destinationDirectory = lines[1].Replace("DestinationDirectory:", "");

                if (!Directory.Exists(sourceDirectory))
                    sourceDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                if (!Directory.Exists(destinationDirectory))
                    destinationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                LoadSourceDirectory(sourceDirectory);
            }
            catch (Exception)
            {

                LoadSourceDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
                destinationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }

            //CheckForEmptyTagsToolStripMenuItem.Text = "Check For Empty Tags: " + ((checkForEmptyTags) ? "Yes" : "No");
            //CheckForEmptyTagsToolStripMenuItem.BackColor = (checkForEmptyTags) ? SystemColors.MenuHighlightColor : SystemColors.ControlColor;

            //RandomizeImagesNoToolStripMenuItem.Text = "Randomize Images: " + ((randomizeImages) ? "Yes" : "No");
            //RandomizeImagesNoToolStripMenuItem.BackColor = (randomizeImages) ? SystemColors.MenuHighlightColor : SystemColors.ControlColor;
        }
        */

        private List<string> GetImageFilenames(string sourcePath)
        {
            var result = new List<string>();
            var fileTypes = new string[] { ".jpg" };//, "jpeg", "gif", "png", };
            foreach (var fileName in Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                if (fileTypes.Contains( Path.GetExtension(fileName)))
                    result.Add(fileName);
            }
            return result;
        }

        private void SetAsMainImage(object sender, SelectionChangedEventArgs e)
        {
            //var img = sender as Image;
            Debug.WriteLine("selected: " + (e.AddedItems[0] as ImageSquare).ImgSource.UriSource);
            mainImageDisplay.Source = (e.AddedItems[0] as ImageSquare).ImgSource;
        }
    }
}