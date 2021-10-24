using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Note_Review
{
    public static class SqlLiteDataAccess
    {
        public static string SQLiteDBLocation { get; set; }
        public static WindowPosition GetWindowPosition(string TargetWindowTitle)
        {
            string sql = $"Select * from WindowPositions where WindowPositionTitle = '{TargetWindowTitle}'";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                var output = cnn.QueryFirstOrDefault<WindowPosition>(sql);
                return output;
            }
        }

        public static SqlICD10Segment GetSegment(int iSegID)
        {
            string sql = $"Select * from ICD10Segments where ICD10SegmentID == {iSegID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                var output = cnn.Query<SqlICD10Segment>(sql).First();
                return output;
            }

        }

        public static List<SqlTagVM> GetTags(string strSearch)
        {
            string sql = $"Select * from Tags where TagText like '%{strSearch.ToLower()}%' COLLATE NOCASE order by TagText;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                var output = cnn.Query<SqlTagVM>(sql).ToList();
                return output;
            }

        }

        public static List<string> GetAllTags()
        {
            string sql = $"Select TagText from Tags;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                var output = cnn.Query<string>(sql).ToList();
                return output;
            }

        }
    }

}
