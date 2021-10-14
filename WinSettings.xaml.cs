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

namespace AI_Note_Review
{
    /// <summary>
    /// Interaction logic for WinSettings.xaml
    /// </summary>
    public partial class WinSettings : Window
    {
        public WinSettings()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_ClickSave(object sender, RoutedEventArgs e)
        {
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.Save();
            
        }

        private void cbYesNo_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbYesNo_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
