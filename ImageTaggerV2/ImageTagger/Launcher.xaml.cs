
using ImageTagger.UI;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Linq;
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
        private ObservableCollection<SearchWindow> searchWindows { get; } = new ObservableCollection<SearchWindow>();
        private NotifyIcon nIcon { get; } = new NotifyIcon();

        private void SetUpNotificationIcon()
        {
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/ImageTagger;component/Resources/cherryBlossomIcon.ico")).Stream;
            nIcon.Icon = new Icon(iconStream);
            nIcon.Text = "image tagger will remain active in background";
            var defaultItems = new MenuItem[]
            {
                new MenuItem("new window", (s, e) => OpenNewWindow()),
                new MenuItem("exit", (s, e) => { Launcher.instance?.Close(); }),
            };


            searchWindows.CollectionChanged += (source, eventArgs) =>
            {
                var mItems = new List<MenuItem>(searchWindows.Select((item, index) =>
                {
                    var result = new MenuItem($"window {index}", (s, e) => 
                    {
                        if (!item.viewSearchWindow.IsVisible)
                        {
                            item.viewSearchWindow.Show();
                            item.viewSearchWindow.WindowState = WindowState.Normal;
                            item.viewSearchWindow.Activate();
                        }
                        else
                            item.viewSearchWindow.Hide();
                    });
                    return result;
                }));
                mItems.AddRange(defaultItems);
                nIcon.ContextMenu = new ContextMenu(mItems.ToArray());
            };
            nIcon.DoubleClick += (s,e)=>
            {
                /*
                var anyVisible = false;
                foreach (var item in searchWindows)
                {
                    if (item.viewSearchWindow.IsVisible && item.viewSearchWindow.WindowState == WindowState.Normal)
                    {
                        anyVisible = true;
                        break;
                    }
                }
                foreach (var item in searchWindows)
                {
                    if (anyVisible)
                    {
                        //item.viewSearchWindow.Hide();
                        item.viewSearchWindow.WindowState = WindowState.Minimized;
                    }
                    else
                    {
                        item.viewSearchWindow.Show();
                        item.viewSearchWindow.WindowState = WindowState.Normal;
                    }
                }
                */
                var first = searchWindows.FirstOrDefault();
                if(first != null)
                {
                    first.viewSearchWindow.Show();
                    first.viewSearchWindow.WindowState = WindowState.Normal;
                    first.viewSearchWindow.Activate();
                }
            };
            nIcon.ContextMenu = new ContextMenu(defaultItems);
            nIcon.Visible = true;
        }

        public static SearchWindow OpenNewWindow()
        {
            var newSearchWindow = GetNewSearchWindow();
            Launcher.instance.searchWindows.Add(newSearchWindow);
            newSearchWindow.Show();

            newSearchWindow.Closed += Launcher.instance.HandleSearchWindowClosedEvent;

            return newSearchWindow;
        }

        public Launcher()
        {
            InitializeComponent();
            instance = this;

            AddShortcutToStartupUtil.Add();

            SetUpNotificationIcon();

            this.Show();
            this.Hide();
            this.ShowInTaskbar = false;

            SetUpNotificationIcon();
            nIcon.Visible = true;
			
            var newWindow = OpenNewWindow();
            var v = Environment.GetCommandLineArgs();
            foreach (var item in v)
            {
                //System.Windows.Forms.MessageBox.Show(item);
                if (item.ToLower().Replace(" ", "").Contains("minimized"))
                    newWindow.viewSearchWindow.Hide();
            }
        }

        private void HandleSearchWindowClosedEvent(object sender, EventArgs e)
        {
            var searchWindow = sender as SearchWindow;
            searchWindows.Remove(searchWindow);
            if(searchWindows.Count == 0)
            {
                nIcon.Visible = false;
                this.Close();
            }
        }

        

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            nIcon.Visible = false;
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var window in searchWindows)
                {
                    window.Close();
                }
                base.OnClosing(e);
                //Debug.WriteLine(Environment.StackTrace);
                // close all active threads
                Environment.Exit(0);
            }, System.Windows.Threading.DispatcherPriority.SystemIdle);
        }
    }
}
