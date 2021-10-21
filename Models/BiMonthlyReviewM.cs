using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AI_Note_Review
{
    public class BiMonthlyReviewM : INotifyPropertyChanged
    {

        #region inotify

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Console.WriteLine($"iNotify property {propertyName}");
        }

        #endregion
        public BiMonthlyReviewM()
        {

        }

        /// <summary>
        /// Get a list of SqlVisitReviews for a specific Bimonth review of a specific provider
        /// </summary>
        public ObservableCollection<SqlVisitReview> VisitDates
        {
            get
            {
                //I don't think I use this
                string sql = ""; //$"select Distinct PtID,VisitDate,ReviewDate from RelCPProvider r Where r.ProviderID = {CF.ReviewDocument.ProviderID} and ReviewDate='{.ReviewDate.ToString("yyyy-MM-dd")}' order by r.VisitDate;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return new ObservableCollection<SqlVisitReview>(cnn.Query<SqlVisitReview>(sql).ToList());
                }
            }
        }

        /// <summary>
        /// Get a list of providers for the west side pod
        /// </summary>
        public static List<SqlProvider> MyPeeps
        {
            get
            {
                string sql = "";
                sql += $"Select * from Providers where IsWestSidePod == '1' order by FullName;"; //this part is to get the ID of the newly created phrase
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.Query<SqlProvider>(sql).ToList();
                }
            }
        }

        /// <summary>
        /// Get BiMonth Reviews for specific provider
        /// </summary>
        public ObservableCollection<SqlVisitReview> ReviewDates
        {
            get
            {
                string sql = $"select Distinct ReviewDate from RelCPProvider r Where r.ProviderID = {CF.ClinicNote.ProviderID} order by r.ReviewDate;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return new ObservableCollection<SqlVisitReview>(cnn.Query<SqlVisitReview>(sql).ToList());
                }
            }
        }


    }
}
