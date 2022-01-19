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
using System.Windows.Input;

namespace AI_Note_Review
{
    public class SqlTagRegExM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private EnumMatch tagRegExMatchType;
        private EnumResult tagRegExMatchResult;
        private EnumResult tagRegExMatchNoResult;

        // Declare the event
        public int TagRegExID { get; set; }
        public int TargetTag { get; set; }

        public int TargetSection { get; set; }



        public string RegExText { get; set; }
        public int TagRegExType { get; set; }

        //1 pass, 2 Hide, 3 Miss, 4 Continue
        public enum EnumResult { Pass = 1, Hide = 2, Miss = 3, Continue = 4 }

        //1 any, 2 all, 3 None, 4 Ask, 5 Regex
        public enum EnumMatch { Any = 1, All = 2, None = 3, Ask = 4, Regex = 5 }

        public EnumMatch TagRegExMatchType { get => tagRegExMatchType; set => tagRegExMatchType = value; }
        public EnumResult TagRegExMatchResult { get => tagRegExMatchResult; set => tagRegExMatchResult = value; }
        public EnumResult TagRegExMatchNoResult { get => tagRegExMatchNoResult; set => tagRegExMatchNoResult = value; }

        public int iTagRegExMatchType { get => (int)tagRegExMatchType; set => tagRegExMatchType = (EnumMatch)value; }
        public int iTagRegExMatchResult { get => (int)tagRegExMatchResult; set => tagRegExMatchResult = (EnumResult)value; }
        public int iTagRegExMatchNoResult { get => (int)tagRegExMatchNoResult; set => tagRegExMatchNoResult = (EnumResult)value; }
        public double? MinAge { get; set; }
        public double MaxAge { get; set; }
        public bool Male { get; set; }
        public bool Female { get; set; }


        public SqlTagRegExM()
        {
        }

        public void SaveToDB()
        {
            string sql = "UPDATE TagRegEx SET " +
                    "TagRegExID=@TagRegExID, " +
                    "TargetTag=@TargetTag, " +
                    "RegExText=@RegExText, " +
                    "TagRegExType=@TagRegExType, " +
                    "TagRegExMatchType=@TagRegExMatchType, " +
                    "TagRegExMatchResult=@TagRegExMatchResult, " +
                    "TagRegExMatchNoResult=@TagRegExMatchNoResult, " +
                    "TargetSection=@TargetSection, " +
                    "MinAge=@MinAge," +
                    "MaxAge=@MaxAge," +
                    "Male=@Male," +
                    "Female=@Female " +
                    "WHERE TagRegExID=@TagRegExID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
        }

        public bool DeleteFromDB()
        {
            string sql = "Delete from TagRegEx WHERE TagRegExID=@TagRegExID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
            return true;
        }
    }


}

