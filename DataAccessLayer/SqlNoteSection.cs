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
    public class SqlNoteSection
    {
        public int SectionID { get; set; }
        public string NoteSectionShortTitle { get; set; }
        public string NoteSectionTitle { get; set; }
        public int ScoreSection { get; set; }
        public static List<SqlNoteSection> NoteSections
        {
            get
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from NoteSections order by SectionOrder;";
                    return cnn.Query<SqlNoteSection>(sql).ToList();
                }
            }

        }

    }

}
