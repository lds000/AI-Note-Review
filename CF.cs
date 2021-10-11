using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace AI_Note_Review
{

    static class GenericMethods
    {
        public static void CreateNewOrUpdateExisting<TKey, TValue>(
this IDictionary<TKey, TValue> map, TKey key, TValue value)
        {
            map[key] = value;
        }

    }

    public class CF
    {

        public static string strRegexPrefix = @"[ \-,.;\n\r\s]";
        public static List<SqlNoteSection> NoteSections { get; set; }
        public static List<SqlICD10Segment> NoteICD10Segments = new List<SqlICD10Segment>();
        public static DocInfo CurrentDoc = new DocInfo();
        public static List<SqlCheckpoint> CheckPointList = new List<SqlCheckpoint>();
        public static List<SqlTagRegExType> TagRegExTypes { get; set; }

        public static List<SqlTagRegExMatchResults> TagRegExMatchResults
        {
            get {
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


        public static void UpdateNoteICD10Segments()
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                string sql = "Select * from ICD10Segments order by icd10Chapter, icd10CategoryStart;";
                NoteICD10Segments = cnn.Query<SqlICD10Segment>(sql).ToList();
            }
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
                        ns.icd10Margin = new Thickness(10, 0, 0, 0);
                    }
                    else
                    {
                        CodeStart = ns.icd10CategoryStart;
                        CodeEnd = ns.icd10CategoryEnd;
                        charChapter = char.Parse(ns.icd10Chapter);
                    }
                }
                else
                {
                    charChapter = char.Parse(ns.icd10Chapter);
                    CodeStart = 0;
                    CodeEnd = 0;
                }

                /*
                 * 
                 *             if (PtAgeYrs > 65) HashTags += "@Elderly, ";
            if (PtSex.StartsWith("M")) HashTags += "@Male, ";
            if (PtSex.StartsWith("F")) HashTags += "@Female, ";
            if (PtAgeYrs < 4) HashTags += "@Child, ";
            if (PtAgeYrs < 2) HashTags += "@Infant, ";
            if (IsHTNUrgency) HashTags += "!HTNUrgency, ";
            if (isO2Abnormal) HashTags += "!Hypoxic, ";
            if (IsPregCapable) HashTags += "@pregnantcapable, ";
            if (PtAgeYrs >= 13) HashTags += "@sexuallyActiveAge, ";
            if (PtAgeYrs >= 16) HashTags += "@DrinkingAge, ";
            if (PtAgeYrs >= 2) HashTags += "@SpeakingAge, ";
            if (PtAgeYrs < 1) HashTags += "@Age<1, ";
            if (PtAgeYrs < 2) HashTags += "@Age<2, ";
            if (PtAgeYrs < 4) HashTags += "@Age<4, ";
            if (GetAgeInDays()<183) HashTags += "@Age<6mo, ";

36	X	99	99	All Diagnosis
40	X	1	1	Hypertensive Urgency
72	X	2	2	Rapid Respiratory Rate
73	X	3	3	High Fever
74	X	4	4	Tachycardia
75	X	5	5	Elderly
76	X	6	6	Infant
80	X	7	7	Children
81	X	8	8	Interactions
82	X	9	9	Possible Pregnant State
83	X	10	10	Lab Considerations
84	X	11	11	Imaging Considerations
85	X	12	12	Referral Considerations
90	X	13	13	ED Transfer
                 */

                if (ns.ICD10SegmentID == 90) //ed transfer, never include
                {
                    ns.IncludeSegment = false;
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
                if (cp.CustomComment!="")
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

            if (!IsOnScreen((int)dblLeft, (int) dblTop))
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
