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
    public class SqlProvider
    {
        public int ProviderID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Cert { get; set; }
        public string HomeClinic { get; set; }
        public int ReviewInterval { get; set; }
        public string FullName { get; set; }
        public bool IsWestSidePod { get; set; }

        public List<SqlMonthReviewSummary> ReviewsByMonth
        {
            get
            {
                string sql = ""; //.ToString("yyyy-MM-dd")
                sql += $"select ProviderID, strftime('%Y-%m', VisitDate) as 'yearmonth', Count(distinct VisitDate || PtID) as Reviews from RelCPPRovider where strftime('%Y', VisitDate) = '2021' AND ProviderID={ProviderID} group by strftime('%Y-%m', VisitDate);";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.Query<SqlMonthReviewSummary>(sql).ToList();
                }
            }

        }

        public List<SqlCurrentReviewsSummary> CurrentReviewsSummary
        {
            get
            {
                string sql = "";
                sql += $"Select distinct VisitDate, PtID from RelCPPRovider where ProviderID={ProviderID} and VisitDate Between '{Properties.Settings.Default.StartReviewDate.ToString("yyyy-MM-dd")}' and '{Properties.Settings.Default.EndReviewDate.ToString("yyyy-MM-dd")}';";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.Query<SqlCurrentReviewsSummary>(sql).ToList();
                }
            }
        }

        public int CurrentReviewCount
        {
            get
            {
                string sql = "";
                sql += $"Select Count(distinct VisitDate || PtID) from RelCPPRovider where ProviderID={ProviderID} and VisitDate Between '{Properties.Settings.Default.StartReviewDate.ToString("yyyy-MM-dd")}' and '{Properties.Settings.Default.EndReviewDate.ToString("yyyy-MM-dd")}';";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.ExecuteScalar<int>(sql);
                }
            }
        }
        public SqlProvider()
        {
        }

        public SqlProvider(string strFirstName, string strLastName, string strCert, string strHomeClinic, int intReviewInterval, string strFullName)
        {
                string sql = "";
                sql = $"INSERT INTO Providers (FirstName,LastName,Cert,HomeClinic,ReviewInterval,FullName, IsWestSidePod) VALUES ('{strFirstName}','{strLastName}','{strCert}','{strHomeClinic}',{intReviewInterval},'{strFullName}', {IsWestSidePod});";
                sql += $"Select * from Providers where FirstName = '{FirstName}' AND LastName = '{LastName}';"; //this part is to get the ID of the newly created phrase
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    SqlProvider p = cnn.QueryFirstOrDefault<SqlProvider>(sql);
                    ProviderID = p.ProviderID;
                }
        }

        public static List<SqlProvider> GetMyPeeps()
        {
            string sql = "";
            sql += $"Select * from Providers where IsWestSidePod == 'true' order by FullName;"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                return cnn.Query<SqlProvider>(sql).ToList();
            }
        }

        public static SqlProvider SqlGetProviderByFullName(string strFullName)
        {
            string sql = "";
            sql += $"Select * from Providers where FullName = '{strFullName}';"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                SqlProvider p = cnn.QueryFirstOrDefault<SqlProvider>(sql);
                if (p != null)
                {
                    return p;
                }
            }
            sql = $"INSERT INTO Providers (FullName) VALUES ('{strFullName}');";
            sql += $"Select * from Providers where FullName = '{strFullName}';"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                SqlProvider p = cnn.QueryFirstOrDefault<SqlProvider>(sql);
                return p;
            }
        }

        public void SaveToDatabase()
        {
            string sql = "UPDATE Providers SET " +
        "FirstName=@FirstName, " +
        "LastName=@LastName, " +
        "Cert=@Cert, " +
        "ReviewInterval=@ReviewInterval, " +
        "HomeClinic=@HomeClinic, " +
        "FullName=@FullName " +
        "IsWestSidePod=@IsWestSidePod " +
        "WHERE ProviderID=@ProviderID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
        }

        public List<DateTime> GetReviewDates(DateTime ReviewDate)
        {
            return new List<DateTime>();
        }

        public List<SqlCheckpoint> GetCheckPoints(DateTime ReviewDate)
        {
            return new List<SqlCheckpoint>();
        }

        public bool IsCheckPointExpired(SqlCheckpoint cp)
        {
            return true;
        }

        public bool IsReviewDue()
        {
            return true;
        }

        public void AddCheckPoint(SqlCheckpoint cp, DateTime dtReviewDate)
        {
            string sql = $"INSERT INTO RelCPPRovider (ProviderID,CheckPointID,PtID,HomeClinic,ReviewInterval,IsWestSidePod) VALUES ({ProviderID},{cp.CheckPointID},{CF.CurrentDoc.PtID},'{dtReviewDate}','{CF.CurrentDoc.VisitDate}',{IsWestSidePod});";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
        }
    }
}
