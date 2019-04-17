
using ImageTagger.DataModels;
using ImageTagger.UI;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageTagger
{
    public class AddSearchTagComponent
    {
        private SearchByTags searchWindow;
        public TextBox AddNewTag_TextBox { get; private set; }
        public Button AddNewTag_AcceptButton { get; private set; }
        public SearchTagsDisplay SearchTagsDisplay { get; private set; }

        public void Initialize(SearchByTags searchWindow, TextBox addNewTag_TextBox, Button addNewTag_AcceptButton, SearchTagsDisplay imageTagsDisplay)
        {
            AddNewTag_TextBox = addNewTag_TextBox ?? throw new ArgumentNullException(nameof(addNewTag_TextBox));
            AddNewTag_AcceptButton = addNewTag_AcceptButton ?? throw new ArgumentNullException(nameof(addNewTag_AcceptButton));
            SearchTagsDisplay = imageTagsDisplay ?? throw new ArgumentNullException(nameof(imageTagsDisplay));

            this.searchWindow = searchWindow;
            AddNewTag_TextBox.KeyDown += HandleKeyDown;
            AddNewTag_AcceptButton.Click += HandleButtonClick;
            AddNewTag_TextBox.LostFocus += HandleTextBoxLostFocus;
            AddNewTag_TextBox.TextChanged += HandleTextChanged;
            AddNewTag_TextBox.GotFocus += HandleTextBoxGotFocus;
            AddNewTag_TextBox.MouseLeftButtonUp += HandleTextBoxClick;
            AddNewTag_TextBox.PreviewTextInput += HandlePreviewTextInput;

            //searchWindow.PreviewMainWindowUnload += UnsubscribeFromAllEvents;
        }

        private void HandlePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var isInvalid = FormatUtil.ContainsInvalidCharacters(e.Text);
            if (isInvalid) e.Handled = true;
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            //searchWindow.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
            AddNewTag_TextBox.KeyDown -= HandleKeyDown;
            AddNewTag_AcceptButton.Click -= HandleButtonClick;
            AddNewTag_TextBox.LostFocus -= HandleTextBoxLostFocus;
            AddNewTag_TextBox.TextChanged -= HandleTextChanged;
            AddNewTag_TextBox.GotFocus -= HandleTextBoxGotFocus;
            AddNewTag_TextBox.MouseLeftButtonUp -= HandleTextBoxClick;
            AddNewTag_TextBox.PreviewTextInput -= HandlePreviewTextInput;
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

            if (Keyboard.FocusedElement != AddNewTag_AcceptButton)
            {
                //AddTags();
                AddNewTag_TextBox.Text = "";
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
            if (AddNewTag_TextBox.Text == "" && !AddNewTag_TextBox.IsFocused) AddNewTag_TextBox.Background.Opacity = 1;

        }

        private void AddTags()
        {
            var currentText = AddNewTag_TextBox.Text;
            if (currentText != "")
            {
                SearchTagsDisplay.Add(new ImageTag(currentText));
                AddNewTag_TextBox.Clear();
            }
        }
    }




    }