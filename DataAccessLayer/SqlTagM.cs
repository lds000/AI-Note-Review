using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AI_Note_Review
{
    public class SqlTagM: INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Database Rows
        public int TagID { get; set; }
        public string TagText { get; set; }

        //Constructor
        public SqlTagM()
        {
        }

        public void RemoveTagRegEx(SqlTagRegExVM str)
        {
            MessageBoxResult mr = MessageBox.Show("Are you sure you want to delete this search term from the database? This is permenant.", "Confirm Delete", MessageBoxButton.YesNo);
            if (mr != MessageBoxResult.Yes)
            {
                return;
            }

            string sql = $"Delete from TagRegEx WHERE TagRegExID={str.TagRegExID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
            OnPropertyChanged("TagRegExs");
        }



        public void SaveToDB()
        {
            string sql = "UPDATE Tags SET " +
                    "TagID=@TagID, " +
                    "TagText=@TagText " +
                    "WHERE TagID=@TagID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
        }

        public void DeleteFromDB()
        {
            MessageBoxResult mr = MessageBox.Show("Are you sure you want to remove this expression? This is permenant and will delete all content.", "Confirm Delete", MessageBoxButton.YesNo);
            if (mr != MessageBoxResult.Yes)
            {
                return;
            }

            string sql = "Delete from Tags TagID Tags=@TagID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
        }
    }



}

