using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageTagger.DataModels
{
    public class ImageInfo
    {
        public ImageInfo()
        {

        }
        public ImageInfo(String imgPath)
        {
            ImgPath = imgPath; //Path.Combine(Environment.CurrentDirectory, "Bilder", "sas.png");
            Load();
        }
        public string ImgPath { get; set; }
        public BitmapImage ImgSource { get; private set; }

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
                    ImgSource = imgBitMap;
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