using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.IO;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ImageTagger
{
	public enum Setting
	{
		SourceDirectory,
		DestinationDirectory,
		TagsRecordDirectory,
		ApiKey,
		SlideshowImgDelay,
	}
    public static class PersistanceUtil
    {
        private static Dictionary<Setting, string> ledger { get; set; } = new Dictionary<Setting, string>();
        public static string SettingsPersistanceDirectory { get { return Directory.GetCurrentDirectory(); } }
        private const string settingsFileExtension = ".tsettings";
        private const string defaultSettingsFilename = "lastSession";
        private static string DefaultImageDirectory { get { return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); } }


        static PersistanceUtil()
        {
            var filename = Path.Combine(SettingsPersistanceDirectory, defaultSettingsFilename + settingsFileExtension);
            if (!File.Exists(filename))
            {
                var newSettingsFile = JsonConvert.SerializeObject(new Dictionary<Setting, string>());
                File.WriteAllText(filename, "");
            }

            Load();
        }


        public static void RecordSetting(Setting setting, string value)
        {
            if (value == "" || value == null)
                EraseSetting(setting);
            else if (HasSetting(setting))
                ledger[setting] = value;
            else
                ledger.Add(setting, value);

            Persist();
        }

        public static void EraseSetting(Setting setting)
        {
            if (HasSetting(setting))
                ledger.Remove(setting);

            Persist();
        }

        public static bool HasSetting(Setting setting)
        {
            return ledger.ContainsKey(setting);
        }

        public static string RetreiveSetting(Setting setting)
        {
            if (HasSetting(setting))
                return ledger[setting];
            else
                return "";
        }

        private static void Persist()
        {
            var filename = Path.Combine(SettingsPersistanceDirectory, defaultSettingsFilename + settingsFileExtension);
            var newSettingsFile = JsonConvert.SerializeObject(ledger);
            var parsedJson = JToken.Parse(newSettingsFile);
            newSettingsFile = parsedJson.ToString(Formatting.Indented);
            File.WriteAllText(filename, newSettingsFile);
            Debug.WriteLine( "saved settings to " + filename);
        }

        private static bool Load()
        {
            var success = false;

            FileStream fs = null;
            StreamReader file = null;
            try
            {
                var filename = Path.Combine(SettingsPersistanceDirectory, defaultSettingsFilename + settingsFileExtension);
                fs = new FileStream(filename, FileMode.OpenOrCreate);
                file = new StreamReader(fs);
                var fileString = file.ReadToEnd();
                ledger = JsonConvert.DeserializeObject<Dictionary<Setting, string>>(fileString);
                if (ledger == null) ledger = new Dictionary<Setting, string>();
                Debug.WriteLine("successfully loaded settings.\n" + fileString);
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

				if (!Directory.Exists(RetreiveSetting(Setting.DestinationDirectory)))
					RecordSetting(Setting.DestinationDirectory, DefaultImageDirectory);
				if (!Directory.Exists(RetreiveSetting(Setting.SourceDirectory)))
					RecordSetting(Setting.SourceDirectory, DefaultImageDirectory);
			}
            return success;
        }


    }
}
