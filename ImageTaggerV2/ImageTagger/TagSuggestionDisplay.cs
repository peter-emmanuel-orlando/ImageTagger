using System;

namespace ImageTagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class TagSuggestionDisplay
    {
        MainWindow main { get; }
        public TagSuggestionDisplay(MainWindow main)
        {
            this.main = main;
            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;

        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
        }
    }
}
