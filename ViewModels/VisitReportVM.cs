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
            passedCPs = new ObservableCollection<ICheckPoint>(); //use interface for compatability with the merge observablecollection
            missedCPs = new ObservableCollection<SqlCheckpointVM>();
            droppedCPs = new ObservableCollection<SqlCheckpointVM>();
            NewEcWDocument();
        }

        //not sure I need this.
        public void NewEcWDocument()
        {
            document.ICD10Segments = null; //reset segments
            iCD10Segments = null; //reset segments
            passedCPs = null;
            missedCPs = null;
            droppedCPs = null;
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

        private ObservableCollection<SqlICD10SegmentVM> iCD10Segments;
        public ObservableCollection<SqlICD10SegmentVM> ICD10Segments
        {
            get
            {
                if (iCD10Segments == null)
                {
                    iCD10Segments = document.ICD10Segments;
                        //new ObservableCollection<SqlICD10SegmentVM>(document.ICD10Segments.OrderByDescending(c => c.Checkpoints.Count));
                    foreach (var tmpSeg in iCD10Segments)
                    {
                        tmpSeg.ParentDocument = document;
                        tmpSeg.ParentReport = this;
                        tmpSeg.PropertyChanged += ICD10Segment_PropertyChanged;
                    }
                }
                return iCD10Segments;
            }
            set
            {
                iCD10Segments = value;
            }
        }

        private void ICD10Segment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RecalculateCPStatus")
            {
                //leave comement for now, this re-selects the point after an CPStatus has changed.
                /*
                SqlICD10SegmentVM tmpSeg = sender as SqlICD10SegmentVM;
                if (tmpSeg != null)
                {
                    int tmpCPID = SelectedCheckPoint.CheckPointID; //get selected CP ID
                    //tmpSeg.Checkpoints = null; //reset checkpoints
                    foreach (var tmpCP in tmpSeg.Checkpoints)//todo: I may not need to set all to null
                    {
                        tmpCP.CPStatus = null;
                    }
                    SelectedCheckPoint = (from c in tmpSeg.Checkpoints where c.CheckPointID == tmpCPID select c).FirstOrDefault();
                }
                */
            }
        }

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

                    ICD10Segments.Add(currentlySelectedSearchICD10);
                    OnPropertyChanged("ICD10Segments");
                    currentlySelectedSearchICD10.CBIncludeSegment = true;
                }
            }
        }

        private ObservableCollection<ICheckPoint> passedCPs;
        public ObservableCollection<ICheckPoint> PassedCPs
        {
            get
            {
                    passedCPs = new ObservableCollection<ICheckPoint>();
                    foreach (var tmpCollection in ICD10Segments)
                    {
                        if (tmpCollection.IncludeSegment)
                        {
                            passedCPs = passedCPs.Union(tmpCollection.PassedCPs).ToObservableCollection();
                        }
                    }
                return passedCPs.OrderByDescending(c => c.ErrorSeverity).ToObservableCollection();
            }
            set
            {
                passedCPs = value;
            }
        }

        /// <summary>
        /// Evaluate each relavent checkpoint and get the status, this is used to sequentially evaluate each checkpoint avoiding the stacked Yes/No Question issue.
        /// </summary>
        public void PopulateCPStatuses()
        {
            foreach (var tmpCollection in ICD10Segments)
            {
                if (tmpCollection.IncludeSegment)
                {
                    var tmpP = PassedCPs; //only run the passedCPs, as this evaluates all CPs to determine which are passed status.
                }
            }
        }

        private ObservableCollection<SqlCheckpointVM> missedCPs;
        public ObservableCollection<SqlCheckpointVM> MissedCPs
        {
            get
            {
                    missedCPs = new ObservableCollection<SqlCheckpointVM>();
                    foreach (var tmpCollection in ICD10Segments) //only run once per report
                    {
                        if (tmpCollection.IncludeSegment)
                        {
                            missedCPs = missedCPs.Union(tmpCollection.MissedCPs).ToObservableCollection(); //run 19 times
                            //var unitedPoints = observableCollection1.Union(observableCollection2).ToObservableCollection();
                        }
                    }
                return missedCPs.OrderByDescending(c => c.ErrorSeverity).ToObservableCollection();
            }
            set
            {
                missedCPs = value;
            }
        }

        private ObservableCollection<SqlCheckpointVM> droppedCPs;
        public ObservableCollection<SqlCheckpointVM> DroppedCPs
        {
            get
            {
                    droppedCPs = new ObservableCollection<SqlCheckpointVM>();
                    foreach (var tmpCollection in ICD10Segments)
                    {
                        if (tmpCollection.IncludeSegment)
                        {
                            droppedCPs = droppedCPs.Union(tmpCollection.DroppedCPs).ToObservableCollection();
                        }
                    }
                return droppedCPs.OrderByDescending(c => c.ErrorSeverity).ToObservableCollection();
            }
            set
            {
                droppedCPs = value;
            }
        }

        public void UpdateCPs()
        {
            OnPropertyChanged("MissedCPs");
            OnPropertyChanged("DroppedCPs");
            OnPropertyChanged("PassedCPs");
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
            foreach (SqlCheckpointVM cp in (from c in PassedCPs orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                {
                    if (cp.CustomComment == null) cp.CustomComment = "";
                    sql += $"Replace INTO RelCPPRovider (ProviderID, CheckPointID, PtID, ReviewDate, VisitDate, CheckPointStatus, Comment) VALUES ({document.ProviderID}, {cp.CheckPointID}, {patient.PtID}, '{report.ReviewDate.ToString("yyyy-MM-dd")}', '{document.VisitDate.ToString("yyyy-MM-dd")}', {(int)SqlRelCPProvider.MyCheckPointStates.Pass}, '{cp.CustomComment}');\n";
                }
            }
            foreach (SqlCheckpointVM cp in (from c in MissedCPs orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                {
                    if (cp.CustomComment == null) cp.CustomComment = "";
                    sql += $"Replace INTO RelCPPRovider (ProviderID, CheckPointID, PtID, ReviewDate, VisitDate, CheckPointStatus, Comment) VALUES ({document.ProviderID}, {cp.CheckPointID}, {patient.PtID}, '{report.ReviewDate.ToString("yyyy-MM-dd")}', '{document.VisitDate.ToString("yyyy-MM-dd")}', {(int)SqlRelCPProvider.MyCheckPointStates.Fail}, '{cp.CustomComment}');\n";
                }
            }
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
            document.Provider.SetCurrentMasterReview(document.VisitDate);
            MessageBox.Show($"{document.Provider.CurrentReviewCount}/10 reports committed.");
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



}
