using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for WinReport.xaml
    /// </summary>
    public partial class WinReport : Window
    {
        public WinReport(bool GeneralCheckPointsOnly = false)
        {
            InitializeComponent();
            CF.CurrentDoc.ICD10Segments = CF.CurrentDoc.GetSegments(GeneralCheckPointsOnly);
            CF.CurrentDoc.GenerateReport(true);
            DataContext = CF.CurrentDoc;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }




        private void lbFail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox lb = sender as ListBox;
            SqlCheckpoint cp = (SqlCheckpoint) lb.SelectedItem;
            WinCheckPointEditor wce = new WinCheckPointEditor(cp);
            wce.Owner = this;
            wce.ShowDialog();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CF.SetWindowPosition(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CF.SaveWindowPosition(this);
        }

        private void Button_Click_Recheck(object sender, RoutedEventArgs e)
        {
            CF.CurrentDoc.GenerateReport();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CF.CurrentDoc.GenerateReport();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //DataContext = null;

            CF.CurrentDoc.GenerateReport();
            //DataContext = CF.CurrentDoc;
            
        }

        private void Button_CopyReportClick(object sender, RoutedEventArgs e)
        {
            double[] PassedScores = new double[] { 0,0,0,0 };
            double[] MissedScores = new double[] { 0, 0, 0, 0 };
            double[] Totals = new double[] { 0, 0, 0, 0 };
            double[] Scores = new double[] { 0, 0, 0, 0 };
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.PassedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                PassedScores[CF.NoteSections.First(c => c.SectionID == cp.TargetSection).ScoreSection] += cp.ErrorSeverity;
            }
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.MissedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                MissedScores[CF.NoteSections.First(c => c.SectionID == cp.TargetSection).ScoreSection] += cp.ErrorSeverity;
            }

            for (int i = 0; i <= 3; i++)
            {
                Totals[i] = PassedScores[i] + MissedScores[i];
            }
            for (int i = 0; i <= 3; i++)
            {
                if (Totals[i] == 0)
                {
                    Scores[i] = 100;
                }
                else
                {
                    Scores[i] = 80 + (PassedScores[i] / Totals[i]) * 20;
                }
            }

            double HPI_Score = Scores[0] / 100 * 2;
            double Exam_Score = Scores[1] / 100 * 2;
            double Dx_Score = Scores[2] / 100 * 2;
            double Rx_Score = Scores[3] / 100 * 4;
            double Total_Score = HPI_Score + Exam_Score + Dx_Score + Rx_Score;

            string tmpCheck = "";
            string strReport = @"<!DOCTYPE html><html><head></head><body>";
            strReport += $"<font size='+3'>Patient ID {CF.CurrentDoc.PtID}</font><br>"; // "This report is using a programmed algorythm that searches for terms in your documentation.  I personally programmed these terms so they may not apply to this clinical scenario.  I'm working on version 1.0 and I know this report is not perfect, but by version infinity.0 it will be. Please let me know how well my program worked (or failed). Your feedback is so much more important than any feedback I may provide you. Most important is that you let me know if this information is in any way incorrect. I will edit or re-write code to make it correct. Thanks for all you do! ";
            strReport += $"<font size='+1'>Date: {CF.CurrentDoc.VisitDate.ToShortDateString()}</font><br>";
            strReport += Environment.NewLine;

            strReport += $"Scores: HPI <b>{HPI_Score.ToString("0.##")}</b> Exam <b>{Exam_Score.ToString("0.##")}</b> Dx <b>{Dx_Score.ToString("0.##")}</b> Treatment <b>{Rx_Score.ToString("0.##")}</b> Total Score<sup>*</sup> <b>{Total_Score.ToString("0.##")}</b><br><hr>";

            foreach (var seg in CF.CurrentDoc.ICD10Segments)
            {
                if (seg.IncludeSegment)
                tmpCheck += $"<li><font size='+1'>{seg.SegmentTitle}</font></li>" + Environment.NewLine;
            }
            if (tmpCheck != "")
            {
                strReport += $"<font size='+3'>Relevant ICD10 Segments</font><br><dl><ul>";
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            tmpCheck = "";
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.PassedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                tmpCheck += $"<li><font size='+1'>{cp.CheckPointTitle}</font> <font size='-1'>(Score Weight:{cp.ErrorSeverity}/10)</font></li>" + Environment.NewLine;
            }
            if (tmpCheck != "")
            {
                strReport += "<font size='+3'>Passed check points:</FONT><BR><dl><ul>" + Environment.NewLine;
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            tmpCheck = "";
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.MissedCheckPoints where c.ErrorSeverity > 0 orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                    tmpCheck += cp.GetReport();        
            }
            if (tmpCheck != "")
            {
                strReport += "<font size='+3'>Missed check points:</font><br><dl><ul>" + Environment.NewLine;
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            tmpCheck = "";
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.MissedCheckPoints where c.ErrorSeverity == 0 orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                    tmpCheck += cp.GetReport();
            }
            if (tmpCheck != "")
            {
                strReport += "<font size='+3'>Minor missed check points:</font><dl><ul>" + Environment.NewLine;
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            tmpCheck = "";
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.RelevantCheckPoints orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                strReport += cp.GetReport();
            }
            if (tmpCheck != "")
            {
                strReport += "<font size='+3'>Other relevant points to consider:</font><dl><ul>" + Environment.NewLine;
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            strReport += "<br><br><hl>";
            strReport += "Footnotes:<br>";
            strReport += "* Total Score = (Total of Score Weights missed) / ((Total of Score Weights missed)+(Total of Score Weights passed)) * 2 + 8<br>";
            strReport += "** Score Weight = An assigned weight of the importance of the checkpoint.<br>";
            strReport += "</body></html>";
            Clipboard.SetText(strReport);
            ClipboardHelper.CopyToClipboard(strReport, "");
            WinPreviewHTML wp = new WinPreviewHTML();
            wp.MyWB.NavigateToString(strReport);
            wp.ShowDialog();

            //MessageBox.Show(strReport);
        }

        private void Button_CommittReportClick(object sender, RoutedEventArgs e)
        {
            string sqlCheck = $"Select Count() from RelCPPRovider where PtID={CF.CurrentDoc.PtID} AND VisitDate='{CF.CurrentDoc.VisitDate.ToString("yyyy-MM-dd")}';";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                int iCount = cnn.ExecuteScalar<int>(sqlCheck);
                if (iCount > 0)
                {
                    MessageBoxResult mr = MessageBox.Show($"The patient ID and visit date already exist with {iCount} checkpoints. Press 'ok' to continue and replace previous report.", "Review Already Exists!", MessageBoxButton.OKCancel);
                    if (mr == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                    string strDelete = $"Delete from RelCPPRovider where PtID={CF.CurrentDoc.PtID} AND VisitDate='{CF.CurrentDoc.VisitDate.ToString("yyyy-MM-dd")}';";
                    using (IDbConnection cnn1 = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        cnn1.Execute(strDelete);
                    }
                }
            }

            CF.CurrentDoc.ReviewDate = DateTime.Now;

            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.MissedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                 cp.Commit(CF.CurrentDoc, SqlRelCPProvider.MyCheckPointStates.Fail);
            }

            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.RelevantCheckPoints orderby c.ErrorSeverity descending select c))
            {
                 cp.Commit(CF.CurrentDoc, SqlRelCPProvider.MyCheckPointStates.Relevant);
            }

            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.PassedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                 cp.Commit(CF.CurrentDoc, SqlRelCPProvider.MyCheckPointStates.Pass);
            }
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.IrrelaventCP orderby c.ErrorSeverity descending select c))
            {
                cp.Commit(CF.CurrentDoc, SqlRelCPProvider.MyCheckPointStates.Irrelevant);
            }

            MessageBox.Show("Report committed.");

        }

        private void Button_ReportsReviewClick(object sender, RoutedEventArgs e)
        {
        }

        private void Button_CopyIndexClick(object sender, RoutedEventArgs e)
        {
            string sqlCheck = $"Select * from(Select distinct CheckPointID rel from RelCPPRovider where PtID={ CF.CurrentDoc.PtID} " +
            $"AND ReviewDate = '{CF.CurrentDoc.ReviewDate.ToString("yyyy-MM-dd")}') " + 
            $"inner join CheckPoints cp on rel = cp.CheckPointID " + 
            $"order by cp.TargetSection, cp.ErrorSeverity desc;";

            List<SqlCheckpoint> cplist = new List<SqlCheckpoint>();
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cplist = cnn.Query<SqlCheckpoint>(sqlCheck).ToList();
            }

            string strOut = "Index for relevant checkpoints." + Environment.NewLine;
            foreach (SqlCheckpoint cp in cplist)
            {
                strOut += cp.GetIndex();
            }
            Clipboard.SetText(strOut);

        }


        private void Button_CheckpointAuditClick(object sender, RoutedEventArgs e)
        {

        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image i = sender as Image;
            SqlCheckpoint cp = i.DataContext as SqlCheckpoint;
            WinShowCheckPointRichText scp = new WinShowCheckPointRichText();
            scp.DataContext = cp;
            scp.Owner = this;
            scp.ImChanged += Scp_AddMe;
            scp.ShowDialog();
        }

        private void Scp_AddMe(object sender, EventArgs e)
        {
            CF.CurrentDoc.GenerateReport();
            //todo: update checkpoints
        }

        private void MovePassedCP(object sender, RoutedEventArgs e)
        {
            MenuItem i = sender as MenuItem;
            SqlCheckpoint cp = i.DataContext as SqlCheckpoint;
            CF.CurrentDoc.CPStatusOverrides.CreateNewOrUpdateExisting(cp, SqlRelCPProvider.MyCheckPointStates.Pass);
            CF.CurrentDoc.GenerateReport();
        }

        private void MoveMissedCP(object sender, RoutedEventArgs e)
        {
            MenuItem i = sender as MenuItem;
            SqlCheckpoint cp = i.DataContext as SqlCheckpoint;
            CF.CurrentDoc.CPStatusOverrides.CreateNewOrUpdateExisting(cp, SqlRelCPProvider.MyCheckPointStates.Fail);
            CF.CurrentDoc.GenerateReport();
        }

        private void DropCP(object sender, RoutedEventArgs e)
        {
            MenuItem i = sender as MenuItem;
            SqlCheckpoint cp = i.DataContext as SqlCheckpoint;
            CF.CurrentDoc.CPStatusOverrides.CreateNewOrUpdateExisting(cp, SqlRelCPProvider.MyCheckPointStates.Irrelevant);
            CF.CurrentDoc.GenerateReport();
        }

        private void AddCommentCP(object sender, RoutedEventArgs e)
        {
            MenuItem i = sender as MenuItem;
            SqlCheckpoint cp = i.DataContext as SqlCheckpoint;
            WinEnterText wet = new WinEnterText($"Add/Edit Comment: {cp.CheckPointTitle}", cp.CustomComment);
            wet.Owner = this;
            wet.ShowDialog();

            cp.CustomComment = wet.ReturnValue;
        }
    }

}
