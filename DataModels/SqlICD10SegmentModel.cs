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
    public class SqlICD10Segment : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public int ICD10SegmentID { get; set; }

        public int LeftOffset { get; set; }
        public string icd10Chapter
        {
            get => icd10Chapter1;
            set
            {
                icd10Chapter1 = value;
            }
        }
        public double icd10CategoryStart { get; set; }
        public double icd10CategoryEnd { get; set; }
        public string SegmentTitle { get; set; }
        public string SegmentComment { get; set; }

        public Thickness Icd10Margin {
            
                get {
                return new Thickness(LeftOffset, 0, 0, 0);
                }
            }

        private bool includeSegment = true;
        private string icd10Chapter1;

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

        public List<SqlCheckpoint> Checkpoints
        {
            get
            {
                string sql = $"Select * from CheckPoints where TargetICD10Segment = {ICD10SegmentID};";
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
                string sql = $"Select Count(*) from CheckPoints where TargetICD10Segment = {ICD10SegmentID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.ExecuteScalar<int>(sql);
                }
            }
        }

        public SqlICD10Segment()
        {
        }

        public SqlICD10Segment(string strSegmentTitle)
        {
            strSegmentTitle = strSegmentTitle.Replace("'", "''"); //used to avoid errors in titles with ' character
            string sql = "";
            sql = $"INSERT INTO ICD10Segments (SegmentTitle) VALUES ('{strSegmentTitle}');";
            sql += $"Select * from ICD10Segments where SegmentTitle = '{strSegmentTitle}';"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                SqlICD10Segment p = cnn.QueryFirstOrDefault<SqlICD10Segment>(sql);
                ICD10SegmentID = p.ICD10SegmentID;
                SegmentTitle = p.SegmentTitle;
            }
        }

        public void AddCheckPoint(SqlCheckpoint cp)
        {
            strng sql = "";
            sql = $"INSERT OR IGNORE INTO relICD10SegmentsCheckPoints (ICD10SegmentID, CheckPointID) VALUES({ICD10SegmentID}, {cp.CheckPointID});";
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
            foreach (SqlICD10Segment ns in CF.NoteICD10Segments)
            {
                iOffset = 0;
                if (charChapter == char.Parse(ns.icd10Chapter))
                {
                    if ((ns.icd10CategoryStart >= CodeStart) && (ns.icd10CategoryEnd <= CodeEnd))
                    {
                        iOffset = 10;
                    }
                    else
                    {
                        CodeStart = ns.icd10CategoryStart;
                        CodeEnd = ns.icd10CategoryEnd;
                        charChapter = char.Parse(ns.icd10Chapter);
                    }
                }
                else
                {
                    charChapter = char.Parse(ns.icd10Chapter);
                    CodeStart = 0;
                    CodeEnd = 0;
                }
                strSql += $"UPDATE ICD10Segments SET LeftOffset = {iOffset} WHERE ICD10SegmentID = {ns.ICD10SegmentID};\n";
            }
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(strSql);
            }
        }

        public void SaveToDB()
        {
            string sql = "UPDATE ICD10Segments SET " +
                    "ICD10SegmentID=@ICD10SegmentID, " +
                    "icd10Chapter=@icd10Chapter, " +
                    "icd10CategoryStart=@icd10CategoryStart, " +
                    "icd10CategoryEnd=@icd10CategoryEnd, " +
                    "SegmentTitle=@SegmentTitle, " +
                    "SegmentComment=@SegmentComment " +
                    "WHERE ICD10SegmentID=@ICD10SegmentID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
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
            WinEnterText wet = new WinEnterText("Please input new title.");
            wet.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wet.ShowDialog();
            if (wet.ReturnValue == null) return;
            if (wet.ReturnValue.Trim() != "")
            {
                s.AddCheckPoint(new SqlCheckpoint(wet.ReturnValue, s.ICD10SegmentID));
            }
        }
        #endregion
    }
}
