using ImageTagger_DataModels;
using System;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows.Controls;

namespace ImageTagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class TagSuggestionDisplay
    {
        MainWindow main { get; }
        ListBox TagSuggestion { get { return main.tagSuggestionDisplay; } }

        private ObservableCollection<ImageTag> SuggestedTags { get; } = new ObservableCollection<ImageTag>();

        private Timer ChangeSuggestions;

        public TagSuggestionDisplay(MainWindow main)
        {
            this.main = main;
            TagSuggestion.ItemsSource = SuggestedTags;
            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;
        }

        private void SetTimer()
        {
            // Create a timer with a two second interval.
            ChangeSuggestions = new Timer(4000);
            // Hook up the Elapsed event for the timer. 
            ChangeSuggestions.Elapsed += OnTimedEvent;
            ChangeSuggestions.AutoReset = true;
            ChangeSuggestions.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //SuggestedTags.Add(//( DirectoryTagUtil.GetTags("loaded").GetEnumerator());
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
        }
    }
}
