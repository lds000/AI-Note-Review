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

    /*
 * Great example I found online.
 class PersonModel {
    public string Name { get; set; }
  }

class PersonViewModel {
    private PersonModel Person { get; set;}
    public string Name { get { return this.Person.Name; } }
    public bool IsSelected { get; set; } // example of state exposed by view model

    public PersonViewModel(PersonModel person) {
        this.Person = person;
    }
}
*/
    public class SqlTagVM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public DocumentVM ParentDocument { get; set; }

        public SqlTagVM(string strTagText)
        {
            strTagText = strTagText.Replace("'", "''"); //used to avoid errors in titles with ' character
            string sql = "";
            sql = $"INSERT INTO Tags (TagText) VALUES ('{strTagText}');";
            sql += $"Select * from Tags where TagText = '{strTagText}';"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                this.SqlTag = cnn.QueryFirstOrDefault<SqlTagM>(sql);
            }
        }

        public SqlTagVM(SqlTagM st)
        {
            this.SqlTag = st;
        }

        public SqlTagVM()
        {
            this.SqlTag = new SqlTagM();
        }

        private SqlTagM SqlTag { get; set; }
        public int TagID { get { return this.SqlTag.TagID; } set { this.SqlTag.TagID = value; } }
        public string TagText { get { return this.SqlTag.TagText; } set { this.SqlTag.TagText = value;} }



        public void RemoveTagRegEx(SqlTagRegExVM str) 
        {
            this.SqlTag.RemoveTagRegEx(str);
            tagRegExs = null; //reset
            OnPropertyChanged("TagRegExs");
        }
        public void SaveToDB() { this.SqlTag.SaveToDB(); }
        public void DeleteFromDB() { this.SqlTag.DeleteFromDB(); }

        public void AddTagRegEx(SqlCheckpointVM cp)
        {
            SqlTagRegExVM srex = new SqlTagRegExVM(TagID, "Search Text", cp.TargetSection, 1);
            srex.ParentTag = this;
            tagRegExs = null; //reset
            OnPropertyChanged("TagRegExs");
        }

        private SqlTagRegExM.EnumResult? matchResult;
        public SqlTagRegExM.EnumResult MatchResult
        {
            get
            {
                if (matchResult == null)
                {
                    matchResult = SqlTagRegExM.EnumResult.Pass;
                    foreach (var tmpTagRegEx in TagRegExs)
                    {
                        if (tmpTagRegEx.MatchStatus != SqlTagRegExM.EnumResult.Pass)
                        {
                            matchResult = tmpTagRegEx.MatchStatus;
                            break;
                        }
                    }
                }
                return (SqlTagRegExM.EnumResult)matchResult;
            }
        }

        private List<SqlTagRegExVM> tagRegExs;
        public List<SqlTagRegExVM> TagRegExs
        {
            get
            {
                if (tagRegExs == null) tagRegExs = GetTagRegExs();
                return tagRegExs;
            }
        }

        /*
        public List<SqlTagRegExVM> TagRegExs
        {
            get
            {
                string sql = $"Select * from TagRegEx where TargetTag = {TagID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    var tmpList = cnn.Query<SqlTagRegExVM>(sql).ToList();
                    foreach (var tmp in tmpList)
                    {
                        tmp.ParentTag = this;
                    }
                    return tmpList;
                }
            }

        }
        */

        private List<SqlTagRegExVM> GetTagRegExs()
        {
            string sql = $"Select * from TagRegEx where TargetTag = {TagID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                var tmpList = cnn.Query<SqlTagRegExVM>(sql).ToList();
                foreach (var tmp in tmpList)
                {
                    tmp.ParentTag = this;
                    tmp.ParentDocumentVM = ParentDocument;
                }
                return tmpList;
            }

        }

        public void UpdateCPStatus()
        {
            //push this upstream to update any pertinent information to the Parenttag, perhaps an event that bubbles up would be better.
            ParentCheckPoint.UpdateCPStatus();
        }

        public void EditTagText()
        {
            WinEnterText wet = new WinEnterText("Edit Title", TagText);
            wet.ShowDialog();
            if (wet.ReturnValue != null)
            {
                TagText = wet.ReturnValue;
                SaveToDB();
                OnPropertyChanged("TagText");
            }
        }

        /// <summary>
        /// not persisted in data, for VM only
        /// </summary>
        public SqlCheckpointVM ParentCheckPoint { get; set; }

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



        #region Add/Remove TagRegEx Command
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

        private ICommand mEditTagText;
        public ICommand EditTagTextCommand
        {
            get
            {
                if (mEditTagText == null)
                    mEditTagText = new TagTextEditor();
                return mEditTagText;
            }
            set
            {
                mEditTagText = value;
            }
        }

        #endregion


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
            SqlTagVM st = parameter as SqlTagVM;
            st.AddTagRegEx(st.ParentCheckPoint);
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
            SqlTagVM st = parameter as SqlTagVM;
            st.ParentCheckPoint.RemoveTag(st);
        }
        #endregion
    }

    class TagTextEditor : ICommand
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
            SqlTagVM st = parameter as SqlTagVM;
            st.EditTagText();
        }
        #endregion
    }

}
