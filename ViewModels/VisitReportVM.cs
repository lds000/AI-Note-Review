﻿using Dapper;
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
            report = new VisitReportM();
            sqlProvider = new SqlProvider(); //Change provider for report
            patientVM = new PatientVM();
            patient = patientVM.Patient;
            document = new DocumentVM(sqlProvider, patientVM);
            GenerateReport(); //first time
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
        public ObservableCollection<SqlCheckpointVM> DroppedCheckPoints { get { return report.DroppedCheckPoints; } set { report.DroppedCheckPoints = value; } }
        public ObservableCollection<SqlCheckpointVM> MissedCheckPoints { get { return report.MissedCheckPoints; } set { report.MissedCheckPoints = value; } }
        public ObservableCollection<SqlCheckpointVM> PassedCheckPoints { get { return report.PassedCheckPoints; } set { report.PassedCheckPoints = value; } }
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
        public ObservableCollection<SqlICD10SegmentVM> ICD10Segments
        {
            get
            {
                var tmpCol = document.ICD10Segments;
                foreach (var tmpSeg in tmpCol)
                {
                    tmpSeg.ParentDocument = document;
                }
                return tmpCol;
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

        public List<SqlCheckpointVM> PassedCPs
        {
            get
            {
                List<SqlCheckpointVM> tmpList = new List<SqlCheckpointVM>();
                foreach (var tmpCollection in ICD10Segments)
                {
                    if (tmpCollection.IncludeSegment)
                        foreach (var tmpCP in tmpCollection.PassedCPs)
                        {
                            tmpList.Add(tmpCP);
                        }
                }
                return tmpList;
            }
        }
        public List<SqlCheckpointVM> MissedCPs
        {
            get
            {
                List<SqlCheckpointVM> tmpList = new List<SqlCheckpointVM>();
                foreach (var tmpCollection in ICD10Segments)
                {
                    foreach (var tmpCP in tmpCollection.MissedCPs)
                    {
                        tmpList.Add(tmpCP);
                    }
                }
                return tmpList;
            }
        }
        public List<SqlCheckpointVM> DroppedCPs
        {
            get
            {
                List<SqlCheckpointVM> tmpList = new List<SqlCheckpointVM>();
                foreach (var tmpCollection in ICD10Segments)
                {
                    foreach (var tmpCP in tmpCollection.DroppedCPs)
                    {
                        tmpList.Add(tmpCP);
                    }
                }
                return tmpList;
            }
        }

        /// <summary>
        /// Holds the current review's Yes/No SqlRegex's
        /// </summary>
        private Dictionary<int, bool> YesNoSqlRegExIndex = new Dictionary<int, bool>();



        /// <summary>
        /// Possible checkpoint match results, FailNoCount no longer used, held for legacy reasons.
        /// </summary>
        SqlTagRegExM.EnumResult TagResult; //{ Pass, Fail, FailNoCount, DropTag };

        /// <summary>
        /// Run the SqlTagRegExes of a tag and return as result, this is the brains of the whole operation.
        /// </summary>
        /// <param name="tmpTagRegExs"></param>
        /// <returns></returns>
        private SqlTagRegExM.EnumResult CheckTagRegExs(List<SqlTagRegExVM> tmpTagRegExs)
        {
            foreach (SqlTagRegExVM TagRegEx in tmpTagRegExs) //cycle through the TagRegExs, usually one or two, fail or hide stops iteration, if continues returns pass.
            {
                if (TagRegEx.RegExText.Contains("prolonged")) //used to debug
                {
                }

                //This boolean shortens the code
                bool StopIfMissOrHide = TagRegEx.TagRegExMatchResult != SqlTagRegExM.EnumResult.Pass;

                // check demographic limits and return result if met.
                //If any TagRegEx fails due to demographics, the entire series fails
                double age = patient.GetAgeInYearsDouble();
                if (age < TagRegEx.MinAge) return SqlTagRegExM.EnumResult.Hide;
                if (age >= TagRegEx.MaxAge) return SqlTagRegExM.EnumResult.Hide;
                if (patient.isMale && !TagRegEx.Male) return SqlTagRegExM.EnumResult.Hide;
                if (!patient.isMale && !TagRegEx.Female) return SqlTagRegExM.EnumResult.Hide;

                //Process each of the tags, if any fail or hide then series stop, otherwise passes.
                //Process Yes/No Tag
                if (TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.Ask) //ask question... pass if yes, fail if no
                {
                    if (Properties.Settings.Default.AskYesNo) //If Bypass is on then assume answer was yes
                    {
                        if (StopIfMissOrHide) return TagRegEx.TagRegExMatchResult; //Match result is the result if a positive "yes" or "no" if set as Result (not "noResult") match is met
                        continue;
                    }
                    else
                    {
                        bool yn = false;
                        if (YesNoSqlRegExIndex.ContainsKey(TagRegEx.TagRegExID))
                        {
                            yn = YesNoSqlRegExIndex[TagRegEx.TagRegExID];
                        }
                        else
                        {
                            WinShowRegExYesNo ws = new WinShowRegExYesNo();
                            if (TagRegEx.RegExText.Contains('|'))
                            {
                                ws.tbQuestion.Text = TagRegEx.RegExText.Split('|')[1];
                            }
                            else
                            {
                                ws.tbQuestion.Text = TagRegEx.RegExText;
                            }
                            ws.DataContext = TagRegEx;
                            ws.ShowDialog();
                            YesNoSqlRegExIndex.Add(TagRegEx.TagRegExID, ws.YesNoResult);
                            yn = ws.YesNoResult;
                        }
                        if (yn == true)
                        {
                            if (StopIfMissOrHide) return TagRegEx.TagRegExMatchResult; //if Yes return 1st Result option if it's fail or hide
                            continue; //continue to next iteration bacause result is pass.
                        }
                        else
                        {
                            if (TagRegEx.TagRegExMatchNoResult != SqlTagRegExM.EnumResult.Pass) return TagRegEx.TagRegExMatchNoResult;
                            continue;  //continue to next iteration bacause result is pass.
                        }
                    }
                }

                //process all,none,any match condition
                //Cycle through the list of terms and search through section of note if term is a match or not
                bool AllTermsMatch = true;
                bool NoTermsMatch = true;

                string strTextToMatch = "";
                if (document.NoteSectionText[TagRegEx.TargetSection] != null) strTextToMatch = document.NoteSectionText[TagRegEx.TargetSection].ToLower();
                foreach (string strRegEx in TagRegEx.RegExText.Split(','))
                {
                    if (strRegEx.Trim() != "")
                    {
                        //This is original: i took the prefix out, not sure why it was there if (Regex.IsMatch(strTextToMatch, CF.strRegexPrefix + strRegEx.Trim(), RegexOptions.IgnoreCase))
                        if (Regex.IsMatch(strTextToMatch, CF.strRegexPrefix + strRegEx.Trim(), RegexOptions.IgnoreCase)) // /i is lower case directive for regex
                        {
                            //Match is found!
                            //ANY condition is met, so stop if miss or hide if that is the 1st action
                            if (StopIfMissOrHide) if (TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.Any) return TagRegEx.TagRegExMatchResult; //Contains Any return 2nd Result - don't continue if type is "ANY NF" this is a stopper.
                            NoTermsMatch = false;
                            if (TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.Any) break; //condition met, no need to check rest
                        }
                        else
                        {
                            AllTermsMatch = false;
                        }
                    }
                }
                //ALL condition met if all terms match
                if (StopIfMissOrHide)
                {
                    if (AllTermsMatch && StopIfMissOrHide)
                    {
                        if (TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.All) return TagRegEx.TagRegExMatchResult; //Contains All return 2nd Result because any clause not reached
                    }
                    if (NoTermsMatch && TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.None) return TagRegEx.TagRegExMatchResult; //Contains Any return 2nd Result - don't continue if type is "ANY NF" this is a stopper.)
                    if (!NoTermsMatch && TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.Any) return TagRegEx.TagRegExMatchNoResult;
                }
                //NONE condition met if no terms match

                if (!NoTermsMatch && TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.Any) continue;

                if (NoTermsMatch && TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.None) //none condition met, carry out pass
                {

                }
                else
                {
                    if (TagRegEx.TagRegExMatchNoResult != SqlTagRegExM.EnumResult.Pass) return TagRegEx.TagRegExMatchNoResult;
                }
                //ASK,ALL, and NONE conditions are note met, so the NoResult condition is the action
            }

            return SqlTagRegExM.EnumResult.Pass; //default is pass
        }

        private void ResetCheckpointStatus()
        {

        }
        /// <summary>
        /// clear note, clear checkpoints, check note
        /// </summary>
        /// <param name="resetOverides"></param>
        public void GenerateReport(bool resetOverides = false)
        {
            Stopwatch stopwatch = new Stopwatch();

            int tmpC = 0;
            stopwatch.Start();

            if (resetOverides) //reset the yesno saved index and cpstatus overides I have marked on the checkbox
            {
                report.CPStatusOverrides.Clear();
                YesNoSqlRegExIndex.Clear();
            }
            report.DocumentTags.Clear();
            report.PassedCheckPoints.Clear();
            report.MissedCheckPoints.Clear();
            report.DroppedCheckPoints.Clear();

            List<int> AlreadyAddedCheckPointIDs = new List<int>();

            foreach (SqlICD10SegmentVM ns in ICD10Segments)
            {
                if (!ns.IncludeSegment) continue;

                foreach (SqlCheckpointVM cp in ns.Checkpoints)
                {
                    foreach (var p in report.CPStatusOverrides)
                    {
                        if (p.Key.CheckPointID == cp.CheckPointID)
                        {
                            report.MissedCheckPoints.Remove(p.Key);
                            report.PassedCheckPoints.Remove(p.Key);
                            if (p.Value == SqlRelCPProvider.MyCheckPointStates.Fail)
                            {
                                report.MissedCheckPoints.Add(p.Key);
                                p.Key.IncludeCheckpoint = true;
                            }
                            if (p.Value == SqlRelCPProvider.MyCheckPointStates.Pass)
                            {
                                report.PassedCheckPoints.Add(p.Key);
                                p.Key.IncludeCheckpoint = false;
                            }
                            AlreadyAddedCheckPointIDs.Add(p.Key.CheckPointID);
                        }
                    }
                    if (AlreadyAddedCheckPointIDs.Contains(cp.CheckPointID)) //no need to double check
                    {
                        continue;
                    }
                    AlreadyAddedCheckPointIDs.Add(cp.CheckPointID);
                    ///Console.WriteLine($"Now analyzing '{cp.CheckPointTitle}' checkpoint.");
                    SqlTagRegExM.EnumResult trTagResult = SqlTagRegExM.EnumResult.Pass;
                    if (cp.CheckPointTitle.Contains("Augmentin XR"))
                    {

                    }
                    foreach (SqlTagVM tagCurrentTag in cp.GetTags())
                    {
                        SqlTagRegExM.EnumResult trCurrentTagResult;
                        List<SqlTagRegExVM> tmpTagRegExs = tagCurrentTag.GetTagRegExs();
                        trCurrentTagResult = CheckTagRegExs(tmpTagRegExs);

                        if (trCurrentTagResult != SqlTagRegExM.EnumResult.Pass)
                        {
                            //tag fails, no match.
                            trTagResult = trCurrentTagResult;
                            break; //if the first tag does not qualify, then do not proceed to the next tag.
                        }
                        report.DocumentTags.Add(tagCurrentTag.TagText);
                    }

                    switch (trTagResult)
                    {
                        case SqlTagRegExM.EnumResult.Pass:
                            cp.IncludeCheckpoint = false;
                            report.PassedCheckPoints.Add(cp); //do not include passed for All diagnosis.
                            break;
                        case SqlTagRegExM.EnumResult.Hide:
                            cp.IncludeCheckpoint = false;
                            report.DroppedCheckPoints.Add(cp);
                            break;
                        case SqlTagRegExM.EnumResult.Miss:
                            cp.IncludeCheckpoint = true;
                            report.MissedCheckPoints.Add(cp);
                            break;
                        default:
                            break;
                    }
                }
            }

            //now re-order checkpoints by severity
            report.PassedCheckPoints = new ObservableCollection<SqlCheckpointVM>(report.PassedCheckPoints.OrderByDescending(c => c.ErrorSeverity));
            report.MissedCheckPoints = new ObservableCollection<SqlCheckpointVM>(report.MissedCheckPoints.OrderByDescending(c => c.ErrorSeverity));
            report.DroppedCheckPoints = new ObservableCollection<SqlCheckpointVM>(report.DroppedCheckPoints.OrderByDescending(c => c.ErrorSeverity));
            stopwatch.Stop();
            Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine(tmpC);
        }


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
            foreach (SqlCheckpointVM cp in (from c in report.PassedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                PassedScores[SqlNoteSection.NoteSections.First(c => c.SectionID == cp.TargetSection).ScoreSection] += cp.ErrorSeverity;
            }
            foreach (SqlCheckpointVM cp in (from c in report.MissedCheckPoints orderby c.ErrorSeverity descending select c))
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
            foreach (SqlCheckpointVM cp in (from c in report.PassedCheckPoints orderby c.ErrorSeverity descending select c))
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
            foreach (SqlCheckpointVM cp in (from c in report.MissedCheckPoints where c.ErrorSeverity > 0 orderby c.ErrorSeverity descending select c))
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
            foreach (SqlCheckpointVM cp in (from c in report.MissedCheckPoints where c.ErrorSeverity == 0 orderby c.ErrorSeverity descending select c))
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

            foreach (SqlCheckpointVM cp in (from c in report.MissedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                Commit(cp, document, patient, report, SqlRelCPProvider.MyCheckPointStates.Fail);
            }

            foreach (SqlCheckpointVM cp in (from c in report.PassedCheckPoints orderby c.ErrorSeverity descending select c))
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


                report.MissedCheckPoints.Clear();
                report.DroppedCheckPoints.Clear();
                report.PassedCheckPoints.Clear();
                string strReturn = "";
                foreach (SqlRelCPProvider r in rlist)
                {
                    SqlCheckpointVM cp = new SqlCheckpointVM(r.CheckPointID);
                    if (r.Comment != "")
                    {
                        cp.CustomComment = r.Comment;
                    }
                    if (r.CheckPointStatus == SqlRelCPProvider.MyCheckPointStates.Pass)
                    {
                        report.PassedCheckPoints.Add(cp);
                    }
                    if (r.CheckPointStatus == SqlRelCPProvider.MyCheckPointStates.Fail)
                    {
                        report.MissedCheckPoints.Add(cp);
                    }
                }

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
            foreach (SqlCheckpointVM cp in (from c in report.PassedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                PassedScores[SqlNoteSection.NoteSections.First(c => c.SectionID == cp.TargetSection).ScoreSection] += cp.ErrorSeverity;
            }
            foreach (SqlCheckpointVM cp in (from c in report.MissedCheckPoints orderby c.ErrorSeverity descending select c))
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
            foreach (SqlCheckpointVM cp in (from c in report.PassedCheckPoints orderby c.ErrorSeverity descending select c))
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
            foreach (SqlCheckpointVM cp in (from c in report.MissedCheckPoints where c.ErrorSeverity > 0 orderby c.ErrorSeverity descending select c))
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
            foreach (SqlCheckpointVM cp in (from c in report.MissedCheckPoints where c.ErrorSeverity == 0 orderby c.ErrorSeverity descending select c))
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


        public void AddCheckPoint(SqlCheckpointM cp, DateTime dtReviewDate)
        {
            string sql = $"INSERT INTO RelCPPRovider (ProviderID,CheckPointID,PtID,HomeClinic,ReviewInterval,IsWestSidePod) VALUES ({document.ProviderID},{cp.CheckPointID},{patient.PtID},'{dtReviewDate}','{document.VisitDate}',{sqlProvider.IsWestSidePod});";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
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
