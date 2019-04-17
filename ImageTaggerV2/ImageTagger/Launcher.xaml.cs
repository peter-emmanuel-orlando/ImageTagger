
using ImageTagger.UI;
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
        public static Launcher instance { get; private set; }
        public IEnumerable<SearchWindow> SearchWindows { get => searchWindows; }
        private HashSet<SearchWindow> searchWindows { get; } = new HashSet<SearchWindow>();
        private static NotifyIcon ni { get; } = new NotifyIcon();

        static Launcher()
        {
            SetUpNotificationIcon();
        }

        private static void SetUpNotificationIcon()
        {
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/ImageTagger;component/Resources/cherryBlossomIcon.ico")).Stream;
            ni.Icon = new Icon(iconStream);
            ni.Text = "image tagger will remain active in background";
            EventHandler OpenWindowEventHandler = delegate (object sender, EventArgs args)
            { };
            ni.DoubleClick += OpenWindowEventHandler;
            ni.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("open", OpenWindowEventHandler),
                new MenuItem("exit", (s, e)=>{Launcher.instance.Close(); }),
            });
            ni.Visible = true;
        }

        public static void OpenNewWindow()
        {
            var newSearchWindow = GetNewSearchWindow();
            //newSearchWindow.Owner = Launcher.instance;
            Launcher.instance.searchWindows.Add(newSearchWindow);
            newSearchWindow.Show();

            newSearchWindow.Closed += Launcher.instance.HandleSearchWindowClosedEvent;
        }

        public Launcher()
        {
            InitializeComponent();
            instance = this;

            AddShortcutToStartupUtil.Add();
            this.Show();
            this.Hide();
            this.ShowInTaskbar = false;


            PersistanceUtil.LoadLocations();
            ImageFiles.ItemAdded += HandleItemAddedEvent;

            OpenNewWindow();

            SetUpNotificationIcon();
            ni.Visible = true;
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

        private void HandleSearchWindowClosedEvent(object sender, EventArgs e)
        {
            var searchWindow = sender as SearchWindow;
            searchWindows.Remove(searchWindow);
            if(searchWindows.Count == 0)
            {
                ni.Visible = false;
                this.Close();
            }
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
                //Environment.Exit(0);
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
    }
}
