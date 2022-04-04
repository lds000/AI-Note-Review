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


        private SqlICD10SegmentM sqlICD10Segment;

        
        public SqlICD10SegmentVM()
        {
            sqlICD10Segment = new SqlICD10SegmentM();
            masterReviewSummary = new MasterReviewSummaryVM();
        }
        

            /// <summary>
            /// Used only one instance, creating a new segment.
            /// </summary>
            /// <param name="strSegmentTitle"></param>
        public SqlICD10SegmentVM(string strSegmentTitle, MasterReviewSummaryVM m)
        {
            sqlICD10Segment = new SqlICD10SegmentM(strSegmentTitle);
            MasterReviewSummary = m;
        }

        public SqlICD10SegmentVM(SqlICD10SegmentM sc, MasterReviewSummaryVM m)
        {
            sqlICD10Segment = sc;
            MasterReviewSummary = m;
        }

        public AlternativeICD10VM AlternativeICD10 { get; set; }

        private List<AlternativeICD10VM> alternativeICD10s;
        public List<AlternativeICD10VM> AlternativeICD10s
        {
            get
            {
                if (alternativeICD10s == null)
                {
                    string sql = $"Select * from  RelAlternativeICD10 where TargetICD10Segment == {ICD10SegmentID};";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        var tmpList = cnn.Query<AlternativeICD10VM>(sql).ToList();
                        foreach (var item in tmpList)
                        {
                            //item.ParentSegment = ;
                        }
                        alternativeICD10s = tmpList;
                    }

                }
                return alternativeICD10s;
            }
        }

        public void AddAlternativeICD10(string strICD10 = "")
        {
            string strTitle = "";

                WinEnterText wet = new WinEnterText("Please input new title.");
                wet.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                wet.ShowDialog();
                if (wet.ReturnValue == null) return;
                if (wet.ReturnValue.Trim() != "")
                {
                    strTitle = wet.ReturnValue;
                }

            if (strICD10 == "")
            {
                WinEnterText wet2 = new WinEnterText($"Please input new ICD10 Code for {strTitle}.");
                wet2.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                wet2.ShowDialog();
                if (wet2.ReturnValue == null) return;
                if (wet2.ReturnValue.Trim() != "")
                {
                    strICD10 = wet2.ReturnValue.Trim();
                }
            }

            var tmpreturn = new AlternativeICD10VM(strTitle, strICD10, ICD10SegmentID);
            alternativeICD10s = null;
            OnPropertyChanged("AlternativeICD10s");
            MasterReviewSummary.ResetMissingDx(); //repopulate
        }

        public SqlICD10SegmentM SqlICD10Segment
        {
            get { return sqlICD10Segment; }
        }






        public SqlCheckpointVM SelectedCheckPoint { get; set; }

        public int ICD10SegmentID { get { return sqlICD10Segment.ICD10SegmentID; } set { sqlICD10Segment.ICD10SegmentID = value; } }
        public string SegmentTitle { get { return sqlICD10Segment.SegmentTitle; } set { sqlICD10Segment.SegmentTitle = value; } }
        public string SegmentComment {
            get { return sqlICD10Segment.SegmentComment; } 
            set { 
                sqlICD10Segment.SegmentComment = value;
                SaveToDB();
            } }
        public string icd10Chapter { get { return sqlICD10Segment.icd10Chapter; } set { sqlICD10Segment.icd10Chapter = value; } }
        public double icd10CategoryStart { get { return sqlICD10Segment.icd10CategoryStart; } set { sqlICD10Segment.icd10CategoryStart = value; } }
        public double icd10CategoryEnd { get { return sqlICD10Segment.icd10CategoryEnd; } set { sqlICD10Segment.icd10CategoryEnd = value; } }
        public int LeftOffset { get { return sqlICD10Segment.LeftOffset; } set { sqlICD10Segment.LeftOffset = value; } }

        public int ParentSegment
        {
            get
            {
                return sqlICD10Segment.ParentSegment;
            }
            set
            {
                sqlICD10Segment.ParentSegment = value;
                SaveToDB();
            }
        }

        private Thickness indent;
        public Thickness Indent
        {
            get
            {
                return indent;
            }
            set
            {
                indent = value;
                OnPropertyChanged();
            }
        }
        

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
                ParentReport.UpdateCPs(); //now recalculate all checkpoints.
            }
        }

        /// <summary>
        /// Calculate all left offsets, property change NoteICD10Segments after setting null.
        /// </summary>
        public void UpdateAll()
        {
            CalculateLeftOffsets();
            noteICD10Segments = null;
            OnPropertyChanged("NoteICD10Segments");
        }

        private ObservableCollection<SqlCheckpointVM> checkpoints;
        /// <summary>
        /// Contains a list of all the checkpoints (passed, missed, dropped) for segment.
        /// </summary>
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
                            cpvm.PropertyChanged += CheckPointVM_PropertyChanged;
                            tmpCol.Add(cpvm);
                        }
                        checkpoints = tmpCol;
                    }
                }
                return checkpoints;
            }
            set
            {
                checkpoints = value;
                OnPropertyChanged();
                MissedCPs = null;
                PassedCPs = null;
                DroppedCPs = null;
            }
        }

        private void CheckPointVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ReorderCheckPoints")
            {
                checkpoints = new ObservableCollection<SqlCheckpointVM>(checkpoints.OrderByDescending(x => x.ErrorSeverity).OrderBy(x => x.CheckPointTypeOrder).ToList());
                OnPropertyChanged("Checkpoints");
            }

            if (e.PropertyName == "ReloadICD10Segments")
            {
                OnPropertyChanged("ReloadICD10Segments"); //todo: should be "ICD10Segments" for 
            }

            if (e.PropertyName == "CPStatus")
            {
                OnPropertyChanged("CPStatus"); //bubble up for VisitReportVM to recalculate checkpoint
            }
        }

        /*
        public void RemoveCheckPoint(SqlCheckpointVM cp)
        {
            cp.DeleteFromDB();
        }
        */

        /*
        public void CheckSegment()
        {
            passedCPs = new List<SqlCheckpointVM>();
            missedCPs = new List<SqlCheckpointVM>();
            droppedCPs = new List<SqlCheckpointVM>();
            OnPropertyChanged("PassedCPs");
            OnPropertyChanged("MissedCPs");
            OnPropertyChanged("DroppedCPs");
        }
        */

        private List<SqlCheckpointVM> passedCPs;
        public List<SqlCheckpointVM> PassedCPs
        {
            get
            {
                if (passedCPs == null)
                {
                    passedCPs = new List<SqlCheckpointVM>(from c in Checkpoints where c.CPStatus == SqlTagRegExM.EnumResult.Pass orderby c.ErrorSeverity descending orderby c.CheckPointTypeOrder  select c);
                }
                return passedCPs;
            }
            set
            {
                passedCPs = value;
                OnPropertyChanged();
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
            set
            {
                missedCPs = value;
                OnPropertyChanged();
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
            set
            {
                droppedCPs = value;
                OnPropertyChanged();

            }
        }

        public ObservableCollection<MasterReviewSummaryVM> MasterReviewSummaryList
        {
            get
            {
                return MasterReviewSummary.MasterReviewSummaries;
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
                ParentReport.UpdateCPs();
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

        /// <summary>
        /// A list of all SqlICD10Segments
        /// </summary>
        private static List<SqlICD10SegmentVM> noteICD10Segments;
        public static List<SqlICD10SegmentVM> NoteICD10Segments
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
                            SqlICD10SegmentVM scvm = new SqlICD10SegmentVM(s, null);
                            lvm.Add(scvm);
                        }
                        return lvm;
                    }
                }
                return noteICD10Segments;
            }
            set
            {
                NoteICD10Segments = value;
            }
        } //static NoteICD10Segments! Move this

        public void AddCheckPoint(SqlCheckpointVM cp)
        {
            cp.ParentSegment = this;
            checkpoints.Add(cp);
            OnPropertyChanged("Checkpoints");
            SelectedCP = checkpoints.Last();
        }

        private SqlCheckpointVM selectedCP;
        public SqlCheckpointVM SelectedCP
        {
            get
            {
                return selectedCP;
            }
            set
            {
                selectedCP = value;
                OnPropertyChanged();
            }
        }


        private string indexHtml;
        public string IndexHtml
        {
            get
            {
                if (indexHtml == null)
                {
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        SqlCheckpointVM cpvm = new SqlCheckpointVM();
                        ObservableCollection<SqlCheckpointVM> lcp = Checkpoints;
                        List<SqlCheckPointType> lcpt = cpvm.CheckPointTypes;
                        string strSummary = "";// @"<style type=""text / css"">< !--.tab { margin - left: 40px; }--></ style >";
                        strSummary += $"<h1>{SegmentTitle}</h1><br>";
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
                       indexHtml = strSummary;
                    }
                }
                return indexHtml;
            }
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

        #region commands
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

        //AddAlternativeCommand
        private ICommand mAddAlternative;
        public ICommand AddAlternativeCommand
        {
            get
            {
                if (mAddAlternative == null)
                    mAddAlternative = new AddAlternative();
                return mAddAlternative;
            }
            set
            {
                mAddAlternative = value;
            }
        }

        #endregion
    }

    #region command classes
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
            ClipboardHelper.CopyToClipboard(seg.IndexHtml, "");
            WinPreviewHTML wp = new WinPreviewHTML();
            wp.MyWB.NavigateToString(HtmlLittlerHelper.FixHtml(seg.IndexHtml));
            wp.ShowDialog();
        }
    }

    //AddAlternative
    class AddAlternative : ICommand
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
            seg.AddAlternativeICD10();
        }
    }

    #endregion

}