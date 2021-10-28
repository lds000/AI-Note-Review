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

        public SqlTagRegExVM()
        {
            this.SqlTagRegEx = new SqlTagRegExM();
        }

        public SqlTagRegExVM(long intTargetTag, string strRegExText, long intTargetSection, long iTagRegExType = 1, long iTagRegExMatchType = 0, long iTagRegExMatchResult = 0, double dMinAge = 0, double dMaxAge = 99, bool bMale = true, bool bFemale = true)
        {
            strRegExText = strRegExText.Replace("'", "''"); //used to avoid errors in titles with ' character
            string sql = "";
            sql = $"INSERT INTO TagRegEx (TargetTag, RegExText, TargetSection, TagRegExType, TagRegExMatchType, TagRegExMatchResult, MinAge, MaxAge, Male, Female) VALUES ({intTargetTag}, '{strRegExText}', {intTargetSection}, {iTagRegExType}, {iTagRegExMatchType}, {iTagRegExMatchResult}, {dMinAge},{dMaxAge},{bMale},{bFemale});SELECT last_insert_rowid();";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                int lastID = cnn.ExecuteScalar<int>(sql); //find ID of the insert tag and retreive it.
                sql = $"Select * from TagRegEx where TagRegExID = ${lastID}";
                this.SqlTagRegEx = cnn.Query<SqlTagRegExM>(sql).FirstOrDefault();
            }

        }

        public SqlTagVM ParentTag { get; set; }
        public int TagRegExID { get { return this.SqlTagRegEx.TagRegExID; } set { this.SqlTagRegEx.TagRegExID = value; OnPropertyChangedSave(); } }
        public int TargetTag { get { return this.SqlTagRegEx.TargetTag; } set { SqlTagRegEx.TargetTag = value; OnPropertyChangedSave(); } }
        public int TargetSection { get { return this.SqlTagRegEx.TargetSection; } set { SqlTagRegEx.TargetSection = value; OnPropertyChangedSave(); } }
        public string RegExText { get { return this.SqlTagRegEx.RegExText; } set { SqlTagRegEx.RegExText = value; OnPropertyChangedSave(); } }
        public int TagRegExType { get { return this.SqlTagRegEx.TagRegExType; } set { SqlTagRegEx.TagRegExType = value; OnPropertyChangedSave(); OnPropertyChanged("TagRegExMatchTypeDescription"); } }
        public double MinAge { get { return (double)this.SqlTagRegEx.MinAge; } set { 
                SqlTagRegEx.MinAge = value; 
                OnPropertyChangedSave();  } }
        public double MaxAge { get { return this.SqlTagRegEx.MaxAge; } set { SqlTagRegEx.MaxAge = value; OnPropertyChangedSave(); } }
        public bool Male { get { return this.SqlTagRegEx.Male; } set { SqlTagRegEx.Male = value; OnPropertyChangedSave(); } }
        public bool Female { get { return this.SqlTagRegEx.Female; } set { SqlTagRegEx.Female = value; OnPropertyChangedSave(); } }

        public string NoteSectionText { 
            get { return ParentTag.ParentCheckPoint.ParentDocument.NoteSectionText[TargetSection]; } 
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
                return $"If {TagRegExTypesToString(TagRegExMatchType)} then {TagRegExMatchToString(TagRegExMatchResult)}, otherwise {TagRegExMatchToString(TagRegExMatchNoResult)}.";
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
    }

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
}
