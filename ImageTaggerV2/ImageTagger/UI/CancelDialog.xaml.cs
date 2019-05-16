using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
        private CancellationToken cToken = new CancellationToken();
        public CancelDialog()
        {
            InitializeComponent();
        }
        public CancelDialog(CancelDialogDataContext context) : this()
        {
            SetContext(context);
        }
        public void SetContext(CancelDialogDataContext context)
        {
            this.Closed += context.OnClosed;
            cancelButton.Click += context.OnCancel;
            progressBar.DataContext = context;
            progressText.DataContext = context;
            if(!IsLoaded)
                Loaded += (s,e)=> Task.Run(context.PerformAction, cToken );
            else
                Task.Run(context.PerformAction, cToken);
        }
        
    }
    public class CancelDialogDataContext: INotifyPropertyChanged
    {
        private int maxValue = 0;
        private int currentValue = 0;
        private RoutedEventHandler onCancel = delegate { };
        private EventHandler onClosed = delegate { };
        private Action performAction = delegate { };


        public int MaxValue { get => maxValue; set { maxValue = value; NotifyPropertyChanged(); } }
        public int CurrentValue { get => currentValue; set { currentValue = value; NotifyPropertyChanged(); } }

        public RoutedEventHandler OnCancel { get => onCancel; set { onCancel = value; NotifyPropertyChanged(); } }
        public EventHandler OnClosed { get => onClosed; set { onClosed = value; NotifyPropertyChanged(); } }
        public Action PerformAction { get => performAction; set { performAction = value; NotifyPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



}
