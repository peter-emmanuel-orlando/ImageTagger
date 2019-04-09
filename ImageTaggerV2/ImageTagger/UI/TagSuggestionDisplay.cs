﻿
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using ImageTagger.DataModels;
using System.Diagnostics;
using System.Collections;
using ImageAnalysisAPI;

namespace ImageTagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public class SuggestedTagGridItem
    {
        public SuggestedTagGridItem(string tagText, int row, int column, string color)
        {
            tag = new ImageTag( tagText);
            // 0 and 11 are used for spacing
            Row = row.Clamp(1, 10);
            Column = column;
            Color = color;
        }

        public string TagText { get { return tag.TagName; } }
        private ImageTag tag { get; }
        public int Row { get; }
        public int Column { get; }
        public string Color { get; } = "#FFFF69B4";

        public static implicit operator ImageTag(SuggestedTagGridItem sTag)
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

        private HashSet<Coordinate> UsedPositions { get; } = new HashSet<Coordinate>();
        private ObservableCollection<SuggestedTagGridItem> SuggestedTagGridItems { get; } = new ObservableCollection<SuggestedTagGridItem>();
        private ObservableCollection<TagCategory> TagCategories { get; } = new ObservableCollection<TagCategory>();

        private bool isDormant = false;

        public TagSuggestionDisplay(MainWindow main)
        {
            this.main = main;
            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;

            TagSuggestion.ItemsSource = SuggestedTagGridItems;
            main.tagSuggestionCategoriesDisplay.ItemsSource = TagCategories;

            main.imageGrid.SelectionChanged += HandleGridSelectionChanged;
            main.reloadTagSuggestions.Click += HandleChangeSuggestionsClickEvent;
            main.closeTagSuggestions.Click += HandleCloseSuggestionsClickEvent;
            TagsManager.TagsLoaded += HandleTagsReloadedEvent;

            CloseSuggestionsPanel();
            TagsManager.Load();
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here

            main.imageGrid.SelectionChanged -= HandleGridSelectionChanged;
            main.reloadTagSuggestions.Click -= HandleChangeSuggestionsClickEvent;
            main.closeTagSuggestions.Click -= HandleCloseSuggestionsClickEvent;
            TagsManager.TagsLoaded -= HandleTagsReloadedEvent;
        }

        private void HandleTagsReloadedEvent(object sender, EventArgs e)
        {
            TagCategories.Clear();
            foreach (var category in TagsManager.GetTagCategories())
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
            //disable and collapse openSuggestions button
            main.reloadTagSuggestions.IsEnabled = false;
            main.reloadTagSuggestions.Visibility = Visibility.Collapsed;
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
            //enable and show openSuggestions button
            main.reloadTagSuggestions.IsEnabled = true;
            main.reloadTagSuggestions.Visibility = Visibility.Visible;
            //set isDormant to true
            isDormant = true;
            //clear tagSuggestions
            SuggestedTagGridItems.Clear();
        }

        private struct Coordinate : IEquatable<Coordinate> {
            public readonly int row, column;
            public Coordinate(int x, int y) { this.row = x; this.column = y; }

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

        internal void ChangeSuggestions(string category = "")
        {
            if (isDormant) return;
            SuggestedTagGridItems.Clear();
            UsedPositions.Clear();
            var mainImgPath = main.ImageDisplay.mainImageInfo.ImgPath;
            if(category == "insight")
            {
                TagsManager.GetImageAnalysisTags(mainImgPath, (result) => {
                    var colorDict = new Dictionary<ImageAnalysisType, string>();
                    colorDict.Add(ImageAnalysisType.moderation, "#f92983");
                    colorDict.Add(ImageAnalysisType.demographics, "#6670e3");
                    colorDict.Add(ImageAnalysisType.general, "#00f3a7");
                    foreach (var item in result)
                    {
                        ImageAnalysisType analysisCategory;
                        Enum.TryParse(item.category, out analysisCategory);
                        AddSuggestions(new TagSuggestion[] { item }, colorDict[analysisCategory]);
                    }
                });
            }
            else
            {
                AddSuggestions(TagsManager.GetTagSuggestions(mainImgPath, category));
            }
        }



        private void AddSuggestions(IEnumerable<TagSuggestion> toAdd, string color = "#ede7da")
        {
            var maxRows = (int)(1 + Math.Floor(TagSuggestion.ActualHeight / 30) % 30);
            var maxColumns = (int)(1 + Math.Floor(TagSuggestion.ActualWidth / 70) % 30);
            var r = new Random(DateTime.UtcNow.Millisecond);
            foreach (var suggestion in toAdd)
            {
                Coordinate coord = new Coordinate();
                for (int i = 0; i < 50; i++)
                {
                    if (i == 0 || UsedPositions.Contains(coord))
                    {
                        coord = new Coordinate(1 + r.Next() % maxRows, 1 + r.Next() % maxColumns);
                    }

                    if (!UsedPositions.Contains(coord))
                    {
                        UsedPositions.Add(coord);
                        try
                        {
                            SuggestedTagGridItems.Add(new SuggestedTagGridItem(suggestion.tag.TagName, coord.row, coord.column, color));
                        }
                        catch (Exception e) { Debug.WriteLine(e); }
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
            var category = (e.OriginalSource as Button).Content as string;
            TagSuggestionDisplay.ChangeSuggestions(category);
        }
    }
}