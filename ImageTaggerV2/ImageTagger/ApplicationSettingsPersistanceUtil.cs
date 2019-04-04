﻿using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.IO;
using System;

namespace ImageTagger
{
    public static class SettingsPersistanceUtil
    {
        private static Dictionary<string, string> ledger { get; set; } = new Dictionary<string, string>();
        public static string SettingsPersistanceDirectory { get { return Directory.GetCurrentDirectory(); } }
        private const string settingsFileExtension = ".tsettings";
        private const string defaultSettingsFilename = "lastSession";

        static SettingsPersistanceUtil()
        {
            Load();
        }


        public static void RecordSetting(string setting, string value)
        {
            if (value == "" || value == null)
                EraseSetting(setting);
            else if (HasSetting(setting))
                ledger[setting] = value;
            else
                ledger.Add(setting, value);

            Persist();
        }

        public static void EraseSetting(string setting)
        {
            if (HasSetting(setting))
                ledger.Remove(setting);

            Persist();
        }

        public static bool HasSetting(string setting)
        {
            return ledger.ContainsKey(setting);
        }

        public static string RetreiveSetting(string setting)
        {
            if (HasSetting(setting))
                return ledger[setting];
            else
                return "";
        }

        private static void Persist()
        {
            var filename = Path.Combine(SettingsPersistanceDirectory, defaultSettingsFilename + settingsFileExtension);
            var newSettingsFile = new JavaScriptSerializer().Serialize(ledger);
            newSettingsFile = newSettingsFile.Replace("{", "{\r\n\t");
            newSettingsFile = newSettingsFile.Replace(",", ",\r\n\t");
            newSettingsFile = newSettingsFile.Replace("}", "\r\n}");
            File.WriteAllText(filename, newSettingsFile);
            Debug.WriteLine( "saved settings to " + filename);
        }

        public static bool Load()
        {
            var success = false;

            FileStream fs = null;
            StreamReader file = null;
            try
            {
                var filename = Path.Combine(SettingsPersistanceDirectory, defaultSettingsFilename + settingsFileExtension);
                string[] files = Directory.GetFiles(SettingsPersistanceDirectory, "*" + settingsFileExtension);
                Array.Sort(files);
                if (files.Length > 0) filename = files[0];
                fs = new FileStream(filename, FileMode.OpenOrCreate);
                file = new StreamReader(fs);
                var fileString = file.ReadToEnd();
                ledger = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(fileString);
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
            }
            return success;
        }


    }
}
