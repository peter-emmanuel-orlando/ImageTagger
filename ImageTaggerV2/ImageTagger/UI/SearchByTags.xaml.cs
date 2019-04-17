using System;
using System.Collections.Generic;
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
        private SearchByTags()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            };
        }

        public static void OpenSearchDialog()
        {
            var viewSearchWindow = new MainWindow(true);
            var searchByTagsWindow = new SearchByTags();

            Launcher.instance.main.IsEnabled = false;
            Launcher.instance.main.Hide();

            viewSearchWindow.Show();
            searchByTagsWindow.Show();
            searchByTagsWindow.Owner = viewSearchWindow;
            viewSearchWindow.Closed += (s, e ) =>
            {
                searchByTagsWindow.Close();
                Launcher.instance.main.IsEnabled = true;
                Launcher.instance.main.Show();
            };
        }

        private void ToggleCollapseButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainPanel.Visibility == Visibility.Collapsed)
            {
                mainPanel.Visibility = Visibility.Visible;
                toggleCollapseButton.Content = "Collapse";
            }
            else
            {
                mainPanel.Visibility = Visibility.Collapsed;
                toggleCollapseButton.Content = "Expand";
            }
        }

        private void CheckBox_None_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Any_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_All_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
