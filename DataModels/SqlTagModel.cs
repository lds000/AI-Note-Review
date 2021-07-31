using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AI_Note_Review
{
    public class SqlTag
    {
        // Declare the event
        public int TagID { get; set; }
        public string TagText { get; set; }

        public List<SqlTagRegEx> TagRegExs
        {
            get
            {
                string sql = $"Select * from TagRegEx where TargetTag = {TagID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.Query<SqlTagRegEx>(sql, this).ToList();
                }
            }

        }


        public SqlTag()
        {
        }

        public SqlTag(string strTagText)
        {
            strTagText = strTagText.Replace("'", "''"); //used to avoid errors in titles with ' character
            string sql = "";
            sql = $"INSERT INTO Tags (TagText) VALUES ('{strTagText}');";
            sql += $"Select * from Tags where TagText = '{strTagText}';"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                SqlTag p = cnn.QueryFirstOrDefault<SqlTag>(sql);
                TagID = p.TagID;
                TagText = p.TagText;
            }
        }

        public List<SqlTagRegEx> GetTagRegExs()
        {
            string sql = $"Select * from TagRegEx where TargetTag = {TagID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                return cnn.Query<SqlTagRegEx>(sql, this).ToList();
            }

        }

        public void SaveToDB()
        {
            string sql = "UPDATE Tags SET " +
                    "TagID=@TagID, " +
                    "TagText=@TagText " +
                    "WHERE TagID=@TagID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
        }

        public bool DeleteFromDB()
        {
            MessageBoxResult mr = MessageBox.Show("Are you sure you want to remove this expression? This is permenant and will delete all content.", "Confirm Delete", MessageBoxButton.YesNo);
            if (mr != MessageBoxResult.Yes)
            {
                return false;
            }

            string sql = "Delete from Tags TagID Tags=@TagID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
            return true;
        }
    }
}

