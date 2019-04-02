using System;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageTagger_DataModels// <-- change to nested namespaces
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


        //public int Width { get; set; }
        //public int Height { get; set; }
    }
}