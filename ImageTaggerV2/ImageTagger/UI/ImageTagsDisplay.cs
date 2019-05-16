
using ImageTagger.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace ImageTagger
{
    public class ImageTagsDisplay
    {
        //need to make rules for tags
        ViewSearchWindow main { get; }

        private ObservableCollection<ImageTag> selectedImageTags { get; } = new ObservableCollection<ImageTag>();
        public IEnumerable<ImageTag> SelectedImageTags { get { return selectedImageTags; } }

        public ImageTagsDisplay(ViewSearchWindow main)
        {
            this.main = main;
            main.tagsDisplay.ItemsSource = selectedImageTags;
            selectedImageTags.CollectionChanged += HandleCollectionChanged;
            main.imageGrid.SelectionChanged += HandleImageGridSelectionChangeEvent;

            main.PreviewMainWindowUnload += UnsubscribeFromAllEvents;
        }

        private void UnsubscribeFromAllEvents(object sender, EventArgs e)
        {
            main.PreviewMainWindowUnload -= UnsubscribeFromAllEvents;
            // unsubscribe from anything else here
            selectedImageTags.CollectionChanged -= HandleCollectionChanged;
            main.imageGrid.SelectionChanged -= HandleImageGridSelectionChangeEvent;
        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (selectedImageTags.Count == 0)
                main.noTagsMessage.Opacity = 1;
            else
                main.noTagsMessage.Opacity = 0;
        }

        private void HandleImageGridSelectionChangeEvent(object sender, SelectionChangedEventArgs e)
        {
            ChangeSelectedImageTags();
        }

        private void ChangeSelectedImageTags()
        {
            HashSet<ImageTag> tags = null;
            foreach (var selected in main.imageGrid.SelectedItems.Cast<ImageInfo>())
            {
                var newTags = ImageFileUtil.GetImageTags(selected.ImgPath);
                if (tags == null)
                    tags = newTags;
                else
                    tags.IntersectWith(newTags);
            }
            selectedImageTags.Clear();
            selectedImageTags.Add(tags);
        }

        public void AddToAll(IEnumerable<ImageTag> items)
        {
            foreach (var item in items)
            {
                AddToAll(item);
            }
        }
        public void AddToAll(ImageTag tag)
        {
            if (!tag.IsEmpty() && !selectedImageTags.Contains(tag))
            {
                selectedImageTags.Add(tag);
                foreach (var selected in main.imageGrid.SelectedItems.Cast<ImageInfo>())
                {
                    var current = ImageFileUtil.GetImageTags(selected.ImgPath);
                    current.Add(tag);
                    ImageFileUtil.ApplyTagsToImage(selected, current);
                }
            }
        }
        
        public bool AllContain(string tagName)
        {
            return AllContain(new ImageTag(tagName));
        }

        public bool AllContain(ImageTag tag)
        {
            return selectedImageTags.Contains(tag);
        }

        public void RemoveFromAll(IEnumerable<ImageTag> items)
        {
            foreach (var item in items)
            {
                RemoveFromAll(item);
            }
        }
        public void RemoveFromAll(string tagName)
        {
            RemoveFromAll(new ImageTag(tagName));
        }
        public void RemoveFromAll(ImageTag tag)
        {
            if (!tag.IsEmpty() && selectedImageTags.Contains(tag))
            {
                selectedImageTags.Remove(tag);
                foreach (var selected in main.imageGrid.SelectedItems.Cast<ImageInfo>())
                {
                    var current = ImageFileUtil.GetImageTags(selected.ImgPath);
                    current.Remove(tag);
                    ImageFileUtil.ApplyTagsToImage(selected, current);
                }
            }
        }
    }

    public partial class ViewSearchWindow
    {
        public void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            string tagName = (e.OriginalSource as CheckBox).Content + "";
            Debug.WriteLine(tagName);
            ImageTagsDisplay.RemoveFromAll(tagName);
            addNewTag_TextBox.Text = tagName;
            addNewTag_TextBox.Focus();
            var suggestionIndex = TagSuggestionDisplay.SuggestedTagGridItems.IndexOf(new SuggestedTagGridItem(tagName, 0, 0, ""));
            if (suggestionIndex != -1)
            {
                var item = TagSuggestionDisplay.SuggestedTagGridItems[suggestionIndex];
                item.IsSelected = !item.IsSelected;
            }
        }
    }
}
