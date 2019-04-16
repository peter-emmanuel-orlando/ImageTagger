
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
            AddShortcutToStartupHelper.Add();
            this.Hide();
            this.ShowInTaskbar = true;


            PersistanceUtil.LoadLocations();
            ImageFiles.ItemAdded += HandleItemAddedEvent;

            main = new MainWindow();
            main.StateChanged += HandleMainWindowStateChangedEvent;
            main.Closing += HandleMainWindowClosingEvent;
            main.Closed += HandleMainWindowClosedEvent;

            SetUpNotificationIcon();

            main.Show();
        }
        private void SetUpNotificationIcon()
        {
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/ImageTagger;component/Resources/cherryBlossomIcon.ico")).Stream;
            ni.Icon = new Icon(iconStream);
            ni.Text = "image tagger will remain active in background";
            EventHandler OpenWindowEventHandler = delegate (object sender, EventArgs args)
            {
                main.Show();
                ni.Visible = false;
                main.WindowState = WindowState.Normal;
            };
            ni.DoubleClick += OpenWindowEventHandler;
            ni.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("open", OpenWindowEventHandler),
                new MenuItem("exit", (s, e)=>{this.Close(); }),
            });
        }

        HashSet<string> newFiles { get; } = new HashSet<string>();
        int showAfter = 8;
        private void HandleItemAddedEvent(object sender, FileSystemEventArgs e)
        {
            newFiles.Add(e.FullPath);
            if(newFiles.Count >= showAfter)
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var tempWindow = new MainWindow();
                    tempWindow.SetSearch(null, false, true);
                    newFiles.Clear();
                    tempWindow.ShowActivated = true;
                    tempWindow.ShowDialog();
                }));
            }
        }

        private void HandleMainWindowClosingEvent(object sender, EventArgs e)
        {

        }

        private void HandleMainWindowStateChangedEvent(object sender, EventArgs e)
        {
            if (main.WindowState == WindowState.Minimized)
            {
                main.Hide();
                ni.Visible = true;
            }
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
