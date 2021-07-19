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
        public static List<SqlNoteSection> NoteSections { get; set; }
        public static List<SqlICD10Segment> ICD10Segments { get; set; }
        public static List<SqlCheckpoint> Checkpoints { get; set; }
        public static WindowPosition GetWindowPosition(string TargetWindowTitle)
        {
            string sql = $"Select * from WindowPositions where WindowPositionTitle = '{TargetWindowTitle}'";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                var output = cnn.QueryFirstOrDefault<WindowPosition>(sql);
                return output;
            }
        }
    }

}
