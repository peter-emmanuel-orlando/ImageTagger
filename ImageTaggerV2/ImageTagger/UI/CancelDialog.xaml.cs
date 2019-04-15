using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageTagger.UI
{
    /// <summary>
    /// Interaction logic for CancelDialog.xaml
    /// </summary>
    public partial class CancelDialog : Window
    {
        public CancelDialog(CancelDialogDataContext context)
        {
            InitializeComponent();
            this.Closed += context.OnClosed;
            cancelButton.Click += context.OnCancel;
            progressBar.DataContext = context;
            progressText.DataContext = context;
        }
        
    }
    public class CancelDialogDataContext: INotifyPropertyChanged
    {
        private int maxValue = 0;
        private int currentValue = 0;
        private RoutedEventHandler onCancel = delegate { };
        private EventHandler onClosed = delegate { };


        public int MaxValue { get => maxValue; set { maxValue = value; NotifyPropertyChanged(); } }
        public int CurrentValue { get => currentValue; set { currentValue = value; NotifyPropertyChanged(); } }

        public RoutedEventHandler OnCancel { get => onCancel; set { onCancel = value; NotifyPropertyChanged(); } }
        public EventHandler OnClosed { get => onClosed; set { onClosed = value; NotifyPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



}
