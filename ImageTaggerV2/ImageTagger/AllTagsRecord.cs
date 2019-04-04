using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace ImageTagger
{
    public static class AllTagsRecord
    {
        private static Dictionary<string, HashSet<string>> TagsRecord { get; set; } = new Dictionary<string, HashSet<string>>();
        public static string TagsRecordDirectory
        {
            get
            {
                var result = SettingsPersistanceUtil.RetreiveSetting("TagsRecordDirectory");
                if(result == "")
                {
                    result = SettingsPersistanceUtil.SettingsPersistanceDirectory;
                    SettingsPersistanceUtil.RecordSetting("TagsRecordDirectory", result);
                }
                return result;
            }
        }
        private const string tagRecordFileExtension = ".tagsRec";
        private const string defaultTagRecordFilename = "myTags";

        static AllTagsRecord()
        {
            Load();
        }

        private static string FormatCategory(string category)
        {
            return category.ToLower();
        }


        public static void RecordTag(string category, string tagText)
        {
            tagText = TagFormatUtil.Fix(tagText);
            category = FormatCategory(category);
            if (tagText == "" || tagText == null)
                return;
            else if (HasCategory(category))
                TagsRecord[category].Add(tagText);
            else
            {
                AddCategory(category);
                TagsRecord[category].Add(tagText);
            }

            Persist();
        }

        private static HashSet<string> reservedCategories { get; } = new HashSet<string>(new string[] { "all", "loaded" });
        public static void AddCategory(string category)
        {
            category = FormatCategory(category);
            if (reservedCategories.Contains(category)) throw new ArgumentException(string.Join(", ", reservedCategories) + " are reserved categories");
            if (!HasCategory(category))
                TagsRecord.Add(category, new HashSet<string>());
        }



        public static void RemoveTagFromRecord(string tag)
        {
            foreach (var category in TagsRecord.Keys)
            {
                if (TagsRecord[category].Contains(tag))
                {
                    TagsRecord[category].Remove(tag);
                }
            }

            Persist();
        }


        public static void RemoveTagFromCategory(string category, string tag)
        {
            if (TagsRecord[category].Contains(tag))
            {
                TagsRecord[category].Remove(tag);
            }

            Persist();
        }

        public static bool HasCategory(string setting)
        {
            return TagsRecord.ContainsKey(setting);
        }

        public static bool HasTag(string tag)
        {
            foreach (var category in TagsRecord.Keys)
            {
                if (TagsRecord[category].Contains(tag))
                {
                    return true;
                }
            }
            return false;
        }

        public static string[] RetreiveTags(string category)
        {
            if (HasCategory(category))
                return TagsRecord[category].ToArray();
            else
                return new string[] { };
        }

        private static void Persist()
        {
            var filename = Path.Combine(TagsRecordDirectory, defaultTagRecordFilename + tagRecordFileExtension);
            var newSettingsFile = new JavaScriptSerializer().Serialize(TagsRecord);
            newSettingsFile = newSettingsFile.Replace("{", "{\r\n\t");
            newSettingsFile = newSettingsFile.Replace("[", "\r\n\t[\r\n\t\t");
            newSettingsFile = newSettingsFile.Replace("\",\"", "\",\r\n\t\t\"");
            newSettingsFile = newSettingsFile.Replace("],", "\r\n\t],\r\n\r\n\t");
            newSettingsFile = newSettingsFile.Replace("]}", "\r\n\t]\r\n}");
            File.WriteAllText(filename, newSettingsFile);
            Debug.WriteLine("saved tags record to " + filename);
        }

        public static bool Load(string tagsRecordDirectoryPath = "")
        {
            var success = false;

            FileStream fs = null;
            StreamReader file = null;
            try
            {
                if (string.IsNullOrEmpty(tagsRecordDirectoryPath) || !File.Exists(tagsRecordDirectoryPath))
                {
                    tagsRecordDirectoryPath = Path.Combine(TagsRecordDirectory, defaultTagRecordFilename + tagRecordFileExtension);
                    string[] files = Directory.GetFiles(TagsRecordDirectory, "*" + tagRecordFileExtension);
                    Array.Sort(files);
                    if (files.Length > 0) tagsRecordDirectoryPath = files[0];
                }
                fs = new FileStream(tagsRecordDirectoryPath, FileMode.OpenOrCreate);
                file = new StreamReader(fs);
                var fileString = file.ReadToEnd();
                var tagsRecordDirty = new JavaScriptSerializer(new TagRecordTypeResolver()).Deserialize<Dictionary<string, List<string>>>(fileString);
                TagsRecord.Clear();
                foreach(string category in tagsRecordDirty.Keys)
                {
                    TagsRecord.Add( FormatCategory(category), new HashSet<string>( tagsRecordDirty[category].Select((s)=> { return TagFormatUtil.Fix(s); })));
                }
                Debug.WriteLine("successfully loaded tags record.\n" + fileString);
                success = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                file?.Close();
                fs?.Close();
                Persist();// <-- so any user changes that have been formatted are persisted
            }
            return success;
        }


        private class TagRecordTypeResolver : JavaScriptTypeResolver
        {
            public override Type ResolveType(string id)
            {
                return Type.GetType(id);
            }

            public override string ResolveTypeId(Type type)
            {
                if (type == null)
                {
                    throw new ArgumentNullException("type can't be null");
                }

                return type.Name;
            }
        }
    }
}
