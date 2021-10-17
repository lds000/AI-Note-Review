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
    public class BiMonthlyReviewViewModel
    {
        private BiMonthlyReview biMonthlyReview;
        

        public BiMonthlyReview BiMonthlyReview
        {
            get { return biMonthlyReview; }
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
