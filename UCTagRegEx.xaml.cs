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

namespace AI_Note_Review
{
    /// <summary>
    /// Interaction logic for UCTagRegEx.xaml
    /// </summary>
    public partial class UCTagRegEx : UserControl
    {
        public event EventHandler DeleteMe;
        public UCTagRegEx()
        {
            InitializeComponent();

        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WinEnterText wet = new WinEnterText("Edit Regular Expression value", tbRegExSearchTerms.Text);
            wet.Owner = Window.GetWindow(this);
            wet.ShowDialog();
            if (wet.ReturnValue != null)
            {
                SqlTagRegEx ParentTagRegEx = DataContext as SqlTagRegEx;
                ParentTagRegEx.RegExText = wet.ReturnValue;
                tbRegExSearchTerms.Text = wet.ReturnValue;
                ParentTagRegEx.SaveToDB();
            }
        }

        private void tbTitle_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Text == "Search Text") tb.Text = "";
            tb.Foreground = Brushes.White;
        }

        private void tbTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            SqlTagRegEx ParentTagRegEx = DataContext as SqlTagRegEx;
            ParentTagRegEx.RegExText = tbRegExSearchTerms.Text.Trim();
            ParentTagRegEx.SaveToDB();

            tbRegExSearchTerms.Foreground = Brushes.Gray;
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            SqlTagRegEx ParentTagRegEx = DataContext as SqlTagRegEx;
            ParentTagRegEx.DeleteFromDB();
            DeleteMe(this, EventArgs.Empty);
        }

        private void ComboBoxTagRegExType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SqlTagRegEx ParentTagRegEx = DataContext as SqlTagRegEx;
            ParentTagRegEx.TagRegExType = int.Parse(cbTagRegExType.SelectedValue.ToString());
            ParentTagRegEx.SaveToDB();
        }

        private void CbTargetSection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SqlTagRegEx ParentTagRegEx = DataContext as SqlTagRegEx;
            ParentTagRegEx.TargetSection = int.Parse(cbTargetSection.SelectedValue.ToString());
            ParentTagRegEx.SaveToDB();
        }
    }
}
