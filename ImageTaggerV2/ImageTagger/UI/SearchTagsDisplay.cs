
using ImageTagger.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ImageTagger.UI;

namespace ImageTagger
{
    public class SearchTagsDisplay
    {
        //need to make rules for tags
        public SearchByTags searchWindow { get; private set; }
        public ListBox TagsDisplay { get; private set; }
        public Label noTagsMessage { get; private set; }
        public TextBox addTagsTextBox { get; private set; }

        private ObservableCollection<ImageTag> mainImageTags { get; } = new ObservableCollection<ImageTag>();
        public IEnumerable<ImageTag> Tags { get { return mainImageTags; } }
        public ImageInfo TagSource { get; private set; }

        public SearchTagsDisplay()
        {
        }

        public void Initialize(SearchByTags searchWindow, ListBox tagsDisplay, Label noTagsMessage, TextBox addTagsTextBox)
        {
            this.searchWindow = searchWindow ?? throw new ArgumentNullException(nameof(searchWindow));
            TagsDisplay = tagsDisplay ?? throw new ArgumentNullException(nameof(tagsDisplay));
            this.noTagsMessage = noTagsMessage ?? throw new ArgumentNullException(nameof(noTagsMessage));
            this.addTagsTextBox = addTagsTextBox ?? throw new ArgumentNullException(nameof(addTagsTextBox));

            TagsDisplay.ItemsSource = mainImageTags;
            mainImageTags.CollectionChanged += HandleCollectionChanged;

            //searchWindow.PreviewMainWindowUnload += UnsubscribeFromAllEvents;
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            //searchWindow.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
            mainImageTags.CollectionChanged -= HandleCollectionChanged;
        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (mainImageTags.Count == 0)
                noTagsMessage.Opacity = 1;
            else
                noTagsMessage.Opacity = 0;
        }

        public void Alert_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            string tagName = (e.OriginalSource as CheckBox).Content + "";
            Debug.WriteLine(tagName);
            Remove(tagName);
            addTagsTextBox.Text = tagName;
            addTagsTextBox .Focus();
        }

        public void Add(IEnumerable<ImageTag> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
        public bool Add(ImageTag item)
        {
            var success = false;
            if (!item.IsEmpty() && !mainImageTags.Contains(item))
            {
                success = true;
                mainImageTags.Add(item);
            }
            return success;
        }
        
        public bool Contains(string tagName)
        {
            return Contains(new ImageTag(tagName));
        }

        public bool Contains(ImageTag tag)
        {
            return mainImageTags.Contains(tag);
        }

        public void Remove(IEnumerable<ImageTag> items)
        {
            foreach (var item in items)
            {
                Remove(item);
            }
        }
        public void Remove(string tagName)
        {
            Remove(new ImageTag(tagName));
        }
        public void Remove(ImageTag tag)
        {
            mainImageTags.Remove(tag);
        }

        public void Clear()
        {
            mainImageTags.Clear();
        }

        public void Refresh()
        {
            TagSource = TagSource;
        }

    }
}
