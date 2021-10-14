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
    public class SqlTag: INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Declare the event
        public int TagID { get; set; }
        public string TagText { get; set; }

        public SqlCheckpoint ParentCheckPoint { get; set; }

        public TextBlock TagCount
        {
            get
            {
                TextBlock tb = new TextBlock();
                string sql = $"Select CheckPointTitle from CheckPoints c inner join RelTagCheckPoint rp on rp.CheckPointID == c.CheckPointID where TagID = {TagID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    List<string> tmpStr = cnn.Query<string>(sql).ToList();
                    string strToolTip = "";
                    foreach (string str in tmpStr)
                    {
                        strToolTip += str + "\n";
                    }
                    tb.ToolTip = strToolTip;
                    tb.Text = "(" + tmpStr.Count().ToString() + ")";
                }

                tb.Foreground = Brushes.White;
                return tb;
            }

        }

        public List<SqlTagRegEx> TagRegExs
        {
            get
            {
                string sql = $"Select * from TagRegEx where TargetTag = {TagID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    List<SqlTagRegEx> tmpList = cnn.Query<SqlTagRegEx>(sql, this).ToList();
                    foreach (SqlTagRegEx st in tmpList)
                    {
                        st.ParentTag = this;
                    }
                    return tmpList;
                }
            }

        }

        private ICommand mAddTagRegEx;
        public ICommand AddTagRegExCommand
        {
            get
            {
                if (mAddTagRegEx == null)
                    mAddTagRegEx = new TagRegExAdder();
                return mAddTagRegEx;
            }
            set
            {
                mAddTagRegEx = value;
            }
        }

        private ICommand mRemoveTag;
        public ICommand RemoveTagCommand
        {
            get
            {
                if (mRemoveTag == null)
                    mRemoveTag = new TagRemover();
                return mRemoveTag;
            }
            set
            {
                mRemoveTag = value;
            }
        }


        public void AddSqlTagRegex(SqlTag st)
        {
            SqlTagRegEx srex = new SqlTagRegEx(st.TagID, "Search Text", 1, 1);
            OnPropertyChanged("TagRegExs");
           
        }
        public SqlTag()
        {
        }

        public SqlTag(string strTagText)
        {
            strTagText = strTagText.Replace("'", "''"); //used to avoid errors in titles with ' character
            string sql = "";
            sql = $"INSERT INTO Tags (TagText) VALUES ('{strTagText}');";
            sql += $"Select * from Tags where TagText = '{strTagText}';"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                SqlTag p = cnn.QueryFirstOrDefault<SqlTag>(sql);
                TagID = p.TagID;
                TagText = p.TagText;
            }
        }

        public void RemoveTagRegEx(SqlTagRegEx str)
        {
            string sql = $"Delete from TagRegEx WHERE TagRegExID={str.TagRegExID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
            OnPropertyChanged("TagRegExs");
        }

        public List<SqlTagRegEx> GetTagRegExs()
        {
            string sql = $"Select * from TagRegEx where TargetTag = {TagID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                return cnn.Query<SqlTagRegEx>(sql, this).ToList();
            }

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

        public bool DeleteFromDB()
        {
            MessageBoxResult mr = MessageBox.Show("Are you sure you want to remove this expression? This is permenant and will delete all content.", "Confirm Delete", MessageBoxButton.YesNo);
            if (mr != MessageBoxResult.Yes)
            {
                return false;
            }

            string sql = "Delete from Tags TagID Tags=@TagID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
            return true;
        }
    }

    class TagRegExAdder : ICommand
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
            SqlTag st = parameter as SqlTag;
            st.AddSqlTagRegex(st);
        }
        #endregion
    }

    class TagRemover : ICommand
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
            SqlTag st = parameter as SqlTag;
            st.ParentCheckPoint.RemoveTag(st);
        }
        #endregion
    }

}

