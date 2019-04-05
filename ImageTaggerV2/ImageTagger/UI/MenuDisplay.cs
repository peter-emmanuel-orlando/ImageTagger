using System;
using System.Windows;
using System.Windows.Controls;

namespace ImageTagger
{

    public class MenuDisplay
    {
        MainWindow main { get; }
        public MenuDisplay(MainWindow main)
        {
            this.main = main;
            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;

            main.setSource_MenuItem.Click += SetSource_MenuItem_Click;
            main.setDestination_MenuItem.Click += SetDestination_MenuItem_Click;
            main.randomize_MenuItem.Checked += Randomize_MenuItem_Checked;
            main.randomize_MenuItem.Unchecked += Randomize_MenuItem_Unchecked;

        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
            main.setSource_MenuItem.Click -= SetSource_MenuItem_Click;
            main.setDestination_MenuItem.Click -= SetDestination_MenuItem_Click;
            main.randomize_MenuItem.Checked -= Randomize_MenuItem_Checked;
            main.randomize_MenuItem.Unchecked -= Randomize_MenuItem_Unchecked;
        }


        private void SetSource_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = "";
            var success = ImageFileUtil.GetDirectoryFromDialog(out result, PersistanceUtil.SourceDirectory);
            if (success)
            {
                PersistanceUtil.ChangeSource(result);
                ImageFiles.Load();
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

        private void Randomize_MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            ImageFiles.Load(true);
            SettingsPersistanceUtil.RecordSetting("randomizeItems", "true");
        }

        private void Randomize_MenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            ImageFiles.Load(false);
            SettingsPersistanceUtil.RecordSetting("randomizeItems", "false");
        }
    }
}
