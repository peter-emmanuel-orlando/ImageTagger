
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace ImageTagger
{
    public partial class Launcher : Window
    {
        MainWindow main;
        NotifyIcon ni = new NotifyIcon();
        public Launcher()
        {
            InitializeComponent();
            this.Hide();
            this.ShowInTaskbar = true;

            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/ImageTagger;component/Resources/cherryBlossomIcon.ico")).Stream;
            ni.Icon = new Icon(iconStream);
            ni.Visible = true;
            /*
            ni.DoubleClick += delegate (object sender, EventArgs args)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            };
            */


            PersistanceUtil.LoadLocations();
            ImageFiles.ItemAdded += HandleItemAddedEvent;

            main = new MainWindow();
            main.Show();
            main.Closing += HandleMainWindowClosingEvent;
            main.Closed += HandleMainWindowClosedEvent;
        }

        HashSet<string> newFiles { get; } = new HashSet<string>();
        int showAfter = 5;
        private void HandleItemAddedEvent(object sender, FileSystemEventArgs e)
        {
            newFiles.Add(e.FullPath);
            if(newFiles.Count >= showAfter)
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var tempWindow = new MainWindow(true, null, true);
                    newFiles.Clear();
                    tempWindow.ShowDialog();
                }));
            }
        }

        private void HandleMainWindowClosingEvent(object sender, EventArgs e)
        {

        }

        private void HandleMainWindowClosedEvent(object sender, EventArgs e)
        {
            this.Close();
        }

        

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            ni.Visible = false;
            App.Current.Dispatcher.Invoke(() =>
            {
                base.OnClosing(e);
                //Debug.WriteLine(Environment.StackTrace);
                // close all active threads
                Environment.Exit(0);
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
    }
}