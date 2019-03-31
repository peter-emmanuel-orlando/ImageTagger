using System;
using System.Windows.Media.Imaging;

namespace ImageTagger
{
    public partial class MainWindow
    {
        public class ImageSquare
        {
            public ImageSquare(String imgPath)
            {
                var path = imgPath; //Path.Combine(Environment.CurrentDirectory, "Bilder", "sas.png");
                var uri = new Uri(path);
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
            public BitmapImage ImgSource { get; set; }
            //public int Width { get; set; }
            //public int Height { get; set; }
        }
    }
}