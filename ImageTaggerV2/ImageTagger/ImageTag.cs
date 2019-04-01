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

        public ImageTag(string tagName)
        {
            TagName = tagName;
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
