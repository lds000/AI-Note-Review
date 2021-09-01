using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AI_Note_Review
{
    public class SqlICD10Segment
    {

        public int ICD10SegmentID { get; set; }
        public string icd10Chapter { get; set; }
        public double icd10CategoryStart { get; set; }
        public double icd10CategoryEnd { get; set; }
        public string SegmentTitle { get; set; }
        public string SegmentComment { get; set; }

        public Thickness icd10Margin { get; set; }

        private bool includeSegment = true;
        public bool IncludeSegment {
            get
            {
                return includeSegment;
            }
            set
            {
                includeSegment = value;
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
            string sql = "";
            sql = $"INSERT OR IGNORE INTO relICD10SegmentsCheckPoints (ICD10SegmentID, CheckPointID) VALUES({ICD10SegmentID}, {cp.CheckPointID});";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }

        }
        public List<SqlCheckpoint> GetCheckPoints()
        {
            string sql = $"Select * from CheckPoints where TargetICD10Segment = {ICD10SegmentID};";

            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
              return  cnn.Query<SqlCheckpoint>(sql).ToList();
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
