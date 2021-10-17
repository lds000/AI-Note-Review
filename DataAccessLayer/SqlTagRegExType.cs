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
    public class SqlTagRegExType
    {

        #region inotify

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Console.WriteLine($"iNotify property {propertyName}");
        }

        #endregion
        // Declare the event
        public int TagRegExTypeID { get; set; }
        public string TagRegExTypeTitle { get; set; }
        public string TagRegExTypeDescription { get; set; }


        /// <summary>
        /// A list of the RegExMatchTypes for populating the Combobox
        /// </summary>
        public static List<SqlTagRegExMatchTypes> TagRegExMatchTypes
        {
            get
            {
                string sql = "Select * from TagRegExMatchTypes;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.Query<SqlTagRegExMatchTypes>(sql).ToList();
                }

            }
        }
    }
}