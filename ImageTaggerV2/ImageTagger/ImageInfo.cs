﻿using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageTagger_Model
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
                    var imgBitMap = new BitmapImage(uri);
                    ImgSource = imgBitMap;
                }
                catch (System.NotSupportedException e)
                {
                    throw new NotSupportedException("this image type is not supported)", e);
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