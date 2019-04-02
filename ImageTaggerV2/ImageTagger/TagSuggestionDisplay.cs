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
        public SuggestedTag(string tagName, Thickness margins)
        {
            TagName = tagName;
            Margins = margins;
        }

        public string TagName { get; }
        public Thickness Margins { get; }
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
            var list = DirectoryTagUtil.GetSuggestedTags(main.ImageDisplay.mainImageInfo.ImgPath);
            var r =  new Random(DateTime.UtcNow.Millisecond);
            foreach ( var suggestion in list)
            {
                SuggestedTags.Add(new SuggestedTag(suggestion.tag.TagName, new Thickness(r.NextDouble() * maxOffsetX, r.NextDouble() * maxOffsetY, 0, 0)));
            }
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
        }
    }
}
