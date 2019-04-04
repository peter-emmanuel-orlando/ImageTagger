using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageTagger.DataModels
{
    public class ImageInfo
    {
        public ImageInfo(String imgPath)
        {
            ImgPath = imgPath; //Path.Combine(Environment.CurrentDirectory, "Bilder", "sas.png");

        }

        private string imgPath;
        public string ImgPath
        {
            get
            {
                return imgPath;
            }
            set
            {
                    imgPath = value;
                    var uri = new Uri(imgPath);
                    try
                    {
                        var imgBitMap = new BitmapImage();
                        imgBitMap.BeginInit();
                        imgBitMap.UriSource = uri;
                        imgBitMap.CacheOption = BitmapCacheOption.OnLoad;
                        imgBitMap.EndInit();
                        ImgSource = imgBitMap;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
            }
        }
        public BitmapImage ImgSource { get; private set; }



        public static implicit operator ImageSource (ImageInfo iInfo)
        {
            return iInfo.ImgSource;
        }

        public override bool Equals(object obj)
        {
            var info = obj as ImageInfo;
            return info != null &&
                   imgPath == info.imgPath;
        }

        public override int GetHashCode()
        {
            return 485363467 + EqualityComparer<string>.Default.GetHashCode(imgPath);
        }


        //public int Width { get; set; }
        //public int Height { get; set; }
    }
}