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
        public static List<SqlNoteSection> NoteSections { get; set; }
        public static List<SqlICD10Segment> ICD10Segments { get; set; }
        public static List<SqlCheckpoint> Checkpoints { get; set; }
    }
}
