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
    public class SqlVisitReview
    {
        public int PtID { get; set; }
        public DateTime VisitDate { get; set; }
        public DateTime ReviewDate { get; set; }

        public List<SqlCheckpointM> RelatedCheckPoints
        {
            get 
            {
                string sql = $"select * from CheckPoints cp inner join RelCPPRovider relcp on relcp.CheckPointID = cp.CheckPointID where relcp.VisitDate = '{VisitDate.ToString("yyyy-MM-dd")}' and PtID = {PtID}";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.Query<SqlCheckpointM>(sql).ToList();
                }
            }
        }
        public List<SqlRelCPProvider> RelCPProviders
        {
            get
            {
                string sql = $"select * from RelCPPRovider relcp inner join CheckPoints cp on relcp.CheckPointID = cp.CheckPointID where relcp.VisitDate = '{VisitDate.ToString("yyyy-MM-dd")}' and PtID = {PtID} order by relcp.CheckPointStatus;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.Query<SqlRelCPProvider>(sql).ToList();
                }
            }
        }

    }
}
