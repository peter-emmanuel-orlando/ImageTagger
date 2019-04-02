using ImageTagger_DataModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageTagger
{
    public class AddNewTagComponent
    {
        private MainWindow main;
        public TextBox AddNewTag_TextBox { get { return main.addNewTag_TextBox; } }
        public Button AddNewTag_AcceptButton { get { return main.addNewTag_AcceptButton; } }
        public ImageTagsDisplay ImageTagsDisplay { get { return main.ImageTagsDisplay; } }
        public MainImageDisplay ImageDisplay { get { return main.ImageDisplay; } }

        public AddNewTagComponent(MainWindow main)
        {
            this.main = main;
            AddNewTag_TextBox.KeyDown += HandleKeyDown;
            AddNewTag_AcceptButton.Click += HandleButtonClick;
            AddNewTag_TextBox.LostFocus += HandleTextBoxLostFocus;
        }


        private void HandleKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddTags();
            }
        }

        private void HandleButtonClick(object sender, RoutedEventArgs e)
        {
            AddTags();
            AddNewTag_TextBox.Focus();
        }

        private void HandleTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (!AddNewTag_AcceptButton.IsFocused)
            {
                AddTags();
            }
        }

        private void AddTags()
        {
            ImageTagsDisplay.Add(new ImageTag(AddNewTag_TextBox.Text));
            ImageTagsDisplay.ApplyTagsToMainImage();
            AddNewTag_TextBox.Clear();
        }
    }
}