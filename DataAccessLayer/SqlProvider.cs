using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
namespace AI_Note_Review
{
    public class SqlProvider : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private MasterReviewSummaryVM parentMasterReviewSummary;
        public MasterReviewSummaryVM ParentMasterReviewSummary 
        {
            get { return parentMasterReviewSummary; }
            set {
                parentMasterReviewSummary = value;
                OnPropertyChanged();
                OnPropertyChanged("CurrentReviewCount");
            }
        }
        public int ProviderID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Cert { get; set; }
        public string HomeClinic { get; set; }
        public int ReviewInterval { get; set; }
        public string FullName { get; set; }
        public bool IsWestSidePod { get; set; }

        public string EMail { get; set; }

        public string PersonalNotes { get; set; }

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


        public void UpdateSqlDocumentReviewsSummaryProperty()
        {
            OnPropertyChanged("SqlDocumentReviewsSummaryProperty");
            OnPropertyChanged("CurrentReviewCount");
        }

        public int CurrentReviewCount
        {
            get
            {
                if (ParentMasterReviewSummary == null && currentMasterReview == null) return -1;
                string sql = "";
                if (currentMasterReview == null)
                {
                    currentMasterReview = parentMasterReviewSummary;
                };

                sql += $"Select Count(distinct VisitDate || PtID) from RelCPPRovider where ProviderID={ProviderID} and VisitDate Between '{currentMasterReview.StartDate.ToString("yyyy-MM-dd")}' and '{currentMasterReview.EndDate.ToString("yyyy-MM-dd")}';";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.ExecuteScalar<int>(sql);
                }
            }
        }

        private MasterReviewSummaryVM currentMasterReview { get; set; }
        public void SetCurrentMasterReview(DateTime dt)
        {
            string sql = $"Select * from MasterReviewSummary;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                foreach (var tmpMR in cnn.Query<MasterReviewSummaryVM>(sql).ToList())
                {
                    if (dt >= tmpMR.StartDate && dt <= tmpMR.EndDate)
                    {
                        currentMasterReview = tmpMR;
                        return;
                    }
                }
                currentMasterReview = null;
            }

        }



        public SqlProvider()
        {
        }

        public SqlProvider(MasterReviewSummaryVM mrs)
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



        public static SqlProvider SqlGetProviderByID(int iProviderID)
        {
            string sql = "";
            sql += $"Select * from Providers where ProviderID = '{iProviderID}';"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                SqlProvider p = cnn.QueryFirstOrDefault<SqlProvider>(sql);
                return p;
            }
        }

        public static SqlProvider SqlGetProviderByFullName(string strFullName)
        {
            if (strFullName == "")
                return new SqlProvider("", "", "", "", 3, "");

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
        "FullName=@FullName, " +
        "IsWestSidePod=@IsWestSidePod, " +
        "PersonalNotes=@PersonalNotes " +
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

        public List<SqlCheckpointM> GetCheckPoints(DateTime ReviewDate)
        {
            return new List<SqlCheckpointM>();
        }

        public bool IsCheckPointExpired(SqlCheckpointM cp)
        {
            return true;
        }

        public bool IsReviewDue()
        {
            return true;
        }


    }


}
