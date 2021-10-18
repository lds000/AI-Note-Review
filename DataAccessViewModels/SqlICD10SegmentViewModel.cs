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
    public class SqlICD10SegmentViewModel : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private SqlICD10Segment sqlICD10Segment;

        public SqlICD10SegmentViewModel()
        {
            sqlICD10Segment = new SqlICD10Segment();
        }

        public SqlICD10SegmentViewModel(SqlICD10Segment sc)
        {
            sqlICD10Segment = sc;
        }


        public SqlICD10Segment SqlICD10Segment
            {
            get
            {
                return sqlICD10Segment;
            } 
        }

        public List<SqlCheckpoint> Checkpoints
        {
            get
            {
                string sql = $"Select * from CheckPoints where TargetICD10Segment = {sqlICD10Segment.ICD10SegmentID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.Query<SqlCheckpoint>(sql).ToList();
                }
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
        public static List<SqlICD10SegmentViewModel> NoteICD10Segments
        {
            get
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from ICD10Segments order by icd10Chapter, icd10CategoryStart;";
                    var l = cnn.Query<SqlICD10Segment>(sql).ToList();
                    List<SqlICD10SegmentViewModel> lvm = new List<SqlICD10SegmentViewModel>();
                    foreach (SqlICD10Segment s in l)
                    {
                        SqlICD10SegmentViewModel scvm = new SqlICD10SegmentViewModel(s);
                        lvm.Add(scvm);
                    }
                    return lvm;
                }
            }
        }


        public void AddCheckPoint(SqlCheckpoint cp)
        {
            MessageBox.Show("not implemented");
            return;
            string sql = "";
            sql = $"INSERT OR IGNORE INTO relICD10SegmentsCheckPoints (ICD10SegmentID, CheckPointID) VALUES({sqlICD10Segment.ICD10SegmentID}, {cp.CheckPointID});";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
            OnPropertyChanged("Checkpoints");
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
            foreach (SqlICD10SegmentViewModel ns in NoteICD10Segments)
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

        public void Execute(object parameter)
        {
            SqlICD10Segment s = parameter as SqlICD10Segment;
            SqlICD10SegmentViewModel sivm = new SqlICD10SegmentViewModel(s);
            WinEnterText wet = new WinEnterText("Please input new title.");
            wet.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wet.ShowDialog();
            if (wet.ReturnValue == null) return;
            if (wet.ReturnValue.Trim() != "")
            {
                sivm.AddCheckPoint(new SqlCheckpoint(wet.ReturnValue, s.ICD10SegmentID));
            }
        }
        #endregion
    }


}
