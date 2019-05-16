using System.Diagnostics;

public static class ExplorerNavigateUtil
{
    public static void GoTo( string fullPath)
    {
        //var root = $"/root,\"{imgPath}\"";
        //var filename = $"/select,\"{imgPath}\"";
        //var args = "/e," + root + "," + filename;
        //args = args.Replace(@"\\", @"\");
        fullPath = System.IO.Path.GetFullPath(fullPath);
        var pKeep = Process.Start("Explorer.exe", string.Format("/select,\"{0}\"", fullPath));
    }
}
