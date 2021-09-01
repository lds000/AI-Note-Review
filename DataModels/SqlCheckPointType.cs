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
    public class CheckPointType
    {
        public CheckPointType()
        {

        }
        public int CheckPointTypeID { get; set; }
        public int ItemOrder { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
    }
}
