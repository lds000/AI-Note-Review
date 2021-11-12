using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
namespace AI_Note_Review
{
    public class SqlICD10SegmentVM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public DocumentVM ParentDocument { get; set; }
        public VisitReportVM ParentReport { get; set; }

        private SqlICD10SegmentM sqlICD10Segment;

        public SqlICD10SegmentM SqlICD10Segment
        {
            get { return sqlICD10Segment; }
        }

        public SqlICD10SegmentVM()
        {
            sqlICD10Segment = new SqlICD10SegmentM();
        }

        public SqlICD10SegmentVM(string strSegmentTitle)
        {
            sqlICD10Segment = new SqlICD10SegmentM(strSegmentTitle);
        }

        public SqlICD10SegmentVM(SqlICD10SegmentM sc)
        {
            sqlICD10Segment = sc;
        }

        public SqlCheckpointVM SelectedCheckPoint { get; set; }

        public int ICD10SegmentID { get { return sqlICD10Segment.ICD10SegmentID; } set { sqlICD10Segment.ICD10SegmentID = value; } }
        public string SegmentTitle { get { return sqlICD10Segment.SegmentTitle; } set { sqlICD10Segment.SegmentTitle = value; } }
        public string SegmentComment { get { return sqlICD10Segment.SegmentComment; } set { sqlICD10Segment.SegmentComment = value; } }
        public string icd10Chapter { get { return sqlICD10Segment.icd10Chapter; } set { sqlICD10Segment.icd10Chapter = value; } }
        public double icd10CategoryStart { get { return sqlICD10Segment.icd10CategoryStart; } set { sqlICD10Segment.icd10CategoryStart = value; } }
        public double icd10CategoryEnd { get { return sqlICD10Segment.icd10CategoryEnd; } set { sqlICD10Segment.icd10CategoryEnd = value; } }
        public int LeftOffset { get { return sqlICD10Segment.LeftOffset; } set { sqlICD10Segment.LeftOffset = value; } }

        public void SaveToDB()
        {
            sqlICD10Segment.SaveToDB();
        }

        private bool includeSegment;
        public bool IncludeSegment
        {
            get
            {
                return includeSegment;
            }
            set
            {
                includeSegment = value;
                OnPropertyChanged("IncludeSegment");
                OnPropertyChanged("CBIncludeSegment");
                OnPropertyChanged("MissedCPs");
                OnPropertyChanged("PassedCPs");
                OnPropertyChanged("DroppedCPs");
            }
        }
        //used to track manual changes to textbox
        public bool CBIncludeSegment
        {
            get
            {
                return includeSegment;
            }
            set
            {
                includeSegment = value;
                OnPropertyChanged("IncludeSegment");
                OnPropertyChanged("CBIncludeSegment");
                ParentReport.ClearCPs(); //now recalculate all checkpoints.
            }
        }
        public void UpdateAll()
        {
            CalculateLeftOffsets();
            noteICD10Segments = null;
            OnPropertyChanged("NoteICD10Segments");
        }

        private ObservableCollection<SqlCheckpointVM> checkpoints;
        public ObservableCollection<SqlCheckpointVM> Checkpoints
        {
            get
            {
                if (checkpoints == null)
                {

                    string sql = $"Select cp.CheckPointID,cp.CheckPointTitle,cp.ErrorSeverity,cp.CheckPointType,cp.TargetSection,cp.TargetICD10Segment,cp.Comment,cp.Action,cp.Link,cp.Expiration from CheckPoints cp inner join CheckPointTypes ns on cp.CheckPointType == ns.CheckPointTypeID where TargetICD10Segment == {sqlICD10Segment.ICD10SegmentID} order by ns.ItemOrder;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        var tmpList = cnn.Query<SqlCheckpointM>(sql).ToList();
                        ObservableCollection<SqlCheckpointVM> tmpCol = new ObservableCollection<SqlCheckpointVM>();
                        foreach (var item in tmpList)
                        {
                            SqlCheckpointVM cpvm = new SqlCheckpointVM(item);
                            cpvm.ParentSegment = this;
                            cpvm.ParentDocument = this.ParentDocument;
                            tmpCol.Add(cpvm);
                        }
                        checkpoints = tmpCol;
                    }
                }
                return checkpoints;
            }
        }

        public void CheckSegment()
        {
            passedCPs = new List<SqlCheckpointVM>();
            missedCPs = new List<SqlCheckpointVM>();
            droppedCPs = new List<SqlCheckpointVM>();
            OnPropertyChanged("PassedCPs");
            OnPropertyChanged("MissedCPs");
            OnPropertyChanged("DroppedCPs");
        }

        private List<SqlCheckpointVM> passedCPs;
        public List<SqlCheckpointVM> PassedCPs
        {
            get
            {
                if (passedCPs == null)
                {
                    passedCPs = new List<SqlCheckpointVM>(from c in Checkpoints where c.CPStatus == SqlTagRegExM.EnumResult.Pass select c);
                }
                return passedCPs;
            }
            set
            {

            }
        }
        private List<SqlCheckpointVM> missedCPs;
        public List<SqlCheckpointVM> MissedCPs
        {
            get
            {
                if (missedCPs == null)
                {
                    missedCPs = new List<SqlCheckpointVM>(from c in Checkpoints where c.CPStatus == SqlTagRegExM.EnumResult.Miss select c);
                }
                return missedCPs;
            }
        }
        private List<SqlCheckpointVM> droppedCPs;
        public List<SqlCheckpointVM> DroppedCPs
        {
            get
            {
                if (droppedCPs == null)
                {
                    droppedCPs = new List<SqlCheckpointVM>(from c in Checkpoints where c.CPStatus == SqlTagRegExM.EnumResult.Hide select c);
                }
                return droppedCPs;
            }
        }

        private ObservableCollection<MasterReviewSummaryVM> masterReviewSummaries;
        public ObservableCollection<MasterReviewSummaryVM> MasterReviewSummaries
        {
            get
            {
                if (masterReviewSummaries == null)
                {
                    string sql = $"Select * from MasterReviewSummary";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        var tmpCol = cnn.Query<MasterReviewSummaryVM>(sql).ToList();
                        masterReviewSummaries = tmpCol.ToObservableCollection();
                    }
                }
                return masterReviewSummaries;
            }
        }

        private MasterReviewSummaryVM masterReviewSummary;
        public MasterReviewSummaryVM SelectedMasterReview
        {
            get {
                if (masterReviewSummary == null)
                {
                    foreach (MasterReviewSummaryVM mrs in MasterReviewSummaries)
                    {
                        
                        if (DateTime.Now >= mrs.StartDate && DateTime.Now <= mrs.EndDate)
                        {
                            masterReviewSummary = mrs;
                        }
                    }
                }
                return masterReviewSummary; 
            }
            set
            {
                masterReviewSummary = value;
                OnPropertyChanged("MasterReviewSummaryICD10Segments");
            }
        }

        //recheck CPs;
        public void UpdateCPs()
        {
            if (ParentReport != null)
            {
                passedCPs = null;
                missedCPs = null;
                droppedCPs = null;
                Console.WriteLine($"Setting passed, missed, and droppedCPs to null for segment {SegmentTitle}.");
                ParentReport.ClearCPs();
            }
        }

        public void UpdateCheckPoints()
        {
            checkpoints = null;
            OnPropertyChanged("Checkpoints");
        }

        public int CheckPointCount
        {
            get
            {
                string sql = $"Select Count(*) from CheckPoints where TargetICD10Segment = {sqlICD10Segment.ICD10SegmentID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.ExecuteScalar<int>(sql);
                }
            }
        }

        public Thickness Icd10Margin
        {

            get
            {
                return new Thickness(sqlICD10Segment.LeftOffset, 0, 0, 0);
            }
        }
        public void UpdateMargin()
        {
            OnPropertyChanged("Icd10Margin");

        }

        public void DeleteSegment()
        {
            sqlICD10Segment.DeleteSegment();
            UpdateAll();
        }

        private ObservableCollection<SqlICD10SegmentVM> masterReviewSummaryICD10Segments;
        public ObservableCollection<SqlICD10SegmentVM> MasterReviewSummaryICD10Segments
        {
            get
            {
                if (masterReviewSummaryICD10Segments == null)
                {
                    masterReviewSummaryICD10Segments = NoteICD10Segments;
                }
                if (SelectedMasterReview != null)
                {
                    if (SelectedMasterReview.MasterReviewSummaryID == 3)
                    {
                        masterReviewSummaryICD10Segments = NoteICD10Segments; //All
                        return masterReviewSummaryICD10Segments;
                    }
                    if (SelectedMasterReview.MasterReviewSummaryID == 1) //general review with X
                    {
                        using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                        {
                            string sql = "Select * from ICD10Segments where icd10Chapter == 'X' order by icd10Chapter, icd10CategoryStart;";
                            var l = cnn.Query<SqlICD10SegmentM>(sql).ToList();
                            List<SqlICD10SegmentVM> lvm = new List<SqlICD10SegmentVM>();
                            foreach (SqlICD10SegmentM s in l)
                            {
                                SqlICD10SegmentVM scvm = new SqlICD10SegmentVM(s);
                                lvm.Add(scvm);
                            }
                            masterReviewSummaryICD10Segments = lvm.ToObservableCollection();
                            return masterReviewSummaryICD10Segments;
                        }
                    }
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        string sql = $"Select * from ICD10Segments icd inner join RelICD10SegmentMasterReviewSummary rel on icd.ICD10SegmentID == rel.ICD10SegmentID where rel.MasterReviewSummaryID == {SelectedMasterReview.MasterReviewSummaryID} order by icd10Chapter, icd10CategoryStart;";
                        var l = cnn.Query<SqlICD10SegmentM>(sql).ToList();
                        List<SqlICD10SegmentVM> lvm = new List<SqlICD10SegmentVM>();
                        foreach (SqlICD10SegmentM s in l)
                        {
                            SqlICD10SegmentVM scvm = new SqlICD10SegmentVM(s);
                            lvm.Add(scvm);
                        }
                        masterReviewSummaryICD10Segments = lvm.ToObservableCollection();
                        return masterReviewSummaryICD10Segments;
                    }
                    

                }
                return masterReviewSummaryICD10Segments;
            }
            set
            {
                masterReviewSummaryICD10Segments = value;
            }
        }

        /// <summary>
        /// A list of all SqlICD10Segments
        /// </summary>Celica9
        private static ObservableCollection<SqlICD10SegmentVM> noteICD10Segments;
        public static ObservableCollection<SqlICD10SegmentVM> NoteICD10Segments
        {
            get
            {
                if (noteICD10Segments == null)
                { 
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        string sql = "Select * from ICD10Segments order by icd10Chapter, icd10CategoryStart;";
                        var l = cnn.Query<SqlICD10SegmentM>(sql).ToList();
                        List<SqlICD10SegmentVM> lvm = new List<SqlICD10SegmentVM>();
                        foreach (SqlICD10SegmentM s in l)
                        {
                            SqlICD10SegmentVM scvm = new SqlICD10SegmentVM(s);
                            lvm.Add(scvm);
                        }
                        return lvm.ToObservableCollection();
                    }
                }
                return noteICD10Segments;
            }
            set
            {
                NoteICD10Segments = value;
            }
        }

        public void AddCheckPoint(SqlCheckpointVM cp)
        {
            cp.ParentSegment = this;
            checkpoints.Add(cp);
            OnPropertyChanged("Checkpoints");
            SelectedCheckPoint = cp;
        }


        /// <summary>
        /// Calculate the indent amount for each ICD10 segment and save it to the database.
        /// </summary>
        public static void CalculateLeftOffsets()
        {
            char charChapter = 'A';
            double CodeStart = 0;
            double CodeEnd = 0;
            string strSql = "";
            int iOffset = 0;
            foreach (SqlICD10SegmentVM ns in NoteICD10Segments)
            {
                iOffset = 0;
                if (ns.SqlICD10Segment.icd10Chapter == null) continue;
                if (charChapter == char.Parse(ns.SqlICD10Segment.icd10Chapter))
                {
                    if ((ns.SqlICD10Segment.icd10CategoryStart >= CodeStart) && (ns.SqlICD10Segment.icd10CategoryEnd <= CodeEnd))
                    {
                        iOffset = 10;
                    }
                    else
                    {
                        CodeStart = ns.SqlICD10Segment.icd10CategoryStart;
                        CodeEnd = ns.SqlICD10Segment.icd10CategoryEnd;
                        charChapter = char.Parse(ns.SqlICD10Segment.icd10Chapter);
                    }
                }
                else
                {
                    charChapter = char.Parse(ns.SqlICD10Segment.icd10Chapter);
                    CodeStart = 0;
                    CodeEnd = 0;
                }
                strSql += $"UPDATE ICD10Segments SET LeftOffset = {iOffset} WHERE ICD10SegmentID = {ns.SqlICD10Segment.ICD10SegmentID};\n";
                ns.LeftOffset = iOffset;
                ns.UpdateMargin();

            }
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(strSql);
            }
        }

        private ICommand mAddCP;
        public ICommand AddCPCommand
        {
            get
            {
                if (mAddCP == null)
                    mAddCP = new CPAdder();
                return mAddCP;
            }
            set
            {
                mAddCP = value;
            }
        }

        private ICommand mEditSegment;
        public ICommand EditSegmentCommand
        {
            get
            {
                if (mEditSegment == null)
                    mEditSegment = new SegmentEditor();
                return mEditSegment;
            }
            set
            {
                mEditSegment = value;
            }
        }

        private ICommand mAddSegment;
        public ICommand AddSegmentCommand
        {
            get
            {
                if (mAddSegment == null)
                    mAddSegment = new SegmentAdder();
                return mAddSegment;
            }
            set
            {
                mAddSegment = value;
            }
        }

        private ICommand mCreateIndex;
        public ICommand CreateIndexCommand
        {
            get
            {
                if (mCreateIndex == null)
                    mCreateIndex = new CreateIndex();
                return mCreateIndex;
            }
            set
            {
                mCreateIndex = value;
            }
        }

        private ICommand mDeleteSegment;
        public ICommand DeleteSegmentCommand
        {
            get
            {
                if (mDeleteSegment == null)
                    mDeleteSegment = new DeleteSegment();
                return mDeleteSegment;
            }
            set
            {
                mDeleteSegment = value;
            }
        }


    }

    /// <summary>
    /// Edit segment
    /// </summary>
    class SegmentEditor : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            SqlICD10SegmentVM sivm = parameter as SqlICD10SegmentVM;
            return sivm != null;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            SqlICD10SegmentVM seg = parameter as SqlICD10SegmentVM;
            if (seg != null)
            {
                WinEditSegment wes = new WinEditSegment(seg);
                wes.ShowDialog();
                seg.UpdateAll();
            }
        }
        #endregion
    }

    /// <summary>
    /// Add checkpoint
    /// </summary>
    class CPAdder : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            SqlICD10SegmentVM sivm = parameter as SqlICD10SegmentVM;
            return sivm != null;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            SqlICD10SegmentVM sivm = parameter as SqlICD10SegmentVM;
            WinEnterText wet = new WinEnterText("Please input new title.");
            wet.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wet.ShowDialog();
            if (wet.ReturnValue == null) return;
            if (wet.ReturnValue.Trim() != "")
            {
                sivm.AddCheckPoint(new SqlCheckpointVM(wet.ReturnValue, sivm.ICD10SegmentID));
            }
        }
        #endregion
    }

    /// <summary>
    /// Add Segment
    /// </summary>
    class SegmentAdder : ICommand
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
            SqlICD10SegmentVM sivm = parameter as SqlICD10SegmentVM;
            SqlICD10SegmentVM seg = new SqlICD10SegmentVM("Enter Segment Title");
            WinEditSegment wes = new WinEditSegment(seg);
            wes.ShowDialog();
            seg.UpdateAll();
        }

    }

    /// <summary>
    /// Add Segment
    /// </summary>
    class DeleteSegment : ICommand
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
            SqlICD10SegmentVM sivm = parameter as SqlICD10SegmentVM;
            sivm.DeleteSegment();
        }

    }

    /// <summary>
    /// CreateIndex
    /// </summary>
    class CreateIndex : ICommand
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
            SqlICD10SegmentVM seg = parameter as SqlICD10SegmentVM;
            if (seg != null)
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    SqlCheckpointVM cpvm = new SqlCheckpointVM();
                    ObservableCollection<SqlCheckpointVM> lcp = seg.Checkpoints;
                    List<SqlCheckPointType> lcpt = cpvm.CheckPointTypes;
                    string strSummary = "";// @"<style type=""text / css"">< !--.tab { margin - left: 40px; }--></ style >";
                    strSummary += $"<h1>{seg.SegmentTitle}</h1><br>";
                    foreach (SqlCheckPointType cpt in lcpt)
                    {
                        string strTempOut = "<ol>";
                        foreach (SqlCheckpointVM cp in lcp)
                        {
                            if (cp.CheckPointType == cpt.CheckPointTypeID)
                            {
                                strTempOut += $"<dl><li><dt><font size='+1'>{cp.CheckPointTitle}</font>" + Environment.NewLine;
                                if (cp.Comment != null)
                                {
                                    string strEncoded = cp.Comment.Replace(Environment.NewLine, "<br style='display: block; margin: 0px; line-height:0px'>");
                                    strTempOut += $"<dd><i>{strEncoded}</i>" + Environment.NewLine;
                                    if (cp.Link != null)
                                    {
                                        strTempOut += $"<br><a href='{cp.Link}'>[Link to source]</a>";
                                    }
                                    if (cp.Images.Count > 0)
                                    {
                                        foreach (var imgCPimage in cp.Images)
                                        {
                                            var b64String = Convert.ToBase64String(imgCPimage.ImageData);
                                            var dataUrl = "data:image/bmp;base64," + b64String;
                                            strTempOut += $"<br><img src=\"{dataUrl}" + "\" />";
                                        }
                                    }
                                    strTempOut += "";
                                }
                                strTempOut += "</dl></li>";
                            }
                        }
                        if (strTempOut != "<ol>")
                        {
                            strSummary += $"<font size='+2'><B  style='margin-left: 10px'>{cpt.Title} </B></font>" + Environment.NewLine;
                            strSummary += strTempOut + "</ol>";
                            strSummary += Environment.NewLine;
                        }
                        else
                        {
                            strTempOut = "";
                        }
                    }
                    //Clipboard.SetText(strSummary, TextDataFormat.Html);
                    ClipboardHelper.CopyToClipboard(strSummary, "");
                    WinPreviewHTML wp = new WinPreviewHTML();
                    wp.MyWB.NavigateToString(HtmlLittlerHelper.FixHtml(strSummary));
                    wp.ShowDialog();
                }
            }


        }
    }


}