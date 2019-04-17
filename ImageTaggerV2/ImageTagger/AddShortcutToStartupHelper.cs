using IWshRuntimeLibrary;
using System;
using System.IO;
using System.Reflection;

namespace ImageTagger
{
    public static class AddShortcutToStartupUtil
    {
        public static void Add()
        {
            var targetFileLocation = Assembly.GetExecutingAssembly().Location;
            string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutLocation = Path.Combine(startupPath, "ImageTagger" + ".lnk");
            if(!System.IO.File.Exists(shortcutLocation))
            {
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
}