
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ImageTagger
{
    public partial class Launcher : Window
    {
        MainWindow main;
        public Launcher()
        {
            InitializeComponent();
            this.Hide();
            this.ShowInTaskbar = true;
            main = new MainWindow();
            main.Closing += HandleMainWindowClosingEvent;
            main.Closed += HandleMainWindowClosedEvent;
            main.Show();
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