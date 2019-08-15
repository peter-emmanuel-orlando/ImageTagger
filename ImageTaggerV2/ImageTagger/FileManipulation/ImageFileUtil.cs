
using ImageTagger.DataModels;
using ImageTagger.UI;
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
                    if (tag.TagName.ToUpper().StartsWith("DATA:"))
                        tagComments += tag.TagName + " ";
                    else if (tag.TagName != "")
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

        public static string GetImageComments(string imagePath)
        {
            var result = "";
            if (File.Exists(imagePath))
            {
                try
                {
                    var sFile = ShellFile.FromParsingName(imagePath);
                    var tagsList = sFile.Properties.System.Comment;
                }
                catch (Exception e) { throw e; }
            }
            return result;
        }
        public static HashSet<ImageTag> GetImageTags(string imagePath)
        {
            var result = new HashSet<ImageTag>(new ImageTagEqualityComparer());

            //Debug.WriteLine("tags at: " + imagePath);
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
            sourcePath = sourcePath ?? PersistanceUtil.RetreiveSetting(Setting.SourceDirectory);
            if (!Directory.Exists(sourcePath))
                sourcePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            tagQueryCriteria = tagQueryCriteria ?? new TagQueryCriteria();

            var result = new List<string>();
            var query = tagQueryCriteria.GetQueryClause(sourcePath, out bool randomize);

            var windowsSearchConnection = @"Provider=Search.CollatorDSO.1;Extended Properties=""Application=Windows""";
			using (OleDbConnection connection = new OleDbConnection(windowsSearchConnection))
            {
                connection.Open();
				var command = new OleDbCommand(query, connection);
				try
				{
					using (var r = command.ExecuteReader())
					{
						var cont = true;
						var context = new CancelDialogDataContext();
						var dialogue = new CancelDialog(context);
						context.OnCancel = (n, m) => cont = false;
						context.OnClosed = (n, m) => context.OnCancel(n, null);

						dialogue.Show();
						while (cont)
						{
							try
							{
								cont = r.Read();
								var filepath = $"{r[0]}";

								if (IsJpg(filepath))
									result.Add(filepath);
							}
							catch (Exception e) { Debug.WriteLine(e); }
							finally { dialogue.Close(); }
						}
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
					var dialogue = new CancelDialog();
					var context = new CancelDialogDataContext();
					int i;
					var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
					context.MaxValue = files.Length;
					context.PerformAction = () =>
					{
						for (i = 0; i < files.Length; i++)
						{
							var fileName = files[i];
							if (IsJpg(fileName) && MatchesCriteria(fileName, tagQueryCriteria))
								result.Add(fileName);

							context.CurrentValue++;
						}
					};
					context.OnCancel = (n, m) => { i = files.Length; dialogue.Close(); };
					context.OnClosed = (n, m) => context.OnCancel(n, null);

					dialogue.SetContext(context);
					dialogue.ShowDialog();
				}
				finally
				{
					connection.Close();
				}
            }
            if (randomize) result.Shuffle();

            return result;
        }

		private static bool MatchesCriteria(string imgPath, TagQueryCriteria criteria)
		{
			var tags = GetImageTags(imgPath).Select(t => t.TagName);
			var success = true;
			var anyFound = false;

			foreach (var tag in tags)
			{
				anyFound |= criteria.anyOfThese.Contains(tag);

				success &= criteria.allOfThese.Count == 0 || criteria.allOfThese.Contains(tag)
					&& !criteria.noneOfThese.Contains(tag);

				if (!success) break;
			}
			success &= (anyFound || criteria.anyOfThese.Count == 0);

			return success;
		}
		private static bool IsJpg(string filepath)
		{
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

			return isJpg;
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
