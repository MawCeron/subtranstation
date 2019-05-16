using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

namespace Tets
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int currentDialog;
        private string fileName;
        private DataTable loadedSubs;

        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }        

        private void Window_Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Window_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Menu_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog filedialog = new OpenFileDialog();
            filedialog.Filter = "SubRip Subtitles (*.srt)|*.srt|All files (*.*)|*.*";
            if (filedialog.ShowDialog() == true)
            {
                loadedSubs = Classes.CheckSubFile(filedialog.FileName);
                if(loadedSubs != null)
                {
                    fileName = filedialog.FileName;
                    currentDialog = 0;
                    UpdateCurrentDialog(loadedSubs, currentDialog, fileName);
                }
            }
               
        }

        private void ChangeDialog(object sender,RoutedEventArgs e)
        {
            if (loadedSubs != null)
            {
                if (!string.IsNullOrEmpty(txtTranslate.Text) || string.IsNullOrWhiteSpace(txtTranslate.Text))
                    loadedSubs.Rows[currentDialog]["Translation"] = txtTranslate.Text.Trim();

                Button btn = (Button)sender;
                switch (btn.Name)
                {
                    case "btFst":
                        currentDialog = 0;
                        UpdateCurrentDialog(loadedSubs, currentDialog, fileName);
                        break;
                    case "btPrv":
                        if (currentDialog != 0)
                        {
                            currentDialog--;
                            UpdateCurrentDialog(loadedSubs, currentDialog, fileName);
                        }
                        else
                        {
                            string errorMsg = String.Format("This is the first subtitle in the file. There are no previous subtitles availables.", fileName);
                            MessageBox.Show(errorMsg, "No more subtitles", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        break;
                    case "btNxt":
                        if (currentDialog != (loadedSubs.Rows.Count - 1))
                        {
                            currentDialog++;
                            UpdateCurrentDialog(loadedSubs, currentDialog, fileName);
                        }
                        else
                        {
                            string errorMsg = String.Format("This is the last subtitle in the file. There are not more subtitles availables.", fileName);
                            MessageBox.Show(errorMsg, "No more subtitles", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        break;
                    case "btLst":
                        currentDialog = loadedSubs.Rows.Count - 1;
                        UpdateCurrentDialog(loadedSubs, currentDialog, fileName);
                        break;
                    case "btGoogle":
                        string alertMsg = "This will generate a suggested translation for the current subtitle using the Google Translate service that it may not be accurate. Do you want to continue?";
                        MessageBox.Show(alertMsg, "External translation", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        break;

                } 
            }
        }

        private void UpdateCurrentDialog(DataTable loadedSubs, int currentDialog, string fileName)
        {
            lblOpenFile.Content = System.IO.Path.GetFileName(fileName);
            lblDialogNum.Content = String.Format("{0} of {1}",(currentDialog + 1),loadedSubs.Rows.Count);
            if (loadedSubs.Columns.Contains("Character"))
                lblCharacter.Content = loadedSubs.Rows[currentDialog]["Character"];
            lblStartTime.Content = String.Format("Start: {0}", loadedSubs.Rows[currentDialog]["Start"].ToString());
            lblEndTime.Content = String.Format("End: {0}", loadedSubs.Rows[currentDialog]["End"].ToString());

            txtDialog.Text = loadedSubs.Rows[currentDialog]["Dialog"].ToString();
            txtTranslate.Text = loadedSubs.Rows[currentDialog]["Translation"].ToString();
        }
    }
}
