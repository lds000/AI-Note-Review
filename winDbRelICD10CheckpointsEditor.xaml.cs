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
                string sql = "Select * from CheckPointTypes;";
                cbTypes.ItemsSource = cnn.Query(sql).ToList();

                CF.UpdateNoteICD10Segments();
                cbTargetICD10.ItemsSource = CF.NoteICD10Segments;

                char charChapter = 'A';
                double CodeStart = 0;
                double CodeEnd = 0;
                foreach (SqlICD10Segment ns in CF.NoteICD10Segments)
                {
                    ns.icd10Margin = new Thickness(0);
                    if (charChapter == char.Parse(ns.icd10Chapter))
                    {
                        if ((ns.icd10CategoryStart >= CodeStart) && (ns.icd10CategoryEnd <= CodeEnd))
                        {
                            ns.icd10Margin = new Thickness(5, 0, 0, 0);
                        }
                        CodeStart = ns.icd10CategoryStart;
                        CodeEnd = ns.icd10CategoryEnd;
                        charChapter = char.Parse(ns.icd10Chapter);
                    }
                    else
                    {
                        charChapter = char.Parse(ns.icd10Chapter);
                        CodeStart = 0;
                        CodeEnd = 0;
                    }
                }
            }
            CF.UpdateNoteICD10Segments();
            lbICD10.ItemsSource = CF.NoteICD10Segments;

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
            if (CurrentCheckpoint == null) return;
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
                //SqlTagRegEx srex = new SqlTagRegEx(tg.TagID, "Search Text", CurrentCheckpoint.TargetSection, 1);

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
                CF.UpdateNoteICD10Segments();
                lbICD10.ItemsSource = CF.NoteICD10Segments;
            }
        }

        private void AddGroupClick(object sender, RoutedEventArgs e)
        {
            SqlICD10Segment seg = new SqlICD10Segment("Enter Segment Title");
            WinEditSegment wes = new WinEditSegment(seg);
            wes.Owner = this;
            wes.ShowDialog();
            lbICD10.ItemsSource = null;
            CF.UpdateNoteICD10Segments();
            cbTargetICD10.ItemsSource = CF.NoteICD10Segments;
            lbICD10.ItemsSource = CF.NoteICD10Segments;
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

        private void CPSummaryClick(object sender, RoutedEventArgs e)
        {
            SqlICD10Segment seg = lbICD10.SelectedItem as SqlICD10Segment;
            if (seg != null)
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = $"Select * from CheckPoints where TargetICD10Segment == {seg.ICD10SegmentID}";
                    List<SqlCheckpoint> lcp = cnn.Query<SqlCheckpoint>(sql).ToList();
                    sql = "Select * from CheckPointTypes order by ItemOrder;";
                    List<CheckPointType> lcpt = cnn.Query<CheckPointType>(sql).ToList();

                    string strSummary = "";
                    foreach (CheckPointType cpt in lcpt)
                    {
                        string strTempOut = "";
                        foreach (SqlCheckpoint cp in lcp)
                        {
                            if (cp.CheckPointType == cpt.CheckPointTypeID)
                            {
                                strTempOut += $"\t- {cp.CheckPointTitle}" + Environment.NewLine;
                                strTempOut += $"\t\t {cp.Comment}" + Environment.NewLine;
                                strTempOut += $"\t\t Severity: {cp.ErrorSeverity}/10" + Environment.NewLine;
                                strTempOut += $"\t\t Action: {cp.Action}" + Environment.NewLine;
                            }
                        }
                        if (strTempOut != "")
                        {
                            strSummary += $"Section: {cpt.Title} ({cpt.Comment})" + Environment.NewLine;
                            strSummary += strTempOut;
                            strSummary += Environment.NewLine;
                        }
                    }
                    Clipboard.SetText(strSummary);
                }

            }
        }

        private void MenuItemCreateSegmentIndex(object sender, RoutedEventArgs e)
        {
            SqlICD10Segment seg = lbICD10.SelectedItem as SqlICD10Segment;
            if (seg != null)
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = $"Select * from CheckPoints where TargetICD10Segment == {seg.ICD10SegmentID}";
                    List<SqlCheckpoint> lcp = cnn.Query<SqlCheckpoint>(sql).ToList();
                    sql = "Select * from CheckPointTypes order by ItemOrder;";
                    List<CheckPointType> lcpt = cnn.Query<CheckPointType>(sql).ToList();

                    string strSummary = $"<h1>{seg.SegmentTitle}</h1><br>";
                    foreach (CheckPointType cpt in lcpt)
                    {
                        string strTempOut = "<ol>";
                        foreach (SqlCheckpoint cp in lcp)
                        {
                            if (cp.CheckPointType == cpt.CheckPointTypeID)
                            {
                                strTempOut += $"<li><dl><dt><font size='+1'>{cp.CheckPointTitle}</font>" + Environment.NewLine;
                                if (cp.Comment != null)
                                {
                                    strTempOut += $"<dd><ul><i>{cp.Comment.Replace(Environment.NewLine, "<br>")}</i>" + Environment.NewLine;
                                    if (cp.Link != null)
                                    {
                                        strTempOut += $"<br><a href='{cp.Link}'>[Link to source]</a>";
                                    }
                                    strTempOut += "</ul>";
                                }
                                strTempOut += "</dl></li>";
                            }
                        }
                        if (strTempOut != "<ol>")
                        {
                            strSummary += $"<font size='+2'><B>{cpt.Title} </B></font>" + Environment.NewLine;
                            strSummary += strTempOut + "</ol>";
                            strSummary += Environment.NewLine;
                        }
                        else
                        {
                            strTempOut = "";
                        }
                    }
                    //Clipboard.SetText(strSummary, TextDataFormat.Html);
                    ClipboardHelper.CopyToClipboard(strSummary, "");

                }
                }
            }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonImage_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            CurrentCheckpoint.AddImageFromClipBoard();


        }

        private void DeleteImage(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            SqlCheckPointImage sc = mi.DataContext as SqlCheckPointImage;
            CurrentCheckpoint.DeleteImage(sc);
        }
    }

    /// <summary>      
    /// Helper to  encode and set HTML fragment to clipboard.<br/>      
    /// See <br/>      
    /// <seealso  cref="CreateDataObject"/>.      
    ///  </summary>      
    /// <remarks>      
    /// The MIT License  (MIT) Copyright (c) 2014 Arthur Teplitzki.      
    ///  </remarks>      
    public static class ClipboardHelper
    {
        #region Fields and Consts      

        /// <summary>      
        /// The string contains index references to  other spots in the string, so we need placeholders so we can compute the  offsets. <br/>      
        /// The  <![CDATA[<<<<<<<]]>_ strings are just placeholders.  We'll back-patch them actual values afterwards. <br/>      
        /// The string layout  (<![CDATA[<<<]]>) also ensures that it can't appear in the body  of the html because the <![CDATA[<]]> <br/>      
        /// character must be escaped. <br/>      
        /// </summary>      
        private const string Header = @"Version:0.9      
StartHTML:<<<<<<<<1      
EndHTML:<<<<<<<<2      
StartFragment:<<<<<<<<3      
EndFragment:<<<<<<<<4      
StartSelection:<<<<<<<<3      
EndSelection:<<<<<<<<4";

        /// <summary>      
        /// html comment to point the beginning of  html fragment      
        /// </summary>      
        public const string StartFragment = "<!--StartFragment-->";

        /// <summary>      
        /// html comment to point the end of html  fragment      
        /// </summary>      
        public const string EndFragment = @"<!--EndFragment-->";

        /// <summary>      
        /// Used to calculate characters byte count  in UTF-8      
        /// </summary>      
        private static readonly char[] _byteCount = new char[1];

        #endregion


        /// <summary>      
        /// Create <see  cref="DataObject"/> with given html and plain-text ready to be  used for clipboard or drag and drop.<br/>      
        /// Handle missing  <![CDATA[<html>]]> tags, specified startend segments and Unicode  characters.      
        /// </summary>      
        /// <remarks>      
        /// <para>      
        /// Windows Clipboard works with UTF-8  Unicode encoding while .NET strings use with UTF-16 so for clipboard to  correctly      
        /// decode Unicode string added to it from  .NET we needs to be re-encoded it using UTF-8 encoding.      
        /// </para>      
        /// <para>      
        /// Builds the CF_HTML header correctly for  all possible HTMLs<br/>      
        /// If given html contains start/end  fragments then it will use them in the header:      
        ///  <code><![CDATA[<html><body><!--StartFragment-->hello  <b>world</b><!--EndFragment--></body></html>]]></code>      
        /// If given html contains html/body tags  then it will inject start/end fragments to exclude html/body tags:      
        ///  <code><![CDATA[<html><body>hello  <b>world</b></body></html>]]></code>      
        /// If given html doesn't contain html/body  tags then it will inject the tags and start/end fragments properly:      
        /// <code><![CDATA[hello  <b>world</b>]]></code>      
        /// In all cases creating a proper CF_HTML  header:<br/>      
        /// <code>      
        /// <![CDATA[      
        /// Version:1.0      
        /// StartHTML:000000177      
        /// EndHTML:000000329      
        /// StartFragment:000000277      
        /// EndFragment:000000295      
        /// StartSelection:000000277      
        /// EndSelection:000000277      
        /// <!DOCTYPE HTML PUBLIC  "-//W3C//DTD HTML 4.0 Transitional//EN">      
        ///  <html><body><!--StartFragment-->hello  <b>world</b><!--EndFragment--></body></html>      
        /// ]]>      
        /// </code>      
        /// See format specification here: [http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp][9]      
        /// </para>      
        /// </remarks>      
        /// <param name="html">a  html fragment</param>      
        /// <param  name="plainText">the plain text</param>      
        public static DataObject CreateDataObject(string html, string plainText)
        {
            html = html ?? String.Empty;
            var htmlFragment = GetHtmlDataString(html);

            // re-encode the string so it will work  correctly (fixed in CLR 4.0)      
            if (Environment.Version.Major < 4 && html.Length != Encoding.UTF8.GetByteCount(html))
                htmlFragment = Encoding.Default.GetString(Encoding.UTF8.GetBytes(htmlFragment));

            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, htmlFragment);
            dataObject.SetData(DataFormats.Text, plainText);
            dataObject.SetData(DataFormats.UnicodeText, plainText);
            return dataObject;
        }

        /// <summary>      
        /// Clears clipboard and sets the given  HTML and plain text fragment to the clipboard, providing additional  meta-information for HTML.<br/>      
        /// See <see  cref="CreateDataObject"/> for HTML fragment details.<br/>      
        /// </summary>      
        /// <example>      
        ///  ClipboardHelper.CopyToClipboard("Hello <b>World</b>",  "Hello World");      
        /// </example>      
        /// <param name="html">a  html fragment</param>      
        /// <param  name="plainText">the plain text</param>      
        public static void CopyToClipboard(string html, string plainText)
        {
            var dataObject = CreateDataObject(html, plainText);
            Clipboard.SetDataObject(dataObject, true);
        }

        /// <summary>      
        /// Generate HTML fragment data string with  header that is required for the clipboard.      
        /// </summary>      
        /// <param name="html">the  html to generate for</param>      
        /// <returns>the resulted  string</returns>      
        private static string GetHtmlDataString(string html)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Header);
            sb.AppendLine(@"<!DOCTYPE HTML  PUBLIC ""-//W3C//DTD HTML 4.0  Transitional//EN"">");

            // if given html already provided the  fragments we won't add them      
            int fragmentStart, fragmentEnd;
            int fragmentStartIdx = html.IndexOf(StartFragment, StringComparison.OrdinalIgnoreCase);
            int fragmentEndIdx = html.LastIndexOf(EndFragment, StringComparison.OrdinalIgnoreCase);

            // if html tag is missing add it  surrounding the given html (critical)      
            int htmlOpenIdx = html.IndexOf("<html", StringComparison.OrdinalIgnoreCase);
            int htmlOpenEndIdx = htmlOpenIdx > -1 ? html.IndexOf('>', htmlOpenIdx) + 1 : -1;
            int htmlCloseIdx = html.LastIndexOf("</html", StringComparison.OrdinalIgnoreCase);

            if (fragmentStartIdx < 0 && fragmentEndIdx < 0)
            {
                int bodyOpenIdx = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
                int bodyOpenEndIdx = bodyOpenIdx > -1 ? html.IndexOf('>', bodyOpenIdx) + 1 : -1;

                if (htmlOpenEndIdx < 0 && bodyOpenEndIdx < 0)
                {
                    // the given html doesn't  contain html or body tags so we need to add them and place start/end fragments  around the given html only      
                    sb.Append("<html><body>");
                    sb.Append(StartFragment);
                    fragmentStart = GetByteCount(sb);
                    sb.Append(html);
                    fragmentEnd = GetByteCount(sb);
                    sb.Append(EndFragment);
                    sb.Append("</body></html>");
                }
                else
                {
                    // insert start/end fragments  in the proper place (related to html/body tags if exists) so the paste will  work correctly      
                    int bodyCloseIdx = html.LastIndexOf("</body", StringComparison.OrdinalIgnoreCase);

                    if (htmlOpenEndIdx < 0)
                        sb.Append("<html>");
                    else
                        sb.Append(html, 0, htmlOpenEndIdx);

                    if (bodyOpenEndIdx > -1)
                        sb.Append(html, htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0, bodyOpenEndIdx - (htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0));

                    sb.Append(StartFragment);
                    fragmentStart = GetByteCount(sb);

                    var innerHtmlStart = bodyOpenEndIdx > -1 ? bodyOpenEndIdx : (htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0);
                    var innerHtmlEnd = bodyCloseIdx > -1 ? bodyCloseIdx : (htmlCloseIdx > -1 ? htmlCloseIdx : html.Length);
                    sb.Append(html, innerHtmlStart, innerHtmlEnd - innerHtmlStart);

                    fragmentEnd = GetByteCount(sb);
                    sb.Append(EndFragment);

                    if (innerHtmlEnd < html.Length)
                        sb.Append(html, innerHtmlEnd, html.Length - innerHtmlEnd);

                    if (htmlCloseIdx < 0)
                        sb.Append("</html>");
                }
            }
            else
            {
                // handle html with existing  startend fragments just need to calculate the correct bytes offset (surround  with html tag if missing)      
                if (htmlOpenEndIdx < 0)
                    sb.Append("<html>");
                int start = GetByteCount(sb);
                sb.Append(html);
                fragmentStart = start + GetByteCount(sb, start, start + fragmentStartIdx) + StartFragment.Length;
                fragmentEnd = start + GetByteCount(sb, start, start + fragmentEndIdx);
                if (htmlCloseIdx < 0)
                    sb.Append("</html>");
            }

            // Back-patch offsets (scan only the  header part for performance)      
            sb.Replace("<<<<<<<<1", Header.Length.ToString("D9"), 0, Header.Length);
            sb.Replace("<<<<<<<<2", GetByteCount(sb).ToString("D9"), 0, Header.Length);
            sb.Replace("<<<<<<<<3", fragmentStart.ToString("D9"), 0, Header.Length);
            sb.Replace("<<<<<<<<4", fragmentEnd.ToString("D9"), 0, Header.Length);

            return sb.ToString();
        }

        /// <summary>      
        /// Calculates the number of bytes produced  by encoding the string in the string builder in UTF-8 and not .NET default  string encoding.      
        /// </summary>      
        /// <param name="sb">the  string builder to count its string</param>      
        /// <param  name="start">optional: the start index to calculate from (default  - start of string)</param>      
        /// <param  name="end">optional: the end index to calculate to (default - end  of string)</param>      
        /// <returns>the number of bytes  required to encode the string in UTF-8</returns>      
        private static int GetByteCount(StringBuilder sb, int start = 0, int end = -1)
        {
            int count = 0;
            end = end > -1 ? end : sb.Length;
            for (int i = start; i < end; i++)
            {
                _byteCount[0] = sb[i];
                count += Encoding.UTF8.GetByteCount(_byteCount);
            }
            return count;
        }
    }
    

}
