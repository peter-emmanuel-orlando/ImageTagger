using ImageTagger.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
    public partial class SearchWindow : Window
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
        public ViewSearchWindow viewSearchWindow { get; }
        private TagQueryCriteria currentQueryCriteria { get; set; } = new TagQueryCriteria();
        private FilterSortDataModel filterSortDataModel { get; } = new FilterSortDataModel();
        private class FilterSortDataModel
        {
            public ObservableCollection<object> OrderByItems { get; } = new ObservableCollection<object>(Enum.GetNames(typeof(OrderBy)).Select((e) => new
            { Ordering = (OrderBy)Enum.Parse(typeof(OrderBy), e) }));
            public ObservableCollection<object> OrderDirectionItems { get; } = new ObservableCollection<object>(Enum.GetNames(typeof(OrderDirection)).Select((e) => new
            { OrderingDirection = (OrderDirection)Enum.Parse(typeof(OrderDirection), e) }));
            public ObservableCollection<object> FilterByItems { get; } = new ObservableCollection<object>(Enum.GetNames(typeof(FilterBy)).Select((e) => new
            { FilterName = (FilterBy)Enum.Parse(typeof(FilterBy), e), FilterState = FilterState.Allow }));
        }
        internal SearchWindow()
        {
            InitializeComponent();

            viewSearchWindow = new ViewSearchWindow(true);
            viewSearchWindow.Closed += (s, e) => Close();
            viewSearchWindow.Show();
            this.Owner = viewSearchWindow;

            this.Loaded += (s, e) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            };

            orderByDisplay.ItemsSource = filterSortDataModel.OrderByItems;
            orderByDisplay.SelectedIndex = 0;
            orderDirectionDisplay.ItemsSource = filterSortDataModel.OrderDirectionItems;
            orderDirectionDisplay.SelectedIndex = 1;
            filtersDisplay.ItemsSource = filterSortDataModel.FilterByItems;

            all_AddSearchTagComponent = new AddSearchTagComponent();
            all_SearchTagsDisplay = new SearchTagsDisplay();
            all_AddSearchTagComponent.Initialize(this, addAllTag_TextBox, addAllTag_AcceptButton, all_SearchTagsDisplay);
            all_SearchTagsDisplay.Initialize(this, allTagsDisplay, noAllTagsMessage, addAllTag_TextBox);
            all_SearchTagsDisplay.CollectionChanged += Search;
            

            any_AddSearchTagComponent = new AddSearchTagComponent();
            any_SearchTagsDisplay = new SearchTagsDisplay();
            any_AddSearchTagComponent.Initialize(this, addAnyTag_TextBox, addAnyTag_AcceptButton, any_SearchTagsDisplay);
            any_SearchTagsDisplay.Initialize(this, anyTagsDisplay, noAnyTagsMessage, addAnyTag_TextBox);
            any_SearchTagsDisplay.CollectionChanged += Search;
            //addAnyTag_TextBox.PreviewTextInput += Search ;

            none_AddSearchTagComponent = new AddSearchTagComponent();
            none_SearchTagsDisplay = new SearchTagsDisplay();
            none_AddSearchTagComponent.Initialize(this, addNoneTag_TextBox, addNoneTag_AcceptButton, none_SearchTagsDisplay);
            none_SearchTagsDisplay.Initialize(this, noneTagsDisplay, noNoneTagsMessage, addNoneTag_TextBox);
            none_SearchTagsDisplay.CollectionChanged += Search;


            ToggleMainPanel();
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

        private void Search(object sender, EventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() => 
            {
                var anyTags = any_SearchTagsDisplay.Select(tag => tag.TagName);
                anyTags = anyTags.Union(new string[] { addAnyTag_TextBox.Text + "*" });
                var allTags = all_SearchTagsDisplay.Select(tag => tag.TagName);
                var noneTags = none_SearchTagsDisplay.Select(tag => tag.TagName);
                var orderBy = Cast(new { Ordering = OrderBy.Name }, orderByDisplay.SelectedItem).Ordering;
                var orderDirection = Cast(new { OrderingDirection = OrderDirection.RANDOM }, orderDirectionDisplay.SelectedItem).OrderingDirection;
                var template = new { FilterName = FilterBy.Untagged, FilterState = FilterState.Allow };
                var filters = new List<Filter>();
                foreach (var filter in filtersDisplay.Items)
                {
                    template = Cast( template, filter);
                    filters.Add(new Filter(template.FilterName, template.FilterState));
                }
                currentQueryCriteria = new TagQueryCriteria(anyTags, allTags, noneTags, orderBy, orderDirection, filters.ToArray());
                viewSearchWindow.SetSearch(currentQueryCriteria);
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
        private static T Cast<T>(T template, Object x)
        {
            // typeHolder above is just for compiler magic
            // to infer the type to cast x to
            return (T)x;
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = e.OriginalSource as Button;
            var filterStates = new List<FilterState>(Enum.GetValues(typeof(FilterState)).Cast<FilterState>());
            var newContext = new { FilterName = FilterBy.Untagged, FilterState = FilterState.Allow };
            newContext = Cast(newContext, btn.DataContext);
            var index = filterStates.IndexOf(newContext.FilterState);
            index = (index + 1) % filterStates.Count;
            newContext = new { FilterName = newContext.FilterName, FilterState = filterStates[index] };
             var i = filterSortDataModel.FilterByItems.IndexOf(btn.DataContext);
            filterSortDataModel.FilterByItems[i] = newContext;
            Search(null, null);
        }

        private void OrderByDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) Search(null, null);
        }

        private void OrderDirectionDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (e.AddedItems.Count > 0) ? e.AddedItems[0].ToString().Split('=').LastOrDefault().Replace("}", "") : "";
            if (Enum.TryParse(selected, out OrderDirection orderDirection))
            {
                if (orderDirection == OrderDirection.RANDOM)
                {
                    orderByDisplay.Visibility = Visibility.Collapsed;
                    reRollButton.Visibility = Visibility.Visible;
                }
                else
                {
                    orderByDisplay.Visibility = Visibility.Visible;
                    reRollButton.Visibility = Visibility.Collapsed;
                }
            }
            if (IsLoaded) Search(null, null);
        }

        private void ReRollButton_Click(object sender, RoutedEventArgs e)
        {
            Search(null, null);
        }
    }

}

namespace ImageTagger
{
    public partial class Launcher
    {
        private static SearchWindow GetNewSearchWindow()
        {
            return new SearchWindow();
        }
    }
}
