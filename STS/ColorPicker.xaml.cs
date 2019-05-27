using System.Windows;

namespace STS
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        public string PickedColor
        {
            get { return cvColor.HexadecimalString; }
        }
        private string picked;
        public ColorPicker()
        {
            InitializeComponent();
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
