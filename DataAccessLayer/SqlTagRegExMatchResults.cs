using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AI_Note_Review
{
    public class SqlTagRegExMatchResults : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public int ResultID { get; set; }
        public string ResultTitle { get; set; }
        public string ResultDescription { get; set; }

        public SqlTagRegExMatchResults()
        {

        }

        /// <summary>
        /// A list of the RegExTypes for populating the ComboBox
        /// </summary>
        public static List<SqlTagRegExType> TagRegExTypes
        {
            get
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from TagRegExTypes;";
                    return cnn.Query<SqlTagRegExType>(sql).ToList();
                }
            }
        }
    }
}
