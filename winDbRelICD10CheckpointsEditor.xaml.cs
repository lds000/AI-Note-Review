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
    /// Interaction logic for winDbRelICD10CheckpointsEditor.xaml
    /// </summary>
    public partial class winDbRelICD10CheckpointsEditor : Window
    {

        SqlCheckpoint CurrentCheckpoint;
        public winDbRelICD10CheckpointsEditor()
        {
            InitializeComponent();
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                string sql = "Select * from ICD10Segments order by icd10Chapter, icd10CategoryStart;";
                SqlLiteDataAccess.ICD10Segments = cnn.Query<SqlICD10Segment>(sql).ToList();
                sql = "Select * from CheckPointTypes;";
                cbTypes.ItemsSource = cnn.Query(sql).ToList();
                sql = "Select * from ICD10Segments  order by icd10Chapter, icd10CategoryStart;";
                cbTargetICD10.ItemsSource = cnn.Query(sql).ToList();
            }
            lbICD10.ItemsSource = SqlLiteDataAccess.ICD10Segments;
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

            sliderValueChanged = false;
            slideSeverity.PreviewMouseUp += new MouseButtonEventHandler(slider_MouseUp);
            slideSeverity.ValueChanged += slider_ValueChanged;

        }

        private void SlideSeverity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            throw new NotImplementedException();
        }

        public bool sliderValueChanged { get; set; }

        void slider_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sliderValueChanged == true)
            {
                int tmpi = (int)slideSeverity.Value;
                CurrentCheckpoint.ErrorSeverity = tmpi;
                CurrentCheckpoint.SaveToDB();
                sliderValueChanged = false;
                e.Handled = true;
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sliderValueChanged = true;
        }


        private void closeclick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void lbICD10_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshICD10();
        }

        private void RefreshICD10()
        {
            SqlICD10Segment seg = lbICD10.SelectedItem as SqlICD10Segment;
            if (seg != null)
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = $"Select * from CheckPointSummary where ICD10SegmentID == {seg.ICD10SegmentID}";
                    try
                    {
                        lbCheckpoints.ItemsSource = cnn.Query(sql).ToList();
                        lbCheckpoints.SelectedValuePath = "CheckPointID";
                        CurrentCheckpoint = null;
                        UpdateCurrentCheckPoint();
                    }
                    catch (Exception e2)
                    {
                        Console.WriteLine($"Error on saving variation data: {e2.Message}");
                    }
                }

            }
        }

        private void lbCheckpoints_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SqlICD10Segment seg = lbICD10.SelectedItem as SqlICD10Segment;
            if (seg != null)
            {
                lbCheckpoints.ItemsSource = seg.GetCheckPoints();
            }
        }

        private void lbCheckpoints_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbCheckpoints.SelectedValue == null)
            {
                Console.WriteLine("null value selected;");
                return;
            }
           int selectedCheckPointID = int.Parse(lbCheckpoints.SelectedValue.ToString());
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                string sql = $"Select * from CheckPoints where CheckPointID == {selectedCheckPointID};";
                    CurrentCheckpoint = cnn.QuerySingle<SqlCheckpoint>(sql);
                    UpdateCurrentCheckPoint();
            }
        }

        private void UpdateCurrentCheckPoint()
        {

            dpCheckpoint.DataContext = null;

            if (CurrentCheckpoint == null) return;
            dpCheckpoint.DataContext = CurrentCheckpoint;

            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //watch.Stop();
            //Console.WriteLine($"GroupPhraseModel RemovePhrase Execution Time: {watch.ElapsedMilliseconds} ms");

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

        }

        private void B_Click(object sender, RoutedEventArgs e)
        {
            WinEnterText wet = new WinEnterText();

            Button b = sender as Button;
            SqlTag st = b.Tag as SqlTag;

            SqlTagRegEx srex = new SqlTagRegEx(st.TagID, "Search Text", CurrentCheckpoint.TargetSection, 1);
            UpdateCurrentCheckPoint();
        }

        private StackPanel MakeUC(SqlTagRegEx strex) //I did this manually instead of UserControl to improve performance... not sure if it helped.
        {
            StackPanel spMain = new StackPanel();
            StackPanel sp = new StackPanel();
            spMain.Children.Add(sp);
            sp.Orientation = Orientation.Horizontal;
            sp.HorizontalAlignment = HorizontalAlignment.Center;
            sp.DataContext = strex;

            Grid g = new Grid();
            
            TextBox tbtitle = new TextBox();
            tbtitle.Foreground = Brushes.White;
            tbtitle.Background = Brushes.Black;
            tbtitle.Text = strex.RegExText;
            tbtitle.GotFocus += Tbtitle_GotFocus;
            tbtitle.LostFocus += Tbtitle_LostFocus;
            if (strex.RegExText == "Search Text") tbtitle.Foreground = Brushes.Gray;
            tbtitle.Tag = strex;
            spMain.Children.Add(tbtitle);

            Image Img = new Image();
            Img.Source = new BitmapImage(
             new Uri(@"pack://application:,,,/Icons/edit_notes.png"));
            Img.Width = 15;
            Img.Tag = strex;
            Img.MouseDown += Img_MouseDown;
            sp.Children.Add(Img);

            ComboBox cbTagRegExType = new ComboBox();
            cbTagRegExType.Width = 60;
            cbTagRegExType.DisplayMemberPath = "TagRegExTypeTitle";
            cbTagRegExType.SelectedValuePath = "TagRegExTypeID";
            Binding myBinding = new Binding("TagRegExType");
            myBinding.Source = strex;
            cbTagRegExType.SetBinding(ComboBox.SelectedValueProperty, myBinding);
            cbTagRegExType.ItemsSource = CF.TagRegExTypes;
            cbTagRegExType.SelectionChanged += CbTagRegExType_SelectionChanged;
            cbTagRegExType.Tag = strex;
            sp.Children.Add(cbTagRegExType);

            ComboBox cbTargetSection = new ComboBox();
            cbTargetSection.Width = 150;
            cbTargetSection.DisplayMemberPath = "NoteSectionTitle";
            cbTargetSection.SelectedValuePath = "SectionID";
            Binding myBinding2 = new Binding("TargetSection");
            myBinding2.Source = strex;
            cbTargetSection.SetBinding(ComboBox.SelectedValueProperty, myBinding2);
            cbTargetSection.ItemsSource = CF.NoteSections;
            cbTargetSection.Tag = strex;
            cbTargetSection.SelectionChanged += CbTargetSection_SelectionChanged;
            sp.Children.Add(cbTargetSection);

            Button b = new Button();
            b.Style = (Style)Application.Current.Resources["LinkButton"];
            b.Content = "Remove";
            b.Margin = new Thickness(10, 0, 0, 0);
            b.Tag = strex;
            b.Click += bRemoveClick; ;
            sp.Children.Add(b);


            return spMain;
        }

        private void Img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image tmpImg = sender as Image;
            SqlTagRegEx strex = tmpImg.Tag as SqlTagRegEx;
            WinEnterText wet = new WinEnterText("Edit Regular Expression value", strex.RegExText);
            wet.ShowDialog();
            if (wet.ReturnValue != null)
            {
                strex.RegExText = wet.ReturnValue;
                strex.SaveToDB();
                UpdateCurrentCheckPoint();
            }
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

        private void CbTargetSection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb == null) return;
            SqlTagRegEx strex = cb.Tag as SqlTagRegEx;
            if (strex == null) return;
            strex.TargetSection = int.Parse(cb.SelectedValue.ToString());
            strex.SaveToDB();
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

        private void cbTargetSection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            if (cbTargetSection.SelectedValue == null) return;
            CurrentCheckpoint.TargetSection = int.Parse(cbTargetSection.SelectedValue.ToString());
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

        private void AddCP(object sender, RoutedEventArgs e)
        {
            SqlICD10Segment seg = lbICD10.SelectedItem as SqlICD10Segment;
            if (seg != null)
            {

                WinEnterText wet = new WinEnterText("Please input new title.");
                wet.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                wet.Owner = this;
                wet.ShowDialog();
                if (wet.ReturnValue == null) return;
                if (wet.ReturnValue.Trim() != "")
                {
                    CurrentCheckpoint = new SqlCheckpoint(wet.ReturnValue, seg.ICD10SegmentID);
                    UpdateCurrentCheckPoint();
                    int tmpID = CurrentCheckpoint.CheckPointID;
                    RefreshICD10();
                    lbCheckpoints.SelectedValue = tmpID;
                    //CurrentCheckpoint.SaveToDB();
                }
            }
            else
            {
                MessageBox.Show("Select an ICD10 segment 1st");
            }
        }

        private void deleteCP(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint.DeleteFromDB())
            {
                dpCheckpoint.DataContext = null;
                
                RefreshICD10();
            }
        }

        private void myRTB_MouseLeave(object sender, MouseEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
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

        private void TextBlock_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SqlICD10Segment seg = lbICD10.SelectedItem as SqlICD10Segment;
            if (seg != null)
            {
                seg.SegmentComment = tbComment.Text;
                seg.SaveToDB();
            }

        }
        private void ButtonEditGroups_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemEditSegment_Click(object sender, RoutedEventArgs e)
        {
            SqlICD10Segment seg = lbICD10.SelectedItem as SqlICD10Segment;
            if (seg != null)
            {
                WinEditSegment wes = new WinEditSegment(seg);
                wes.Owner = this;
                wes.ShowDialog();
                lbICD10.ItemsSource = null;
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from ICD10Segments order by icd10Chapter, icd10CategoryStart;";
                    SqlLiteDataAccess.ICD10Segments = cnn.Query<SqlICD10Segment>(sql).ToList();
                }
                lbICD10.ItemsSource = SqlLiteDataAccess.ICD10Segments;
            }
        }

        private void AddGroupClick(object sender, RoutedEventArgs e)
        {
            SqlICD10Segment seg = new SqlICD10Segment("Enter Segment Title");
            WinEditSegment wes = new WinEditSegment(seg);
            wes.Owner = this;
            wes.ShowDialog();
            lbICD10.ItemsSource = null;
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                string sql = "Select * from ICD10Segments order by icd10Chapter, icd10CategoryStart;";
                SqlLiteDataAccess.ICD10Segments = cnn.Query<SqlICD10Segment>(sql).ToList();
            }
            lbICD10.ItemsSource = SqlLiteDataAccess.ICD10Segments;
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StackPanel parent = (StackPanel)sender;
            int cpID = int.Parse(parent.Tag.ToString());
            //This next statement is really slowing down the operation
            //DragDrop.DoDragDrop(parent, cpID, DragDropEffects.Move);
        }
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            SqlICD10Segment CurrentSeg = lbICD10.SelectedItem as SqlICD10Segment;
            if (CurrentSeg != null)
            {
                Grid g = (Grid)sender;
                SqlICD10Segment DestinationSeg = g.DataContext as SqlICD10Segment;
                int cpID = (int)e.Data.GetData(typeof(int));
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = $"Select * from CheckPoints where CheckPointID = {cpID};";
                    SqlCheckpoint cp = cnn.Query<SqlCheckpoint>(sql).FirstOrDefault();
                    cp.TargetICD10Segment = DestinationSeg.ICD10SegmentID;
                    cp.SaveToDB();
                    RefreshICD10();
                }
            }
        }

        private void UCTag1_AddMe(object sender, EventArgs e)
        {
            UpdateCurrentCheckPoint();
        }

        private void slideSeverity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }
    }


}
