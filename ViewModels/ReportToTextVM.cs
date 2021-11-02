using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace AI_Note_Review
{
    public class ReportToTextVM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ReportToTextVM reportToHtmlVM;
        private SqlProvider sqlProvider;
        private int ptID;
        private DateTime visitDate;
        private List<ReportToHtmlM> lReportToHtmlM;
        public ReportToTextVM ReportToHtml
        {
            get
            {
                return reportToHtmlVM;
            }
        }

        public ReportToTextVM()
        {
        }

        public ReportToTextVM(SqlProvider sProvider, DateTime dt, int ptID)
        {
            sqlProvider = sProvider;
            this.ptID = ptID;
            this.visitDate = dt;
            string sqlCheck = $"Select * from(Select distinct CheckPointID rel,Comment RelComment,CheckPointStatus from RelCPPRovider where PtID={ptID} " +
                $"AND VisitDate = '{dt.ToString("yyyy-MM-dd")}') " +
                $"inner join CheckPoints cp on rel = cp.CheckPointID " +
                $"order by cp.TargetSection, cp.ErrorSeverity desc;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                lReportToHtmlM = cnn.Query<ReportToHtmlM>(sqlCheck).ToList();
            }
        }

        public string CheckPointsSummaryHTML
        {
            get
            {
                List<ReportToHtmlM> PassedCPs = (from c in lReportToHtmlM where c.CheckPointStatus == ReportToHtmlM.CPStates.Pass orderby c.ErrorSeverity descending select c).ToList();
                List<ReportToHtmlM> MissedCPs = (from c in lReportToHtmlM where c.CheckPointStatus == ReportToHtmlM.CPStates.Fail orderby c.ErrorSeverity descending select c).ToList();
                string strReturn = "";

                double[] PassedScores = new double[] { 0, 0, 0, 0 };
                double[] MissedScores = new double[] { 0, 0, 0, 0 };
                double[] Totals = new double[] { 0, 0, 0, 0 };
                double[] Scores = new double[] { 0, 0, 0, 0 };
                foreach (ReportToHtmlM cp in PassedCPs)
                {
                    PassedScores[SqlNoteSection.NoteSections.First(c => c.SectionID == cp.TargetSection).ScoreSection] += cp.ErrorSeverity;
                }
                foreach (ReportToHtmlM cp in MissedCPs)
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
                string strReport = $"{sqlProvider.FullName}";
                strReport += $"{ptID}"; // "This report is using a programmed algorythm that searches for terms in your documentation.  I personally programmed these terms so they may not apply to this clinical scenario.  I'm working on version 1.0 and I know this report is not perfect, but by version infinity.0 it will be. Please let me know how well my program worked (or failed). Your feedback is so much more important than any feedback I may provide you. Most important is that you let me know if this information is in any way incorrect. I will edit or re-write code to make it correct. Thanks for all you do! ";
                strReport += $"<font size='+1'>Date: {visitDate.ToShortDateString()}</font><br>";
                strReport += Environment.NewLine;

                strReport += $"Scores: HPI <b>{HPI_Score.ToString("0.##")}</b> Exam <b>{Exam_Score.ToString("0.##")}</b> Dx <b>{Dx_Score.ToString("0.##")}</b> Treatment <b>{Rx_Score.ToString("0.##")}</b> <a href='#footnote'>Total Score<sup>*</sup></a> <b>{Total_Score.ToString("0.##")}</b><br><hr>";

                if (tmpCheck != "")
                {
                    strReport += $"<font size='+3'>Relevant ICD10 and Review Topic Segments</font><br><dl><ul>";
                    strReport += tmpCheck;
                    strReport += "</ul></dl>" + Environment.NewLine;
                }

                tmpCheck = "";
                foreach (ReportToHtmlM cp in PassedCPs)
                {
                    tmpCheck += $"<li><font size='+1'>{cp.CheckPointTitle}</font> <font size='-1'>(Score Weight:{cp.ErrorSeverity}/10)</font></li>" + Environment.NewLine;
                    if (cp.RelComment != "" && cp.RelComment != null)
                    {
                        tmpCheck += $"<br><b>Note: {cp.RelComment}</b><br>";
                    }
                }
                if (tmpCheck != "")
                {
                    strReport += "<font size='+3'>Passed check points:</FONT><BR><dl><ul>" + Environment.NewLine;
                    strReport += tmpCheck;
                    strReport += "</ul></dl>" + Environment.NewLine;
                }

                tmpCheck = "";
                foreach (ReportToHtmlM cp in (from c in MissedCPs where c.ErrorSeverity > 0 select c))
                {
                    tmpCheck += GetReport(cp);
                }
                if (tmpCheck != "")
                {
                    strReport += "<font size='+3'>Missed check points:</font><br><dl><ul>" + Environment.NewLine;
                    strReport += tmpCheck;
                    strReport += "</ul></dl>" + Environment.NewLine;
                }

                tmpCheck = "";
                foreach (ReportToHtmlM cp in (from c in MissedCPs where c.ErrorSeverity == 0 select c))
                {
                    tmpCheck += GetReport(cp);
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
                strReport += "** Score Weight = An assigned weight of my estimated importance of the checkpoint.<br>";
                strReport += "</body></html>";

                //System.Windows.Clipboard.SetText(strReport);
                //ClipboardHelper.CopyToClipboard(strReport, "");
                return strReport;
            }
        }

        public string GetReport(ReportToHtmlM sqlCheckpoint)
        {
            string strReturn = "";
            strReturn += $"<li><dt><font size='+1'>{sqlCheckpoint.CheckPointTitle}</font><font size='-1'> (Score Weight<sup>**</sup>:{sqlCheckpoint.ErrorSeverity}/10)</font></dt><dd><i>{sqlCheckpoint.Comment}</i></dd></li>" + Environment.NewLine;
            if (sqlCheckpoint.RelComment != "" && sqlCheckpoint.RelComment != null)
            {
                strReturn += $"<b>Comment: {sqlCheckpoint.RelComment}</b><br>";
            }
            if (sqlCheckpoint.Link != "" && sqlCheckpoint.Link != null)
            {
                strReturn += $"<a href={sqlCheckpoint.Link}>Click here for reference.</a><br>";
            }
            strReturn += $"<a href='mailto:Lloyd.Stolworthy@PrimaryHealth.com?subject=Feedback on review of {ptID} on {visitDate.ToShortDateString()}. (Ref:{ptID}|{visitDate.ToShortDateString()}|{sqlCheckpoint.CheckPointID})'>Feedback</a>";

            /*
            strReturn += $"\tSignificance {ErrorSeverity}/10." + Environment.NewLine;
            strReturn += $"\tRecommended Remediation: {Action}" + Environment.NewLine;
            strReturn += $"\tExplanation: {Comment}" + Environment.NewLine;
            if (Link != "")
            strReturn += $"\tLink: {Link}" + Environment.NewLine;
            strReturn += Environment.NewLine;
            strReturn += Environment.NewLine;
            */
            return strReturn;
        }
    }
}
