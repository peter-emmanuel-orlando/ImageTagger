using System.Collections.Generic;

namespace ImageTagger
{
    public static class ReservedTagCategories
    {
        public static HashSet<string> Names { get; } = new HashSet<string>(new string[] { "loaded", "insight" });
        public static bool Contains (string category)
        {
            return Names.Contains(category);
        }
    }
}
