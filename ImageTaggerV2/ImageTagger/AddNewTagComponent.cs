using ImageTagger_DataModels;
using System;
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
            AddNewTag_TextBox.TextChanged += HandleTextChanged;
            AddNewTag_TextBox.GotFocus += HandleTextBoxGotFocus;
            AddNewTag_TextBox.MouseLeftButtonUp += HandleTextBoxClick;

            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
            AddNewTag_TextBox.KeyDown -= HandleKeyDown;
            AddNewTag_AcceptButton.Click -= HandleButtonClick;
            AddNewTag_TextBox.LostFocus -= HandleTextBoxLostFocus;
            AddNewTag_TextBox.TextChanged -= HandleTextChanged;
            AddNewTag_TextBox.GotFocus -= HandleTextBoxGotFocus;
            AddNewTag_TextBox.MouseLeftButtonUp -= HandleTextBoxClick;
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

        private void HandleTextBoxClick(object sender, RoutedEventArgs e)
        {
            AddNewTag_TextBox.SelectAll();
        }

        private void HandleTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (!AddNewTag_AcceptButton.IsFocused)
            {
                AddTags();
            }
            if (AddNewTag_TextBox.Text == "") AddNewTag_TextBox.Opacity = 0;
        }

        private void HandleTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            AddNewTag_TextBox.Opacity = 1;
            AddNewTag_TextBox.SelectAll();
        }

        private void HandleTextChanged(object sender, TextChangedEventArgs e)
        {
            if ( AddNewTag_TextBox.Text == "" && !AddNewTag_TextBox.IsFocused ) AddNewTag_TextBox.Background.Opacity = 1;
        }
        


        private void AddTags()
        {
            var currentText = AddNewTag_TextBox.Text;
            if(currentText != "")
            {
                ImageTagsDisplay.Add(new ImageTag(currentText));
                ImageTagsDisplay.ApplyTagsToMainImage();
                AddNewTag_TextBox.Clear();
            }
        }
    }
}