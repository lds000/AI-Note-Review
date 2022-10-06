﻿using Dapper;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace AI_Note_Review
{
    /// <summary>
    /// Class that holds all the view models of the program.
    /// </summary>
    public class MasterReviewSummaryVM : INotifyPropertyChanged
    {

        #region inotify
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void OnPropertyChangedSave([CallerMemberName] string name = null)
        {
            if (PropertyChanged != null)
            {
                SaveToDB();
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
        
        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public MasterReviewSummaryVM()
        {
            masterReviewSummary = new SqlMasterReviewSummaryM();
            providerBiMonthlyReview = new SqlRelProviderMasterReviewSummaryM();
            AddLog("MasterReviewSummaryVM() executed.");
            RegisterEvents(); //not used, here in case I need it
        }

        #region EventManagement (empty)
        private void RegisterEvents()
        {
            Messenger.Default.Register<NotificationMessage>(this, NotifyMe);
        }

        private void NotifyMe(NotificationMessage obj)
        {
        }
        #endregion


        //not sure I need this.
        //public SqlMasterReviewSummaryM MasterReviewSummary { get; set; }

        //todo: not currently used, but consider :)
        private bool monitorActiveNote;
        public bool MonitorActiveNote
        {
            get
            {
                return monitorActiveNote;
            }
            set
            {
                monitorActiveNote = true;
            }
        }

        //I don't think I need this.
        //public SqlICD10SegmentVM SqlICD10SegmentVM { get; set; }

        private BiMonthlyReviewVM biMonthlyReviewVM;
        /// <summary>
        /// Used for the bimonthly review view
        /// </summary>
        public BiMonthlyReviewVM BiMonthlyReviewVM
        {
            get {
                if (biMonthlyReviewVM == null) 
                {
                    BiMonthlyReviewVM = new BiMonthlyReviewVM(this);
                    AddLog("Creating BiMonthlyReviewVM.");
                }
                return biMonthlyReviewVM; 
            }
            set 
            { 
                biMonthlyReviewVM = value; 
            }
        }

        private DocumentVM document;
        /// <summary>
        /// The VM of the DocumentM
        /// </summary>
        public DocumentVM Document
        {
            get
            {
                if (document == null) document = new DocumentVM(this);
                    return document;
            }
        }

        private NoteDataVM parentNoteDataVM;
        public NoteDataVM ParentNoteData
        {
            set
            {
                parentNoteDataVM = value;
                if (parentNoteDataVM != null)
                {
                    string strHTML = "";
                    strHTML = Encryption.Decrypt(parentNoteDataVM.NoteString);
                    //Convert string html to html document object
                    WebBrowser browser = new WebBrowser();
                    browser.ScriptErrorsSuppressed = true;
                    browser.DocumentText = strHTML;
                    browser.Document.OpenNew(true);
                    browser.Document.Write(strHTML);
                    browser.Refresh();

                    Document.NoteHTML = browser.Document;
                    VisitReport.PopulateCPStatuses();
                    OnPropertyChanged("StrBimonthlyReviewComment");
                }
            }
            get
            {
                return parentNoteDataVM;
            }
        }

        public void DeleteParentNoteData()
        {
            if (parentNoteDataVM == null) return;
            string sql = $"Delete from data where NoteID = {parentNoteDataVM.NoteID}";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteNotesLocation))
            {
                var d = cnn.Query(sql);
            }
            parentNoteDataVM = null;
            OnPropertyChanged("ParentNoteData");
        }

        public void GetNextParentNote()
        {
            string sql = $"Select * from Data Limit 1";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteNotesLocation))
            {
                var d = cnn.Query<NoteDataVM>(sql).FirstOrDefault();
                ParentNoteData = d;
            }
        }


        private PatientVM patient;
        /// <summary>
        /// PatientVM attatched to masterreviewsummary
        /// </summary>
        public PatientVM Patient
        {
            get
            {
                if (patient == null) patient = new PatientVM(this);
                return patient;
            }
        }

        private VisitReportVM visitReport;
        /// <summary>
        /// The VisitReportVM associated with the MRS.
        /// </summary>
        public VisitReportVM VisitReport
        {
            get 
            {
                if (visitReport == null) 
                { 
                    visitReport = new VisitReportVM(this); 
                }
                return visitReport; 
            }
            private set { visitReport = value; } 
        }

        /// <summary>
        /// review for provider submitted every two months
        /// </summary>
        private SqlRelProviderMasterReviewSummaryM providerBiMonthlyReview { get; set; }
        public int RelProviderBiMonthlyID { get { return providerBiMonthlyReview.RelProviderMasterReviewSummaryID; } set { providerBiMonthlyReview.RelProviderMasterReviewSummaryID = value; OnPropertyChanged(); } }
        public int RelBiMonthlyReviewID { get { return providerBiMonthlyReview.RelMasterReviewSummaryID; } set { providerBiMonthlyReview.RelMasterReviewSummaryID = value; OnPropertyChanged(); } }
        public int RelProviderID { get { return providerBiMonthlyReview.RelProviderID; } set { providerBiMonthlyReview.RelProviderID = value; OnPropertyChanged(); } }
        public string ProviderBiMonthlyReviewMComment
        { 
            get 
            { 
                return providerBiMonthlyReview.RelComment; 
            } 
            set 
            {
                providerBiMonthlyReview.RelComment = value;
                OnPropertyChanged(); 
            } 
        }

        private string strBimonthlyReviewComment;
        public string StrBimonthlyReviewComment
        {
            get
            {
                if (Document.Provider == null)
                    return null;
                string sql = "";
                //todo: change from 5 to document date MRS.
                sql += $"Select RelComment from RelProviderMasterReviewSummary where RelProviderID={Document.Provider.ProviderID} and RelMasterReviewSummaryID=5;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    strBimonthlyReviewComment = cnn.ExecuteScalar<string>(sql);
                }
                return strBimonthlyReviewComment;
            }
            set
            {
                if (Document.Provider == null)
                    return;
                string sql = "";
                sql = $"Delete from RelProviderMasterReviewSummary Where RelProviderID={Document.Provider.ProviderID} and RelMasterReviewSummaryID=5;";
                sql += $"Insert INTO RelProviderMasterReviewSummary (RelComment,RelProviderID,RelMasterReviewSummaryID) VALUES ('{value.Replace("'", "''")}',{Document.Provider.ProviderID},5);";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    cnn.Execute(sql);
                }
                strBimonthlyReviewComment = value;
            }
        }

        /// <summary>
        /// Record containing the review start, end date and topic information
        /// </summary>
        private SqlMasterReviewSummaryM masterReviewSummary { get; set; }
        public int MasterReviewSummaryID { get { return masterReviewSummary.MasterReviewSummaryID; } set { masterReviewSummary.MasterReviewSummaryID = value; OnPropertyChanged(); } }
        public DateTime StartDate { get { return masterReviewSummary.StartDate; } set { masterReviewSummary.StartDate = value; OnPropertyChangedSave(); } }
        public DateTime EndDate { get { return masterReviewSummary.EndDate; } set { masterReviewSummary.EndDate = value; OnPropertyChangedSave(); } }
        public string MasterReviewSummaryTitle { get { return masterReviewSummary.MasterReviewSummaryTitle; } set { masterReviewSummary.MasterReviewSummaryTitle = value; OnPropertyChangedSave(); } }
        public string MasterReviewSummarySubject { get { return masterReviewSummary.MasterReviewSummarySubject; } set { masterReviewSummary.MasterReviewSummarySubject = value; OnPropertyChangedSave(); } }
        public string MasterReviewSummaryComment {
            get { return masterReviewSummary.MasterReviewSummaryComment; } 
            set { masterReviewSummary.MasterReviewSummaryComment = value; OnPropertyChangedSave(); } 
        }
        public string MasterReviewSummaryImpression {
            get {
                return masterReviewSummary.MasterReviewSummaryImpression; } 
            set 
            {
                masterReviewSummary.MasterReviewSummaryImpression = value; 
                OnPropertyChangedSave(); 
            } 
        }

        public void SaveToDB()
        {
            masterReviewSummary.SaveToDB();
        }

        public string MasterReviewSummaryToString
        {
            get
            {
                return $"{StartDate.ToString("yyyy/MM/dd")}-{ EndDate.ToString("yyyy/MM/dd")} {MasterReviewSummaryTitle}";
            }
        }

        public static MasterReviewSummaryVM CurrentMasterReview
        {
            get
            {
                string sql = $"Select * from MasterReviewSummary";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    var tmpCol = cnn.Query<MasterReviewSummaryVM>(sql).ToList();
                    foreach (MasterReviewSummaryVM mrs in tmpCol)
                    {

                        if (DateTime.Now >= mrs.StartDate && DateTime.Now <= mrs.EndDate)
                        {
                            return mrs;
                        }
                    }
                }
                return null;
            }
        }


        //not sure this is used.
        private List<SqlICD10SegmentVM> iCD10List;
        public List<SqlICD10SegmentVM> ICD10List
        {
            get
            {
                if (iCD10List == null)
                {
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        string sql = $"Select * from ICD10Segments icd inner join RelICD10SegmentMasterReviewSummary rel on icd.ICD10SegmentID == rel.ICD10SegmentID where rel.MasterReviewSummaryID == {MasterReviewSummaryID} order by icd10Chapter, icd10CategoryStart, icd10CategoryEnd DESC;";
                        var l = cnn.Query<SqlICD10SegmentM>(sql).ToList();
                        List<SqlICD10SegmentVM> lvm = new List<SqlICD10SegmentVM>();
                        foreach (SqlICD10SegmentM s in l)
                        {
                            SqlICD10SegmentVM scvm = new SqlICD10SegmentVM(s, this);
                            lvm.Add(scvm);
                        }
                        iCD10List = lvm;
                    }
                }
                return iCD10List;
            }
            set
            {
                ICD10List = value;
                OnPropertyChanged();
            }
        }


        public bool ContainsDocument(DocumentVM d)
        {
            foreach (SqlICD10SegmentVM icd10 in d.ICD10Segments)
            {
                foreach (SqlICD10SegmentVM mrsICD10 in ICD10List)
                {
                    if (mrsICD10.ICD10SegmentID == icd10.ICD10SegmentID) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// A list of all masterreviewsummaries
        /// </summary>
        public ObservableCollection<MasterReviewSummaryVM> MasterReviewSummaries
        {
            get
            {
                    string sql = $"Select * from MasterReviewSummary";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        var tmpCol = cnn.Query<MasterReviewSummaryVM>(sql).ToList();
                        return tmpCol.ToObservableCollection();
                    }
            }
        }



        private List<SqlICD10SegmentVM> iCD10Segments;
        /// <summary>
        /// A list of ICD10 Segments that belong to the MasterReview, used for the CheckPointEditor
        /// </summary>
        public List<SqlICD10SegmentVM> ICD10Segments
        {
            get
            {
                if (iCD10Segments != null) return iCD10Segments;

                List<SqlICD10SegmentM> l = new List<SqlICD10SegmentM>();

                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    //return all for masterreview All ID=3
                    if (MasterReviewSummaryID == 3)
                    {
                        //string sql3 = $"Select * from ICD10Segments icd inner join RelICD10SegmentMasterReviewSummary rel on icd.ICD10SegmentID == rel.ICD10SegmentID order by icd10Chapter, icd10CategoryStart, icd10CategoryEnd DESC;";
                        string sql3 = $"Select * from ICD10Segments order by icd10Chapter, icd10CategoryStart, icd10CategoryEnd DESC;";
                        l = cnn.Query<SqlICD10SegmentM>(sql3).ToList();
                    }
                    //if MasterReviewSummaryID=1 then return general review withicd10Chapter  X, this is for a general review
                    else
                    if (MasterReviewSummaryID == 1)
                    {
                        string sql1 = "Select * from ICD10Segments where icd10Chapter == 'X' order by icd10Chapter, icd10CategoryStart, icd10CategoryEnd DESC;";
                        l = cnn.Query<SqlICD10SegmentM>(sql1).ToList();
                    }
                    else
                    {
                        string sql = $"Select * from ICD10Segments icd inner join RelICD10SegmentMasterReviewSummary rel on icd.ICD10SegmentID == rel.ICD10SegmentID where rel.MasterReviewSummaryID == {MasterReviewSummaryID} order by icd10Chapter, icd10CategoryStart, icd10CategoryEnd DESC;";
                        l = cnn.Query<SqlICD10SegmentM>(sql).ToList();
                    }
                    //For all others
                }
                List<SqlICD10SegmentVM> lvm = new List<SqlICD10SegmentVM>();
                foreach (SqlICD10SegmentM seg in l)
                {
                    SqlICD10SegmentVM scvm = new SqlICD10SegmentVM(seg, this);
                    lvm.Add(scvm);
                }

                foreach (var seg in lvm)
                {
                    seg.PropertyChanged += SqlICD10SegmentVM_PropertyChanged;
                }
                iCD10Segments = lvm;

                return iCD10Segments;
            }
            set
            {
                iCD10Segments = value;
                OnPropertyChanged();
                OnPropertyChanged("SelectedICD10Segment");
            }
        }

        private void SqlICD10SegmentVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
                if (e.PropertyName == "ReloadICD10Segments")
                {

                }
        }

        private string indexHtml;
        public string IndexHtml
        {
            get
            {
                if (indexHtml == null)
                {
                    string strSummary = $"<font size='+4'><b>Index for review: {MasterReviewSummaryTitle}</b></font><br>";
                    strSummary += $"<font size='+1'>{MasterReviewSummarySubject}</font><br>";
                    strSummary += $"<font size='+1'>Dates: {StartDate.ToString("MM/dd/yyyy")}-{EndDate.ToString("MM/dd/yyyy")}</font><br>";
                    strSummary += $"<font size='+0'>{MasterReviewSummaryComment}</font><br><br>";
                    strSummary += $"<font size='+1'>ICD-10 Breakdown:</font><br>";
                    iCD10Segments = null;
                    foreach (var seg in ICD10Segments)
                    {
                        if (seg.LeftOffset == 10)
                        {
                            strSummary += $"<span style='padding-left: 15px;'>";
                        }
                        else
                        {
                            strSummary += $"<span style='padding-left: 5px;'>";
                        }
                        strSummary += $"{seg.SegmentTitle} {seg.icd10Chapter}{seg.icd10CategoryStart}-{seg.icd10CategoryEnd}";
                        if (seg.AlternativeICD10s.Count > 0)
                        {
                            strSummary += " also includes (";
                            foreach (var tmpAlt in seg.AlternativeICD10s)
                            {
                                strSummary += $"{tmpAlt.AlternativeICD10} {tmpAlt.AlternativeICD10Title}, ";
                            }
                            strSummary = strSummary.Trim().TrimEnd(',');
                            strSummary += ")";
                        }
                        if (seg.LeftOffset == 10) strSummary += $"</span>";
                        strSummary += "<br>";
                    }

                    foreach (var seg in ICD10Segments)
                    {
                        strSummary += seg.IndexHtml;
                    }
                    indexHtml = strSummary;
                }
                return indexHtml;
            }
        }

        private string keyPointHtml;
        public string KeyPointHtml
        {
            get
            {
                if (keyPointHtml == null)
                {
                    string strSummary = $"<font size='+4'><b>Index for review: {MasterReviewSummaryTitle}</b></font><br>";
                    strSummary += $"<font size='+1'>{MasterReviewSummarySubject}</font><br>";
                    strSummary += $"<font size='+1'>Dates: {StartDate.ToString("MM/dd/yyyy")}-{EndDate.ToString("MM/dd/yyyy")}</font><br>";
                    strSummary += $"<font size='+0'>{MasterReviewSummaryComment}</font><br><br>";

                    foreach (var seg in ICD10Segments)
                    {
                        strSummary += seg.KeyPointHtml;
                    }
                    keyPointHtml = strSummary;
                }
                return keyPointHtml;
            }
        }

        private string mainLog;
        public string MainLog 
        {
            get
            {
                if (mainLog == null) mainLog = "";
                return mainLog; 
            }
            set
            {
                mainLog += value;
                OnPropertyChanged("MainLog");
            }
        }
        public void AddLog(string str)
        {
            mainLog += $"-{str}\n";
            OnPropertyChanged("MainLog");
        }

        public void ClearLog()
        {
            mainLog = "";
            OnPropertyChanged("MainLog");
        }

        private List<SqlMissingICD10CodesM> topMissingDxs;
        public List<SqlMissingICD10CodesM> TopMissingDxs
        {
            get
            {
                if (topMissingDxs == null)
                {
                    //Select all records in missingICD10Codes that do not exist in RelAlternativeICD10
                    string sql = $"SELECT StrCode, count(StrCode) as Count FROM MissingICD10Codes t1 LEFT JOIN RelAlternativeICD10 t2 ON t2.AlternativeICD10 = t1.StrCode WHERE AlternativeICD10 IS NULL group by StrCode ORDER BY count(StrCode) DESC;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        topMissingDxs = cnn.Query<SqlMissingICD10CodesM>(sql).ToList();
                    }
                    foreach (var tmpCode in topMissingDxs)
                    {
                        tmpCode.ParentMasterReviewSummary = this;
                    }
                }
                return topMissingDxs;
            }
        }

        public bool? showAllPeeps;
        public bool? ShowAllPeeps
        {
            get 
            {
                if (showAllPeeps == null)
                    showAllPeeps = true;
                return showAllPeeps;
            }
            set
            {
            showAllPeeps = value;
                OnPropertyChanged();
                OnPropertyChanged("AllPeeps");
            }

        }


        /// <summary>
        /// Get a list of providers for the west side pod
        /// </summary>
        public List<ProviderVM> AllPeeps
        {
            get
            {
                string sql = "";
                if (ShowAllPeeps == true)
                sql += $"Select * from Providers where FullName != '' order by FullName;"; //this part is to get the ID of the newly created phrase
                else
                sql += $"Select * from Providers where FullName != '' and IsWestSidePod == 1 order by FullName;"; //this part is to get the ID of the newly created phrase

                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    List<ProviderM> tmpList = cnn.Query<ProviderM>(sql).ToList();
                    List<ProviderVM> tmpListVM = new List<ProviderVM>();
                    foreach (var tmp in tmpList)
                    {
                        tmpListVM.Add(new ProviderVM(tmp));
                    }
                    return tmpListVM;
                }
            }
        }

        public void ResetMissingDx()
        {
            topMissingDxs = null;
            OnPropertyChanged("TopMissingDxs");
        }

        private ICommand mSaveNoteCommand;
        public ICommand SaveNoteCommand
        {
            #region Command Def
            get
            {
                if (mSaveNoteCommand == null)
                    mSaveNoteCommand = new SaveNote();
                return mSaveNoteCommand;
            }
            set
            {
                mSaveNoteCommand = value;
            }
            #endregion
        }

        private ICommand mLoadNoteCommand;
        public ICommand LoadNoteCommand
        {
            #region Command Def
            get
            {
                if (mLoadNoteCommand == null)
                    mLoadNoteCommand = new LoadNote();
                return mLoadNoteCommand;
            }
            set
            {
                mLoadNoteCommand = value;
            }
            #endregion
        }


        private ICommand mShowMasterReview;
        public ICommand ShowMasterReviewCommand
        {
            #region Command Def
            get
            {
                if (mShowMasterReview == null)
                    mShowMasterReview = new ShowMasterReview();
                return mShowMasterReview;
            }
            set
            {
                mShowMasterReview = value;
            }
            #endregion
        }

        private ICommand mClearLog;
        public ICommand ClearLogCommand
        {
            #region Command Def
            get
            {
                if (mClearLog == null)
                    mClearLog = new ClearLog();
                return mClearLog;
            }
            set
            {
                mClearLog = value;
            }
            #endregion
        }

        //CreateMasterIndexCommand
        private ICommand mCreateMasterIndex;
        public ICommand CreateMasterIndexCommand
        {
            #region Command Def
            get
            {
                if (mCreateMasterIndex == null)
                    mCreateMasterIndex = new CreateMasterIndex();
                return mCreateMasterIndex;
            }
            set
            {
                mCreateMasterIndex = value;
            }
            #endregion
        }

        //CreateMasterIndexCommand
        private ICommand mCreateKeyPointIndex;
        public ICommand CreateKeyPointIndexCommand
        {
            #region Command Def
            get
            {
                if (mCreateKeyPointIndex == null)
                    mCreateKeyPointIndex = new CreateKeyPointIndex();
                return mCreateKeyPointIndex;
            }
            set
            {
                mCreateKeyPointIndex = value;
            }
            #endregion
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

        private ICommand mProviderEditor;
        public ICommand ProviderEditorCommand
        {
            get
            {
                if (mProviderEditor == null)
                    mProviderEditor = new ProviderEditor();
                return mProviderEditor;
            }
            set
            {
                mProviderEditor = value;
            }
        }

    }

    class LoadNote : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion

        public void Execute(object parameter)
        {
            MasterReviewSummaryVM MRS = parameter as MasterReviewSummaryVM;
            WinSelectNote wsn = new WinSelectNote();
            wsn.DataContext = MRS;
            wsn.ShowDialog();
        }
    }


    //
    class SaveNote : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion

        public void Execute(object parameter)
        {
            MasterReviewSummaryVM MRS = parameter as MasterReviewSummaryVM;
            MRS.Document.SaveNote();
        }
    }


    class ProviderEditor : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion

        public void Execute(object parameter)
        {
            MasterReviewSummaryVM mrs = parameter as MasterReviewSummaryVM;
            ProviderEditorV  w = new ProviderEditorV();
            w.DataContext = mrs;
            w.Show();
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
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion

        public void Execute(object parameter)
        {
            //MasterReviewSummaryVM mrs = parameter as MasterReviewSummaryVM;
            CheckPointEditorV w = new CheckPointEditorV();
            //w.DataContext = mrs;
            w.DataContext = new CheckPointEditorVM();
            w.Show();
        }
    }
    class ShowMasterReview : ICommand
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
            MasterReviewSummaryVM mrs = parameter as MasterReviewSummaryVM;

            MasterReviewsV wp = new MasterReviewsV();
            wp.DataContext = mrs;
            wp.ShowDialog();
        }
    }

    class ClearLog : ICommand
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
            MasterReviewSummaryVM mrs = parameter as MasterReviewSummaryVM;
            mrs.ClearLog();
        }
    }

    //CreateMasterIndexCommand
    class CreateMasterIndex : ICommand
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
            MasterReviewSummaryVM mrs = parameter as MasterReviewSummaryVM;
            ClipboardHelper.CopyToClipboard(mrs.IndexHtml, "");
            WinPreviewHTML wp = new WinPreviewHTML();
            wp.MyWB.NavigateToString(HtmlLittlerHelper.FixHtml(mrs.IndexHtml));
            wp.ShowDialog();
        }
    }

    //CreateMasterIndexCommand
    class CreateKeyPointIndex : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion

        public void Execute(object parameter)
        {
            MasterReviewSummaryVM mrs = parameter as MasterReviewSummaryVM;
            ClipboardHelper.CopyToClipboard(mrs.KeyPointHtml, "");
            WinPreviewHTML wp = new WinPreviewHTML();
            wp.MyWB.NavigateToString(HtmlLittlerHelper.FixHtml(mrs.KeyPointHtml));
            wp.ShowDialog();
        }
    }



}
