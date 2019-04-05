using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace ImageTagger.TagManipulation.Internal
{ 
    public static class MyTagsRecord
    {
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
        
        public static void Persist(ConcurrentDictionary<string, HashSet<string>> toSave)
        {
            var filename = Path.Combine(TagsRecordDirectory, defaultTagRecordFilename + tagRecordFileExtension);
            var newTagRecordFile = JsonConvert.SerializeObject(toSave);
            var parsedJson = JToken.Parse(newTagRecordFile);
            newTagRecordFile = parsedJson.ToString(Formatting.Indented);
            File.WriteAllText(filename, newTagRecordFile);
            Debug.WriteLine("saved tags record to " + filename);
        }

        public static bool Load(ref ConcurrentDictionary<string, HashSet<string>> addTo, string tagsRecordDirectoryPath = "" )
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
                var tagsRecordDirty = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(fileString);

                foreach(string category in tagsRecordDirty.Keys)
                {
                    addTo.TryAdd( FormatUtil.FixCategory(category), new HashSet<string>( tagsRecordDirty[category].Select((s)=> { return FormatUtil.FixTag(s); })));
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
                Persist(addTo);// <-- so any user changes that have been formatted are persisted
            }
            return success;
        }
        
    }
}
