using ImageTagger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ImageTagger.DataModels
{
    public class ImageTag : INotifyPropertyChanged, IComparable<ImageTag>, IEquatable<ImageTag>
    {
        private string tagName = "";
        public string TagName { get=>tagName; set { tagName = FormatUtil.FixTag(value); NotifyPropertyChanged(); } }
        
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
            return this.Equals(asImgTag);
        }

        public bool Equals(ImageTag other)
        {
            if (other != null)
            {
                return TagName.Equals(other.TagName);
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

    public class ImageTagEqualityComparer : IEqualityComparer<ImageTag>
    {
        public bool Equals(ImageTag x, ImageTag y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(ImageTag obj)
        {
            return obj.TagName.GetHashCode();
        }
    }
}



