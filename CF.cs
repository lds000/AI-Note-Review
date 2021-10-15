using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace AI_Note_Review
{

    public static class GenericMethods
    {
        public static void CreateNewOrUpdateExisting<TKey, TValue>(
this IDictionary<TKey, TValue> map, TKey key, TValue value)
        {
            map[key] = value;
        }

    }

    public class CF : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public CF()
        {

        }

        /// <summary>
        /// This prefix is used to match whole words and not parts of words
        /// </summary>
        //public static string strRegexPrefix = @"[ \-,.;\n\r\s^]";
        public static string strRegexPrefix = @"\b";
        public static List<SqlNoteSection> NoteSections
        {
            get
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from NoteSections order by SectionOrder;";
                    return cnn.Query<SqlNoteSection>(sql).ToList();
                }
            }

        }
        public static List<SqlICD10Segment> NoteICD10Segments
        {
            get
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from ICD10Segments order by icd10Chapter, icd10CategoryStart;";
                    return cnn.Query<SqlICD10Segment>(sql).ToList();
                }
            }
        }

        public static DocInfo CurrentDoc = new DocInfo();
        public static List<SqlCheckpoint> CheckPointList
        {
            get
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from CheckPoints;";
                    return cnn.Query<SqlCheckpoint>(sql).ToList();
                }
            }
        }
        public static Dictionary<int, bool> YesNoSqlRegExIndex = new Dictionary<int, bool>();
        public static List<SqlTagRegExType> TagRegExTypes {
            get 
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from TagRegExTypes;";
                    return cnn.Query<SqlTagRegExType>(sql).ToList();
                }
            }
            }

        public static bool IsReviewWindowOpen = false;
        private static List<SqlNoteSection> noteSections;
        private static List<SqlICD10Segment> noteICD10Segments;
        private static List<SqlCheckpoint> checkPointList;
        private static List<SqlTagRegExType> tagRegExTypes;

        public static List<SqlTagRegExMatchResults> TagRegExMatchResults
        {
            get
            {
                string sql = "Select * from TagRegExMatchResults;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.Query<SqlTagRegExMatchResults>(sql).ToList();
                }

            }
        }

        public static List<SqlTagRegExMatchTypes> TagRegExMatchTypes
        {
            get
            {
                string sql = "Select * from TagRegExMatchTypes;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.Query<SqlTagRegExMatchTypes>(sql).ToList();
                }

            }
        }


        public static string CurrentDocToHTML()
        {
            double[] PassedScores = new double[] { 0, 0, 0, 0 };
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

            strReport += $"Scores: HPI <b>{HPI_Score.ToString("0.##")}</b> Exam <b>{Exam_Score.ToString("0.##")}</b> Dx <b>{Dx_Score.ToString("0.##")}</b> Treatment <b>{Rx_Score.ToString("0.##")}</b> <a href='#footnote'>Total Score<sup>*</sup></a> <b>{Total_Score.ToString("0.##")}</b><br><hr>";

            foreach (var seg in CF.CurrentDoc.ICD10Segments)
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
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.PassedCheckPoints orderby c.ErrorSeverity descending select c))
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

        public static void SetWindowPosition(Window _Window)
        {
            WindowPosition wp = SqlLiteDataAccess.GetWindowPosition(_Window.Title);
            if (wp == null)
            {
                wp = new WindowPosition(_Window.Title, (int)_Window.Top, (int)_Window.Left);
                wp.SaveToDB();
                return;
            }

            double dblTop = wp.WindowPositionTop;
            double dblLeft = wp.WindowPositionLeft;

            //if (dblTop < 0) dblTop = 0; //do this if you don't have two screens.
            //if (dblLeft < 0) dblLeft = 0;

            if (!IsOnScreen((int)dblLeft, (int)dblTop))
            {
                dblTop = 0;
                dblLeft = 0;
            }
            _Window.Top = dblTop;
            _Window.Left = dblLeft;
        }

        public static bool IsOnScreen(int iLeft, int iTop)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                System.Drawing.Point formTopLeft = new System.Drawing.Point(iLeft, iTop);

                if (screen.WorkingArea.Contains(formTopLeft))
                {
                    return true;
                }
            }

            return false;
        }
        public static void SaveWindowPosition(Window _Window)
        {
            WindowPosition wp = SqlLiteDataAccess.GetWindowPosition(_Window.Title);
            if (wp == null)
            {
                return;
            }

            double dblTop = wp.WindowPositionTop;
            double dblLeft = wp.WindowPositionLeft;

            if ((_Window.Top != dblTop) || (_Window.Left != dblLeft))
            {
                wp.WindowPositionTop = (int)_Window.Top;
                wp.WindowPositionLeft = (int)_Window.Left;
                wp.SaveToDB();
            }
        }


    }


}
