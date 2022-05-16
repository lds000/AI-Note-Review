using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace AI_Note_Review
{
    //interface is not being used yet.
    #region Interface
    public interface ICheckPoint
    {
        string CheckPointTitle
        {
            get; set;
        }
        DocumentVM ParentDocument
        {
            get; set;
        }
        SqlTagRegExM.EnumResult? CPStatus
        {
            get;
        }
        SqlICD10SegmentVM ParentSegment
        {
            get; set;
        }
        int CheckPointType
        {
            get; 
            set;
        }
        string Comment
        {
            get; set;
        }
        int ErrorSeverity
        {
            get; set;
        }
        int TargetSection
        {
            get; set;
        }
        int TargetICD10Segment
        {
            get; set;
        }
        string Action
        {
            get; set;
        }
        string Link
        {
            get; set;
        }
        int Expiration
        {
            get; set;
        }
        ObservableCollection<SqlCheckPointImageVM> Images
        {
            get;
        }
    }
    #endregion

    public class SqlCheckpointVM : INotifyPropertyChanged, ICheckPoint
    {
        #region property changed
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChangedSave([CallerMemberName] string name = null)
        {
            if (PropertyChanged != null)
            {
                SaveToDB();
                Console.WriteLine($"Property {name} was saved from SqlCheckpointVM!");
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region constructors
        /// <summary>
        /// parameterless constructor for dapper
        /// </summary>
        public SqlCheckpointVM()
        {
            SqlCheckpoint = new SqlCheckpointM();
        }

        /// <summary>
        /// Create viewmodel based on an existing model
        /// </summary>
        /// <param name="cp"></param>
        public SqlCheckpointVM(SqlCheckpointM cp)
        {
            this.SqlCheckpoint = cp;
        }

        /// <summary>
        /// Create a new checkpoint
        /// </summary>
        /// <param name="strCheckPointTitle">Title, does not have to be unique</param>
        /// <param name="iTargetICD10Segment">TargetSegment, only one</param>
        public SqlCheckpointVM(string strCheckPointTitle, int iTargetICD10Segment)
        {
            this.SqlCheckpoint = new SqlCheckpointM(strCheckPointTitle, iTargetICD10Segment);
        }

        /// <summary>
        /// Load SqlCheckpoint from database
        /// </summary>
        /// <param name="cpID"></param>
        public SqlCheckpointVM(int cpID)
        {
            string sql = $"Select * from CheckPoints WHERE CheckPointID={cpID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                this.SqlCheckpoint = cnn.QueryFirstOrDefault<SqlCheckpointM>(sql);
            }
        }
        #endregion

        /// <summary>
        /// Usuallly assigned once and not changed, unless reassigning to a different segment, used 8 times at least
        /// </summary>
        public SqlICD10SegmentVM ParentSegment
        {
            get; set;
        }

        /// <summary>
        /// All checkpoints have a parent document, used in test for status (male, age, etc...)
        /// </summary>
        public DocumentVM ParentDocument
        {
            get; set;
        }

        /// <summary>
        /// The Model of the MVVM
        /// </summary>
        public SqlCheckpointM SqlCheckpoint
        {
            get; set;
        }

        #region VM mirror of Model
        public int CheckPointID
        {
            get
            {
                return this.SqlCheckpoint.CheckPointID;
            }
            set
            {
                this.SqlCheckpoint.CheckPointID = value;
                OnPropertyChangedSave();
            }
        }
        public string CheckPointTitle
        {
            get
            {
                return this.SqlCheckpoint.CheckPointTitle;
            }
            set
            {
                if (this.SqlCheckpoint.CheckPointTitle != value)
                {
                    this.SqlCheckpoint.CheckPointTitle = value;
                    OnPropertyChangedSave();
                    OnPropertyChanged("CPTitleChanged");
                }
            }
        }
        public int CheckPointType
        {
            get
            {
                return this.SqlCheckpoint.CheckPointType;
            }
            set
            {
                this.SqlCheckpoint.CheckPointType = value;
                OnPropertyChangedSave();
                OnPropertyChanged("StrCheckPointType");
                OnPropertyChanged("ReorderCheckPoints");
            }
        }

        public string Comment
        {
            get
            {
                return this.SqlCheckpoint.Comment;
            }
            set
            {
                this.SqlCheckpoint.Comment = value;
                OnPropertyChangedSave();
                OnPropertyChanged("Comment");
            }
        }
        public int ErrorSeverity
        {
            get
            {
                return this.SqlCheckpoint.ErrorSeverity;
            }
            set
            {
                this.SqlCheckpoint.ErrorSeverity = value;
                OnPropertyChangedSave();
                OnPropertyChanged("ErrorSeverity");
                OnPropertyChanged("ReorderCheckPoints");
                OnPropertyChanged("SeverityColor");
            }
        }
        public int TargetSection
        {
            get
            {
                return this.SqlCheckpoint.TargetSection;
            }
            set
            {
                this.SqlCheckpoint.TargetSection = value;
                this.cPStatus = null;
                OnPropertyChangedSave();
                OnPropertyChanged("CPStatus");
            }
        }
        public int TargetICD10Segment
        {
            get
            {
                return this.SqlCheckpoint.TargetICD10Segment;
            }
            set
            {
                if (this.SqlCheckpoint.TargetICD10Segment == value)
                    return;
                this.SqlCheckpoint.TargetICD10Segment = value;
                OnPropertyChangedSave();
                OnPropertyChanged("ReloadICD10Segments"); //todo: should be "ICD10Segments" for 
            }
        }
        public string Action
        {
            get
            {
                return this.SqlCheckpoint.Action;
            }
            set
            {
                this.SqlCheckpoint.Action = value;
                OnPropertyChangedSave();
            }
        }
        public string Link
        {
            get
            {
                return this.SqlCheckpoint.Link;
            }
            set
            {
                this.SqlCheckpoint.Link = value;
                OnPropertyChangedSave();
            }
        }
        public int Expiration
        {
            get
            {
                return this.SqlCheckpoint.Expiration;
            }
            set
            {
                this.SqlCheckpoint.Expiration = value;
                OnPropertyChangedSave();
            }
        }
        public ObservableCollection<SqlCheckPointImageVM> Images
        {
            get
            {
                string sql = $"select * from CheckPointImages where CheckPointID = @CheckPointID;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    var tmpList = cnn.Query<SqlCheckPointImageVM>(sql, this).ToList();
                    foreach (var tmpEl in tmpList)
                    {
                        tmpEl.ParentCheckPoint = this;
                    }
                    return tmpList.ToObservableCollection();
                }
            }
        }
        #endregion

        public Brush SeverityColor
        {
            get
            {
                if (this.ErrorSeverity >= 7)
                    return Brushes.Red;
                if (this.ErrorSeverity >= 5)
                    return Brushes.Yellow;
                if (this.ErrorSeverity >= 1)
                    return Brushes.Green;
                return Brushes.White;

            }
        }

        public Brush CheckPointBackGroundColor
        {
            get
            {
                if (this.Tags.Count == 0)
                    return Brushes.Red;
                return Brushes.Black;

            }
        }

        

        public Visibility OveriddenVisibility
        {
            get
            {
                if (cPoverideStatus != null)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Holds a status that is manually set, ie the reviewer wants the status to be set to pass instead of miss.  This will overide any checking.
        /// </summary>
        private SqlTagRegExM.EnumResult? cPoverideStatus;
        private SqlTagRegExM.EnumResult? cPStatus;
        /// <summary>
        /// The current status of the checkpoint: pass, miss, hide
        /// </summary>
        public SqlTagRegExM.EnumResult? CPStatus
        {
            get
            {
                //check if I have manually overidden the checkpoint and keep that assignment, since I am the genius here, not the program.
                if (cPoverideStatus != null)
                {
                    cPStatus = (SqlTagRegExM.EnumResult)cPoverideStatus;
                    IncludeCheckpoint = false;
                    if (cPStatus == SqlTagRegExM.EnumResult.Miss || cPStatus == SqlTagRegExM.EnumResult.Pass) //include checkpoint is linked to the checkbox button
                    {
                        IncludeCheckpoint = true;
                    }
                    return (SqlTagRegExM.EnumResult)cPoverideStatus;
                }

                //check if status is null, this means the checkpoint needs to be evaluated, otherwise return the already evaluated checkpoint (much faster) to avoid checking status more than once
                if (cPStatus == null)
                {
                    //this should only be run once for each checkpoint every time the report is opened. Unless it is edited and set back to null;
                    //default is pass.
                    cPStatus = SqlTagRegExM.EnumResult.Pass;
                    
                    //Now iterate through each tag, checking for pass status, when pass status met then iteration breaks.
                    //todo: consider adding "continue" status
                    foreach (SqlTagVM tagCurrentTag in Tags)
                    {
                        if (tagCurrentTag.MatchResult != SqlTagRegExM.EnumResult.Pass) //If result is pass, do not break, continue to next Tag until all tags process. Later change this to continue
                        {
                            cPStatus = tagCurrentTag.MatchResult;
                            break; //miss or hide condition met, no need to proceed to additional tags.
                        }
                    }
                }

                if (cPStatus == SqlTagRegExM.EnumResult.Miss || cPStatus == SqlTagRegExM.EnumResult.Pass) //include checkpoint is linked to the checkbox button
                {
                    IncludeCheckpoint = true;
                }
                return (SqlTagRegExM.EnumResult)cPStatus;
            }
            set
            {
                cPStatus = value;
                OnPropertyChanged("CPStatus");
            }
        }

        /// <summary>
        /// Function call to overide status
        /// </summary>
        /// <param name="newStatus">Status to set</param>
        public void OverideStatus(SqlTagRegExM.EnumResult newStatus)
        {
            //if (SqlCheckpoint.CheckPointID == 28)
            //    Console.WriteLine($"Changing ID for {SqlCheckpoint.CheckPointTitle}");
            cPoverideStatus = newStatus;
            OnPropertyChanged("OveriddenVisibility");
            OnPropertyChanged("RecalculateCPStatus");
        }

        /// <summary>
        /// Holds the current checkpoints Yes/No SqlRegex's
        /// </summary>
        public Dictionary<int, bool> YesNoSqlRegExIndex = new Dictionary<int, bool>();

        public string StrCheckPointType
        {
            get
            {
                return (from c in ParentSegment.CheckPointTypes where c.CheckPointTypeID == CheckPointType select c).FirstOrDefault().Title;
            }
        }

        public void SaveToDB()
        {
            this.SqlCheckpoint.SaveToDB();
        }

        public void DeleteFromDB()
        {
            this.SqlCheckpoint.DeleteFromDB();
            OnPropertyChanged("CheckPointCount");
        }

        public void UpdateImages()
        {
            OnPropertyChanged("Images");
        }

        public void AddImageFromClipBoard()
        {
            this.SqlCheckpoint.AddImageFromClipBoard();
            OnPropertyChanged("Images");
        }

        /// <summary>
        /// Get the tags associated with the checkpoint
        /// </summary>
        public List<SqlTagVM> tags;

        public List<SqlTagVM> Tags
        {
            get
            {
                if (tags == null)
                {
                    string sql = $"select t.TagID, TagText from Tags t inner join RelTagCheckPoint relTC on t.TagID = relTC.TagID where CheckPointID = {CheckPointID} order by RelTagCheckPointID;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        var tmpList = cnn.Query<SqlTagVM>(sql, this).ToList();
                        foreach (var tmp in tmpList)
                        {
                            tmp.ParentCheckPoint = this;
                            tmp.ParentDocument = ParentDocument; //maybe move this into the constructor
                            tmp.PropertyChanged += Tmp_PropertyChanged;
                        }
                        tags = tmpList;
                    }
                }
                return tags;
            }
        }

        private void Tmp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RecalculateCPStatus")
            {
                CPStatus = null;
                OnPropertyChanged("RecalculateCPStatus");
            }
        }

        public void AddTag(SqlTagVM tg)
        {
            string sql = "";
            sql = $"INSERT INTO RelTagCheckPoint (TagID, CheckPointID) VALUES ({tg.TagID},{CheckPointID});";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
            tags = null; //reset tags.
            OnPropertyChanged("Tags");
            OnPropertyChanged("CheckPointBackGroundColor");
        }

        public void RemoveTag(SqlTagVM st)
        {
            string sql = $"Delete From RelTagCheckPoint where CheckPointID = {CheckPointID} AND TagID = {st.TagID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
            tags = null; //reset tags.
            OnPropertyChanged("Tags");
        }

        private bool includeCheckpoint;

        /// <summary>
        /// A boolean indicating if the checkpoint will be included in the report.  This is true by default for all missed checkpoint, false for for all passed check point. It is not saved in the database.
        /// </summary>
        public bool IncludeCheckpoint
        {
            get
            {
                return includeCheckpoint;
            }
            set
            {
                includeCheckpoint = value;
                OnPropertyChanged();
            }
        }

        public List<SqlCheckpointM> GetCPsFromSegment(int SegmentID)
        {
            string sql = $"Select * from CheckPoints where TargetICD10Segment == {SegmentID}";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                return cnn.Query<SqlCheckpointM>(sql).ToList();
            }

        }

        /// <summary>
        /// A personal comment added to a checkpoint that is saved in the database under the commit (not checkpoint model).
        /// </summary>
        public string CustomComment
        {
            get; set;
        }

        public int CheckPointTypeOrder
        {
            get
            {
                var t = (from c in ParentSegment.CheckPointTypes where c.CheckPointTypeID == CheckPointType select c).FirstOrDefault();
                return t.ItemOrder;
            }
        }





        #region Update Command

        private ICommand mUpdateCP;
        public ICommand UdateCPCommand
        {
            get
            {
                if (mUpdateCP == null)
                    mUpdateCP = new CPUpdater();
                return mUpdateCP;
            }
            set
            {
                mUpdateCP = value;
            }
        }
        #endregion



        #region EditCheckPoint Command

        private ICommand mEditCheckPoint;
        public ICommand EditCheckPointCommand
        {
            get
            {
                if (mEditCheckPoint == null)
                    mEditCheckPoint = new EditCheckPoint();
                return mEditCheckPoint;
            }
            set
            {
                mEditCheckPoint = value;
            }
        }
        #endregion

        #region AddTag Command
        private ICommand mAddTag;
        public ICommand AddTagCommand
        {
            get
            {
                if (mAddTag == null)
                    mAddTag = new TagAdder();
                return mAddTag;
            }
            set
            {
                mAddTag = value;
            }
        }
        #endregion

        #region AddImageFromClipBoard command
        private ICommand mAddImage;
        public ICommand AddImageFromClipBoardCommand
        {
            get
            {
                if (mAddImage == null)
                    mAddImage = new AddImageFromClipBoard();
                return mAddImage;
            }
            set
            {
                mAddImage = value;
            }
        }
        #endregion

        #region ShowCPRichText Command

        private ICommand mShowCPRichText;
        public ICommand ShowCPRichTextCommand
        {
            get
            {
                if (mShowCPRichText == null)
                    mShowCPRichText = new ShowCPRichText();
                return mShowCPRichText;
            }
            set
            {
                mShowCPRichText = value;
            }
        }
        #endregion

        #region MoveMissed Command
        private ICommand mMoveMissed;
        public ICommand MoveMissedCommand
        {
            get
            {
                if (mMoveMissed == null)
                    mMoveMissed = new MoveMissed();
                return mMoveMissed;
            }
            set
            {
                mMoveMissed = value;
            }
        }
        #endregion
        #region MovePassed Command
        private ICommand mMovePassed;
        public ICommand MovePassedCommand
        {
            get
            {
                if (mMovePassed == null)
                    mMovePassed = new MovePassed();
                return mMovePassed;
            }
            set
            {
                mMovePassed = value;
            }
        }
        #endregion
        #region MoveMissed Command
        private ICommand mMoveDropped;
        public ICommand MoveDroppedCommand
        {
            get
            {
                if (mMoveDropped == null)
                    mMoveDropped = new MoveDropped();
                return mMoveDropped;
            }
            set
            {
                mMoveDropped = value;
            }
        }
        #endregion

        //FollowLink
        private ICommand mFollowLink;
        public ICommand FollowLinkCommand
        {
            get
            {
                if (mFollowLink == null)
                    mFollowLink = new FollowLink();
                return mFollowLink;
            }
            set
            {
                mFollowLink = value;
            }
        }

        //RemoveCheckPoint
        private ICommand mRemoveCheckPoint;
        public ICommand RemoveCheckPointCommand
        {
            get
            {
                if (mRemoveCheckPoint == null)
                    mRemoveCheckPoint = new RemoveCheckPoint();
                return mRemoveCheckPoint;
            }
            set
            {
                mRemoveCheckPoint = value;
            }
        }
    }

    class TagAdder : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            SqlCheckpointVM CurrentCheckpoint = parameter as SqlCheckpointVM;
            if (CurrentCheckpoint == null)
                return;
            string strSuggest = "#";
            if (CurrentCheckpoint.CheckPointType == 1)
                strSuggest = "#Query";
            if (CurrentCheckpoint.CheckPointType == 2)
                strSuggest = "#Exam";
            if (CurrentCheckpoint.CheckPointType == 3)
                strSuggest = "#Lab";
            if (CurrentCheckpoint.CheckPointType == 4)
                strSuggest = "#Imaging";
            if (CurrentCheckpoint.CheckPointType == 5)
                strSuggest = "#Condition";
            if (CurrentCheckpoint.CheckPointType == 6)
                strSuggest = "#CurrentMed";
            if (CurrentCheckpoint.CheckPointType == 7)
                strSuggest = "#Edu";
            if (CurrentCheckpoint.CheckPointType == 8)
                strSuggest = "#Exam";
            if (CurrentCheckpoint.CheckPointType == 9)
                strSuggest = "#CurrentMed";
            if (CurrentCheckpoint.CheckPointType == 10)
                strSuggest = "#Demographic";
            if (CurrentCheckpoint.CheckPointType == 11)
                strSuggest = "#HPI";
            if (CurrentCheckpoint.CheckPointType == 12)
                strSuggest = "#Vitals";
            if (CurrentCheckpoint.CheckPointType == 13)
                strSuggest = "#Rx";
            if (CurrentCheckpoint.CheckPointType == 14)
                strSuggest = "#Refer";
            if (CurrentCheckpoint.CheckPointType == 15)
                strSuggest = "#BEERS";
            //WinEnterText wet = new WinEnterText("Please enter a unique (not previously used) name for the new tag.", strSuggest, 200);
            //wet.strExclusions = SqlLiteDataAccess.GetAllTags();
            //wet.Owner = this;
            //wet.ShowDialog();

            WinAddTag wat = new WinAddTag();
            wat.tbSearch.Text = strSuggest;
            wat.ShowDialog();

            if (wat.ReturnValue != null)
            {
                SqlTagVM tg = SqlLiteDataAccess.GetTags(wat.ReturnValue).FirstOrDefault();
                if (tg == null)
                    tg = new SqlTagVM(wat.ReturnValue);
                tg.ParentCheckPoint = CurrentCheckpoint;
                CurrentCheckpoint.AddTag(tg);

                //SqlTagRegEx srex = new SqlTagRegEx(tg.TagID, "Search Text", CurrentCheckpoint.TargetSection, 1);
            }
        }
        #endregion
    }

    #region Commands
    //EditCheckPoint Command
    class EditCheckPoint : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            SqlCheckpointVM cp = parameter as SqlCheckpointVM;
            //set current CP to this one.
            cp.ParentSegment.ParentReport.SelectedCheckPoint = cp;
            WinCheckPointEditor wce = new WinCheckPointEditor(cp);
            wce.DataContext = cp.ParentSegment.ParentReport;
            wce.Show();
        }
        #endregion
    }

    //ShowCPRichText Command
    class ShowCPRichText : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            SqlCheckpointVM cp = parameter as SqlCheckpointVM;
            //set the selectedCP to this one.
            cp.ParentSegment.ParentReport.SelectedCheckPoint = cp;
            WinShowCheckPointRichText scp = new WinShowCheckPointRichText();
            scp.DataContext = cp.ParentSegment.ParentReport;
            //scp.ImChanged += Scp_AddMe;
            scp.Show();
            //updown = scp.UpDownPressed;
        }
        #endregion
    }

    class CPUpdater : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            SqlCheckpointM CP = parameter as SqlCheckpointM;
            CP.SaveToDB();
        }
        #endregion
    }


    class AddImageFromClipBoard : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            SqlCheckpointVM CP = parameter as SqlCheckpointVM;
            CP.AddImageFromClipBoard();
        }
        #endregion
    }

    class MoveMissed : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion

        public void Execute(object parameter)
        {
            SqlCheckpointVM CP = parameter as SqlCheckpointVM;
            CP.OverideStatus(SqlTagRegExM.EnumResult.Miss);
        }
    }

    class MovePassed : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion

        public void Execute(object parameter)
        {
            SqlCheckpointVM CP = parameter as SqlCheckpointVM;
            CP.OverideStatus(SqlTagRegExM.EnumResult.Pass);
        }
    }

    class MoveDropped : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion

        public void Execute(object parameter)
        {
            SqlCheckpointVM CP = parameter as SqlCheckpointVM;
            CP.OverideStatus(SqlTagRegExM.EnumResult.Hide);
        }
    }

    /// <summary>
    /// Follow a link
    /// </summary>
    class FollowLink : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion
        public void Execute(object parameter)
        {
            SqlCheckpointVM CP = parameter as SqlCheckpointVM;
            if (!string.IsNullOrWhiteSpace(CP.Link))
                System.Diagnostics.Process.Start(CP.Link);

        }

    }

    /// <summary>
    /// CreateIndex
    /// </summary>
    class RemoveCheckPoint : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion
        public void Execute(object parameter)
        {
            SqlCheckpointVM cp = parameter as SqlCheckpointVM;
            cp.DeleteFromDB();
            cp.ParentSegment.UpdateCheckPoints();
        }
    }
    #endregion
}
