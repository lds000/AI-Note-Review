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
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace AI_Note_Review
{
    #region inotify
    public class ProviderVM : INotifyPropertyChanged
    {
        private ProviderM provider;

        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        private MasterReviewSummaryVM parentMasterReviewSummary;
        public MasterReviewSummaryVM ParentMasterReviewSummary 
        {
            get { return parentMasterReviewSummary; }
            set {
                if (parentMasterReviewSummary != value)
                {
                    parentMasterReviewSummary = value;
                    OnPropertyChanged();
                    OnPropertyChanged("CurrentReviewCount");
                }
            }
        }

        //for dapper
        public ProviderVM()
        {
            if (provider == null)
                provider = new ProviderM();
        }

        public ProviderVM(ProviderM p)
        {
            provider = p;
        }

        public ProviderVM(MasterReviewSummaryVM mrs)
        {
            provider = new ProviderM();
            parentMasterReviewSummary = mrs;
        }

        public ProviderVM(string strFirstName, string strLastName, string strCert, string strHomeClinic, int intReviewInterval, string strFullName)
        {
            if (strFirstName == "")
                return; //I'm getting too many blank providers
            string sql = "";
            if (strFullName == "")
                MessageBox.Show("Creating blank provider!");
            sql = $"INSERT INTO Providers (FirstName,LastName,Cert,HomeClinic,ReviewInterval,FullName, IsWestSidePod) VALUES ('{strFirstName}','{strLastName}','{strCert}','{strHomeClinic}',{intReviewInterval},'{strFullName}', {IsWestSidePod});";
            sql += $"Select * from Providers where FirstName = '{FirstName}' AND LastName = '{LastName}';"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                ProviderVM p = cnn.QueryFirstOrDefault<ProviderVM>(sql);
                ProviderID = p.ProviderID;
            }
        }



        public static ProviderVM SqlGetProviderByID(int iProviderID)
        {
            string sql = "";
            sql += $"Select * from Providers where ProviderID = '{iProviderID}';"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                ProviderVM p = cnn.QueryFirstOrDefault<ProviderVM>(sql);
                return p;
            }
        }

        #region link ProviderVM to providerM
        public int ProviderID {
            get
            {
                return provider.ProviderID;
            }
            set
            {
                provider.ProviderID = value;
                OnPropertyChanged();
            }
        }

        public string FirstName { 
            get { return provider.FirstName; } 
            set {
                if (provider.FirstName != value)
                {
                    provider.FirstName = value;
                    OnPropertyChanged();
                    SaveToDatabase();
                }
            } 
        }

        public string LastName
        {
            get
            {
                return provider.LastName;
            }
            set
            {
                if (provider.LastName != value)
                {
                    provider.LastName = value;
                    OnPropertyChanged();
                    SaveToDatabase();
                }
            }
        }
        public string Cert {
            get
            {
                return provider.Cert;
            }
            set
            {
                provider.Cert = value;
                OnPropertyChanged();
                SaveToDatabase();
            }
        }

        public string HomeClinic 
        {
            get
            {
                return provider.HomeClinic;
            }
            set
            {
                provider.HomeClinic = value;
                OnPropertyChanged();
                SaveToDatabase();
            }
        }

        public int ReviewInterval 
        {
            get
            {
                return provider.ReviewInterval;
            }

            set
            {
                provider.ReviewInterval = value;
                OnPropertyChanged();
                SaveToDatabase();
            }
        }

        public string FullName {
            get
            {
                return provider.FullName;
            }
            set
            {
                if (value != provider.FullName)
                {
                    provider.FullName = value;
                    OnPropertyChanged();
                    SaveToDatabase();
                }
            }
        }

        public bool IsWestSidePod {
            get
            {
                return provider.IsWestSidePod;
            }
            set
            {
                if (provider.IsWestSidePod != value)
                {
                    provider.IsWestSidePod = value;
                    OnPropertyChanged("ProviderColor");
                    OnPropertyChanged();
                    SaveToDatabase();
                }
            }
        }

        public string EMail 
        {
            get
            {
                return provider.EMail;
            }
            set
            {
                if (provider.EMail != value)
                {
                    EMail = value;
                    OnPropertyChanged();
                    SaveToDatabase();
                }
            }
        }

        public string PersonalNotes 
        {
            get
            {
                return provider.PersonalNotes;

            }
            set
            {
                provider.PersonalNotes = value;
                OnPropertyChanged();
                SaveToDatabase();
            }
        }
        #endregion

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

                //sql += $"Select Count(distinct VisitDate || PtID) from RelCPPRovider where ProviderID={ProviderID} and VisitDate Between '{currentMasterReview.StartDate.ToString("yyyy-MM-dd")}' and '{currentMasterReview.EndDate.ToString("yyyy-MM-dd")}';";
                //messed up
                sql += $"Select Count(distinct VisitDate || PtID) from RelCPPRovider where ProviderID={ProviderID} and ReviewDate Between '{currentMasterReview.MyReviewStartDate.ToString("yyyy-MM-dd")}' and '{currentMasterReview.MyReviewEndDate.ToString("yyyy-MM-dd")}';";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.ExecuteScalar<int>(sql);
                }
            }
        }

        public int CurrentNoteDataCount
        {
            get
            {
                string sql = $"Select Count() from Data where ProviderID={ProviderID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteNotesLocation))
                {
                    return cnn.ExecuteScalar<int>(sql);
                }
            }

        }

        public void AddNote(DocumentVM doc)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteNotesLocation))
            {
                NoteDataVM nvm = new NoteDataVM(doc);
            }
            OnPropertyChanged("CurrentNoteDataCount");
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


        public static ProviderVM SqlGetProviderByFullName(string strFullName)
        {
            //if (strFullName == "")                return new ProviderVM("", "", "", "", 3, "");

            string sql = "";
            sql += $"Select * from Providers where FullName = '{strFullName}';"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                ProviderM p = cnn.QueryFirstOrDefault<ProviderM>(sql);
                if (p != null)
                {
                    return new ProviderVM(p);
                }
            }
            if (strFullName == "")
                MessageBox.Show("Creating blank name provider!");
            sql = $"INSERT INTO Providers (FullName) VALUES ('{strFullName}');";
            sql += $"Select * from Providers where FullName = '{strFullName}';"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                ProviderM p = cnn.QueryFirstOrDefault<ProviderM>(sql);
                return new ProviderVM(p);
            }
        }

        public Brush ProviderColor
        {
            get
            {
                if (IsWestSidePod)
                    return System.Windows.Media.Brushes.White;
                return System.Windows.Media.Brushes.Gray;
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
