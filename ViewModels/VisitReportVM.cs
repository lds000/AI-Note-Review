using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace AI_Note_Review
{
    /// <summary>
    /// Serves as the view model for the visit report, the report that checks the visit and produces passed, missed checkpoints.
    /// </summary>
    public class VisitReportVM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //key entities
        private VisitReportM report;
        private DocumentVM document;
        private PatientVM patientVM;
        private PatientM patient;
        private SqlProvider sqlProvider;

        public VisitReportVM()
        {
            report = new VisitReportM(); //1st executed command in program
            sqlProvider = new SqlProvider(); //Change provider for report
            patientVM = new PatientVM();
            patient = patientVM.Patient;
            document = new DocumentVM(sqlProvider, patientVM);
        }

        #region Report VM definitions - boring stuff
        public VisitReportM Report
        {
            get
            {
                return report;
            }
        }
        public DateTime ReviewDate { get { return report.ReviewDate; } set { report.ReviewDate = value; } }
        public Dictionary<SqlCheckpointVM, SqlRelCPProvider.MyCheckPointStates> CPStatusOverrides { get { return report.CPStatusOverrides; } set { report.CPStatusOverrides = value; } }
        public ObservableCollection<string> DocumentTags { get { return report.DocumentTags; } set { report.DocumentTags = value; } }
        #endregion
        #region Document and Document VM definitions
        public DocumentVM Document
        {
            get
            {
                return document;
            }
        }

        public string Facility { get { return document.Facility; } set { document.Facility = value; } }
        public string Provider { get { return document.Provider; } set { document.Provider = value; } }
        public SqlProvider ProviderSql { get { return document.ProviderSql; } set { document.ProviderSql = value; } }
        public DateTime VisitDate { get { return document.VisitDate; } set { document.VisitDate = value; } }
        public string HashTags { get { return document.HashTags; } set { document.HashTags = value; } }
        public ObservableCollection<string> ICD10s { get { return document.ICD10s; } set { document.ICD10s = value; } }

        private ObservableCollection<SqlICD10SegmentVM> iCD10Segments;
        public ObservableCollection<SqlICD10SegmentVM> ICD10Segments
        {
            get
            {
                if (iCD10Segments == null)
                {
                    iCD10Segments = document.ICD10Segments;
                    foreach (var tmpSeg in iCD10Segments)
                    {
                        tmpSeg.ParentDocument = document;
                        tmpSeg.ParentReport = this;
                    }
                }
                return iCD10Segments;
            }
            set
            {
                iCD10Segments = value;
            }
        }

        #endregion
        #region SqlProvider definitions
        public SqlProvider SqlProvider
        {
            get
            {
                return sqlProvider;
            }
            set
            {
                sqlProvider = value;
            }
        }
        #endregion
        #region Patient yadda tadd
        public PatientM Patient
        {
            get
            {
                return patient;
            }
        }

        public PatientVM PatientVM
        {
            get
            {
                return patientVM;
            }
        }
        #endregion

        private List<SqlCheckpointVM> passedCPs;
        public List<SqlCheckpointVM> PassedCPs 
        { 
            get
            {
                if (passedCPs == null)
                {
                    passedCPs = new List<SqlCheckpointVM>();
                    foreach (var tmpCollection in ICD10Segments)
                    {
                        if (tmpCollection.IncludeSegment)
                        {
                            passedCPs = passedCPs.Concat(tmpCollection.PassedCPs).ToList();
                        }
                    }
                }
                return passedCPs;
            }
            set
            {
                passedCPs = value;
            }
        }

        private List<SqlCheckpointVM> missedCPs;
        public List<SqlCheckpointVM> MissedCPs
        {
            get
            {
                if (missedCPs == null)
                {
                    missedCPs = new List<SqlCheckpointVM>();
                    foreach (var tmpCollection in ICD10Segments) //only run once per report
                    {
                        if (tmpCollection.IncludeSegment)
                        {
                            missedCPs = missedCPs.Concat(tmpCollection.MissedCPs).ToList(); //run 19 times
                        }
                    }
                }
                return missedCPs;
            }
            set
            {
                missedCPs = value;
            }
        }

        private List<SqlCheckpointVM> droppedCPs;
        public List<SqlCheckpointVM> DroppedCPs
        {
            get
            {
                if (droppedCPs == null)
                {
                    droppedCPs = new List<SqlCheckpointVM>();
                    foreach (var tmpCollection in ICD10Segments)
                    {
                        if (tmpCollection.IncludeSegment)
                        {
                            droppedCPs = droppedCPs.Concat(tmpCollection.DroppedCPs).ToList();
                        }
                    }
                }
                return droppedCPs;
            }
            set
            {
                droppedCPs = value;
            }
        }

        public void SetCPs()
        {
        }

        /// <summary>
        /// Holds the current review's Yes/No SqlRegex's
        /// </summary>
        private Dictionary<int, bool> YesNoSqlRegExIndex = new Dictionary<int, bool>();


        public string GetReport(SqlCheckpointVM sqlCheckpointVM, DocumentVM doc, PatientM pt)
        {
            string strReturn = "";
            strReturn += $"<li><dt><font size='+1'>{sqlCheckpointVM.CheckPointTitle}</font><font size='-1'> (Score Weight<sup>**</sup>:{sqlCheckpointVM.ErrorSeverity}/10)</font></dt><dd><i>{sqlCheckpointVM.Comment}</i></dd></li>" + Environment.NewLine;
            if (sqlCheckpointVM.CustomComment != "")
            {
                strReturn += $"<b>Comment: {sqlCheckpointVM.CustomComment}</b><br>";
            }
            if (sqlCheckpointVM.Link != "" && sqlCheckpointVM.Link != null)
            {
                strReturn += $"<a href={sqlCheckpointVM.Link}>Click here for reference.</a><br>";
            }
            strReturn += $"<a href='mailto:Lloyd.Stolworthy@PrimaryHealth.com?subject=Feedback on review of {pt.PtID} on {doc.VisitDate.ToShortDateString()}. (Ref:{pt.PtID}|{doc.VisitDate.ToShortDateString()}|{sqlCheckpointVM.CheckPointID})'>Feedback</a>";
            /*
            strReturn += $"\tSignificance {ErrorSeverity}/10." + Environment.NewLine;
            strReturn += $"\tRecommended Remediation: {Action}" + Environment.NewLine;
            strReturn += $"\tExplanation: {Comment}" + Environment.NewLine;
            if (Link != "")
            strReturn += $"\tLink: {Link}" + Environment.NewLine;
            strReturn += Environment.NewLine;
            strReturn += Environment.NewLine;
            */

            //HPi, exam, Dx, Rx

            return strReturn;
        }


        private void CopyReportClick(object sender, RoutedEventArgs e)
        {
            double[] PassedScores = new double[] { 0, 0, 0, 0 };
            double[] MissedScores = new double[] { 0, 0, 0, 0 };
            double[] Totals = new double[] { 0, 0, 0, 0 };
            double[] Scores = new double[] { 0, 0, 0, 0 };
            foreach (SqlCheckpointVM cp in (from c in PassedCPs orderby c.ErrorSeverity descending select c))
            {
                PassedScores[SqlNoteSection.NoteSections.First(c => c.SectionID == cp.TargetSection).ScoreSection] += cp.ErrorSeverity;
            }
            foreach (SqlCheckpointVM cp in (from c in MissedCPs orderby c.ErrorSeverity descending select c))
            {
                MissedScores[SqlNoteSection.NoteSections.First(c => c.SectionID == cp.TargetSection).ScoreSection] += cp.ErrorSeverity;
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
            strReport += $"<font size='+3'>PatientM ID {patient.PtID}</font><br>"; // "This report is using a programmed algorythm that searches for terms in your documentation.  I personally programmed these terms so they may not apply to this clinical scenario.  I'm working on version 1.0 and I know this report is not perfect, but by version infinity.0 it will be. Please let me know how well my program worked (or failed). Your feedback is so much more important than any feedback I may provide you. Most important is that you let me know if this information is in any way incorrect. I will edit or re-write code to make it correct. Thanks for all you do! ";
            strReport += $"<font size='+1'>Date: {document.VisitDate.ToShortDateString()}</font><br>";
            strReport += Environment.NewLine;

            strReport += $"Scores: HPI <b>{HPI_Score.ToString("0.##")}</b> Exam <b>{Exam_Score.ToString("0.##")}</b> Dx <b>{Dx_Score.ToString("0.##")}</b> Treatment <b>{Rx_Score.ToString("0.##")}</b> <a href='#footnote'>Total Score<sup>*</sup></a> <b>{Total_Score.ToString("0.##")}</b><br><hr>";

            foreach (var seg in document.ICD10Segments)
            {
                if (seg.IncludeSegment)
                    tmpCheck += $"<li><font size='+1'>{seg.SegmentTitle}</font></li>" + Environment.NewLine;
            }
            if (tmpCheck != "")
            {
                strReport += $"<font size='+3'>Relevant ICD10 and Review Topic Segments</font><br><dl><ul>";
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            tmpCheck = "";
            foreach (SqlCheckpointVM cp in (from c in PassedCPs orderby c.ErrorSeverity descending select c))
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
            foreach (SqlCheckpointVM cp in (from c in MissedCPs where c.ErrorSeverity > 0 orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                    tmpCheck += GetReport(cp, document, patient);
            }
            if (tmpCheck != "")
            {
                strReport += "<font size='+3'>Missed check points:</font><br><dl><ul>" + Environment.NewLine;
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            tmpCheck = "";
            foreach (SqlCheckpointVM cp in (from c in PassedCPs where c.ErrorSeverity == 0 orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                    tmpCheck += GetReport(cp, document, patient);
            }
            if (tmpCheck != "")
            {
                strReport += "<font size='+3'>Minor missed check points:</font><dl><ul>" + Environment.NewLine;
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            if (tmpCheck != "")
            {
                strReport += "<font size='+3'>Other relevant points to consider:</font><dl><ul>" + Environment.NewLine;
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            strReport += "<br><br><hl>";
            strReport += "Footnotes:<br>";
            strReport += "<p id='footnote'>* Total Score = (Total of Score Weights missed) / ((Total of Score Weights missed)+(Total of Score Weights passed)) * 2 + 8</p><br>";
            strReport += "** Score Weight = An assigned weight of the importance of the checkpoint.<br>";
            strReport += "</body></html>";
            Clipboard.SetText(strReport);
            ClipboardHelper.CopyToClipboard(strReport, "");
            WinPreviewHTML wp = new WinPreviewHTML();
            wp.MyWB.NavigateToString(strReport);
            wp.ShowDialog();

            //MessageBox.Show(strReport);
        }

        public void CommitReport()
        {
            string sqlCheck = $"Select Count() from RelCPPRovider where PtID={patient.PtID} AND VisitDate='{document.VisitDate.ToString("yyyy-MM-dd")}';";
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
                    string strDelete = $"Delete from RelCPPRovider where PtID={patient.PtID} AND VisitDate='{document.VisitDate.ToString("yyyy-MM-dd")}';";
                    using (IDbConnection cnn1 = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        cnn1.Execute(strDelete);
                    }
                }
            }

            report.ReviewDate = DateTime.Now;

            foreach (SqlCheckpointVM cp in (from c in MissedCPs orderby c.ErrorSeverity descending select c))
            {
                Commit(cp, document, patient, report, SqlRelCPProvider.MyCheckPointStates.Fail);
            }

            foreach (SqlCheckpointVM cp in (from c in PassedCPs orderby c.ErrorSeverity descending select c))
            {
                Commit(cp, document, patient, report, SqlRelCPProvider.MyCheckPointStates.Pass);
            }
            MessageBox.Show($"{document.ProviderSql.CurrentReviewCount}/10 reports committed.");
        }


        public void Commit(SqlCheckpointVM sqlCheckpoint, DocumentVM doc, PatientM pt, VisitReportM rpt, SqlRelCPProvider.MyCheckPointStates cpState)
        {
            if (sqlCheckpoint.CustomComment == null) sqlCheckpoint.CustomComment = "";
            string sql = $"Replace INTO RelCPPRovider (ProviderID, CheckPointID, PtID, ReviewDate, VisitDate, CheckPointStatus, Comment) VALUES ({doc.ProviderID}, {sqlCheckpoint.CheckPointID}, {pt.PtID}, '{rpt.ReviewDate.ToString("yyyy-MM-dd")}', '{doc.VisitDate.ToString("yyyy-MM-dd")}', {(int)cpState}, '{sqlCheckpoint.CustomComment}');";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        public string VisitCheckPointReportHTML
        {
            get
            {
                List<SqlRelCPProvider> rlist;
                string sql = $"Select * from RelCPPRovider where PtID={patient.PtID} and VisitDate='{document.VisitDate.ToString("yyyy-MM-dd")}';";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    rlist = cnn.Query<SqlRelCPProvider>(sql).ToList();
                }


                string strReturn = "";

                return CurrentDocToHTML();
            }
        }

        /// <summary>
        /// Return a string of HTML code representing the current document report
        /// </summary>
        /// <returns></returns>
        public string CurrentDocToHTML()
        {
            double[] PassedScores = new double[] { 0, 0, 0, 0 };
            double[] MissedScores = new double[] { 0, 0, 0, 0 };
            double[] Totals = new double[] { 0, 0, 0, 0 };
            double[] Scores = new double[] { 0, 0, 0, 0 };
            foreach (SqlCheckpointVM cp in (from c in PassedCPs orderby c.ErrorSeverity descending select c))
            {
                PassedScores[SqlNoteSection.NoteSections.First(c => c.SectionID == cp.TargetSection).ScoreSection] += cp.ErrorSeverity;
            }
            foreach (SqlCheckpointVM cp in (from c in MissedCPs orderby c.ErrorSeverity descending select c))
            {
                MissedScores[SqlNoteSection.NoteSections.First(c => c.SectionID == cp.TargetSection).ScoreSection] += cp.ErrorSeverity;
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
            strReport += $"<font size='+3'>PatientM ID {patient.PtID}</font><br>"; // "This report is using a programmed algorythm that searches for terms in your documentation.  I personally programmed these terms so they may not apply to this clinical scenario.  I'm working on version 1.0 and I know this report is not perfect, but by version infinity.0 it will be. Please let me know how well my program worked (or failed). Your feedback is so much more important than any feedback I may provide you. Most important is that you let me know if this information is in any way incorrect. I will edit or re-write code to make it correct. Thanks for all you do! ";
            strReport += $"<font size='+1'>Date: {document.VisitDate.ToShortDateString()}</font><br>";
            strReport += Environment.NewLine;

            strReport += $"Scores: HPI <b>{HPI_Score.ToString("0.##")}</b> Exam <b>{Exam_Score.ToString("0.##")}</b> Dx <b>{Dx_Score.ToString("0.##")}</b> Treatment <b>{Rx_Score.ToString("0.##")}</b> <a href='#footnote'>Total Score<sup>*</sup></a> <b>{Total_Score.ToString("0.##")}</b><br><hr>";

            foreach (var seg in document.ICD10Segments)
            {
                if (seg.IncludeSegment)
                    tmpCheck += $"<li><font size='+1'>{seg.SegmentTitle}</font></li>" + Environment.NewLine;
            }
            if (tmpCheck != "")
            {
                strReport += $"<font size='+3'>Relevant ICD10 and Review Topic Segments</font><br><dl><ul>";
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            tmpCheck = "";
            foreach (SqlCheckpointVM cp in (from c in PassedCPs orderby c.ErrorSeverity descending select c))
            {
                tmpCheck += $"<li><font size='+1'>{cp.CheckPointTitle}</font> <font size='-1'>(Score Weight:{cp.ErrorSeverity}/10)</font></li>" + Environment.NewLine;
                if (cp.CustomComment != "")
                {
                    tmpCheck += $"<br><b>Note: {cp.CustomComment}</b><br>";
                }
            }
            if (tmpCheck != "")
            {
                strReport += "<font size='+3'>Passed check points:</FONT><BR><dl><ul>" + Environment.NewLine;
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            tmpCheck = "";
            foreach (SqlCheckpointVM cp in (from c in MissedCPs where c.ErrorSeverity > 0 orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                    tmpCheck += GetReport(cp, document, patient);
            }
            if (tmpCheck != "")
            {
                strReport += "<font size='+3'>Missed check points:</font><br><dl><ul>" + Environment.NewLine;
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            tmpCheck = "";
            foreach (SqlCheckpointVM cp in (from c in MissedCPs where c.ErrorSeverity == 0 orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                    tmpCheck += GetReport(cp, document, patient);
            }
            if (tmpCheck != "")
            {
                strReport += "<font size='+3'>Minor missed check points:</font><dl><ul>" + Environment.NewLine;
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            if (tmpCheck != "")
            {
                strReport += "<font size='+3'>Other relevant points to consider:</font><dl><ul>" + Environment.NewLine;
                strReport += tmpCheck;
                strReport += "</ul></dl>" + Environment.NewLine;
            }

            strReport += "<br><br><hl>";
            strReport += "Footnotes:<br>";
            strReport += "<p id='footnote'>* Total Score = (Total of Score Weights missed) / ((Total of Score Weights missed)+(Total of Score Weights passed)) * 2 + 8</p><br>";
            strReport += "** Score Weight = An assigned weight of the importance of the checkpoint.<br>";
            strReport += "</body></html>";

            //System.Windows.Clipboard.SetText(strReport);
            //ClipboardHelper.CopyToClipboard(strReport, "");
            return strReport;
        }

        private ICommand mCommitReport;
        public ICommand CommitMyReportCommand
        {
            get
            {
                if (mCommitReport == null)
                    mCommitReport = new CommitMyReport();
                return mCommitReport;
            }
            set
            {
                mCommitReport = value;
            }
        }

    }

    class CommitMyReport : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            VisitReportVM rvm = parameter as VisitReportVM;
            rvm.CommitReport();
        }
        #endregion
    }


}
