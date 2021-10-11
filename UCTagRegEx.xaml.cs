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


        private void tbTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void SaveData()
        {
            if (!this.IsLoaded) return;
            SqlTagRegEx ParentTagRegEx = DataContext as SqlTagRegEx;
            double tmpOut;
            ParentTagRegEx.MinAge = double.Parse(tbMinAge.Text);
            ParentTagRegEx.MaxAge = double.Parse(tbMaxAge.Text);
            ParentTagRegEx.SaveToDB();
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            SqlTagRegEx ParentTagRegEx = DataContext as SqlTagRegEx;
            ParentTagRegEx.DeleteFromDB();
            DeleteMe(this, EventArgs.Empty);
        }


        private void ComboBoxTagRegExType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveData();
        }

        private void CbTargetSection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveData();
        }

        private void tblostfocus(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void cblostfocus(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
                bool approvedDecimalPoint = false;

                if (e.Text == ".")
                {
                    if (!((TextBox)sender).Text.Contains("."))
                        approvedDecimalPoint = true;
                }

                if (!(char.IsDigit(e.Text, e.Text.Length - 1) || approvedDecimalPoint))
                    e.Handled = true;
        }

        private void Btn_UpdateClick(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void cbTagRexExMatchResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveData();
        }

        private void cbTagRexExMatchType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveData();
        }

        private void cbTagRexExMatchNoResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveData();
        }
    }
}
