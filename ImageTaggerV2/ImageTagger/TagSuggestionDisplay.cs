using ImageTagger_DataModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace ImageTagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public class SuggestedTag
    {
        public SuggestedTag(string tagText, int row, int column)
        {
            tag = new ImageTag( tagText);
            // 0 and 11 are used for spacing
            Row = row.Clamp(1, 10);
            Column = column;
        }

        public string TagText { get { return tag.TagName; } }
        private ImageTag tag { get; }
        public int Row { get; }
        public int Column { get; }

        public static implicit operator ImageTag(SuggestedTag sTag)
        {
            return sTag.tag;
        }

        public override string ToString()
        {
            return TagText;
        }
    }

    public class TagCategory
    {
        public TagCategory(string categoryName)
        {
            CategoryName = categoryName;
        }

        public string CategoryName { get; }
    }
    

    public class TagSuggestionDisplay
    {
        MainWindow main { get; }
        ItemsControl TagSuggestion { get { return main.tagSuggestionDisplay; } }

        private ObservableCollection<SuggestedTag> SuggestedTags { get; } = new ObservableCollection<SuggestedTag>();
        private ObservableCollection<TagCategory> TagCategories { get; } = new ObservableCollection<TagCategory>();

        private bool isDormant = false;

        public TagSuggestionDisplay(MainWindow main)
        {
            this.main = main;
            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;

            TagSuggestion.ItemsSource = SuggestedTags;
            main.tagSuggestionCategoriesDisplay.ItemsSource = TagCategories;

            main.imageGrid.SelectionChanged += HandleGridSelectionChanged;
            main.reloadTagSuggestions.Click += HandleChangeSuggestionsClickEvent;
            main.closeTagSuggestions.Click += HandleCloseSuggestionsClickEvent;
            DirectoryTagUtil.TagsLoaded += HandleTagsReloadedEvent;

            CloseSuggestionsPanel();
            DirectoryTagUtil.Load();
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here

            main.imageGrid.SelectionChanged -= HandleGridSelectionChanged;
            main.reloadTagSuggestions.Click -= HandleChangeSuggestionsClickEvent;
            main.closeTagSuggestions.Click -= HandleCloseSuggestionsClickEvent;
            DirectoryTagUtil.TagsLoaded -= HandleTagsReloadedEvent;
        }

        private void HandleTagsReloadedEvent(object sender, EventArgs e)
        {
            TagCategories.Clear();
            foreach( var category in DirectoryTagUtil.GetTagCategories())
            {
                TagCategories.Add(new TagCategory(category));
            }
        }

        private void HandleCloseSuggestionsClickEvent(object sender, RoutedEventArgs e)
        {
            CloseSuggestionsPanel();
        }

        private void HandleGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeSuggestions();
        }

        private void HandleChangeSuggestionsClickEvent(object sender, RoutedEventArgs e)
        {
            OpenSuggestionsPanel();
        }

        private void OpenSuggestionsPanel()
        {
            //vvv probably uneccessary vvv
            //  set grid splitter position to 1/2 the total
            //  main.suggestedTagsGridArea.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            //  main.suggestedTagsGridArea.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
            //enable and show display tags area
            main.tagSuggestionDisplay.IsEnabled = true;
            main.tagSuggestionDisplay.Visibility = Visibility.Visible;
            //enable and show display tag categories area
            main.tagSuggestionCategoriesDisplay_Viewer.IsEnabled = true;
            main.tagSuggestionCategoriesDisplay_Viewer.Visibility = Visibility.Visible;
            //enable and show grid splitter
            main.suggestedTagsSplitter.IsEnabled = true;
            main.suggestedTagsSplitter.Visibility = Visibility.Visible;
            //enable and show closeSuggestions button
            main.closeTagSuggestions.IsEnabled = true;
            main.closeTagSuggestions.Visibility = Visibility.Visible;
            //set isDormant to false
            isDormant = false;
            //call change suggestions to get new suggestions

            ChangeSuggestions();
        }
        private void CloseSuggestionsPanel()
        {
            //  vvv possibly uneccessary vvv
            //  set grid splitter position to min
            //  main.suggestedTagsGridArea.RowDefinitions[1].Height = new GridLength(0);
            //disable and hide display tags area
            main.tagSuggestionDisplay.IsEnabled = false;
            main.tagSuggestionDisplay.Visibility = Visibility.Hidden;
            //enable and show display tag categories area
            main.tagSuggestionCategoriesDisplay_Viewer.IsEnabled = false;
            main.tagSuggestionCategoriesDisplay_Viewer.Visibility = Visibility.Collapsed;
            //disable and hide grid splitter
            main.suggestedTagsSplitter.IsEnabled = false;
            main.suggestedTagsSplitter.Visibility = Visibility.Hidden;
            //disable and hide closeSuggestions button
            main.closeTagSuggestions.IsEnabled = false;
            main.closeTagSuggestions.Visibility = Visibility.Collapsed;
            //set isDormant to true
            isDormant = true;
            //clear tagSuggestions
            SuggestedTags.Clear();
        }

        private struct Coordinate : IEquatable<Coordinate> {
            public readonly int row, column;
            public Coordinate(int x, int y){ this.row = x; this.column = y; }

            public override bool Equals(object obj)
            {
                if (!(obj is Coordinate))
                {
                    return false;
                }

                var coordinate = (Coordinate)obj;
                return row == coordinate.row &&
                       column == coordinate.column;
            }

            public bool Equals(Coordinate other)
            {
                return Equals(other as object);
            }

            public override int GetHashCode()
            {
                var hashCode = 1502939027;
                hashCode = hashCode * -1521134295 + row.GetHashCode();
                hashCode = hashCode * -1521134295 + column.GetHashCode();
                return hashCode;
            }

            public override string ToString()
            {
                return "row:" + row + " column:" + column;
            }
        }
        
        private void ChangeSuggestions()
        {
            if (isDormant) return;
            var used = new HashSet<Coordinate>();
            SuggestedTags.Clear();
            //max = actualheight / tagheight rounded up so at least something is visible
            var maxRows = (int)(1 + Math.Floor(TagSuggestion.ActualHeight / 30) % 30);
            var maxColumns = (int)(1 + Math.Floor(TagSuggestion.ActualWidth / 70) % 30);
            var list = DirectoryTagUtil.GetSuggestedTags(main.ImageDisplay.mainImageInfo.ImgPath);
            var r =  new Random(DateTime.UtcNow.Millisecond);
            foreach ( var suggestion in list)
            {
                Coordinate coord = new Coordinate();
                for (int i = 0; i < 50; i++)
                {
                    if(i == 0 || used.Contains(coord))
                    {
                        coord = new Coordinate(1 + r.Next() % maxRows, 1 + r.Next() % maxColumns);
                    }

                    if (!used.Contains(coord))
                    {
                        used.Add(coord);
                        SuggestedTags.Add(new SuggestedTag(suggestion.tag.TagName, coord.row, coord.column));
                        break;
                    }
                }
            }
        }

    }



    public partial class MainWindow
    {

        private void HandleTagSuggestionButtonClickEvent(object sender, RoutedEventArgs e)
        {
            var btn = e.OriginalSource as Button;
            if(btn.Opacity == 0.5)
            {
                ImageTagsDisplay.Remove(btn.Content + "");
                btn.Opacity = 1;
            }
            else
            {
                ImageTagsDisplay.Add(new ImageTag(btn.Content + ""));
                btn.Opacity = 0.5;
            }
            ImageTagsDisplay.ApplyTagsToMainImage();
        }


        private void HandleTagSuggestionCategoryButtonClickEvent(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
