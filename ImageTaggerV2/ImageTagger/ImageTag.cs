using ImageTagger;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ImageTagger.DataModels
{
    public class ImageTag : INotifyPropertyChanged, IComparable<ImageTag>
    {
        private string tagName = "";
        public string TagName { get=>tagName; set { tagName = FormatUtil.FixTag(value); NotifyPropertyChanged(); } }
        public static readonly ImageTag NoTagsPlaceholder = new ImageTag("[no tags yet...]");

        public ImageTag(string tagName)
        {
            TagName = tagName;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return TagName;
        }
    }
}



