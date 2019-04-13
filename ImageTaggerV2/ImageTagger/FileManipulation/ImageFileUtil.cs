
using ImageTagger.DataModels;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
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

        public static List<string> GetImageFilenames(string sourcePath = null, TagQueryCriteria tagQueryCriteria = null)
        {
            sourcePath = sourcePath ?? PersistanceUtil.SourceDirectory;
            tagQueryCriteria = tagQueryCriteria ?? new TagQueryCriteria();

            var result = new List<string>();

            var query = $"SELECT System.ItemPathDisplay FROM SystemIndex WHERE SCOPE='{PersistanceUtil.SourceDirectory}'" +
                @" AND (System.ItemName LIKE '%.jpg' OR System.ItemName LIKE '%.jpeg')";
            if (tagQueryCriteria != null) query += " AND " + tagQueryCriteria.GetQueryClause();


            var windowsSearchConnection = @"Provider=Search.CollatorDSO;Extended Properties=""Application=Windows""";
            using (OleDbConnection connection = new OleDbConnection(windowsSearchConnection))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand(query, connection);
                using (var r = command.ExecuteReader())
                {
                    while (r.Read())
                    {
                        result.Add("" + r[0]);
                    }
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



        public static bool MoveFile(string imgPath, string newDirectory, out string newPath)
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
