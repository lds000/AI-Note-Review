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
    public class SqlCheckpoint : INotifyPropertyChanged
    {
        private bool includeCheckpoint = true;

        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public int CheckPointID { get; set; }
        public string CheckPointTitle { get; set; }


        public bool IncludeCheckpoint
        {
            get
            {
                return includeCheckpoint;
            }
            set
            {
                includeCheckpoint = value;
            }
        }
        public int ErrorSeverity { get; set; }
        public int CheckPointType { get; set; }
        public int TargetSection { get; set; }
        public string Comment { get; set; }
        public string Action { get; set; }
        public string Link { get; set; }

        public int Expiration { get; set; }
        public SqlCheckpoint()
        {
        }

        public List<SqlTag> Tags
        {
            get
            {
                string sql = $"select t.TagID, TagText from Tags t inner join RelTagCheckPoint relTC on t.TagID = relTC.TagID where CheckPointID = {CheckPointID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.Query<SqlTag>(sql, this).ToList();
                }
            }
        }


        public string RichText { get; set; }
        public int TargetICD10Segment { get; set; }




        public SqlCheckpoint(string strCheckPointTitle, int iTargetICD10Segment)
        {
            strCheckPointTitle = strCheckPointTitle.Replace("'", "''"); //used to avoid errors in titles with ' character
            string sql = "";
            int targetType = 1;
            int targetSection = 1;

            //now guess the categories to save time.
            if (strCheckPointTitle.ToLower().Contains("history"))
            {
                targetType = 1;
                targetSection = 1;
            }

            if (strCheckPointTitle.ToLower().Contains("exam"))
            {
                targetType = 8;
                targetSection = 8;
            }

            if (strCheckPointTitle.ToLower().Contains("elderly"))
            {
                targetType = 10;
                targetSection = 9;
            }

            if (strCheckPointTitle.ToLower().Contains("lab"))
            {
                targetType = 3;
                targetSection = 11;
            }

            if (strCheckPointTitle.ToLower().Contains("educat"))
            {
                targetType = 7;
                targetSection = 10;
            }

            if (strCheckPointTitle.ToLower().Contains("xr"))
            {
                targetType = 4;
                targetSection = 12;
            }

            if (strCheckPointTitle.ToLower().Contains("imag"))
            {
                targetType = 4;
                targetSection = 12;
            }

            if (strCheckPointTitle.ToLower().Contains("query"))
            {
                targetSection = 1;
            }



            sql = $"INSERT INTO CheckPoints (CheckPointTitle, TargetICD10Segment, TargetSection, CheckPointType) VALUES ('{strCheckPointTitle}', {iTargetICD10Segment}, {targetSection}, {targetType});";
            sql += $"Select * from CheckPoints where CheckPointTitle = '{strCheckPointTitle}' AND TargetICD10Segment = {iTargetICD10Segment};"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                SqlCheckpoint p = cnn.QueryFirstOrDefault<SqlCheckpoint>(sql);
                CheckPointID = p.CheckPointID;
                TargetICD10Segment = p.TargetICD10Segment;
                TargetSection = p.TargetSection;
                CheckPointType = p.CheckPointType;
                CheckPointTitle = p.CheckPointTitle;
            }
        }

        public void Commit(DocInfo di, SqlRelCPProvider.MyCheckPointStates cpState)
        {
            string sql = $"Replace INTO RelCPPRovider (ProviderID, CheckPointID, PtID, ReviewDate, VisitDate, CheckPointStatus) VALUES ({di.ProviderID}, {CheckPointID}, {di.PtID}, '{di.ReviewDate.ToString("yyyy-MM-dd")}', '{di.VisitDate.ToString("yyyy-MM-dd")}', {(int)cpState});";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }

        }

            public List<SqlTag> GetTags()
        {
            string sql = $"select t.TagID, TagText from Tags t inner join RelTagCheckPoint relTC on t.TagID = relTC.TagID where CheckPointID = {CheckPointID} order by RelTagCheckPointID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                return cnn.Query<SqlTag>(sql, this).ToList();
            }

        }

        public void RemoveTag(SqlTag st)
        {
            string sql = $"Delete From RelTagCheckPoint where CheckPointID = {CheckPointID} AND TagID = {st.TagID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }

        }

        public string GetIndex()
        {
            string strReturn = "";
            strReturn += $"CheckPoint: '{CheckPointTitle}'" + Environment.NewLine;
            //strReturn += $"\tSignificance {ErrorSeverity}/10." + Environment.NewLine;
            strReturn += $"\tRecommended Remediation: {Action}" + Environment.NewLine;
            strReturn += $"\tExplanation: {Comment}" + Environment.NewLine;
            if (Link != "")
                strReturn += $"\tLink: {Link}" + Environment.NewLine;
            strReturn += Environment.NewLine;
            strReturn += Environment.NewLine;
            return strReturn;
        }
        public string GetReport()
        {
            string strReturn = ""; 
            strReturn += $"\t'{CheckPointTitle}'" + Environment.NewLine;
            
            /*
            strReturn += $"\tSignificance {ErrorSeverity}/10." + Environment.NewLine;
            strReturn += $"\tRecommended Remediation: {Action}" + Environment.NewLine;
            strReturn += $"\tExplanation: {Comment}" + Environment.NewLine;
            if (Link != "")
            strReturn += $"\tLink: {Link}" + Environment.NewLine;
            strReturn += Environment.NewLine;
            strReturn += Environment.NewLine;
            */

            return strReturn;
        }

        public void SaveToDB()
        {
            string sql = "UPDATE CheckPoints SET " +
                    "CheckPointID=@CheckPointID, " +
                    "CheckPointTitle=@CheckPointTitle, " +
                    "CheckPointType=@CheckPointType, " +
                    "TargetSection=@TargetSection, " +
                    "TargetICD10Segment=@TargetICD10Segment, " +
                    "Comment=@Comment, " +
                    "Action=@Action, " +
                    "RichText=@RichText, " +
                    "ErrorSeverity=@ErrorSeverity, " +
                    "Link=@Link, " +
                    "Expiration=@Expiration " +
                    "WHERE CheckPointID=@CheckPointID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
        }

        public bool DeleteFromDB()
        {
            MessageBoxResult mr = MessageBox.Show("Are you sure you want to remove this diagnosis? This is permenant and will delete all content.", "Confirm Delete", MessageBoxButton.YesNo);
            if (mr != MessageBoxResult.Yes)
            {
                return false;
            }

            string sql = "Delete from CheckPoints WHERE CheckPointID=@CheckPointID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
            return true;
        }
    }
}
