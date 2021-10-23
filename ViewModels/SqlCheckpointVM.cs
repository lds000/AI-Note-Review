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
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public SqlCheckpointVM()
        {
        }

        public SqlCheckpointVM(SqlCheckpointM cp, SqlICD10SegmentVM icd10)
        {
            this.SqlCheckpoint = cp;
            ParentICD10SegmentVM = icd10;
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

        public SqlICD10SegmentVM ParentICD10SegmentVM { get; set; }

        public SqlCheckpointM SqlCheckpoint { get; set; }
        public int CheckPointID { get { return this.SqlCheckpoint.CheckPointID; } set { this.SqlCheckpoint.CheckPointID = value; } }
        public string CheckPointTitle { get { return this.SqlCheckpoint.CheckPointTitle; } set { this.SqlCheckpoint.CheckPointTitle = value; } }
        public int CheckPointType { get { return this.SqlCheckpoint.CheckPointType; } set { this.SqlCheckpoint.CheckPointType = value; } }
        public string Comment { get { return this.SqlCheckpoint.Comment; } set { this.SqlCheckpoint.Comment = value; } }
        public int ErrorSeverity { get { return this.SqlCheckpoint.ErrorSeverity; } set { this.SqlCheckpoint.ErrorSeverity = value; } }
        public int TargetSection { get { return this.SqlCheckpoint.TargetSection; } set { this.SqlCheckpoint.TargetSection = value; } }
        public int TargetICD10Segment { get { return this.SqlCheckpoint.TargetICD10Segment; } set { this.SqlCheckpoint.TargetICD10Segment = value; } }
        public string Action { get { return this.SqlCheckpoint.Action; } set { this.SqlCheckpoint.Action = value; } }
        public string Link { get { return this.SqlCheckpoint.Link; } set { this.SqlCheckpoint.Link = value; } }
        public int Expiration { get { return this.SqlCheckpoint.Expiration; } set { this.SqlCheckpoint.Expiration = value; } }


        public ObservableCollection<SqlCheckPointImage> Images
        {
            get
            {
                string sql = $"select * from CheckPointImages where CheckPointID = @CheckPointID;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    var tmpImages = new ObservableCollection<SqlCheckPointImage>(cnn.Query<SqlCheckPointImage>(sql, this).ToList());
                    foreach (var tmpImage in tmpImages)
                    {
                        tmpImage.ParentCheckPointVM = this;
                    }
                    return tmpImages;
                }
            }
        }

        public void UpDateImages()
        {
            OnPropertyChanged("Images");
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
        public void DeleteImage(SqlCheckPointImage sci)
        {
            this.SqlCheckpoint.DeleteImage(sci);
        }
        public void AddImageFromClipBoard()
        {
            this.SqlCheckpoint.AddImageFromClipBoard();
            OnPropertyChanged("Images");

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


        public List<SqlCheckpointVM> GetCPsFromSegment(int SegmentID)
        {
            string sql = $"Select * from CheckPoints where TargetICD10Segment == {SegmentID}";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                return cnn.Query<SqlCheckpointVM>(sql).ToList();
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

        class CPRemover : ICommand
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
                cp.DeleteFromDB();
                cp.ParentICD10SegmentVM.UpdateCheckPoints();
            }
            #endregion
        }

        private ICommand mRemoveCP;
        public ICommand RemoveCPCommand
        {
            get
            {
                if (mRemoveCP == null)
                    mRemoveCP = new CPRemover();
                return mRemoveCP;
            }
            set
            {
                mRemoveCP = value;
            }
        }

        private ICommand mAddImage;
        public ICommand AddImageCommand
        {
            get
            {
                if (mAddImage == null)
                    mAddImage = new CPImageAdder();
                return mAddImage;
            }
            set
            {
                mAddImage = value;
            }
        }

    }

    class CPImageAdder : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            SqlCheckpointVM CP = parameter as SqlCheckpointVM;
            return CP != null;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            SqlCheckpointVM CP = parameter as SqlCheckpointVM;
            if (CP == null) return;
            CP.AddImageFromClipBoard();
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
            //CP.SaveToDB();
        }
        #endregion
    }
}
