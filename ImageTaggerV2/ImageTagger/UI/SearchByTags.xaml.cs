using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageTagger.UI
{
    /// <summary>
    /// Interaction logic for SearchByTags.xaml
    /// </summary>
    public partial class SearchByTags : Window
    {
        private HashSet<string> Result { get; set; } = new HashSet<string>();
        private SearchByTags()
        {
            InitializeComponent();
        }

        public static void OpenSearchDialog()
        {
            var mainWindow = new MainWindow(true);
            var searchByTagsWindow = new SearchByTags();
            mainWindow.ShowDialog();
            var tagQueryCriteria = new TagQueryCriteria(new string[] { }, new string[] { }, new string[] { "*red*", "*orange*", "*green*", "*yellow*", "*blue*", "*indigo*", "*violet*" });
            mainWindow.SetSearch(tagQueryCriteria);

            searchByTagsWindow.ShowDialog();
        }
    }
}
