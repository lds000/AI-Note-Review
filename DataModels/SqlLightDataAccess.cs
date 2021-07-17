using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Note_Review
{
    public static class SqlLightDataAccess
    {
        public static string SQLiteDBLocation { get; set; }
        public static List<MySqlNoteSection> NoteSections { get; set; }

    }
}
