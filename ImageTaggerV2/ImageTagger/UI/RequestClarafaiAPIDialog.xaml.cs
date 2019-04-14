using ImageAnalysisAPI;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for RequestClarafaiAPIDialog.xaml
    /// </summary>
    public partial class RequestClarafaiAPIDialog : Window
    {
        public string ApiKey { get; private set; } = "";
        private RequestClarafaiAPIDialog()
        {
            InitializeComponent();
            var currentKey = SettingsPersistanceUtil.RetreiveSetting("apiKey");
            this.inputBox.Text = currentKey;
        }

        public static string GetAPIKeyViaDialog()
        {
            var result = "";
            var getKeyWindow = new RequestClarafaiAPIDialog();
            var success = getKeyWindow.ShowDialog() ?? false;
            if (success)
            {
                result = getKeyWindow.ApiKey;
                MessageBox.Show("api key is set to " + result);
                SettingsPersistanceUtil.RecordSetting("apiKey", result);
            }
            ImageAnalysis.RefreshAPIKey();
            return result;

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var inputBox = e.OriginalSource as TextBox;
            ApiKey = inputBox.Text;
            if (inputBox.Text == "")
                inputBox.Opacity = 0;
            else
                inputBox.Opacity = 1;
        }
    }
}
