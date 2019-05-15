using System;
using System.Windows;
using System.Windows.Controls;

namespace ImageTagger
{

    public class MenuDisplay
    {
        ViewSearchWindow main { get; }
        public MenuDisplay(ViewSearchWindow main)
        {
            this.main = main;
            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;

            main.setSource_MenuItem.Click += SetSource_MenuItem_Click;
            main.setDestination_MenuItem.Click += SetDestination_MenuItem_Click;

        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
            main.setSource_MenuItem.Click -= SetSource_MenuItem_Click;
            main.setDestination_MenuItem.Click -= SetDestination_MenuItem_Click;
        }


        private void SetSource_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = "";
            var success = ImageFileUtil.GetDirectoryFromDialog(out result, PersistanceUtil.SourceDirectory);
            if (success)
            {
                PersistanceUtil.ChangeSource(result);
                main.ImageFiles.Load(main.ImageFiles.currentQuery);
            }
        }

        private void SetDestination_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = "";
            var success = ImageFileUtil.GetDirectoryFromDialog(out result, PersistanceUtil.DestinationDirectory);
            if (success)
            {
                PersistanceUtil.ChangeDestination(result);
            }
        }

    }
}
