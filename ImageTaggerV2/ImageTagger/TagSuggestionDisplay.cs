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
        public string TagName { get; }
        public int OffsetX { get; }
        public int OffsetY { get; }

        public SuggestedTag(string tagName, int offsetX, int offsetY)
        {
            TagName = tagName;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }

    public class TagSuggestionDisplay
    {
        MainWindow main { get; }
        ListBox TagSuggestion { get { return main.tagSuggestionDisplay; } }

        private ObservableCollection<SuggestedTag> SuggestedTags { get; } = new ObservableCollection<SuggestedTag>();
        
        public TagSuggestionDisplay(MainWindow main)
        {
            this.main = main;
            TagSuggestion.ItemsSource = SuggestedTags;
            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;

            main.reloadTagSuggestions.Click += HandleChangeSuggestionsClickEvent;
            ChangeSuggestions();
        }

        private void HandleChangeSuggestionsClickEvent(object sender, RoutedEventArgs e)
        {
            ChangeSuggestions();
        }

        private void ChangeSuggestions()
        {
            var maxOffsetX = TagSuggestion.ActualWidth - 70;
            var maxOffsetY = TagSuggestion.ActualHeight - 70;

            SuggestedTags.Clear();
            var list = new List<string>(DirectoryTagUtil.GetTags("loaded"));
            list.Shuffle();
            var r =  new Random(DateTime.UtcNow.Millisecond);
            foreach ( string tagName in list)
            {
                SuggestedTags.Add(new SuggestedTag(tagName, (int)(r.Next() * maxOffsetX), (int)(r.Next() * maxOffsetY)));
            }
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
        }
    }
}
