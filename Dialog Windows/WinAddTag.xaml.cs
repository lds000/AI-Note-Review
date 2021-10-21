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
    /// Interaction logic for WinAddTag.xaml
    /// </summary>
    public partial class WinAddTag : Window
    {
        public WinAddTag()
        {
            InitializeComponent();
        }

        public string ReturnValue = null;
        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbSearch.Text.Length >= 2)
            {
                List<SqlTagVM> taglist = SqlLiteDataAccess.GetTags(tbSearch.Text);
                if (taglist.Count == 0)
                {
                    lbTags.Visibility = Visibility.Collapsed;
                    btnAdd.Visibility = Visibility.Visible;
                    btnAdd.Content = $"Add tag with title '{tbSearch.Text}'.";
                }
                else
                {
                    btnAdd.Visibility = Visibility.Hidden;
                    lbTags.Visibility = Visibility.Visible;
                    lbTags.ItemsSource = taglist;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ReturnValue = tbSearch.Text;
            if (tbSearch.Text.Trim() == "") ReturnValue = null;
            this.Close();
        }

        private void lbTags_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbTags.SelectedItem != null)
            {
                ReturnValue = lbTags.SelectedValue.ToString();
                this.Close();
            }
        }

        private void btnClueClick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            tbSearch.Text = b.Tag.ToString();
        }
    }
}
