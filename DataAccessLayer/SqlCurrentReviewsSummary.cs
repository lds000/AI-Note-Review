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

    }
}
