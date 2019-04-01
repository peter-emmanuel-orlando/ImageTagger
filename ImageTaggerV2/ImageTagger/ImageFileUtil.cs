using ImageTagger_DataModels;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ImageTagger
{
    public class ImageFileUtil
    {

        private bool ApplyTagsToImage(string imagePath, IEnumerable<ImageTag> tags)
        {
            try
            {
                var tagString = "";
                foreach (var tag in tags)
                {
                    if (tag.TagName != "")
                        tagString += tag.TagName + "; ";
                }
                //var tags = TagsDisplay.Text.Replace(TagsDisplayPlaceholder, "").Replace(" ", "").Split(';');
                var sFile = ShellFile.FromParsingName(imagePath);
                var w = sFile.Properties.GetPropertyWriter();
                w.WriteProperty(SystemProperties.System.Keywords, tags);
                w.Close();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("failed to apply changes! Error message:[ " + e + " ]");
                return false;
            }
        }
    }
}
