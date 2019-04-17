using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageTagger.UI
{
    public partial class SearchByTags : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


        private HashSet<string> Result { get; set; } = new HashSet<string>();
        private AddSearchTagComponent all_AddSearchTagComponent { get; }
        private SearchTagsDisplay all_SearchTagsDisplay { get; }
        private AddSearchTagComponent any_AddSearchTagComponent { get; }
        private SearchTagsDisplay any_SearchTagsDisplay { get; }
        private AddSearchTagComponent none_AddSearchTagComponent { get; }
        private SearchTagsDisplay none_SearchTagsDisplay { get; }
        private MainWindow viewSearchWindow { get; }
        private TagQueryCriteria currentQueryCriteria { get; set; } = new TagQueryCriteria();

        private SearchByTags()
        {
            InitializeComponent();

            viewSearchWindow = new MainWindow(true);
            viewSearchWindow.Closed += (s, e) => Close();
            viewSearchWindow.Show();
            this.Owner = viewSearchWindow;

            this.Loaded += (s, e) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            };

            all_AddSearchTagComponent = new AddSearchTagComponent();
            all_SearchTagsDisplay = new SearchTagsDisplay();
            all_AddSearchTagComponent.Initialize(this, addAllTag_TextBox, addAllTag_AcceptButton, all_SearchTagsDisplay);
            all_SearchTagsDisplay.Initialize(this, allTagsDisplay, noAllTagsMessage, addAllTag_TextBox);

            any_AddSearchTagComponent = new AddSearchTagComponent();
            any_SearchTagsDisplay = new SearchTagsDisplay();
            any_AddSearchTagComponent.Initialize(this, addAnyTag_TextBox, addAnyTag_AcceptButton, any_SearchTagsDisplay);
            any_SearchTagsDisplay.Initialize(this, anyTagsDisplay, noAnyTagsMessage, addAnyTag_TextBox);

            none_AddSearchTagComponent = new AddSearchTagComponent();
            none_SearchTagsDisplay = new SearchTagsDisplay();
            none_AddSearchTagComponent.Initialize(this, addNoneTag_TextBox, addNoneTag_AcceptButton, none_SearchTagsDisplay);
            none_SearchTagsDisplay.Initialize(this, noneTagsDisplay, noNoneTagsMessage, addNoneTag_TextBox);
        }


        public static void OpenSearchDialog()
        {
            var searchByTagsWindow = new SearchByTags();
            searchByTagsWindow.Show();

            Launcher.instance.main.IsEnabled = false;
            Launcher.instance.main.Hide();

            searchByTagsWindow.Closed += (s, e) =>
            {
                Launcher.instance.main.IsEnabled = true;
                Launcher.instance.main.Show();
            };
        }

        private void ToggleCollapseButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMainPanel();
        }

        private void ToggleMainPanel()
        {
            if (mainPanel.Visibility == Visibility.Collapsed)
            {
                mainPanel.Visibility = Visibility.Visible;
                //this.Title = this.Title.Replace("Expand", "Collapse");
                toggleCollapseButton.Content = "Collapse";
            }
            else
            {
                mainPanel.Visibility = Visibility.Collapsed;
                //this.Title = this.Title.Replace("Collapse", "Expand");
                toggleCollapseButton.Content = "Expand";
            }
        }

        private void CheckBox_None_Unchecked(object sender, RoutedEventArgs e)
        {
            none_SearchTagsDisplay.Alert_CheckBox_Unchecked(sender, e);
        }

        private void CheckBox_Any_Unchecked(object sender, RoutedEventArgs e)
        {
            any_SearchTagsDisplay.Alert_CheckBox_Unchecked(sender, e);
        }

        private void CheckBox_All_Unchecked(object sender, RoutedEventArgs e)
        {
            all_SearchTagsDisplay.Alert_CheckBox_Unchecked(sender, e);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var anyTags = any_SearchTagsDisplay.Select(tag => tag.TagName);
            var allTags = all_SearchTagsDisplay.Select(tag => tag.TagName);
            var noneTags = none_SearchTagsDisplay.Select(tag => tag.TagName);
            currentQueryCriteria = new TagQueryCriteria( anyTags, allTags, noneTags);
            viewSearchWindow.SetSearch(currentQueryCriteria);
            ToggleMainPanel();
        }
        
    }
}
