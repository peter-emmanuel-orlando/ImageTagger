using ImageTagger;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ImageTagger_DataModels
{
    public class ImageTag : IComparable<ImageTag>
    {
        
        public string TagName { get; }
        public static readonly ImageTag NoTagsPlaceholder = new ImageTag("[no tags yet...]");

        public ImageTag(string tagName)
        {
            var dump = new System.Collections.Generic.List<char>();
            TagName = TagFormatUtil.Fix( tagName, TagCasing.SnakeCase, out dump );
        }

        public bool IsEmpty()
        {
            return TagName == null || TagName == "";
        }

        public int CompareTo(ImageTag other)
        {
            return TagName.CompareTo(other.TagName);
        }

        public override bool Equals(object obj)
        {
            var asImgTag = obj as ImageTag;
            if (asImgTag != null)
            {
                return TagName.Equals(asImgTag.TagName);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return TagName.GetHashCode();
        }

        public override string ToString()
        {
            return TagName;
        }
    }
}
