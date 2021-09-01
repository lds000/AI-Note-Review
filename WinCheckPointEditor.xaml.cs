using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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

namespace AI_Note_Review
{
    /// <summary>
    /// Interaction logic for WinCheckPointEditor.xaml
    /// </summary>
    public partial class WinCheckPointEditor : Window
    {

        public SqlCheckpoint CurrentCheckpoint;

        public WinCheckPointEditor()
        {
            InitializeComponent();
        }
        public WinCheckPointEditor(SqlCheckpoint cp)
        {
            InitializeComponent();
            CurrentCheckpoint = cp;

            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                string sql = "Select * from CheckPointTypes;";
                cbTypes.ItemsSource = cnn.Query(sql).ToList();
                sql = "Select * from ICD10Segments  order by icd10Chapter, icd10CategoryStart;";
                cbTargetICD10.ItemsSource = cnn.Query(sql).ToList();
            }
            cbTargetSection.ItemsSource = CF.NoteSections;
            IList dictionaries = SpellCheck.GetCustomDictionaries(tbComment);
            dictionaries.Add(new Uri(@"pack://application:,,,/MedTerms.lex"));
            //I'm getting an error below
            try
            {
                tbComment.SpellCheck.IsEnabled = true;
            }
            catch (Exception)
            {

            }

            UpdateCurrentCheckPoint();
        }

        private void UpdateCurrentCheckPoint()
        {
            dpCheckpoint.DataContext = null;
            if (CurrentCheckpoint == null) return;
            dpCheckpoint.DataContext = CurrentCheckpoint;
            string strRtext = @"{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Times New Roman;}{\f2\fcharset0 Segoe UI;}}{\colortbl\red0\green0\blue0;\red255\green255\blue255;}\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs18\f2\cf0 \cf0\ql}}";
            if (CurrentCheckpoint != null)
            {
                if (CurrentCheckpoint.RichText != null)
                {
                    strRtext = CurrentCheckpoint.RichText;
                }
            }
            //this is not working. dang.
            StrToRTB(strRtext, myRTB);
            string str2 = RTBtoStr(myRTB);
            StrToRTB(str2, myRTB);

/*            
            using (var d = Dispatcher.DisableProcessing())
            {

                spTags.Children.Clear();
                foreach (SqlTag st in CurrentCheckpoint.GetTags())
                {
                    TextBlock tb = new TextBlock();
                    tb.Foreground = Brushes.White;
                    tb.Background = Brushes.Black;
                    tb.FontSize = 16;
                    tb.Text = st.TagText;
                    spTags.Children.Add(tb);
                    foreach (SqlTagRegEx strex in st.GetTagRegExs())
                    {
                        //spTags.Children.Add(MakeUC(strex));
                        UCTagRegEx uctrex = new UCTagRegEx(strex);
                        uctrex.DeleteMe += Uctrex_DeleteMe;
                        spTags.Children.Add(uctrex);
                    }

                    Button b = new Button();
                    b.Style = (Style)Application.Current.Resources["LinkButton"];
                    b.Content = "Add Search Pattern";
                    b.Margin = new Thickness(10, 0, 0, 0);
                    b.Tag = st;
                    b.Click += B_Click;
                    spTags.Children.Add(b);

                }
            }
  */      
        }

        private void Uctrex_DeleteMe(object sender, EventArgs e)
        {
            UpdateCurrentCheckPoint();
        }

        private void B_Click(object sender, RoutedEventArgs e)
        {
            WinEnterText wet = new WinEnterText();

            Button b = sender as Button;
            SqlTag st = b.Tag as SqlTag;

            SqlTagRegEx srex = new SqlTagRegEx(st.TagID, "Search Text", CurrentCheckpoint.TargetSection, 1);
            UpdateCurrentCheckPoint();
        }

        private void myRTB_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            CurrentCheckpoint.RichText = RTBtoStr(myRTB);
            CurrentCheckpoint.SaveToDB();
        }

        private void StrToRTB(string strIn, RichTextBox rtb)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(strIn);
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                TextRange tr = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                tr.Load(ms, DataFormats.Rtf);
            }
        }

        private string RTBtoStr(RichTextBox rtb)
        {
            string rtfText; //string to save to db
            TextRange tr = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Rtf);
                rtfText = Encoding.ASCII.GetString(ms.ToArray());
            }
            return rtfText;

        }

        private void CbTargetSection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb == null) return;
            SqlTagRegEx strex = cb.Tag as SqlTagRegEx;
            if (strex == null) return;
            strex.TargetSection = int.Parse(cb.SelectedValue.ToString());
            strex.SaveToDB();
        }
        private void cbTargetSection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            if (cbTargetSection.SelectedValue == null) return;
            CurrentCheckpoint.TargetSection = int.Parse(cbTargetSection.SelectedValue.ToString());
            CurrentCheckpoint.SaveToDB();
        }
        private void CbTagRegExType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb == null) return;
            SqlTagRegEx strex = cb.Tag as SqlTagRegEx;
            if (strex == null) return;
            strex.TagRegExType = int.Parse(cb.SelectedValue.ToString());
            strex.SaveToDB();
        }
        private void Tbtitle_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Text == "Search Text") tb.Text = "";

            tb.Foreground = Brushes.White;
        }

        private void bRemoveClick(object sender, RoutedEventArgs e)
        {

            Button b = sender as Button;
            SqlTagRegEx strex = b.Tag as SqlTagRegEx;
            strex.DeleteFromDB();
            UpdateCurrentCheckPoint();
        }

        private void Tbtitle_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null) return;
            SqlTagRegEx strex = tb.Tag as SqlTagRegEx;
            if (strex == null) return;
            strex.RegExText = tb.Text.Trim();
            strex.SaveToDB();
        }

        private void Img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image tmpImg = sender as Image;
            SqlTagRegEx strex = tmpImg.Tag as SqlTagRegEx;
            WinEnterText wet = new WinEnterText("Edit Regular Expression value", strex.RegExText);
            wet.Owner = this;
            wet.ShowDialog();
            if (wet.ReturnValue != null)
            {
                strex.RegExText = wet.ReturnValue;
                strex.SaveToDB();
                UpdateCurrentCheckPoint();
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CF.SetWindowPosition(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CF.SaveWindowPosition(this);
        }


        private void cbTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            if (cbTypes.SelectedValue == null) return;
            CurrentCheckpoint.CheckPointType = int.Parse(cbTypes.SelectedValue.ToString());
            CurrentCheckpoint.SaveToDB();
        }

        private void cbTargetICD10_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            if (cbTargetICD10.SelectedValue == null) return;
            CurrentCheckpoint.TargetICD10Segment = int.Parse(cbTargetICD10.SelectedValue.ToString());
            CurrentCheckpoint.SaveToDB();
        }

        private void tbComment_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            CurrentCheckpoint.Comment = tbComment.Text;
            CurrentCheckpoint.SaveToDB();
        }

        private void tbAction_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            CurrentCheckpoint.Action = tbAction.Text;
            CurrentCheckpoint.SaveToDB();
        }

        private void tbLink_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            CurrentCheckpoint.Link = tbLink.Text;
            CurrentCheckpoint.SaveToDB();
        }

        private void tbTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            CurrentCheckpoint.CheckPointTitle = tbTitle.Text;
            CurrentCheckpoint.SaveToDB();

        }


        private void AddTag(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            string strSuggest = "#";
            if (CurrentCheckpoint.CheckPointType == 1) strSuggest = "#Query";
            if (CurrentCheckpoint.CheckPointType == 2) strSuggest = "#Exam";
            if (CurrentCheckpoint.CheckPointType == 3) strSuggest = "#Lab";
            if (CurrentCheckpoint.CheckPointType == 4) strSuggest = "#Imaging";
            if (CurrentCheckpoint.CheckPointType == 5) strSuggest = "#Condition";
            if (CurrentCheckpoint.CheckPointType == 6) strSuggest = "#CurrentMed";
            if (CurrentCheckpoint.CheckPointType == 7) strSuggest = "#Edu";
            if (CurrentCheckpoint.CheckPointType == 8) strSuggest = "#Exam";
            if (CurrentCheckpoint.CheckPointType == 9) strSuggest = "#CurrentMed";
            if (CurrentCheckpoint.CheckPointType == 10) strSuggest = "#Demographic";
            if (CurrentCheckpoint.CheckPointType == 11) strSuggest = "#HPI";
            if (CurrentCheckpoint.CheckPointType == 12) strSuggest = "#Vitals";
            if (CurrentCheckpoint.CheckPointType == 13) strSuggest = "#Rx";
            if (CurrentCheckpoint.CheckPointType == 14) strSuggest = "#Refer";
            if (CurrentCheckpoint.CheckPointType == 15) strSuggest = "#BEERS";
            //WinEnterText wet = new WinEnterText("Please enter a unique (not previously used) name for the new tag.", strSuggest, 200);
            //wet.strExclusions = SqlLiteDataAccess.GetAllTags();
            //wet.Owner = this;
            //wet.ShowDialog();

            WinAddTag wat = new WinAddTag();
            wat.tbSearch.Text = strSuggest;
            wat.Owner = this;
            wat.ShowDialog();

            if (wat.ReturnValue != null)
            {
                SqlTag tg = SqlLiteDataAccess.GetTags(wat.ReturnValue).FirstOrDefault();
                if (tg == null) tg = new SqlTag(wat.ReturnValue);
                string sql = "";
                sql = $"INSERT INTO RelTagCheckPoint (TagID, CheckPointID) VALUES ({tg.TagID},{CurrentCheckpoint.CheckPointID});";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    cnn.Execute(sql);
                }

                SqlTagRegEx srex = new SqlTagRegEx(tg.TagID, "Search Text", CurrentCheckpoint.TargetSection, 1);

                UpdateCurrentCheckPoint();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UCTag1_AddMe(object sender, EventArgs e)
        {
            UpdateCurrentCheckPoint();
        }
    }
}
