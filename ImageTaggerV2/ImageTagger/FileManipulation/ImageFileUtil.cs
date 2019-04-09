﻿
using ImageTagger.DataModels;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using WinForms = System.Windows.Forms;

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
                    if (tag.TagName != "" && tag.TagName != ImageTag.NoTagsPlaceholder.TagName)
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

        public static HashSet<ImageTag> GetImageTags(string imagePath)
        {
            var result = new HashSet<ImageTag>();

            Debug.WriteLine("tags at: " + imagePath);
            if (File.Exists(imagePath))
            {
                try
                {
                    var sFile = ShellFile.FromParsingName(imagePath);
                    var tagsList = sFile.Properties.System.Keywords.Value;
                    if (tagsList != null)
                    {
                        foreach (var tagText in tagsList)
                        {
                            var newTag = new ImageTag(tagText);//perhaps automatically do to lower?
                            if (!result.Contains(newTag))
                                result.Add(newTag);
                        }
                    }
                }
                catch (Exception e) { throw e; }
            }

            return result;
        }
        public static string[] acceptedFileTypes { get { return new string[] { ".jpg", ".jpeg" }; } }//, "jpeg", "gif", "png", };

        public static List<string> GetImageFilenames(string sourcePath)
        {
            var result = new List<string>();
            if(sourcePath != null)
            {
                //try ccatch this
                foreach (var fileName in Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    if (acceptedFileTypes.Contains(Path.GetExtension(fileName)))
                        result.Add(fileName);
                }
            }
            return result;
        }

        public static bool GetDirectoryFromDialog(out string result, string initialLocation = "")
        {
            result = initialLocation;
            var fbd = new WinForms.FolderBrowserDialog();
            fbd.SelectedPath = initialLocation;
            var success = fbd.ShowDialog() == WinForms.DialogResult.OK;
            if (success)
                result = fbd.SelectedPath;
            return success;
        }



        public static bool MoveToDestination(ImageInfo imgInfo, string newDirectory)
        {
            string newPath = "";
            var success = MoveImageFile(imgInfo.ImgPath, newDirectory, out newPath);
            if (success)
            {
                //change file path to new filepath
                var fileNameIndex = ImageFiles.IndexOf(imgInfo.ImgPath);
                if (fileNameIndex != -1) ImageFiles.Set(fileNameIndex, newPath);
            }
            return success;
        }

        private static bool MoveImageFile(string imgPath, string newDirectory, out string newPath)
        {
            newPath = imgPath;
            var success = !string.IsNullOrEmpty(imgPath) &&
                !string.IsNullOrWhiteSpace(imgPath) &&
                !string.IsNullOrEmpty(newDirectory) &&
                !string.IsNullOrWhiteSpace(newDirectory);
            if(success)
            {
                //try-catch this
                (new FileInfo(newDirectory)).Directory.Create();
                var possibleNewPath = Path.Combine(newDirectory, Path.GetFileName(imgPath));
                Debug.WriteLine("moved " + imgPath + " to " + possibleNewPath);
                File.Move(imgPath, Path.Combine(imgPath, possibleNewPath));
                newPath = possibleNewPath;
            }
            return success;
        }
    }
}