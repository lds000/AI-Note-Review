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

        private SqlICD10SegmentM sqlICD10Segment;

        public SqlICD10SegmentVM()
        {
            sqlICD10Segment = new SqlICD10SegmentM();
        }

        public SqlICD10SegmentVM(SqlICD10SegmentVM sc)
        {
            sqlICD10Segment = sc.sqlICD10Segment;
        }

        public SqlICD10SegmentVM(string strSegmentTitle)
        {
            sqlICD10Segment = new SqlICD10SegmentM(strSegmentTitle);
        }

        public DocumentVM ParentDocument { get; set; }
        public VisitReportVM ParentReport { get; set; }

        public SqlICD10SegmentM SqlICD10Segment
        {
            get
            {
                return sqlICD10Segment;
            }
        }
        public int ICD10SegmentID { get { return sqlICD10Segment.ICD10SegmentID; } set { sqlICD10Segment.ICD10SegmentID = value; } }
        public int LeftOffset { get { return sqlICD10Segment.LeftOffset; } set { sqlICD10Segment.LeftOffset = value; } }
        public string icd10Chapter { get { return sqlICD10Segment.icd10Chapter; } set { sqlICD10Segment.icd10Chapter = value; } }
        public double icd10CategoryStart { get { return sqlICD10Segment.icd10CategoryStart; } set { sqlICD10Segment.icd10CategoryStart = value; } }
        public double icd10CategoryEnd { get { return sqlICD10Segment.icd10CategoryEnd; } set { sqlICD10Segment.icd10CategoryEnd = value; } }
        public string SegmentTitle { get { return sqlICD10Segment.SegmentTitle; } set { sqlICD10Segment.SegmentTitle = value; } }
        public string SegmentComment { get { return sqlICD10Segment.SegmentComment; } set { sqlICD10Segment.SegmentComment = value; } }

        private ObservableCollection<SqlCheckpointVM> checkpoints;
        public ObservableCollection<SqlCheckpointVM> Checkpoints
        {
            get
            {
                if (checkpoints == null)
                {
                    string sql = $"Select * from CheckPoints where TargetICD10Segment = {sqlICD10Segment.ICD10SegmentID};";
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

        //recheck CPs;
        public void UpdateCPs()
        {
            passedCPs = null;
            missedCPs = null;
            droppedCPs = null;
            Console.WriteLine($"Setting passed, missed, and droppedCPs to null for segment {SegmentTitle}.");
            ParentReport.UpdateCPs();
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

        /// <summary>
        /// A list of all SqlICD10Segments
        /// </summary>Celica9
        public static List<SqlICD10SegmentVM> NoteICD10Segments
        {
            get
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from ICD10Segments order by icd10Chapter, icd10CategoryStart;";
                    var l = cnn.Query<SqlICD10SegmentVM>(sql).ToList();
                    List<SqlICD10SegmentVM> lvm = new List<SqlICD10SegmentVM>();
                    foreach (SqlICD10SegmentVM s in l)
                    {
                        SqlICD10SegmentVM scvm = new SqlICD10SegmentVM(s);
                        lvm.Add(scvm);
                    }
                    return lvm;
                }
            }
        }


        public void AddCheckPoint(SqlCheckpointM cp)
        {
            checkpoints.Add(new SqlCheckpointVM(cp));
            OnPropertyChanged("Checkpoints");
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

        private ICommand mSegIncludeChecked;
        public ICommand SegIncludeCheckedCommand
        {
            get
            {
                if (mSegIncludeChecked == null)
                    mSegIncludeChecked = new SegIncludeChecked();
                return mSegIncludeChecked;
            }
            set
            {
                mSegIncludeChecked = value;
            }
        }

    }

    class SegIncludeChecked : ICommand
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
            SqlICD10SegmentVM ssvm = parameter as SqlICD10SegmentVM;
            ssvm.ParentReport.SetCPs();
        }
        #endregion
    }

    class CPAdder : ICommand
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
            SqlICD10SegmentVM s = parameter as SqlICD10SegmentVM;
            SqlICD10SegmentVM sivm = new SqlICD10SegmentVM(s);

            WinEnterText wet = new WinEnterText("Please input new title.");
            wet.ShowDialog();
            if (wet.ReturnValue == null) return;
            if (wet.ReturnValue.Trim() != "")
            {
                sivm.AddCheckPoint(new SqlCheckpointM(wet.ReturnValue, s.ICD10SegmentID));
            }
        }
    }


}
