using ImageTagger_DataModels;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace ImageTagger
{
    public static class ImageFileUtil
    {

        public static bool ApplyTagsToImage(ImageInfo imageInfo, IEnumerator<ImageTag> tags) 
        {
            return ApplyTagsToImage(imageInfo.ImgPath, tags);
        }

        public static bool ApplyTagsToImage(string imagePath, IEnumerator<ImageTag> tags)
        {
            try
            {
                var tagString = "";
                while (tags.MoveNext())
                {
                    var tag = tags.Current;
                    if (tag.TagName != "")
                        tagString += tag.TagName + "; ";
                }
                var sFile = ShellFile.FromParsingName(imagePath);
                var w = sFile.Properties.GetPropertyWriter();
                w.WriteProperty(SystemProperties.System.Keywords, tagString);
                w.Close();
                Debug.WriteLine("applied tags to file successfully");
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("failed to apply changes! Error message:[ " + e + " ]");
                Debug.WriteLine("unsuccessfully applied tags to file");
                return false;
            }
        }
    }
}
