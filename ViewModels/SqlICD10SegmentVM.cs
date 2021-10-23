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

        public SqlICD10SegmentM SqlICD10Segment
        {
                get { return sqlICD10Segment;}
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

        public int ICD10SegmentID { get { return sqlICD10Segment.ICD10SegmentID; } set { sqlICD10Segment.ICD10SegmentID = value; } }
        public string SegmentTitle { get { return sqlICD10Segment.SegmentTitle; } set { sqlICD10Segment.SegmentTitle = value; } }
        public string SegmentComment { get { return sqlICD10Segment.SegmentComment; } set { sqlICD10Segment.SegmentComment = value; } }
        
        public string icd10Chapter { get { return sqlICD10Segment.icd10Chapter; } set { sqlICD10Segment.icd10Chapter = value; } }
        public double icd10CategoryStart { get { return sqlICD10Segment.icd10CategoryStart; } set { sqlICD10Segment.icd10CategoryStart = value; } }
        public double icd10CategoryEnd { get { return sqlICD10Segment.icd10CategoryEnd; } set { sqlICD10Segment.icd10CategoryEnd = value; } }
        public int LeftOffset { get { return sqlICD10Segment.LeftOffset; } set { sqlICD10Segment.LeftOffset = value; } }

        public  void SaveToDB()
        {
            sqlICD10Segment.SaveToDB();
        }

        private bool includeSegment = true;
        public bool IncludeSegment
        {
            get
            {
                includeSegment = true;
                if (ICD10SegmentID == 90) //ed transfer, never include
                {
                    includeSegment = false;
                }
                return includeSegment;
            }
            set
            {
                includeSegment = value;
            }
        }

        public void UpdateAll()
        {
            OnPropertyChanged("SqlICD10Segment");
        }
            
        public ObservableCollection<SqlCheckpointVM> Checkpoints
        {
            get
            {
                string sql = $"Select * from CheckPoints where TargetICD10Segment = {sqlICD10Segment.ICD10SegmentID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    var tmpList = cnn.Query<SqlCheckpointM>(sql).ToList();
                    ObservableCollection<SqlCheckpointVM> tmpCol = new ObservableCollection<SqlCheckpointVM>();
                    foreach (var item in tmpList)
                    {
                        tmpCol.Add(new SqlCheckpointVM(item, this));
                    }
                    return tmpCol;
                }
            }
        }

        public void UpdateCheckPoints()
        {
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
                    var l = cnn.Query<SqlICD10SegmentM>(sql).ToList();
                    List<SqlICD10SegmentVM> lvm = new List<SqlICD10SegmentVM>();
                    foreach (SqlICD10SegmentM s in l)
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


    }

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
                sivm.AddCheckPoint(new SqlCheckpointM(wet.ReturnValue, sivm.ICD10SegmentID));
            }
        }
        #endregion
    }



}
