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
    public class SqlCurrentReviewsSummary
    {
        public DateTime VisitDate { get; set; }
        public int PtID { get; set; }

        public string CheckPointsSummary
        {
            get
            {
                List<SqlRelCPProvider> rlist;
                string sql = $"Select * from RelCPPRovider where PtID={PtID} and VisitDate='{VisitDate.ToString("yyyy-MM-dd")}';";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    rlist = cnn.Query<SqlRelCPProvider>(sql).ToList();
                }


                CF.CurrentDoc.MissedCheckPoints.Clear();
                CF.CurrentDoc.DroppedCheckPoints.Clear();
                CF.CurrentDoc.IrrelaventCP.Clear();
                CF.CurrentDoc.PassedCheckPoints.Clear();
                CF.CurrentDoc.PtID = PtID.ToString();
                CF.CurrentDoc.VisitDate = VisitDate;
                string strReturn = "";
                foreach (SqlRelCPProvider r in rlist)
                {
                    SqlCheckpoint cp = SqlCheckpoint.GetCP(r.CheckPointID);
                    if (r.Comment != "")
                    {
                        cp.CustomComment = r.Comment;
                    }
                    if (r.CheckPointStatus == SqlRelCPProvider.MyCheckPointStates.Pass)
                    {
                        CF.CurrentDoc.PassedCheckPoints.Add(cp);
                    }
                    if (r.CheckPointStatus == SqlRelCPProvider.MyCheckPointStates.Fail)
                    {
                        CF.CurrentDoc.MissedCheckPoints.Add(cp);
                    }
                    if (r.CheckPointStatus == SqlRelCPProvider.MyCheckPointStates.Irrelevant)
                    {
                        CF.CurrentDoc.IrrelaventCP.Add(cp);
                    }
                }

                return CF.CurrentDocToHTML();
            }
        }
    }
}
