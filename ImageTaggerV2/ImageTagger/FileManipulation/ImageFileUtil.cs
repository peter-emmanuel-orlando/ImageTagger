
using ImageTagger.DataModels;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace ImageTagger
{
    public static class ImageFileUtil
    {

        public static bool ApplyTagsToImage(ImageInfo imageInfo, IEnumerable<ImageTag> tags)
        {
            return ApplyTagsToImage(imageInfo.ImgPath, tags);
        }

        public static void BatchApplyTagsToImages(Dictionary<string, IEnumerable<ImageTag>> toCombine)
        {
            foreach (var path in toCombine.Keys)
            {
                ApplyTagsToImage(path, toCombine[path]);
            }
        }

        public static bool ApplyTagsToImage(string imagePath, IEnumerable<ImageTag> tags)
        {
            try
            {
                var tagString = "";
                var tagComments = "";
                foreach (var tag in tags)
                {
                    if (tag.TagName.StartsWith("DATA:"))
                        tagComments += tag.TagName + " ";
                    if (tag.TagName != "")
                        tagString += tag.TagName + "; ";
                }
                var sFile = ShellFile.FromParsingName(imagePath);
                using (var w = sFile.Properties.GetPropertyWriter())
                {
                    w.WriteProperty(SystemProperties.System.Comment, tagComments);
                    w.WriteProperty(SystemProperties.System.Keywords, tagString);
                }
                Debug.WriteLine("applied tags to file successfully");
                return true;
            }
            catch (InvalidComObjectException e)
            {
                MessageBox.Show("failed to apply changes! Error message:[ " + e + " ]");
                Debug.WriteLine("unsuccessfully applied tags to file");
                return false;
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
            var result = new HashSet<ImageTag>(new ImageTagEqualityComparer());

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
                            var newTag = new ImageTag(tagText);
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
            if (!Directory.Exists(sourcePath))
                sourcePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            tagQueryCriteria = tagQueryCriteria ?? new TagQueryCriteria();

            var result = new List<string>();
            var query = tagQueryCriteria.GetQueryClause(sourcePath, out bool randomize);

            var windowsSearchConnection = @"Provider=Search.CollatorDSO;Extended Properties=""Application=Windows""";
            using (OleDbConnection connection = new OleDbConnection(windowsSearchConnection))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand(query, connection);
                using (var r = command.ExecuteReader())
                {
                    var cont = true;
                    while (cont)
                    {
                        try
                        {
                            cont = r.Read();
                            var filepath = $"{r[0]}";
                            //var x = r[1];
                            //var y = r[2];
                            var isJpg = false;
                            using (var stream = new FileStream(filepath, FileMode.Open))
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                                string bytes = stream.ReadByte().ToString("X2");
                                bytes += stream.ReadByte().ToString("X2");
                                isJpg = bytes == "FFD8";
                                //Debug.WriteLine(bytes + " " + isJpg);
                            }

                            if (isJpg)
                                result.Add(filepath);
                        }
                        catch (Exception e) { Debug.WriteLine(e); }
                    }
                }
            }
            if (randomize) result.Shuffle();

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
    }
}
