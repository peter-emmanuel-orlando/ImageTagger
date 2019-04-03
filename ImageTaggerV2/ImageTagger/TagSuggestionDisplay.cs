using ImageTagger_DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    

    public class TagSuggestionDisplay
    {
        MainWindow main { get; }
        ItemsControl TagSuggestion { get { return main.tagSuggestionDisplay; } }

        private ObservableCollection<SuggestedTag> SuggestedTags { get; } = new ObservableCollection<SuggestedTag>();
        
        public TagSuggestionDisplay(MainWindow main)
        {
            this.main = main;
            TagSuggestion.ItemsSource = SuggestedTags;
            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;
            main.imageGrid.SelectionChanged += HandleGridSelectionChanged;

            main.reloadTagSuggestions.Click += HandleChangeSuggestionsClickEvent;
        }

        private void HandleGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeSuggestions();
        }

        private void HandleChangeSuggestionsClickEvent(object sender, RoutedEventArgs e)
        {
            ChangeSuggestions();
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

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
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
    }
}
