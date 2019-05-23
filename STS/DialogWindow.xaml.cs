using System;
using System.Collections.Generic;
using System.IO;
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

namespace STS
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        private bool isGT = false;
        private bool isWarning = false;
        private string subtitle = String.Empty;
        private string title = String.Empty;
        private string message = String.Empty;
        private string suggestion = String.Empty;
        public const int ErrorType = 0;
        public const int InfoType = 1;
        public const int GoogleType = 2;
        public const int WarningType = 3;

        public string SuggestedSub
        {
            get { return suggestion; }
        }

        public string DialogTitle
        {
            set { txTitle.Text = value; }
        }

        public string Message
        {
            set { txMsg.Text = value; }
        }
        public string Subtitle
        {
            set { subtitle = value; }
        }
        public int Type
        {
            set { CheckType(value); }
        }

        private void CheckType(int type)
        {
            
            switch (type)
            {
                case ErrorType:
                    icon.Content = Application.Current.Resources["Error"];
                    vbIcon.Margin = new Thickness(0, 0, 0, 0);
                    break;
                case InfoType:
                    icon.Content = Application.Current.Resources["Info"];
                    vbIcon.Margin = new Thickness(0, 0, 0, 0);
                    break;
                case GoogleType:
                    isGT = true;
                    FillLangCombos();
                    icon.Content = Application.Current.Resources["GTLogo2"];
                    btCancel.Visibility = Visibility.Visible;
                    gridGT.Visibility = Visibility.Visible;
                    break;
                case WarningType:
                    isWarning = true;
                    icon.Content = Application.Current.Resources["Warning"];
                    vbIcon.Margin = new Thickness(0, 0, 0, 0);
                    btCancel.Visibility = Visibility.Visible;
                    break;
            }
        }

        public DialogWindow()
        {            
            InitializeComponent();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {

            if (isGT)
            {
                string from = ((ComboBoxItem)cbFrom.SelectedItem).Tag.ToString();
                string to = ((ComboBoxItem)cbTo.SelectedItem).Tag.ToString();
                suggestion = SharedClasses.GetGoogleTranslation(subtitle, from, to);
                this.DialogResult = true;
                this.Close();
            }

            if (isWarning)
            {
                this.DialogResult = true;
                this.Close();
            }                

            this.Close();

        }

        private void FillLangCombos()
        {
            var langFile = Directory.GetCurrentDirectory() + @"\Assets\langs.csv";
            var languages = File.ReadAllLines(langFile);

            foreach (string lang in languages)
            {
                string[] elements = lang.Split(',');
                ComboBoxItem item = new ComboBoxItem();
                ComboBoxItem itemTo = new ComboBoxItem();
                item.Content = elements[0];
                item.Tag = elements[1];
                itemTo.Content = elements[0];
                itemTo.Tag = elements[1];
                cbFrom.Items.Add(item);
                cbTo.Items.Add(itemTo);
            }
        }
    }
}
