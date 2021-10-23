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
    /// Interaction logic for CheckPointEditorV.xaml
    /// </summary>
    public partial class CheckPointEditorV : Window
    {

        SqlCheckpointM CurrentCheckpoint;
        public CheckPointEditorV()
        {
            InitializeComponent();
            #region SetDictionary
            //spellcheck setup
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
            #endregion  

            lbICD10.DataContext = new SqlICD10SegmentVM();
        }

        private void closeclick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CF.SetWindowPosition(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CF.SaveWindowPosition(this);
        }

        private void deleteCP(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint.DeleteFromDB())
            {
                dpCheckpoint.DataContext = null;
            }
        }

        private void myRTB_MouseLeave(object sender, MouseEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
        }

        /*
        private void StrToRTB(string strIn, RichTextBox rtb)
         {
            byte[] byteArray = Encoding.ASCII.GetBytes(strIn);
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                TextRange tr = new TextRange(rtb.DocumentM.ContentStart, rtb.DocumentM.ContentEnd);
                tr.Load(ms, DataFormats.Rtf);
            }
        }

        private string RTBtoStr(RichTextBox rtb)
        {
            string rtfText; //string to save to db
            TextRange tr = new TextRange(rtb.DocumentM.ContentStart, rtb.DocumentM.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Rtf);
                rtfText = Encoding.ASCII.GetString(ms.ToArray());
            }
            return rtfText;

        }
        */

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
                SqlTagVM tg = SqlLiteDataAccess.GetTags(wat.ReturnValue).FirstOrDefault();
                if (tg == null) tg = new SqlTagVM(wat.ReturnValue);
                string sql = "";
                sql = $"INSERT INTO RelTagCheckPoint (TagID, CheckPointID) VALUES ({tg.TagID},{CurrentCheckpoint.CheckPointID});";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    cnn.Execute(sql);
                }
                //SqlTagRegEx srex = new SqlTagRegEx(tg.TagID, "Search Text", CurrentCheckpoint.TargetSection, 1);
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
            }
        }

        private void AddGroupClick(object sender, RoutedEventArgs e)
        {
            SqlICD10Segment seg = new SqlICD10Segment("Enter Segment Title");
            WinEditSegment wes = new WinEditSegment(seg);
            wes.Owner = this;
            wes.ShowDialog();
            SqlICD10SegmentVM.CalculateLeftOffsets();
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
                    SqlCheckpointM cp = cnn.Query<SqlCheckpointM>(sql).FirstOrDefault();
                    cp.TargetICD10Segment = DestinationSeg.ICD10SegmentID;
                    cp.SaveToDB();
                }
            }
        }

        private void UCTag1_AddMe(object sender, EventArgs e)
        {
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
                    List<SqlCheckpointM> lcp = cnn.Query<SqlCheckpointM>(sql).ToList();
                    sql = "Select * from CheckPointTypes order by ItemOrder;";
                    List<SqlCheckPointType> lcpt = cnn.Query<SqlCheckPointType>(sql).ToList();

                    string strSummary = "";
                    foreach (SqlCheckPointType cpt in lcpt)
                    {
                        string strTempOut = "";
                        foreach (SqlCheckpointM cp in lcp)
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
                    SqlCheckpointVM cpvm = new SqlCheckpointVM();
                    List<SqlCheckpointVM> lcp = cpvm.GetCPsFromSegment(seg.ICD10SegmentID);
                    List<SqlCheckPointType> lcpt = cpvm.CheckPointTypes;
                    string strSummary = $"<h1>{seg.SegmentTitle}</h1><br>";
                    foreach (SqlCheckPointType cpt in lcpt)
                    {
                        string strTempOut = "<ol>";
                        foreach (SqlCheckpointVM cp in lcp)
                        {
                            if (cp.CheckPointType == cpt.CheckPointTypeID)
                            {
                                strTempOut += $"<li><dl><dt><font size='+1'>{cp.CheckPointTitle}</font>" + Environment.NewLine;
                                if (cp.Comment != null)
                                {
                                    strTempOut += $"<dd><i>{cp.Comment.Replace(Environment.NewLine, "<br>")}</i>" + Environment.NewLine;
                                    if (cp.Link != null)
                                    {
                                        strTempOut += $"<br><a href='{cp.Link}'>[Link to source]</a>";
                                    }
                                    if (cp.Images.Count > 0)
                                    {
                                        foreach (var imgCPimage in cp.Images)
                                        {
                                            var b64String = Convert.ToBase64String(imgCPimage.ImageData);
                                            var dataUrl = "data:image/bmp;base64," + b64String;
                                            strTempOut += $"<br><img src=\"{dataUrl}" + "\" />";
                                        }
                                    }
                                    strTempOut += "";
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
                    WinPreviewHTML wp = new WinPreviewHTML();
                    wp.MyWB.NavigateToString(strSummary);
                    wp.ShowDialog();

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

        private void btnLinkClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(CurrentCheckpoint.Link);
        }

        private void UpdateCP(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            SqlCheckpointM cp = tb.DataContext as SqlCheckpointM;

            cp.SaveToDB();
        }
    }


}
