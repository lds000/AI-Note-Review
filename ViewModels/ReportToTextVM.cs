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
        private ProviderVM providerVM;
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

        public ReportToTextVM(ProviderVM sProvider, DateTime dt, int ptID)
        {
            providerVM = sProvider;
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

        public string AccountNumber
        { 
            get
            {
                return ptID.ToString();
            }
        }

        public string DOS
        {
            get
            {
                return visitDate.ToString("MM-dd-yyyy");
            }
        }

        public void calculateScores()
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

            hPIScore = Scores[0] / 100 * 2;
            examScore = Scores[1] / 100 * 2;
            dxScore = Scores[2] / 100 * 2;
            rxScore = Scores[3] / 100 * 4;
            totalScore = hPIScore + examScore + dxScore + rxScore;

        }

        public double? hPIScore;
        public string HPIScore
        {
            get
            {
                if (hPIScore == null) calculateScores();
                return ((double)hPIScore).ToString("0.##");
            }
        }

        public double? examScore;
        public string ExamScore
        {
            get
            {
                if (examScore == null) calculateScores();
                return ((double)examScore).ToString("0.##");
            }
        }
        public double? dxScore;
        public string DxScore
        {
            get
            {
                if (dxScore == null) calculateScores();
                return ((double)dxScore).ToString("0.##");
            }
        }
        public double? rxScore;
        public string RxScore
        {
            get
            {
                if (rxScore == null) calculateScores();
                return ((double)rxScore).ToString("0.##");
            }
        }
        public double? totalScore;
        public string TotalScore
        {
            get
            {
                if (examScore == null) calculateScores();
                return ((double)examScore).ToString("0.##");
            }
        }

        public string Notes
        {
            get
            {
                List<ReportToHtmlM> PassedCPs = (from c in lReportToHtmlM where c.CheckPointStatus == ReportToHtmlM.CPStates.Pass orderby c.ErrorSeverity descending select c).ToList();
                List<ReportToHtmlM> MissedCPs = (from c in lReportToHtmlM where c.CheckPointStatus == ReportToHtmlM.CPStates.Fail orderby c.ErrorSeverity descending select c).ToList();
                string strReport = "";
                string tmpCheck = "";
                foreach (ReportToHtmlM cp in PassedCPs)
                {
                    tmpCheck += $"- {cp.CheckPointTitle} (Score Weight:{cp.ErrorSeverity}/10)" + Environment.NewLine;
                    if (cp.RelComment != "" && cp.RelComment != null)
                    {
                        tmpCheck += $"  Note: {cp.RelComment}" + Environment.NewLine;
                    }
                }
                if (tmpCheck != "")
                {
                    strReport += "Passed check points:" + Environment.NewLine;
                    strReport += tmpCheck;
                    strReport += Environment.NewLine;
                }
                tmpCheck = "";
                foreach (ReportToHtmlM cp in (from c in MissedCPs where c.ErrorSeverity > 0 select c))
                {
                    tmpCheck += GetReportText(cp);
                }
                if (tmpCheck != "")
                {
                    strReport += "Missed check points:" + Environment.NewLine;
                    strReport += tmpCheck;
                    strReport += Environment.NewLine;
                }

                tmpCheck = "";
                foreach (ReportToHtmlM cp in (from c in MissedCPs where c.ErrorSeverity == 0 select c))
                {
                    tmpCheck += GetReportText(cp);
                }
                if (tmpCheck != "")
                {
                    strReport += "Minor missed check points:" + Environment.NewLine;
                    strReport += tmpCheck;
                    strReport += Environment.NewLine;
                }
                return strReport;
            }
        }

        public string GetReportText(ReportToHtmlM sqlCheckpoint)
        {
            string strReturn = "";
            strReturn += $"***{sqlCheckpoint.CheckPointTitle} (Score Weight:{sqlCheckpoint.ErrorSeverity}/10) {sqlCheckpoint.Comment}" + Environment.NewLine;
            if (sqlCheckpoint.RelComment != "" && sqlCheckpoint.RelComment != null)
            {
                strReturn += $"***Comment: {sqlCheckpoint.RelComment}" + Environment.NewLine;
            }
            if (sqlCheckpoint.Link != "" && sqlCheckpoint.Link != null)
            {
                strReturn += $"***Link:{sqlCheckpoint.Link}" + Environment.NewLine;
            }
            return strReturn;
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
