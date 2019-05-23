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
using System.Data;
using System.IO;

namespace STS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int currentDialog;
        private string fileName;
        private DataTable loadedSubs;
        private DataSet subScript;
        private bool unsavedSubs = false;
        private bool fileOpened = false;
        public string suggestedTrans { get; set; }

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
            if (fileOpened)
            {
                if (!string.IsNullOrEmpty(txtTranslate.Text) || !string.IsNullOrWhiteSpace(txtTranslate.Text))
                {
                    if (String.IsNullOrEmpty(loadedSubs.Rows[currentDialog]["Translation"].ToString()))
                        unsavedSubs = true;

                    if (txtTranslate.Text.Trim() != loadedSubs.Rows[currentDialog]["Translation"].ToString())
                        unsavedSubs = true;
                }

                if (!IsUnsaved())
                {
                    Close();
                }
            }
            else
                Close();
        }
        private void Menu_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog filedialog = new OpenFileDialog();
            filedialog.Filter = "All files (*.*)|*.*|SubRip Subtitles (*.srt)|*.srt|Subtitle TranStation Project (*.tra)|*.tra";
            if (filedialog.ShowDialog() == true)
            {
                subScript = SharedClasses.CheckSubFile(filedialog.FileName);
                
                if(subScript != null)
                {
                    loadedSubs = subScript.Tables["Dialogue"];
                    fileName = filedialog.FileName;
                    currentDialog = 0;
                    UpdateCurrentDialog(loadedSubs, currentDialog, fileName);
                    fileOpened = true;
                    exportMenu.IsEnabled = true;
                }
            }               
        }

        private void Menu_Save(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtTranslate.Text) || string.IsNullOrWhiteSpace(txtTranslate.Text))
                loadedSubs.Rows[currentDialog]["Translation"] = txtTranslate.Text.Trim();

            string openFile = lblOpenFile.Content.ToString();
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Subtitle TranStation Project (*.tra)|*.tra";
            saveDialog.AddExtension = true;
            saveDialog.FileName = Path.GetFileNameWithoutExtension(lblOpenFile.Content.ToString());

            if(saveDialog.ShowDialog() == true)
            {
                if (SharedClasses.SaveProject(saveDialog.OpenFile(), subScript))
                {
                    string message = String.Format("The translation project has been saved successfully.", fileName);
                    DialogWindow errorDialog = new DialogWindow();
                    errorDialog.DialogTitle = "Exporting Subtitles";
                    errorDialog.Message = message;
                    errorDialog.Type = DialogWindow.InfoType;
                    errorDialog.Owner = this;
                    errorDialog.Width = 400;
                    errorDialog.Height = 120;
                    errorDialog.Show();
                } else
                {
                    string errorMsg = String.Format("An error ocurred while saving the translation project, please try again.", fileName);
                    DialogWindow errorDialog = new DialogWindow();
                    errorDialog.DialogTitle = "Exporting Subtitles";
                    errorDialog.Message = errorMsg;
                    errorDialog.Type = DialogWindow.ErrorType;
                    errorDialog.Owner = this;
                    errorDialog.Width = 400;
                    errorDialog.Height = 120;
                    errorDialog.Show();
                }
            }
        }

        private void Menu_Export(object sender, RoutedEventArgs e)
        {            
            string newFile = String.Empty;
            SaveFileDialog exportDialog = new SaveFileDialog();
            exportDialog.Filter = "SubRip Subtitles (*.srt) | *.srt|SubStation Alpha Subtitles (*.ass)|*.ass";
            exportDialog.AddExtension = true;            
            exportDialog.FileName = Path.GetFileNameWithoutExtension(lblOpenFile.Content.ToString());
            if(exportDialog.ShowDialog() == true)
            {
                try
                {
                    SharedClasses.ExportTranslation(exportDialog.FileName,
                                                    exportDialog.OpenFile(),
                                                    subScript);

                    string message = String.Format("The subtitles has been exported successfully.", fileName);
                    DialogWindow errorDialog = new DialogWindow();
                    errorDialog.DialogTitle = "Exporting Subtitles";
                    errorDialog.Message = message;
                    errorDialog.Type = DialogWindow.InfoType;
                    errorDialog.Owner = this;
                    errorDialog.Width = 400;
                    errorDialog.Height = 120;
                    errorDialog.Show();
                }
                catch (Exception)
                {
                    string errorMsg = String.Format("An error ocurred exporting the subtitles, please try again.", fileName);
                    DialogWindow errorDialog = new DialogWindow();
                    errorDialog.DialogTitle = "Exporting Subtitles";
                    errorDialog.Message = errorMsg;
                    errorDialog.Type = DialogWindow.ErrorType;
                    errorDialog.Owner = this;
                    errorDialog.Width = 400;
                    errorDialog.Height = 120;
                    errorDialog.Show();
                }
            }
        }

        private void ChangeDialogue(object sender,RoutedEventArgs e)
        {
            if (loadedSubs != null && loadedSubs.Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(txtTranslate.Text) || !string.IsNullOrWhiteSpace(txtTranslate.Text))
                {
                    loadedSubs.Rows[currentDialog]["Translation"] = txtTranslate.Text.Trim();
                    unsavedSubs = true;
                }
                    

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
                            DialogWindow errorDialog = new DialogWindow();
                            errorDialog.DialogTitle = "No more subtitles";
                            errorDialog.Message = errorMsg;
                            errorDialog.Type = DialogWindow.InfoType;
                            errorDialog.Owner = this;
                            errorDialog.Width = 400;
                            errorDialog.Height = 120;
                            errorDialog.Show();
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
                            DialogWindow errorDialog = new DialogWindow();
                            errorDialog.DialogTitle = "No more subtitles";
                            errorDialog.Message = errorMsg;
                            errorDialog.Owner = this;
                            errorDialog.Type = DialogWindow.InfoType;
                            errorDialog.Width = 400;
                            errorDialog.Height = 120;
                            errorDialog.Show();
                        }
                        break;
                    case "btLst":
                        currentDialog = loadedSubs.Rows.Count - 1;
                        UpdateCurrentDialog(loadedSubs, currentDialog, fileName);
                        break;
                    case "btGoogle":
                        string message = "This will generate a suggested translation for the current subtitle using the Google Translate service that it may not be accurate.";
                        DialogWindow dialog = new DialogWindow();
                        dialog.DialogTitle = "Suggested Translation";
                        dialog.Message = message;
                        dialog.Type = DialogWindow.GoogleType;
                        dialog.Subtitle = loadedSubs.Rows[currentDialog]["Dialog"].ToString();
                        dialog.Owner = this;                        
                        if (dialog.ShowDialog() == true)
                        {
                            string sugestion = dialog.SuggestedSub;
                            txtTranslate.Text = sugestion;
                            dialog.Close();
                        }
                        break;

                } 
            } 
        }

        private void UpdateCurrentDialog(DataTable loadedSubs, int currentDialog, string fileName)
        {
            lblOpenFile.Content = System.IO.Path.GetFileName(fileName);
            lblDialogNum.Content = String.Format("{0} of {1}",(currentDialog + 1),loadedSubs.Rows.Count);
            if (loadedSubs.Columns.Contains("Name"))
            {
                if (!string.IsNullOrEmpty(loadedSubs.Rows[currentDialog]["Name"].ToString()))
                {
                    lblCharacter.Content = loadedSubs.Rows[currentDialog]["Name"];
                } 
            }
            lblStartTime.Content = String.Format("Start: {0}", loadedSubs.Rows[currentDialog]["Start"].ToString());
            lblEndTime.Content = String.Format("End: {0}", loadedSubs.Rows[currentDialog]["End"].ToString());

            string htmlDialog = SharedClasses.VisualDialogue(loadedSubs.Rows[currentDialog]["Text"].ToString());
            txtDialog.NavigateToString(htmlDialog);
            txtTranslate.Text = loadedSubs.Rows[currentDialog]["Translation"].ToString();            
        }

        private bool IsUnsaved()
        {
            if (unsavedSubs)
            {
                string errorMsg = "Translation haven't been saved, all unsaved changes will be lost. Are you sure you want to continue?";
                DialogWindow unsavedDialog = new DialogWindow();
                unsavedDialog.DialogTitle = "Exporting Subtitles";
                unsavedDialog.Message = errorMsg;
                unsavedDialog.Type = DialogWindow.WarningType;
                unsavedDialog.Owner = this;
                unsavedDialog.Width = 400;
                unsavedDialog.Height = 120;
                if(unsavedDialog.ShowDialog() == false)
                {
                    return true;
                } 
            }
            return false;
        }

        private void txtTranslate_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                if (!string.IsNullOrEmpty(txtTranslate.Text) || !string.IsNullOrWhiteSpace(txtTranslate.Text))
                {
                    loadedSubs.Rows[currentDialog]["Translation"] = txtTranslate.Text.Trim();
                    unsavedSubs = true;
                }

                if (currentDialog != (loadedSubs.Rows.Count - 1))
                {
                    currentDialog++;
                    UpdateCurrentDialog(loadedSubs, currentDialog, fileName);
                }
                else
                {
                    string errorMsg = String.Format("This is the last subtitle in the file. There are not more subtitles availables.", fileName);
                    DialogWindow errorDialog = new DialogWindow();
                    errorDialog.DialogTitle = "No more subtitles";
                    errorDialog.Message = errorMsg;
                    errorDialog.Owner = this;
                    errorDialog.Type = DialogWindow.InfoType;
                    errorDialog.Width = 400;
                    errorDialog.Height = 120;
                    errorDialog.Show();
                }
            }

        }
    }
}
