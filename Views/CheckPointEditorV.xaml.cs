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

        SqlCheckpointVM CurrentCheckpoint;
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

        private void MenuItemEditSegment_Click(object sender, RoutedEventArgs e)
        {
        }

        private void AddGroupClick(object sender, RoutedEventArgs e)
        {
            SqlICD10SegmentVM seg = new SqlICD10SegmentVM("Enter Segment Title");
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
            SqlICD10SegmentM CurrentSeg = lbICD10.SelectedItem as SqlICD10SegmentM;
            if (CurrentSeg != null)
            {
                Grid g = (Grid)sender;
                SqlICD10SegmentM DestinationSeg = g.DataContext as SqlICD10SegmentM;
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

        private void CPSummaryClick(object sender, RoutedEventArgs e)
        {
            SqlICD10SegmentM seg = lbICD10.SelectedItem as SqlICD10SegmentM;
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
            SqlICD10SegmentM seg = lbICD10.SelectedItem as SqlICD10SegmentM;
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

        private void btnLinkClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(CurrentCheckpoint.Link);
        }

    }
}
