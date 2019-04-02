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
        private readonly string TagsDisplayPlaceholder = "[type tags for image]";

        public AddNewTagComponent(MainWindow main)
        {
            this.main = main;
            AddNewTag_TextBox.KeyDown += HandleKeyDown;
            AddNewTag_AcceptButton.Click += HandleButtonClick;
            AddNewTag_TextBox.LostFocus += HandleTextBoxLostFocus;
            AddNewTag_TextBox.TextChanged += HandleTextChanged;
            AddNewTag_TextBox.GotFocus += HandleTextBoxGotFocus;
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
            if (AddNewTag_TextBox.Text == "") AddNewTag_TextBox.Text = TagsDisplayPlaceholder;
        }

        private void HandleTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (AddNewTag_TextBox.Text == TagsDisplayPlaceholder)
                AddNewTag_TextBox.Text = "";
            AddNewTag_TextBox.SelectAll();
        }

        private void HandleTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AddNewTag_TextBox.IsFocused && AddNewTag_TextBox.Text == "") AddNewTag_TextBox.Text = TagsDisplayPlaceholder;
        }



        private void AddTags()
        {
            var currentText = AddNewTag_TextBox.Text;
            if(currentText != "" && currentText != TagsDisplayPlaceholder)
            {
                ImageTagsDisplay.Add(new ImageTag(currentText));
                ImageTagsDisplay.ApplyTagsToMainImage();
                AddNewTag_TextBox.Clear();
            }
        }
    }
}