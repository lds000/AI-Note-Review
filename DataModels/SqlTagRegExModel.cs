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
    public class SqlTagRegEx : INotifyPropertyChanged
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

        public SqlTag ParentTag { get; set; }
        public int TargetSection { get; set; }

        public string TargetSectionTitle
        {
            get
            {
                string sql = $"Select NoteSectionTitle from NoteSections WHERE SectionID={TargetSection};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.ExecuteScalar<string>(sql);
                }
            }
        }

        public string RegExText { get; set; }
        public int TagRegExType { get; set; }

        //1 pass, 2 Hide, 3 Miss
        public enum EnumResult { Pass = 1, Hide = 2, Miss = 3 }

        //1 any, 2 all, 3 None, 4 Ask
        public enum EnumMatch { Any = 1, All = 2, None = 3, Ask = 4 }

        public EnumMatch TagRegExMatchType { get => tagRegExMatchType; set => tagRegExMatchType = value; }
        public EnumResult TagRegExMatchResult { get => tagRegExMatchResult; set => tagRegExMatchResult = value; }
        public EnumResult TagRegExMatchNoResult { get => tagRegExMatchNoResult; set => tagRegExMatchNoResult = value; }

        public int iTagRegExMatchType { get => (int)tagRegExMatchType; set => tagRegExMatchType = (EnumMatch) value; }
        public int iTagRegExMatchResult { get => (int)tagRegExMatchResult; set => tagRegExMatchResult = (EnumResult)value; }
        public int iTagRegExMatchNoResult { get => (int)tagRegExMatchNoResult; set => tagRegExMatchNoResult = (EnumResult)value; }

        public double MinAge { get; set; }
        public double MaxAge { get; set; }
        public bool Male { get; set; }
        public bool Female { get; set; }

        private ICommand mDeleteTagRegEx;
        public ICommand DeleteTagRegExCommand
        {
            get
            {
                if (mDeleteTagRegEx == null)
                    mDeleteTagRegEx = new DeleteTagRegEx();
                return mDeleteTagRegEx;
            }
            set
            {
                mDeleteTagRegEx = value;
            }
        }

        public SqlTagRegEx()
        {
        }


        public SqlTagRegEx(int intTargetTag, string strRegExText, int intTargetSection, int iTagRegExType = 1, int iTagRegExMatchType = 0, int iTagRegExMatchResult = 0, double dMinAge = 0, double dMaxAge = 99, bool bMale = true, bool bFemale = true)
        {
            strRegExText = strRegExText.Replace("'", "''"); //used to avoid errors in titles with ' character
            string sql = "";
            sql = $"INSERT INTO TagRegEx (TargetTag, RegExText, TargetSection, TagRegExType, TagRegExMatchType, TagRegExMatchResult, MinAge, MaxAge, Male, Female) VALUES ({intTargetTag}, '{strRegExText}', {intTargetSection}, {iTagRegExType}, {iTagRegExMatchType}, {iTagRegExMatchResult}, {dMinAge},{dMaxAge},{bMale},{bFemale});";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
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
    class DeleteTagRegEx : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            SqlTagRegEx t = parameter as SqlTagRegEx;
            t.ParentTag.RemoveTagRegEx(t);
        }
        #endregion
    }

}

