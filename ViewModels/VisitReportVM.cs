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
        #region inotify
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        //key entities
        private VisitReportM report;
        private DocumentVM document;
        private PatientVM patient;

        private VisitReportV currentVisitReportV;
        public VisitReportV CurrentVisitReportV
        {
            get
            {
                return currentVisitReportV;
            }
            set
            {
                currentVisitReportV = value;
            }
        }

        private MasterReviewSummaryVM masterReviewSummary;
        public MasterReviewSummaryVM MasterReviewSummary
        {
            get
            {
                return masterReviewSummary;
            }
            set
            {
                masterReviewSummary = value;
                OnPropertyChanged();
            }
        }

        public VisitReportVM(MasterReviewSummaryVM mrs)
        {
            masterReviewSummary = mrs;
            report = new VisitReportM(); //1st executed command in program
            patient = mrs.Patient;
            document = mrs.Document;
            document.ICD10Segments = null; //reset segments
        }

        private SqlCheckpointVM selectedCheckPoint;
        public SqlCheckpointVM SelectedCheckPoint
        {
            get
            {
                return selectedCheckPoint;
            }
            set
            {
                selectedCheckPoint = value;
                OnPropertyChanged();
            }
        }

        #region Report VM definitions - boring stuff
        /// <summary>
        /// The VisitReport Model
        /// </summary>
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
        public ProviderVM Provider
        { get { return document.Provider; } set { document.Provider = value; } }
        public DateTime VisitDate { get { return document.VisitDate; } set { document.VisitDate = value; } }
        public string HashTags { get { return document.HashTags; } set { document.HashTags = value; } }
        public ObservableCollection<string> ICD10s { get { return document.ICD10s; } set { document.ICD10s = value; } }

        #endregion
        #region Patient yadda tadd
        public PatientVM Patient
        {
            get
            {
                return patient;
            }
        }
        #endregion



        private string searchICD10Term;
        public string SearchICD10Term
        {
            get 
            {
                return searchICD10Term; 
            }
            set
            {
                searchICD10Term = value;
                iCD10SegmentSearchResult = null;
                OnPropertyChanged();
                OnPropertyChanged("ICD10SegmentSearchResult");
            }
        }

        private List<SqlICD10SegmentVM> iCD10SegmentSearchResult;
        public List<SqlICD10SegmentVM> ICD10SegmentSearchResult
        {
            get
            {
                if (iCD10SegmentSearchResult == null && searchICD10Term != null)
                {
                    if (searchICD10Term.Length > 2)
                    {
                       iCD10SegmentSearchResult = (from c in SqlICD10SegmentVM.NoteICD10Segments where c.SegmentTitle.ToLower().Contains(searchICD10Term.ToLower()) select c).ToList();
                    }
                }
                return iCD10SegmentSearchResult;
            }
        }

        private SqlICD10SegmentVM currentlySelectedSearchICD10;
        public SqlICD10SegmentVM CurrentlySelectedSearchICD10
        {
            get { return currentlySelectedSearchICD10; }
            set
            {
                currentlySelectedSearchICD10 = value;
                if (value != null)
                {
                    currentlySelectedSearchICD10.ParentReport = this;
                    currentlySelectedSearchICD10.ParentDocument = document;
                    Document.ICD10Segments.Add(currentlySelectedSearchICD10);
                    currentlySelectedSearchICD10.CBIncludeSegment = true;
                }
            }
        }

        /// <summary>
        /// Evaluate each relavent checkpoint and get the status, this is used to sequentially evaluate each checkpoint avoiding the stacked Yes/No Question issue.
        /// </summary>
        public void PopulateCPStatuses()
        {
            foreach (var tmpCollection in document.ICD10Segments)
            {
                if (tmpCollection.IncludeSegment)
                {
                    foreach (var cp in tmpCollection.Checkpoints)
                    {
                        var result = cp.CPStatus;
                    }
                    //var tmpP = tmpCollection.PassedCPs; //only run the passedCPs, as this evaluates all CPs to determine which are passed status.
                }
            }
        }

        /// <summary>
        /// Holds the current review's Yes/No SqlRegex's
        /// </summary>
        private Dictionary<int, bool> YesNoSqlRegExIndex = new Dictionary<int, bool>();
        
        /// <summary>
        /// Save visit report as a review with overides and comments
        /// </summary>
        public void CommitReport()
        {
            //todo: move this up sooner to avoid extra work
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

            string sql = "";

            foreach (var tmpCollection in Document.ICD10Segments) //only run once per report
            {
                if (tmpCollection.IncludeSegment)
                {
                    foreach (SqlCheckpointVM cp in (from c in tmpCollection.PassedCPs orderby c.ErrorSeverity descending select c))
                    {
                        if (cp.IncludeCheckpoint)
                        {
                            if (cp.CustomComment == null)
                                cp.CustomComment = "";
                            sql += $"Replace INTO RelCPPRovider (ProviderID, CheckPointID, PtID, ReviewDate, VisitDate, CheckPointStatus, Comment) VALUES ({document.Provider.ProviderID}, {cp.CheckPointID}, {patient.PtID}, '{report.ReviewDate.ToString("yyyy-MM-dd")}', '{document.VisitDate.ToString("yyyy-MM-dd")}', {(int)SqlRelCPProvider.MyCheckPointStates.Pass}, '{cp.CustomComment}');\n";
                        }
                    }
                    foreach (SqlCheckpointVM cp in (from c in tmpCollection.MissedCPs orderby c.ErrorSeverity descending select c))
                    {
                        if (cp.IncludeCheckpoint)
                        {
                            if (cp.CustomComment == null)
                                cp.CustomComment = "";
                            sql += $"Replace INTO RelCPPRovider (ProviderID, CheckPointID, PtID, ReviewDate, VisitDate, CheckPointStatus, Comment) VALUES ({document.Provider.ProviderID}, {cp.CheckPointID}, {patient.PtID}, '{report.ReviewDate.ToString("yyyy-MM-dd")}', '{document.VisitDate.ToString("yyyy-MM-dd")}', {(int)SqlRelCPProvider.MyCheckPointStates.Fail}, '{cp.CustomComment}');\n";
                        }
                    }
                }
            }


            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
            //document.Provider.SetCurrentMasterReview(document.VisitDate);
            document.Provider.SetCurrentMasterReview(DateTime.Now);
            int tmpc = document.Provider.CurrentNoteDataCount-1;
            string strResult = "";
            if (tmpc > 0)
            {
                strResult = $"{tmpc} notes left.";
            }
            else
            {
                strResult = "No notes left.";
            }
            if (tmpc == 1) strResult = "1 note left.";
        MessageBox.Show($"{document.Provider.CurrentReviewCount}/10 reports committed for {document.Provider.FullName}. {strResult}.");
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

        private ICommand mShowNote;
        public ICommand ShowNoteCommand
        {
            get
            {
                if (mShowNote == null)
                    mShowNote = new ShowNoteEx();
                return mShowNote;
            }
            set
            {
                mCommitReport = value;
            }
        }


        private ICommand mGrabNextNote;
        public ICommand GrabNextNoteCommand
        {
            get
            {
                if (mGrabNextNote == null)
                    mGrabNextNote = new GrabNextNote();
                return mGrabNextNote;
            }
            set
            {
                mGrabNextNote = value;
            }

        }

        private ICommand mSkipNote;
        public ICommand SkipNoteCommand
        {
            get
            {
                if (mSkipNote == null)
                    mSkipNote = new SkipNote();
                return mSkipNote;
            }
            set
            {
                mSkipNote = value;
            }

        }

    }



}
