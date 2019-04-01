using System;

namespace ImageTagger_Model
{
    public class ImageTag : IComparable<ImageTag>
    {
        public ImageTag(string tagName)
        {
            TagName = tagName;
        }

        public string TagName { get; }

        public int CompareTo(ImageTag other)
        {
            return TagName.CompareTo(other.TagName);
        }

        public override string ToString()
        {
            return TagName;
        }
    }
}
