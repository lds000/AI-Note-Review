using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Note_Review
{
    public class SqlMonthReviewSummary
    {
        public int ProviderID { get; set; }
        public string yearmonth { get; set; }
        public int Reviews { get; set; }

        public ObservableCollection<SqlVisitReview> VisitDates
        {
            get
            {
                string sql = $"select Distinct PtID,VisitDate,ReviewDate from RelCPProvider r Where r.ProviderID = {ProviderID} and VisitDate Like '{yearmonth}%' order by r.VisitDate;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return new ObservableCollection<SqlVisitReview>(cnn.Query<SqlVisitReview>(sql).ToList());
                }
            }
        }

    }
}
