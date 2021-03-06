﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageTagger.DataModels
{
    public class ImageInfo : INotifyPropertyChanged
    {
        public ImageInfo()
        {
            Unload();
        }
        public ImageInfo(String imgPath) : this()
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
        public int PixelDimensions { get; private set; } = 0;
        public void Load(int pixelDimensions = -1, DispatcherPriority priority = DispatcherPriority.SystemIdle)
        {
            if (ImgPath != null)
            {
                if (pixelDimensions == this.PixelDimensions && IsLoaded)
                    return;

                var prevPixelDimensions = this.PixelDimensions;
                var timestamp = DateTime.Now.Ticks;
                App.Current.Dispatcher.BeginInvoke(priority, new Action(() =>
                {
                    if (pixelDimensions == this.PixelDimensions && IsLoaded)
                        return;
                    else
                    {
                        isLoading = true;
                        _Load(pixelDimensions, timestamp);
                    }
                }));
            }
        }

        private long mostRecentLoad = 0;
        private void _Load(int pixelDimensions, long timestamp)
        {
            if (timestamp < mostRecentLoad || !isLoading)//if this request is older than the most recent request, return
                return;

            if (ImgPath != null)
            {
                if (pixelDimensions == this.PixelDimensions && IsLoaded)
                    return;
                var prevPixelDimensions = this.PixelDimensions;
                FileStream stream = null;
                try
                {
                    isLoading = true;
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
                    this.PixelDimensions = pixelDimensions;
                    mostRecentLoad = timestamp;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    this.PixelDimensions = prevPixelDimensions;
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                    isLoading = false;
                }
            }
        }

        public void RequestLoad(int pixelDimensions = -1)
        {
            if (ImgPath != null)
            {
                if (pixelDimensions == this.PixelDimensions && IsLoaded)
                    return;

                var prevPixelDimensions = this.PixelDimensions;
                var timestamp = DateTime.Now.Ticks;
                ProcessRequest(() =>
                {
                    if (ImgPath != null)
                    {
                        var mem = 0;// Process.GetCurrentProcess().PrivateMemorySize64;// GC.GetTotalMemory(false);
                        if (pixelDimensions == this.PixelDimensions && IsLoaded)
                            return;
                        else if (mem > 700000000)
                        {
                            pixelDimensions = 150;
                        }
                        else
                        {
                            isLoading = true;
                            App.Current.Dispatcher.Invoke(() => _Load(pixelDimensions, timestamp), DispatcherPriority.ApplicationIdle);
                        }
                    }
                });
            }
        }


        private static bool isInProgress = false;
        private static readonly Stack<Action> toProcess = new Stack<Action>();
        private static void ProcessRequest(Action a)
        {
            toProcess.Push(a);
            if (!isInProgress)
            {
                isInProgress = true;
                Task.Run(async () =>
                {
                    while (toProcess.Count > 0)
                    {
                        var current = toProcess.Pop();
                        await App.Current.Dispatcher.InvokeAsync(current);
                    }
                    isInProgress = false;
                });
            }
        }
        public void Unload()
        {
            isLoading = false;
            mostRecentLoad = 0;
            ImgSource = null;
            //RequestLoad(150);
        }

        public void CloneFrom(ImageInfo other, int pixelDimensions = -1, DispatcherPriority priority = DispatcherPriority.ApplicationIdle)
        {
            ImgPath = other.ImgPath;
            //ImgSource = other.ImgSource;
            //pixelDimensions = int.MinValue;
            //mostRecentLoad = 0;
            isLoading = false;
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
            return base.GetHashCode() - 1090012821 + EqualityComparer<string>.Default.GetHashCode(ImgPath);
        }


        //public int Width { get; set; }
        //public int Height { get; set; }
    }
}