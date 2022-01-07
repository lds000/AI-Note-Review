using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
    public class SqlTagRegExVM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        protected void OnPropertyChangedSave([CallerMemberName] string name = null)
        {
            if (PropertyChanged != null)
            {
                SaveToDB();
                Console.WriteLine($"Property {name} was saved on TagRegEx!");
                UpdateTagRegExCPStatus();
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private SqlTagRegExM SqlTagRegEx { get; set; }

        public DocumentVM ParentDocumentVM { get; set; }
        public SqlTagVM ParentTag { get; set; }

        public SqlTagRegExVM()
        {
            this.SqlTagRegEx = new SqlTagRegExM();
        }

        /// <summary>
        /// Create and save to database
        /// </summary>
        /// <param name="intTargetTag"></param>
        /// <param name="strRegExText"></param>
        /// <param name="intTargetSection"></param>
        /// <param name="iTagRegExType"></param>
        /// <param name="iTagRegExMatchType"></param>
        /// <param name="iTagRegExMatchResult"></param>
        /// <param name="dMinAge"></param>
        /// <param name="dMaxAge"></param>
        /// <param name="bMale"></param>
        /// <param name="bFemale"></param>
        public SqlTagRegExVM(long intTargetTag, string strRegExText, long intTargetSection, long iTagRegExType = 1, long iTagRegExMatchType = 1, long iTagRegExMatchResult = 1,long iTagRegExMatchNoResult=3, double dMinAge = 0, double dMaxAge = 99, bool bMale = true, bool bFemale = true)
        {
            strRegExText = strRegExText.Replace("'", "''"); //used to avoid errors in titles with ' character
            string sql = "";
            sql = $"INSERT INTO TagRegEx (TargetTag, RegExText, TargetSection, TagRegExType, TagRegExMatchType, TagRegExMatchResult, TagRegExMatchNoResult, MinAge, MaxAge, Male, Female) VALUES ({intTargetTag}, '{strRegExText}', {intTargetSection}, {iTagRegExType}, {iTagRegExMatchType}, {iTagRegExMatchResult}, {iTagRegExMatchNoResult}, {dMinAge},{dMaxAge},{bMale},{bFemale});SELECT last_insert_rowid();";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                int lastID = cnn.ExecuteScalar<int>(sql); //find ID of the insert tag and retreive it.
                sql = $"Select * from TagRegEx where TagRegExID = {lastID}";
                this.SqlTagRegEx = cnn.Query<SqlTagRegExM>(sql).FirstOrDefault();
            }
        }

        #region Mirror the Model properties
        public int TagRegExID { get { return this.SqlTagRegEx.TagRegExID; } set { this.SqlTagRegEx.TagRegExID = value; OnPropertyChangedSave(); } }
        public int TargetTag { get { return this.SqlTagRegEx.TargetTag; } set { SqlTagRegEx.TargetTag = value; OnPropertyChangedSave(); } }
        public int TargetSection { get { return this.SqlTagRegEx.TargetSection; } set { SqlTagRegEx.TargetSection = value; OnPropertyChangedSave(); } }
        public string RegExText { get { return this.SqlTagRegEx.RegExText; } set { SqlTagRegEx.RegExText = value; OnPropertyChangedSave();  } }
        public int TagRegExType { get { return this.SqlTagRegEx.TagRegExType; } set { SqlTagRegEx.TagRegExType = value; OnPropertyChangedSave(); OnPropertyChanged("TagRegExMatchTypeDescription"); } }
        public double MinAge
        {
            get { return (double)this.SqlTagRegEx.MinAge; }
            set
            {
                SqlTagRegEx.MinAge = value;
                OnPropertyChangedSave();
            }
        }
        public double MaxAge { get { return this.SqlTagRegEx.MaxAge; } set { SqlTagRegEx.MaxAge = value; OnPropertyChangedSave(); } }
        public bool Male { get { return this.SqlTagRegEx.Male; } set { SqlTagRegEx.Male = value; OnPropertyChangedSave(); } }
        public bool Female { get { return this.SqlTagRegEx.Female; } set { SqlTagRegEx.Female = value; OnPropertyChangedSave(); } }
        #endregion

        public void UpdateParentCP()
        {
            if (ParentTag != null)
            if (ParentTag.ParentCheckPoint != null)
            ParentTag.ParentCheckPoint.UpdateCPStatus();
        }

        //process all,none,any match condition
        //Cycle through the list of terms and search through section of note if term is a match or not

        private bool allTermsMatch;
        public bool AllTermsMatch
        {
            get
            {
                return allTermsMatch;
            }
            set
            {
                allTermsMatch = value;
            }
        }

        private bool noTermsMatch;
        public bool NoTermsMatch
        {
            get
            {
                return noTermsMatch;
            }
            set
            {
                noTermsMatch = value;
            }
        }

        //This boolean shortens the code execution for example, if miss if any, then it will stop on the first match
        bool StopIfMissOrHide
        {
            get
            {
                return TagRegExMatchResult != SqlTagRegExM.EnumResult.Pass;
            }
        }

        private bool? ask;
        public bool Ask
        {
            get
            {
                if (Properties.Settings.Default.AskYesNo) ask = true; //if the default setting is not to ask then pass.
                if (ask == null)
                {
                    WinShowRegExYesNo ws = new WinShowRegExYesNo();
                    if (RegExText.Contains('|'))
                    {
                        ws.tbQuestion.Text = RegExText.Split('|')[1];
                    }
                    else
                    {
                        ws.tbQuestion.Text = RegExText;
                    }
                    ws.DataContext = this;
                    ws.ShowDialog();
                    ParentTag.ParentCheckPoint.YesNoSqlRegExIndex.Add(TagRegExID, ws.YesNoResult);
                    ask = ws.YesNoResult;
                }
                return (bool)ask;
            }
        }

        private bool? isHidden;
        public bool IsHidden
        {
            get
            {
                if (isHidden == null)
                {
                    isHidden = false;
                    double age = ParentDocumentVM.Patient.GetAgeInYearsDouble;
                    if (age < MinAge)
                    {
                        isHidden = true;
                        return true;
                    }
                    if (age >= MaxAge)
                    {
                        isHidden = true;
                        return true;
                    }
                    if (ParentDocumentVM.Patient.isMale && !Male)
                    {
                        isHidden = true;
                        return true;
                    }
                    if (!ParentDocumentVM.Patient.isMale && !Female)
                    {
                        isHidden = true;
                        return true;
                    }
                }
                return (bool)isHidden;
            }
        }

        private SqlTagRegExM.EnumResult? matchStatus;
        public SqlTagRegExM.EnumResult MatchStatus
        {
            get
            {
                if (matchStatus == null)
                {
                    matchStatus = getMatchStatus();
                }
                return (SqlTagRegExM.EnumResult)matchStatus;
            }
        }

        /// <summary>
        /// This is the guts of the program.  Crunches the match status out
        /// </summary>
        /// <returns></returns>
        private SqlTagRegExM.EnumResult getMatchStatus()
        {
            // check demographic limits and return result if met.
            //If any TagRegEx fails due to demographics, the entire series fails
            if (IsHidden)
            {
                return SqlTagRegExM.EnumResult.Hide;
            }

            //Process Yes/No Tag
            if (TagRegExMatchType == SqlTagRegExM.EnumMatch.Ask) //ask question... pass if yes, fail if no
            {
                if (Ask) //I made this into a property so it is remembered.
                {
                    if (StopIfMissOrHide) return TagRegExMatchResult; //if Yes return 1st Result option if it's fail or hide
                }
                else
                {
                    if (TagRegExMatchNoResult != SqlTagRegExM.EnumResult.Pass) return TagRegExMatchNoResult; //pass simply continues to match to the next tagregex
                }
            }

            AllTermsMatch = true;
            NoTermsMatch = true;
            //Process each of the tags, if any fail or hide then series stop, otherwise passes.
            foreach (string strRegEx in RegExText.Split(','))
            {
                if (strRegEx.Trim() == "") continue;
                if (strRegEx == "focal neuro")
                {
                }
                //This is original: i took the prefix out, not sure why it was there if (Regex.IsMatch(strTextToMatch, CF.strRegexPrefix + strRegEx.Trim(), RegexOptions.IgnoreCase))
                if (Regex.IsMatch(DocumentNoteSectionTextToMatch, CF.strRegexPrefix + strRegEx.Trim(), RegexOptions.IgnoreCase)) // /i is lower case directive for regex
                {
                    NoTermsMatch = false;
                    //Match is found!
                    //ANY condition is met, so stop if miss or hide if that is the 1st action
                    if (TagRegExMatchType == SqlTagRegExM.EnumMatch.Any) return TagRegExMatchResult; //Contains Any return 1st result
                    if (TagRegExMatchType == SqlTagRegExM.EnumMatch.None) return TagRegExMatchNoResult; //Contains none is no longer met return noresult.
                }
                else
                {
                    AllTermsMatch = false;
                    if (TagRegExMatchType == SqlTagRegExM.EnumMatch.All) return TagRegExMatchNoResult; //Contains all condition not met, return noresult
                }
            }
            if (AllTermsMatch)
            {
                if (TagRegExMatchType == SqlTagRegExM.EnumMatch.All) return TagRegExMatchResult; //All condition met, return result
            }
            if (NoTermsMatch)
            {
                if (TagRegExMatchType == SqlTagRegExM.EnumMatch.None) return TagRegExMatchResult;
                if (TagRegExMatchType == SqlTagRegExM.EnumMatch.Any) return TagRegExMatchNoResult;
            }


            return SqlTagRegExM.EnumResult.Pass; //default is pass
        }

        /// <summary>
        /// Maps to the DocumentVM text section that is used to match this TagRexEx to.
        /// </summary>
        public string DocumentNoteSectionTextToMatch
        {
            get
            {
                if (ParentTag.ParentCheckPoint.ParentDocument.NoteSectionText[TargetSection] == null) return "";
                return ParentTag.ParentCheckPoint.ParentDocument.NoteSectionText[TargetSection];
            }
        }

        public IEnumerable<SqlTagRegExM.EnumMatch> MyMatchTypeValues
        {
            get
            {
                return Enum.GetValues(typeof(SqlTagRegExM.EnumMatch))
                    .Cast<SqlTagRegExM.EnumMatch>();
            }
        }

        public SqlTagRegExM.EnumMatch TagRegExMatchType
        {
            get { return this.SqlTagRegEx.TagRegExMatchType; }
            set
            {
                this.SqlTagRegEx.TagRegExMatchType = value;
                OnPropertyChangedSave();
                OnPropertyChanged("TagRegExMatchTypeDescription");
            }
        }

        public string TagRegExMatchTypeDescription
        {
            get
            {
                return $"{TagRegExTypesToString(TagRegExMatchType)} then {TagRegExMatchToString(TagRegExMatchResult)}, otherwise {TagRegExMatchToString(TagRegExMatchNoResult)}.";
            }
        }

        public IEnumerable<SqlTagRegExM.EnumResult> MyResultTypeValues
        {
            get
            {
                return Enum.GetValues(typeof(SqlTagRegExM.EnumResult))
                    .Cast<SqlTagRegExM.EnumResult>();
            }
        }

        public SqlTagRegExM.EnumResult TagRegExMatchResult
        {
            get { return this.SqlTagRegEx.TagRegExMatchResult; }
            set
            {
                this.SqlTagRegEx.TagRegExMatchResult = value;
                OnPropertyChangedSave();
                OnPropertyChanged("TagRegExMatchType");
            }
        }

        public SqlTagRegExM.EnumResult TagRegExMatchNoResult
        {
            get { return this.SqlTagRegEx.TagRegExMatchNoResult; }
            set
            {
                this.SqlTagRegEx.TagRegExMatchNoResult = value;
                OnPropertyChanged("TagRegExMatchType");
                OnPropertyChangedSave();
            }
        }

        public string TagRegExMatchToString(SqlTagRegExM.EnumResult enumTagRegExMatch) //either Result or noresults match can be used as arguement;
        {
            switch (enumTagRegExMatch)
            {
                case SqlTagRegExM.EnumResult.Pass:
                    {
                        return "the checkpoint will pass";
                    }
                case SqlTagRegExM.EnumResult.Hide:
                    {
                        return "the checkpoint will be hidden";
                    }
                case SqlTagRegExM.EnumResult.Miss:
                    {
                        return "the checkpoint will miss";
                    }
                default:
                    return "";
            }
        }

        public string TagRegExTypesToString(SqlTagRegExM.EnumMatch enumTagRegExMatch) //either Result or noresults match can be used as arguement;
        {
            switch (enumTagRegExMatch)
            {
                case SqlTagRegExM.EnumMatch.Any:
                    {
                        return "If any of the following terms match";
                    }
                case SqlTagRegExM.EnumMatch.All:
                    {
                        return "If all of the following terms match";
                    }
                case SqlTagRegExM.EnumMatch.None:
                    {
                        return "If none of the following terms match";
                    }
                case SqlTagRegExM.EnumMatch.Ask:
                    {
                        return "Ask question, if yes";
                    }
                default:
                    return "";
            }
        }

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
        public void SaveToDB()
        {
            SqlTagRegEx.SaveToDB();
        }

        public bool DeleteFromDB()
        {
            return SqlTagRegEx.DeleteFromDB();
        }

        public void UpdateTagRegExCPStatus()
        {
            //push this upstream to update any pertinent information to the Parenttag, perhaps an event that bubbles up would be better.
            ParentTag.UpdateCPStatus();
        }

        public static List<SqlNoteSection> NoteSections //todo, move this out to a common function.
        {
            get
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from NoteSections order by SectionOrder;";
                    return cnn.Query<SqlNoteSection>(sql).ToList();
                }
            }

        }

        #region Commands
        private ICommand mSaveTagRegEx;
        public ICommand SaveTagRegExCommand
        {
            get
            {
                if (mSaveTagRegEx == null)
                    mSaveTagRegEx = new SaveTagRegEx();
                return mSaveTagRegEx;
            }
            set
            {
                mSaveTagRegEx = value;
            }
        }

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

        private ICommand mEditTagRegEx;
        public ICommand EditTagRegExCommand
        {
            get
            {
                if (mEditTagRegEx == null)
                    mEditTagRegEx = new EditTagRegEx();
                return mEditTagRegEx;
            }
            set
            {
                mEditTagRegEx = value;
            }
        }

        #endregion
    }

    #region Command Classes
    class SaveTagRegEx : ICommand
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
            SqlTagRegExVM t = parameter as SqlTagRegExVM;
            t.SaveToDB();
        }
        #endregion
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
            SqlTagRegExVM t = parameter as SqlTagRegExVM;
            t.ParentTag.RemoveTagRegEx(t);
        }
        #endregion
    }

    class EditTagRegEx : ICommand
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
            SqlTagRegExVM t = parameter as SqlTagRegExVM;
            WinEnterText wet = new WinEnterText("Edit Regular Expression value", t.RegExText);
            wet.ShowDialog();
            if (wet.ReturnValue != null)
            {
                t.RegExText = wet.ReturnValue;
            }
        }
        #endregion
    }
    #endregion
}
