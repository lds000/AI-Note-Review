using Dapper;
using GalaSoft.MvvmLight.Messaging;
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

    public class SqlTagVM : INotifyPropertyChanged
    {
        // Declare the event
        #region Inotify
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        /// <summary>
        /// Used to determine age, sex, etc of patient and also get NoteSectionText
        /// </summary>
        public DocumentVM ParentDocument { get; set; }

        /// <summary>
        /// Create a Tag
        /// </summary>
        /// <param name="strTagText"></param>
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


        /// <summary>
        /// Parameterless constructor for Dapper
        /// </summary>
        public SqlTagVM()
        {
            this.SqlTag = new SqlTagM();
        }

        private SqlTagM SqlTag { get; set; }


        #region Mirror Model properties
        public int TagID { 
            get { return this.SqlTag.TagID; } 
            set { 
                this.SqlTag.TagID = value;
            }
        }
        public string TagText { 
            get {
                return this.SqlTag.TagText; 
            } 
            set 
            {
                this.SqlTag.TagText = value;
                OnPropertyChanged();
                SaveToDB();
            } 
        }
        #endregion


        public void RemoveTagRegEx(SqlTagRegExVM str) 
        {
            this.SqlTag.RemoveTagRegEx(str);
            TagRegExs = null; //reset
            OnPropertyChanged("RecalculateCPStatus");
        }

        public void SaveToDB() { this.SqlTag.SaveToDB(); }

        public void DeleteFromDB() 
        { 
            this.SqlTag.DeleteFromDB();
            TagRegExs = null; //reset
            OnPropertyChanged("RecalculateCPStatus");
        }

        /// <summary>
        /// Add new TagRegEx to checkpoint
        /// </summary>
        /// <param name="cp">The checkpoint to add the TagRexEx to</param>
        public void AddTagRegEx(SqlCheckpointVM cp)
        {
            SqlTagRegExVM srex = new SqlTagRegExVM(TagID, "Search Text", cp.TargetSection, 1);
            srex.ParentTag = this;
            TagRegExs = null; //reset
            OnPropertyChanged("RecalculateCPStatus");
        }

        private SqlTagRegExM.EnumResult? matchResult;
        /// <summary>
        /// The match result of the Tag (Pass, Fail, Drop)
        /// </summary>
        public SqlTagRegExM.EnumResult? MatchResult
        {
            get
            {
                if (matchResult == null)
                {
                    matchResult = SqlTagRegExM.EnumResult.Pass;
                    foreach (var tmpTagRegEx in TagRegExs)
                    {
                        if (tmpTagRegEx.MatchStatus != SqlTagRegExM.EnumResult.Pass) //if any RegEx fails then all fail and no need to continue
                        {
                            matchResult = tmpTagRegEx.MatchStatus;
                            break;
                        }
                    }
                }
                return (SqlTagRegExM.EnumResult)matchResult;
            }
            set
            {
                if (matchResult != value)
                {
                    matchResult = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<SqlTagRegExVM> tagRegExs;
        /// <summary>
        /// Collection of TagRegExs that belong to this tag
        /// </summary>
        public List<SqlTagRegExVM> TagRegExs
        {
            get
            {
                if (tagRegExs == null)
                {
                    string sql = $"Select * from TagRegEx where TargetTag = {TagID};";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        var tmpList = cnn.Query<SqlTagRegExVM>(sql).ToList();
                        foreach (var tmp in tmpList)
                        {
                            tmp.ParentTag = this;
                            tmp.PropertyChanged += RegExVM_PropertyChanged;
                        }
                        tagRegExs = tmpList;
                    }
                }
                return tagRegExs;
            }
            set
            {
                if (tagRegExs != value)
                {
                    tagRegExs = value;
                    OnPropertyChanged();
                }
            }
        }

        private void RegExVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RecalculateCPStatus")
            {
                MatchResult = null;
                OnPropertyChanged("RecalculateCPStatus"); //Bubble event up
            }
        }

        /// <summary>
        /// Change the title of the Tag
        /// </summary>
        public void EditTagText()
        {
            WinEnterText wet = new WinEnterText("Edit Title", TagText);
            wet.ShowDialog();
            if (wet.ReturnValue != null)
            {
                TagText = wet.ReturnValue;
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
