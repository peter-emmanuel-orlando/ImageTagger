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
        public string ImgPath { get => imgPath; set { imgPath = value; NotifyPropertyChanged(); } }
        private BitmapImage imgSource;
        public BitmapImage ImgSource { get => imgSource; set { imgSource = value; NotifyPropertyChanged(); } }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool isLoading = false;
        public void Load()
        {
            if(ImgPath != null)
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
                    isLoading = true;
                    App.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                    {
                        if(isLoading)ImgSource = imgBitMap;
                    }));
                }
                catch (Exception) { }
                finally
                {
                    if(stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
            }
        }

        public void Unload()
        {
            isLoading = false;
            ImgSource = null;
        }


        public static implicit operator ImageSource (ImageInfo iInfo)
        {
            return iInfo.ImgSource;
        }

        public override bool Equals(object obj)
        {
            var info = obj as ImageInfo;
            return info != null &&
                   ImgPath == info.ImgPath;
        }

        public override int GetHashCode()
        {
            return 485363467 + EqualityComparer<string>.Default.GetHashCode(ImgPath);
        }


        //public int Width { get; set; }
        //public int Height { get; set; }
    }
}