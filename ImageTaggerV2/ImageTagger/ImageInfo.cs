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
        private int desiredDimensions = 100;
        public int DesiredDimensions { get => desiredDimensions.Clamp(0, int.MaxValue); set { desiredDimensions = value; NotifyPropertyChanged(); } }
        private BitmapImage imgSource;
        public BitmapImage ImgSource { get => imgSource; private set { imgSource = value; NotifyPropertyChanged(); } }
        public bool IsLoaded { get => ImgSource != null; }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool isLoading = false;
        private int pixelDimensions = 0;
        /// <summary>
        /// dimension of -1 will set to maxResolution
        /// </summary>
        /// <param name="pixelDimensions"></param>
        /// <param name="priority"></param>
        public void Load( int pixelDimensions = -1, DispatcherPriority priority = DispatcherPriority.ContextIdle)
        {
            if(ImgPath != null)
            {
                if (pixelDimensions == this.pixelDimensions && IsLoaded)
                    return;

                isLoading = true;
                var prevPixelDimensions = this.pixelDimensions;
                //ImgTags = ImageFileUtil.GetImageTags(ImgPath);
                App.Current.Dispatcher.BeginInvoke(priority, new Action(() =>
                {
                    if (pixelDimensions == this.pixelDimensions && IsLoaded)
                        return;
                    FileStream stream = null;
                    try
                    {
                        stream = File.OpenRead(ImgPath);
                        var imgBitMap = new BitmapImage();

                        imgBitMap.BeginInit();
                        imgBitMap.StreamSource = stream;
                        imgBitMap.CacheOption = BitmapCacheOption.OnLoad;
                        if (pixelDimensions >= 0)
                            imgBitMap.DecodePixelWidth = pixelDimensions;
                        imgBitMap.EndInit();

                        imgBitMap.Freeze();
                        if (isLoading)
                        {
                            ImgSource = imgBitMap;
                        }
                        this.pixelDimensions = pixelDimensions;
                    }
                    catch (Exception e) { Debug.WriteLine(e); this.pixelDimensions = prevPixelDimensions; }
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
            //ImgTags = null;
            ImgSource = null;
        }

        public void CloneFrom(ImageInfo other, int pixelDimensions = -1, DispatcherPriority priority = DispatcherPriority.ApplicationIdle)
        {
            ImgPath = other.ImgPath;
            ImgSource = other.ImgSource;
            //ImgTags = other.ImgTags;
            this.pixelDimensions = -int.MaxValue;
            this.Load(pixelDimensions, priority);
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