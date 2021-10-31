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
    public class SqlICD10SegmentM : INotifyPropertyChanged
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


        private string icd10Chapter1;

        public SqlICD10SegmentM()
        {
        }


        public SqlICD10SegmentM(string strSegmentTitle)
        {
            strSegmentTitle = strSegmentTitle.Replace("'", "''"); //used to avoid errors in titles with ' character
            string sql = "";
            sql = $"INSERT INTO ICD10Segments (SegmentTitle) VALUES ('{strSegmentTitle}');";
            sql += $"Select * from ICD10Segments where SegmentTitle = '{strSegmentTitle}';"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                SqlICD10SegmentM p = cnn.QueryFirstOrDefault<SqlICD10SegmentM>(sql);
                ICD10SegmentID = p.ICD10SegmentID;
                SegmentTitle = p.SegmentTitle;
            }
        }

        public void DeleteSegment()
        {
            MessageBoxResult mr = MessageBox.Show("Are you sure you want to remove this segment? This is permenant and will delete all content.", "Confirm Delete", MessageBoxButton.YesNo);
            if (mr != MessageBoxResult.Yes)
            {
                return;
            }

            string sql = $"Delete from ICD10Segments where ICD10SegmentID = {ICD10SegmentID};"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
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

}
