using ImageTagger;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ImageTagger.DataModels
{
    public class ImageTag : ImageTag_Base, IComparable<ImageTag>
    {

        public override string TagName { get { return base.TagName; } }
        public ImageTag(string tagName) : base(tagName)
        { }

        public int CompareTo(ImageTag other)
        {
            return base.CompareTo(other);
        }
    }

    public class EditableImageTag : ImageTag_Base, IComparable<ImageTag>
    {
        public EditableImageTag(string tagName) : base(tagName)
        { }

        public int CompareTo(ImageTag other)
        {
            return base.CompareTo(other);
        }
    }

    public abstract class ImageTag_Base : IComparable<ImageTag_Base>
    {

        public virtual string TagName { get; set; }
        public static readonly ImageTag NoTagsPlaceholder = new ImageTag("[no tags yet...]");

        public ImageTag_Base(string tagName)
        {
            var dump = new System.Collections.Generic.List<char>();
            TagName = FormatUtil.FixTag(tagName, TagCasing.SnakeCase, out dump);
        }

        public bool IsEmpty()
        {
            return TagName == null || TagName == "";
        }

        public int CompareTo(ImageTag_Base other)
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



