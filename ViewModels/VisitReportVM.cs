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
        private PatientVM patient;
        private SqlProvider sqlProvider;
        private MasterReviewSummaryVM masterReviewSummary;

        public VisitReportVM()
        {
            report = new VisitReportM(); //1st executed command in program
            sqlProvider = new SqlProvider(); //Change provider for report
            patient = new PatientVM();
            document = new DocumentVM(sqlProvider, patient);
            passedCPs = new ObservableCollection<SqlCheckpointVM>();
            missedCPs = new ObservableCollection<SqlCheckpointVM>();
            droppedCPs = new ObservableCollection<SqlCheckpointVM>();
            GeneralCheckPointsOnly = false;
            NewEcWDocument();
        }

        public VisitReportVM(List<SqlRelCPProvider> cplist)
        {
            report = new VisitReportM(); //1st executed command in program
            sqlProvider = new SqlProvider(); //Change provider for report
            patient = new PatientVM();
            document = new DocumentVM(sqlProvider, patient);
            passedCPs = new ObservableCollection<SqlCheckpointVM>();
            missedCPs = new ObservableCollection<SqlCheckpointVM>();
            droppedCPs = new ObservableCollection<SqlCheckpointVM>();
            GeneralCheckPointsOnly = false;
            NewEcWDocument();
        }

        //not sure I need this.
        public void NewEcWDocument()
        {
            passedCPs.Clear();
            missedCPs.Clear();
            droppedCPs.Clear();
            iCD10Segments = null; //reset segments
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
        public PatientVM Patient
        {
            get
            {
                return patient;
            }
        }
        #endregion

        private ObservableCollection<SqlCheckpointVM> passedCPs;
        public ObservableCollection<SqlCheckpointVM> PassedCPs
        {
            get
            {
                if (passedCPs == null)
                {
                    passedCPs = new ObservableCollection<SqlCheckpointVM>();
                    foreach (var tmpCollection in ICD10Segments)
                    {
                        if (tmpCollection.IncludeSegment)
                        {
                            passedCPs = passedCPs.Union(tmpCollection.PassedCPs).ToObservableCollection();
                        }
                    }
                }
                return passedCPs.OrderByDescending(c => c.ErrorSeverity).ToObservableCollection();
            }
            set
            {
                passedCPs = value;
            }
        }

        private ObservableCollection<SqlCheckpointVM> missedCPs;
        public ObservableCollection<SqlCheckpointVM> MissedCPs
        {
            get
            {
                if (missedCPs == null)
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
                if (droppedCPs == null)
                {
                    droppedCPs = new ObservableCollection<SqlCheckpointVM>();
                    foreach (var tmpCollection in ICD10Segments)
                    {
                        if (tmpCollection.IncludeSegment)
                        {
                            droppedCPs = droppedCPs.Union(tmpCollection.DroppedCPs).ToObservableCollection();
                        }
                    }
                }
                return droppedCPs.OrderByDescending(c => c.ErrorSeverity).ToObservableCollection();
            }
            set
            {
                droppedCPs = value;
            }
        }

        /// <summary>
        /// used for the 1st review
        /// </summary>
        private bool generalCheckPointsOnly;
        public bool GeneralCheckPointsOnly { 
            get
            {
                return generalCheckPointsOnly;
            }
            set
            {
                generalCheckPointsOnly = value;
                document.GeneralCheckPointsOnly = value;
            }
        }

        private SqlCheckpointVM selectedItem;
        public SqlCheckpointVM SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (value == selectedItem)
                    return;
                if (value == null)
                    return;
                selectedItem = value;

                OnPropertyChanged("SelectedItem");
                if (selectedItem != null)
                Console.WriteLine($"Current checkpoint is '{selectedItem.CheckPointTitle}'");

                // selection changed - do something special
            }
        }

        public void UpdateCPs()
            {
            missedCPs.Clear();
            passedCPs.Clear();
            droppedCPs.Clear();
                foreach (var tmpCollection in ICD10Segments) //only run once per report
                {
                    if (tmpCollection.IncludeSegment)
                    {
                        missedCPs = missedCPs.Union(tmpCollection.MissedCPs).ToObservableCollection(); //run 19 times
                        passedCPs = passedCPs.Union(tmpCollection.PassedCPs).ToObservableCollection(); //run 19 times
                        droppedCPs = droppedCPs.Union(tmpCollection.DroppedCPs).ToObservableCollection(); //run 19 times
                }
            }
            missedCPs = missedCPs.OrderByDescending(c => c.ErrorSeverity).ToObservableCollection();
            passedCPs = passedCPs.OrderByDescending(c => c.ErrorSeverity).ToObservableCollection();
            droppedCPs = droppedCPs.OrderByDescending(c => c.ErrorSeverity).ToObservableCollection();
            Console.WriteLine("Setting missed,dropped,passed cp's to null on visit report.");
            OnPropertyChanged("MissedCPs");
            OnPropertyChanged("DroppedCPs");
            OnPropertyChanged("PassedCPs");
            OnPropertyChanged("SelectedItem");
        }
        public void SetCPs()
        {
        }

        /// <summary>
        /// Holds the current review's Yes/No SqlRegex's
        /// </summary>
        private Dictionary<int, bool> YesNoSqlRegExIndex = new Dictionary<int, bool>();
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
            document.ProviderSql.SetCurrentMasterReview(document.VisitDate);
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

        private ICommand mCheckPointEditor;
        public ICommand CheckPointEditorCommand
        {
            get
            {
                if (mCheckPointEditor == null)
                    mCheckPointEditor = new CheckPointEditor();
                return mCheckPointEditor;
            }
            set
            {
                mCheckPointEditor = value;
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
        #endregion

        public void Execute(object parameter)
        {
            VisitReportVM rvm = parameter as VisitReportVM;
            rvm.CommitReport();
        }
    }

    class CheckPointEditor : ICommand
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
        #endregion

        public void Execute(object parameter)
        {
            VisitReportVM rvm = parameter as VisitReportVM;
            CheckPointEditorV w = new CheckPointEditorV();
            w.Show();
        }
    }
}
