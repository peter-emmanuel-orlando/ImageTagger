using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Debug = System.Diagnostics.Debug;

namespace ImageTagger
{
    public partial class MainWindow
    {
        private void HandleImageGridSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var newImageInfo = (e.AddedItems[0] as ImageInfo);
                Debug.WriteLine("selected: " + newImageInfo.ImgPath);

                mainImageDisplay.Source = newImageInfo.ImgSource;

                var newTags = GetImageTags(newImageInfo.ImgPath);
                Debug.WriteLine("new tags: " + newTags);
                tagsDisplay.Text = newTags;
            }
        }

        private void HandleImageGridScrollChange(object sender, ScrollChangedEventArgs e)
        {
            Debug.WriteLine(
                " e.VerticalChange:" +
                e.VerticalChange +
                "e.ViewportHeight:" +
                e.ViewportHeight +
                " e.VerticalOffset:" +
                e.VerticalOffset +
                " e.ExtentHeight:" +
                e.ExtentHeight +
                " e.ExtentHeightChange:" +
                e.ExtentHeightChange +
                " scrollableHeight:" +
                (e.OriginalSource as ScrollViewer).ScrollableHeight
              );
            var scrollableHeight = (e.OriginalSource as ScrollViewer).ScrollableHeight;
            if (e.VerticalChange > 0 || scrollableHeight == 0)
            {
                if (e.VerticalOffset + e.ViewportHeight == e.ExtentHeight || scrollableHeight == 0)
                {
                    Debug.WriteLine("At the bottom of the list!");
                    LoadMoreImages();
                }
            }
        }

        private void LoadMoreImages()
        {
            if (loadedImages.Count() < imageFileNames.Count())
            {
                var loadedCount = loadedImages.Count();
                var fileNameCount = imageFileNames.Count();
                for (int i = loadedCount; i < Math.Min(10 + loadedCount, fileNameCount); i++)
                {
                    try
                    {
                        //Debug.WriteLine("trying to add file to list:" + imageFileNames[i]);
                        var newSquare = new ImageInfo(imageFileNames[i]);
                        loadedImages.Add(newSquare);
                    }
                    catch (Exception) { }
                }
                var selectedIndex = imgGrid.SelectedIndex;
                imgGrid.ItemsSource = loadedImages.ToArray();
                //this is a kinda janky workaround. what i need to do is handle arrow keypresses and move focus manually
                ListBoxItem myListBoxItem = (ListBoxItem)(imgGrid.ItemContainerGenerator.ContainerFromItem(imgGrid.Items[selectedIndex]));
                //imgGrid.ScrollIntoView(myListBoxItem);
                myListBoxItem.Focus();

                myListBoxItem.KeyDown += LastImage_KeyDown;
                return;
            }
        }

        private void LastImage_KeyDown(object sender, KeyEventArgs e)
        {
            //var container = e.OriginalSource as ListBoxItem;
            //container.Focus();
            //e.Handled = true;
            //var item = imgGrid.ItemContainerGenerator.ItemFromContainer(container);
            //var index = imgGrid.Items.IndexOf(item);
            //imgGrid.SelectedIndex = index + 1;
            //Debug.WriteLine(index);
        }
    }
}
