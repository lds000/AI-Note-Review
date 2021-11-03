using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AI_Note_Review
{
    public class SqlDocumentReviewSummaryM
    {
        public DateTime VisitDate { get; set; }
        public int PtID { get; set; }
        VisitReportM visitReport = new VisitReportM();
        public SqlProvider ParentProvider { get; set; }
        //for dapper
        public SqlDocumentReviewSummaryM()
        {
        }

    }

}



