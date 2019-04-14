using IWshRuntimeLibrary;
using System;
using System.IO;
using System.Reflection;

namespace ImageTagger
{
    public static class AddShortcutToStartupHelper
    {
        public static void Add(string publisherName, string productName)
        {
            var targetFileLocation = Assembly.GetExecutingAssembly().Location;
            string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutLocation = Path.Combine(startupPath, "ImageTagger" + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.Description = "ImageTagger";   // The description of the shortcut
            //shortcut.IconLocation = @"";           // The icon of the shortcut
            shortcut.TargetPath = targetFileLocation;                 // The path of the file that will launch when the shortcut is run
            shortcut.Arguments = "Minimized";
            shortcut.Save();
        }
    }
}