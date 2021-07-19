using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using Dapper;
using System.Windows;

namespace AI_Note_Review
{
    public class WindowPosition
    {
        public int WindowPositionID { get; set; }
        public string WindowPositionTitle { get; set; }
        public int WindowPositionLeft { get; set; }
        public int WindowPositionTop { get; set; }

        public WindowPosition()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strTargetWindowTitle"></param>
        /// <param name="strTargetHeaderClue"></param>
        /// <param name="strTargetWindowDescription"></param>
        /// <param name="strTargetWindowRemedyAction"></param>
        public WindowPosition(string strWindowPositionTitle, int intWindowPositionLeft, int intWindowPositionTop)
        {
            string sql = $"INSERT INTO WindowPositions (WindowPositionTitle,WindowPositionLeft,WindowPositionTop) VALUES ('{strWindowPositionTitle}',{intWindowPositionLeft},{intWindowPositionTop});";
            sql += $"Select * from WindowPositions where WindowPositionTitle = '{strWindowPositionTitle}';";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                var p = cnn.QueryFirstOrDefault<WindowPosition>(sql); //Todo: could not use <TargetWindow> due to error
                WindowPositionID = p.WindowPositionID;
                WindowPositionTitle = p.WindowPositionTitle;
                WindowPositionLeft = p.WindowPositionLeft;
                WindowPositionTop = p.WindowPositionTop;
            }
        }



        public void SaveToDB()
        {


            string sql = "UPDATE WindowPositions SET " +
                    "WindowPositionID=@WindowPositionID, " +
                    "WindowPositionTitle=@WindowPositionTitle, " +
                    "WindowPositionLeft=@WindowPositionLeft, " +
                    "WindowPositionTop=@WindowPositionTop " +
                    "WHERE WindowPositionID=@WindowPositionID;";

            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                var affectedRows = cnn.Execute(sql, this);
            }

        }

    }
}



