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
using System.Windows.Media.Imaging;


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
    public class SqlCheckpointVM : INotifyPropertyChanged
    {
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
        public SqlCheckpointVM()
        {
        }

        public SqlCheckpointVM(SqlCheckpointM cp)
        {
            this.SqlCheckpoint = cp;
        }

        public SqlCheckpointVM(string strCheckPointTitle, int iTargetICD10Segment)
        {
            this.SqlCheckpoint = new SqlCheckpointM(strCheckPointTitle, iTargetICD10Segment);
        }

        public SqlCheckpointVM(int cpID)
        {
            string sql = $"Select * from CheckPoints WHERE CheckPointID={cpID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                this.SqlCheckpoint = cnn.QueryFirstOrDefault<SqlCheckpointM>(sql);
            }
        }

        public SqlICD10SegmentVM ParentSegment { get; set; }
        public DocumentVM ParentDocument { get; set; }
        public SqlCheckpointM SqlCheckpoint { get; set; }
        public int CheckPointID { get { return this.SqlCheckpoint.CheckPointID; } set { this.SqlCheckpoint.CheckPointID = value; OnPropertyChangedSave(); } }
        public string CheckPointTitle { get { return this.SqlCheckpoint.CheckPointTitle; } set { this.SqlCheckpoint.CheckPointTitle = value; OnPropertyChangedSave(); } }
        public int CheckPointType { get { return this.SqlCheckpoint.CheckPointType; } set { this.SqlCheckpoint.CheckPointType = value; OnPropertyChangedSave(); } }
        public string Comment { get { return this.SqlCheckpoint.Comment; } set { this.SqlCheckpoint.Comment = value;
                OnPropertyChangedSave(); 
            } }
        public int ErrorSeverity { get { return this.SqlCheckpoint.ErrorSeverity; } set { this.SqlCheckpoint.ErrorSeverity = value; OnPropertyChangedSave(); } }
        public int TargetSection { get { return this.SqlCheckpoint.TargetSection; } set { this.SqlCheckpoint.TargetSection = value; } }
        public int TargetICD10Segment { get { return this.SqlCheckpoint.TargetICD10Segment; } set { this.SqlCheckpoint.TargetICD10Segment = value; OnPropertyChangedSave(); } }
        public string Action { get { return this.SqlCheckpoint.Action; } set { this.SqlCheckpoint.Action = value; OnPropertyChangedSave(); } }
        public string Link { get { return this.SqlCheckpoint.Link; } set { this.SqlCheckpoint.Link = value; OnPropertyChangedSave(); } }
        public int Expiration { get { return this.SqlCheckpoint.Expiration; } set { this.SqlCheckpoint.Expiration = value; OnPropertyChangedSave(); } }
        public ObservableCollection<SqlCheckPointImageVM> Images { get { return this.SqlCheckpoint.Images; } }
        public string[] NoteSectionText { get { return ParentDocument.NoteSectionText; } }

        private SqlTagRegExM.EnumResult? cPoverideStatus;
        private SqlTagRegExM.EnumResult? cPStatus;
        public SqlTagRegExM.EnumResult CPStatus
        {
            get
            {
                if (cPStatus == null) 
                {
                    //this should only be run once for each checkpoint every time the report is opened.
                    //check if I have manually overidden the checkpoint and keep that assignment, since I am the genius here, not the program.
                    if (cPoverideStatus != null) cPStatus = (SqlTagRegExM.EnumResult)cPoverideStatus;
                    OnPropertyChanged("CPStatus");

                    SqlTagRegExM.EnumResult trTagResult = SqlTagRegExM.EnumResult.Pass;
                    if (CheckPointTitle.Contains("Augmentin XR"))
                    {
                        //use this for testing...
                    }
                    foreach (SqlTagVM tagCurrentTag in GetTags())
                    {
                        SqlTagRegExM.EnumResult trCurrentTagResult;
                        List<SqlTagRegExVM> tmpTagRegExs = tagCurrentTag.GetTagRegExs();
                        trCurrentTagResult = CheckTagRegExs(tmpTagRegExs);

                        if (trCurrentTagResult != SqlTagRegExM.EnumResult.Pass)
                        {
                            //tag fails, no match.
                            trTagResult = trCurrentTagResult;
                            break; //if the first tag does not qualify, then do not proceed to the next tag.
                        }
                        //report.DocumentTags.Add(tagCurrentTag.TagText); Don't I need this.
                    }
                    cPStatus = trTagResult;                    
                }
                if (cPStatus == SqlTagRegExM.EnumResult.Miss || cPStatus == SqlTagRegExM.EnumResult.Pass) //include checkpoint is linked to the checkbox button
                {
                    IncludeCheckpoint = true;
                    OnPropertyChanged("IncludeCheckpoint");
                }
                return (SqlTagRegExM.EnumResult)cPStatus;
            }
            set
            {
                cPStatus = value;
            }
        }

        public void CalculateStatus()
        {
        }

        public void UpdateCPStatus()
        {
            //push this upstream to report to update any pertinent information to the Parenttag, perhaps an event that bubbles up would be better.
            cPStatus = null;
            Console.WriteLine("Setting CPStatus to null on SqlCheckpointVM");
            ParentSegment.UpdateCPs();
        }

        /// <summary>
        /// Holds the current review's Yes/No SqlRegex's
        /// </summary>
        private Dictionary<int, bool> YesNoSqlRegExIndex = new Dictionary<int, bool>();


        /// <summary>
        /// Run the SqlTagRegExes of a tag and return as result, this is the brains of the whole operation.
        /// </summary>
        /// <param name="tmpTagRegExs"></param>
        /// <returns></returns>
        private SqlTagRegExM.EnumResult CheckTagRegExs(List<SqlTagRegExVM> tmpTagRegExs)
        {
            foreach (SqlTagRegExVM TagRegEx in tmpTagRegExs) //cycle through the TagRegExs, usually one or two, fail or hide stops iteration, if continues returns pass.
            {
                if (TagRegEx.RegExText.Contains("prolonged")) //used to debug
                {
                }

                //This boolean shortens the code
                bool StopIfMissOrHide = TagRegEx.TagRegExMatchResult != SqlTagRegExM.EnumResult.Pass;

                // check demographic limits and return result if met.
                //If any TagRegEx fails due to demographics, the entire series fails
                double age = ParentDocument.Patient.GetAgeInYearsDouble();
                if (age < TagRegEx.MinAge) return SqlTagRegExM.EnumResult.Hide;
                if (age >= TagRegEx.MaxAge) return SqlTagRegExM.EnumResult.Hide;
                if (ParentDocument.Patient.isMale && !TagRegEx.Male) return SqlTagRegExM.EnumResult.Hide;
                if (!ParentDocument.Patient.isMale && !TagRegEx.Female) return SqlTagRegExM.EnumResult.Hide;

                //Process each of the tags, if any fail or hide then series stop, otherwise passes.
                //Process Yes/No Tag
                if (TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.Ask) //ask question... pass if yes, fail if no
                {
                    if (Properties.Settings.Default.AskYesNo) //If Bypass is on then assume answer was yes
                    {
                        if (StopIfMissOrHide) return TagRegEx.TagRegExMatchResult; //Match result is the result if a positive "yes" or "no" if set as Result (not "noResult") match is met
                        continue;
                    }
                    else
                    {
                        bool yn = false;
                        if (YesNoSqlRegExIndex.ContainsKey(TagRegEx.TagRegExID))
                        {
                            yn = YesNoSqlRegExIndex[TagRegEx.TagRegExID];
                        }
                        else
                        {
                            WinShowRegExYesNo ws = new WinShowRegExYesNo();
                            if (TagRegEx.RegExText.Contains('|'))
                            {
                                ws.tbQuestion.Text = TagRegEx.RegExText.Split('|')[1];
                            }
                            else
                            {
                                ws.tbQuestion.Text = TagRegEx.RegExText;
                            }
                            ws.DataContext = TagRegEx;
                            ws.ShowDialog();
                            YesNoSqlRegExIndex.Add(TagRegEx.TagRegExID, ws.YesNoResult);
                            yn = ws.YesNoResult;
                        }
                        if (yn == true)
                        {
                            if (StopIfMissOrHide) return TagRegEx.TagRegExMatchResult; //if Yes return 1st Result option if it's fail or hide
                            continue; //continue to next iteration bacause result is pass.
                        }
                        else
                        {
                            if (TagRegEx.TagRegExMatchNoResult != SqlTagRegExM.EnumResult.Pass) return TagRegEx.TagRegExMatchNoResult;
                            continue;  //continue to next iteration bacause result is pass.
                        }
                    }
                }

         

                //process all,none,any match condition
                //Cycle through the list of terms and search through section of note if term is a match or not
                bool AllTermsMatch = true;
                bool NoTermsMatch = true;

                string strTextToMatch = "";
                if (ParentDocument.NoteSectionText[TagRegEx.TargetSection] != null) strTextToMatch = ParentDocument.NoteSectionText[TagRegEx.TargetSection].ToLower();
                foreach (string strRegEx in TagRegEx.RegExText.Split(','))
                {
                    if (strRegEx.Trim() != "")
                    {
                        //This is original: i took the prefix out, not sure why it was there if (Regex.IsMatch(strTextToMatch, CF.strRegexPrefix + strRegEx.Trim(), RegexOptions.IgnoreCase))
                        if (Regex.IsMatch(strTextToMatch, CF.strRegexPrefix + strRegEx.Trim(), RegexOptions.IgnoreCase)) // /i is lower case directive for regex
                        {
                            //Match is found!
                            //ANY condition is met, so stop if miss or hide if that is the 1st action
                            if (StopIfMissOrHide) if (TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.Any) return TagRegEx.TagRegExMatchResult; //Contains Any return 2nd Result - don't continue if type is "ANY NF" this is a stopper.
                            NoTermsMatch = false;
                            if (TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.Any) break; //condition met, no need to check rest
                        }
                        else
                        {
                            AllTermsMatch = false;
                        }
                    }
                }
                //ALL condition met if all terms match
                if (StopIfMissOrHide)
                {
                    if (AllTermsMatch && StopIfMissOrHide)
                    {
                        if (TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.All) return TagRegEx.TagRegExMatchResult; //Contains All return 2nd Result because any clause not reached
                    }
                    if (NoTermsMatch && TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.None) return TagRegEx.TagRegExMatchResult; //Contains Any return 2nd Result - don't continue if type is "ANY NF" this is a stopper.)
                    if (!NoTermsMatch && TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.Any) return TagRegEx.TagRegExMatchNoResult;
                }
                //NONE condition met if no terms match

                if (!NoTermsMatch && TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.Any) continue;

                if (NoTermsMatch && TagRegEx.TagRegExMatchType == SqlTagRegExM.EnumMatch.None) //none condition met, carry out pass
                {

                }
                else
                {
                    if (TagRegEx.TagRegExMatchNoResult != SqlTagRegExM.EnumResult.Pass) return TagRegEx.TagRegExMatchNoResult;
                }
                //ASK,ALL, and NONE conditions are note met, so the NoResult condition is the action
            }

            return SqlTagRegExM.EnumResult.Pass; //default is pass
        }


        public string StrCheckPointType
        {
            get
            {
                return (from c in CheckPointTypes where c.CheckPointTypeID == CheckPointType select c).FirstOrDefault().Title;
            }
        }
        public void SaveToDB()
        {
            this.SqlCheckpoint.SaveToDB();
        }
        public void DeleteFromDB()
        {
            this.SqlCheckpoint.DeleteFromDB();
        }
        public void UpdateImages()
        {
            OnPropertyChanged("Images");
        }
        public void AddImageFromClipBoard()
        {
            this.SqlCheckpoint.AddImageFromClipBoard();
        }
        /// <summary>
        /// Get the tags associated with the checkpoint
        /// </summary>
        public List<SqlTagVM> Tags
        {
            get
            {
                string sql = $"select t.TagID, TagText from Tags t inner join RelTagCheckPoint relTC on t.TagID = relTC.TagID where CheckPointID = {CheckPointID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    var tmpList = cnn.Query<SqlTagVM>(sql, this).ToList();
                    foreach (SqlTagVM st in tmpList)
                    {
                        st.ParentCheckPoint = this;
                    }
                    return tmpList;
                }
            }
        }

        public List<SqlTagVM> GetTags()
        {
            string sql = $"select t.TagID, TagText from Tags t inner join RelTagCheckPoint relTC on t.TagID = relTC.TagID where CheckPointID = {CheckPointID} order by RelTagCheckPointID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                return cnn.Query<SqlTagVM>(sql, this).ToList();
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
            OnPropertyChanged("Tags");
        }

        public void RemoveTag(SqlTagVM st)
        {
            string sql = $"Delete From RelTagCheckPoint where CheckPointID = {CheckPointID} AND TagID = {st.TagID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
            OnPropertyChanged("Tags");
        }

        /// <summary>
        /// A boolean indicating if the checkpoint will be included in the report.  This is true by default for all missed checkpoint, false for for all passed check point. It is not saved in the database.
        /// </summary>
        public bool IncludeCheckpoint
        {
            get; set;
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
        public string CustomComment { get; set; }

        public List<SqlCheckPointType> CheckPointTypes
        {
            get
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from CheckPointTypes order by ItemOrder;";
                    return cnn.Query<SqlCheckPointType>(sql).ToList();
                }
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
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            SqlCheckpointVM CurrentCheckpoint = parameter as SqlCheckpointVM;
            if (CurrentCheckpoint == null) return;
            string strSuggest = "#";
            if (CurrentCheckpoint.CheckPointType == 1) strSuggest = "#Query";
            if (CurrentCheckpoint.CheckPointType == 2) strSuggest = "#Exam";
            if (CurrentCheckpoint.CheckPointType == 3) strSuggest = "#Lab";
            if (CurrentCheckpoint.CheckPointType == 4) strSuggest = "#Imaging";
            if (CurrentCheckpoint.CheckPointType == 5) strSuggest = "#Condition";
            if (CurrentCheckpoint.CheckPointType == 6) strSuggest = "#CurrentMed";
            if (CurrentCheckpoint.CheckPointType == 7) strSuggest = "#Edu";
            if (CurrentCheckpoint.CheckPointType == 8) strSuggest = "#Exam";
            if (CurrentCheckpoint.CheckPointType == 9) strSuggest = "#CurrentMed";
            if (CurrentCheckpoint.CheckPointType == 10) strSuggest = "#Demographic";
            if (CurrentCheckpoint.CheckPointType == 11) strSuggest = "#HPI";
            if (CurrentCheckpoint.CheckPointType == 12) strSuggest = "#Vitals";
            if (CurrentCheckpoint.CheckPointType == 13) strSuggest = "#Rx";
            if (CurrentCheckpoint.CheckPointType == 14) strSuggest = "#Refer";
            if (CurrentCheckpoint.CheckPointType == 15) strSuggest = "#BEERS";
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
                if (tg == null) tg = new SqlTagVM(wat.ReturnValue);
                CurrentCheckpoint.AddTag(tg);

                //SqlTagRegEx srex = new SqlTagRegEx(tg.TagID, "Search Text", CurrentCheckpoint.TargetSection, 1);
            }
        }
        #endregion
    }

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
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            SqlCheckpointVM cp = parameter as SqlCheckpointVM;
            //set current CP to this one.
            cp.ParentSegment.ParentReport.SelectedItem = cp;
            WinCheckPointEditor wce = new WinCheckPointEditor(cp);
            wce.DataContext = cp.ParentSegment.ParentReport;
            wce.Show();
            cp.ParentSegment.ParentReport.UpdateCPs();
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
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            SqlCheckpointVM cp = parameter as SqlCheckpointVM;
            //set the selectedCP to this one.
            cp.ParentSegment.ParentReport.SelectedItem = cp;
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
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
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
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            SqlCheckpointM CP = parameter as SqlCheckpointM;
            CP.AddImageFromClipBoard();
        }
        #endregion
    }


    

}
