using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageTagger.DataModels
{
    public class ImageInfo : INotifyPropertyChanged
    {
        public ImageInfo()
        {

        }
        public ImageInfo(String imgPath)
        {
            ImgPath = imgPath; //Path.Combine(Environment.CurrentDirectory, "Bilder", "sas.png");
        }

        private string imgPath;
        public string ImgPath
        {
            get => imgPath;
            set
            {
                var prevPath = imgPath;
                imgPath = value;
                if (imgPath != prevPath) Unload();
                NotifyPropertyChanged();
            }
        }
        private HashSet<ImageTag> imgTags = new HashSet<ImageTag>();
        public HashSet<ImageTag> ImgTags { get => imgTags; set { imgTags = value; NotifyPropertyChanged(); } }
        private BitmapImage imgSource;
        public BitmapImage ImgSource { get => imgSource; set { imgSource = value; NotifyPropertyChanged(); } }
        public bool IsLoaded { get => ImgSource != null; }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool isLoading = false;
        public void Load(DispatcherPriority priority = DispatcherPriority.ApplicationIdle)
        {
            if(ImgPath != null)
            {
                isLoading = true;
                ImgTags = ImageFileUtil.GetImageTags(ImgPath);
                App.Current.Dispatcher.BeginInvoke(priority, new Action(() =>
                {
                    FileStream stream = null;
                    try
                    {
                        stream = File.OpenRead(ImgPath);
                        var imgBitMap = new BitmapImage();

                        imgBitMap.BeginInit();
                        imgBitMap.StreamSource = stream;
                        imgBitMap.CacheOption = BitmapCacheOption.OnLoad;
                        imgBitMap.EndInit();

                        imgBitMap.Freeze();
                        if (isLoading) ImgSource = imgBitMap;
                    }
                    catch (Exception) { }
                    finally
                    {
                        if (stream != null)
                        {
                            stream.Close();
                            stream.Dispose();
                        }
                        isLoading = false;
                    }
                }));
            }
        }

        public void Unload()
        {
            isLoading = false;
            ImgTags = null;
            ImgSource = null;
        }

        public void CloneFrom(ImageInfo other, DispatcherPriority priority = DispatcherPriority.ApplicationIdle)
        {
            ImgPath = other.ImgPath;
            ImgSource = other.ImgSource;
            ImgTags = other.ImgTags;
            if (other.isLoading) this.Load(priority);
        }

        public override bool Equals(object obj)
        {
            var info = obj as ImageInfo;
            return info != null &&
                   ImgPath == info.ImgPath;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() -1090012821 + EqualityComparer<string>.Default.GetHashCode(ImgPath);
        }


        //public int Width { get; set; }
        //public int Height { get; set; }
    }
}