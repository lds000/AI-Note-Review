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
    public class SqlDocumentReviewSummaryM
    {
        public DateTime VisitDate { get; set; }
        public int PtID { get; set; }
        VisitReportM rpt = new VisitReportM();

        public string CheckPointsSummaryHTML
        {
            get
            {
                ReportToHtmlVM r = new ReportToHtmlVM(VisitDate, PtID);
                return r.CheckPointsSummaryHTML;
            }
        }

    }
}



