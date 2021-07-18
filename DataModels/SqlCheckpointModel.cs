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
    public class SqlCheckpoint
    {
        public int CheckPointID { get; set; }
        public string CheckPointTitle { get; set; }
        public int CheckPointType { get; set; }
        public int TargetSection { get; set; }
        public string Comment { get; set; }
        public string Link { get; set; }

        public int TargetICD10Segment { get; set; }

        public SqlCheckpoint()
        {
        }

        public SqlCheckpoint(string strCheckPointTitle)
        {
            strCheckPointTitle = strCheckPointTitle.Replace("'", "''"); //used to avoid errors in titles with ' character
            string sql = "";
            sql = $"INSERT INTO CheckPoints (CheckPointTitle) VALUES ('{strCheckPointTitle}');";
            sql += $"Select * from Phrases where PhraseTitle = '{strCheckPointTitle}';"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLightDataAccess.SQLiteDBLocation))
            {
                SqlCheckpoint p = cnn.QueryFirstOrDefault<SqlCheckpoint>(sql);
                CheckPointID = p.CheckPointID;
                CheckPointTitle = p.CheckPointTitle;
            }
        }

        public void SaveToDB()
        {
            string sql = "UPDATE CheckPoints SET " +
                    "CheckPointID=@CheckPointID, " +
                    "CheckPoint=@CheckPoint, " +
                    "CheckPointType=@CheckPointType, " +
                    "TargetSection=@TargetSection, " +
                    "Comment=@Comment, " +
                    "Link=@Link " +
                    "WHERE CheckPointID=@CheckPointID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLightDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
        }
    }
}
