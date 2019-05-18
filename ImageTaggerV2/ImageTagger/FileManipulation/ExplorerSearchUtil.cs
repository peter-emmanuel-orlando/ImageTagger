using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ImageTagger.UI
{
    public static class ExplorerSearchUtil
    {
        private const string genericSearch =
           @"<?xml version=""1.0""?>

                <persistedQuery version = ""1.0"" >
                    <viewInfo viewMode=""icons"" iconSize=""96"" stackIconSize=""0"" displayName=""Search Results in Subfolders"" autoListFlags=""0"">
                    </viewInfo>
                    <query>
	                    <conditions>
		                    ConditionsPlaceHolder
                        </conditions>
                        <kindList>
                            <kind name = ""item"" />
                        </kindList >
                        <scope >
                            <include path=""PathPlaceHolder"" nonRecursive=""false""/>
                        </scope>
                    </query>
                </persistedQuery>
            ";
        private const string genericEqualityTestCondition =
           @"
                                <condition 
                                    type=""leafCondition""
                                    property=""System.FileName"" 
                                    operator=""eq""
                                    value=""PlaceHolder""
                                />";
        private const string genericOrConditionContainer =
        @"  
                            <condition type=""orCondition"">
                                PlaceHolder
                            </condition>
            ";


        public static Process ShowInFolder( params string[] selectedFiles)
        {
            return ShowInFolder(ProcessWindowStyle.Maximized, selectedFiles);
        }
        public static Process ShowInFolder(ProcessWindowStyle windowStyle = ProcessWindowStyle.Maximized, params string[] selectedFiles)
        {
            return ShowInFolder((IEnumerable<string>)selectedFiles, windowStyle);
        }
        public static Process ShowInFolder(IEnumerable<string> selectedFiles, ProcessWindowStyle windowStyle = ProcessWindowStyle.Maximized)
        {
            if (selectedFiles == null || !selectedFiles.Any())
                return null;

            var rootDirectory = System.IO.Path.GetDirectoryName(selectedFiles.First());
            foreach (var path in selectedFiles)
            {
                //var otherDirectory = System.IO.Path.GetDirectoryName(path); //not neccessary
                rootDirectory = string.Concat(rootDirectory.TakeWhile((thisChar, index) => thisChar == path[index]));
            }

            var searchMSContent = genericSearch;
            searchMSContent = searchMSContent.Replace("PathPlaceHolder", rootDirectory);
            searchMSContent = searchMSContent.Replace("ConditionsPlaceHolder", genericOrConditionContainer);

            var conditionsString = "";
            foreach (var filePath in selectedFiles)
            {
                conditionsString += genericEqualityTestCondition.Replace("PlaceHolder", System.IO.Path.GetFileName(filePath)) + "\n\r";
            }
            searchMSContent = searchMSContent.Replace("PlaceHolder", conditionsString);

            var filename = Directory.GetCurrentDirectory() + @"\search.search-ms";
            File.WriteAllText(filename, searchMSContent);
            Process search = new Process();
            search.StartInfo.WindowStyle = windowStyle;
            search.StartInfo.FileName = filename;
            search.Start();
            return search;
        }
        public static void MoveWindow(Process windowProcess, int x, int y, int width, int height, bool repaint)
        {
            try
            {
                MoveWindow(windowProcess.MainWindowHandle, x, y, width, height, repaint);
            }
            catch (Exception e) { Debug.WriteLine(e); }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    }
}
