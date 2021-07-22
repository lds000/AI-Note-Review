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
    /// Interaction logic for WinEnterText.xaml
    /// </summary>
    public partial class WinEnterText : Window
    {

        public string ReturnValue { get; set; }
        public WinEnterText()
        {
            InitializeComponent();
            tbReply.Focus();
            tbReply.SelectAll();
        }
        private int intMaxChar;

        public WinEnterText(string strHeader, string strSuggestion = "", int maxChar = int.MaxValue)
        {
            InitializeComponent();
            lblTitle.Content = strHeader;
            tbReply.Text = strSuggestion;
            intMaxChar = maxChar;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private bool boolGoToEnd = false;
        public void GoToEnd()
        {
            boolGoToEnd = true;
        }


        private bool allowReturns = false;
        public void AllowReturns()
        {
            tbReply.AcceptsReturn = true;
            allowReturns = true;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            ReturnValue = tbReply.Text;
            if (tbReply.Text == "") ReturnValue = null;
            this.Close();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) || (e.Key == Key.Tab))
            {
                if (allowReturns && e.Key == Key.Enter) return;
                ReturnValue = tbReply.Text;
                if (tbReply.Text == "") ReturnValue = null;
                this.Close();
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            ReturnValue = null;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbReply.Focus();
            if (boolGoToEnd)
            {
                tbReply.CaretIndex = tbReply.Text.Length;
            }
            else
            {
                tbReply.SelectAll();
            }
        }

        private void tbReply_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (tbReply.Text.Count() >= intMaxChar)
            {
                if (e.Key == Key.Back) return;
                if (e.Key == Key.Delete) return;
                if (e.Key == Key.Left) return;
                if (e.Key == Key.Right) return;
                if (e.Key == Key.Up) return;
                if (e.Key == Key.Down) return;
                if (e.Key == Key.LeftShift) return;
                if (e.Key == Key.RightShift) return;
                if (e.Key == Key.Enter) return;
                if (e.Key == Key.End) return;
                if (e.Key == Key.Home) return;

                e.Handled = true;
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            this.Focus();
            tbReply.Focus();
            if (tbReply.Text.Length != 0)
            {
                tbReply.SelectionStart = tbReply.Text.Length; // add some logic if length is 0
                tbReply.SelectionLength = 0;
            }
        }

        public List<string> strExclusions;
        private void tbReply_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbReply.Text.Length>=3)
            {
                tbReply.Background = Brushes.Black;
                tbReply.Foreground = Brushes.White;
                if (strExclusions != null)
                {
                    if (strExclusions.Contains(tbReply.Text))
                    {
                        tbReply.Background = Brushes.Red;
                        tbReply.Foreground = Brushes.Black;
                    }
                }
            }
        }
    }
}
