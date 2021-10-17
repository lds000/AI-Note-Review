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
    public class SqlCheckpointViewModel : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        /// <summary>
        /// not sure if this is correct for MVVM, but required for Dapper - a parameterless constructor
        /// </summary>
        /// 
        private SqlCheckpoint sqlCheckpoint;

        public SqlCheckpointViewModel()
        {
            sqlCheckpoint = new SqlCheckpoint();
        }

        public SqlCheckpointViewModel(SqlCheckpoint cp)
        {
            sqlCheckpoint = cp;
        }

        public SqlCheckpointViewModel(int cpID)
        {
                string sql = $"Select * from CheckPoints WHERE CheckPointID={cpID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                sqlCheckpoint = cnn.QueryFirstOrDefault<SqlCheckpoint>(sql);
                }
        }


        public SqlCheckpoint SqlCheckpoint
        {
            get { return sqlCheckpoint; }
        }

        private bool includeCheckpoint = true;
        /// <summary>
        /// bool indicating if checkpoint is included in report
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
                OnPropertyChanged("IncludeCheckpoint");
            }
        }

        public List<SqlCheckPointType> ListCheckPointTypes
        {
            get
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from CheckPointTypes;";
                    return cnn.Query<SqlCheckPointType>(sql).ToList();
                }
            }

        }

        /// <summary>
        /// Returns a string representation of the (int) checkpoint type
        /// </summary>
        public string StrCheckPointType
        {
            get
            {
                string sql = "";
                sql = $"Select Title from CheckPointTypes where CheckPointTypeID == {sqlCheckpoint.CheckPointType};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return cnn.ExecuteScalar<string>(sql);
                }
            }

        }

        private string customComment = "";
        /// <summary>
        /// Used to hold additional comments at the time of the review specific to the review / patient / provider, etc...
        /// </summary>
        public string CustomComment
        {
            get
            {
                return customComment;
            }
            set
            {
                customComment = value;
                OnPropertyChanged("CustomComment");
            }
        }

        /// <summary>
        /// Get the tags associated with the checkpoint
        /// </summary>
        public List<SqlTag> Tags
        {
            get
            {
                string sql = $"select t.TagID, TagText from Tags t inner join RelTagCheckPoint relTC on t.TagID = relTC.TagID where CheckPointID = {sqlCheckpoint.CheckPointID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    var tmpList = cnn.Query<SqlTag>(sql, this).ToList();
                    foreach (SqlTag st in tmpList)
                    {
                        st.ParentCheckPoint = sqlCheckpoint;
                    }
                    return tmpList;
                }
            }
        }

        public void AddTag(SqlTag tg)
        {
            string sql = "";
            sql = $"INSERT INTO RelTagCheckPoint (TagID, CheckPointID) VALUES ({tg.TagID},{sqlCheckpoint.CheckPointID});";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
            OnPropertyChanged("Tags");
        }

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


        public void DeleteImage(SqlCheckPointImage sci)
        {
            sci.DeleteFromDB();
            OnPropertyChanged("Images");
        }

        public void AddImageFromClipBoard()
        {
            BitmapSource bs = Clipboard.GetImage();
            if (bs == null) return;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bs));
            encoder.QualityLevel = 100;
            // byte[] bit = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(bs));
                encoder.Save(stream);
                byte[] bit = stream.ToArray();
                stream.Close();
            }
            new SqlCheckPointImage(sqlCheckpoint.CheckPointID, bs);
            OnPropertyChanged("Images");
        }

        public ObservableCollection<SqlCheckPointImage> Images
        {
            get
            {
                string sql = $"select * from CheckPointImages where CheckPointID = @CheckPointID;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return new ObservableCollection<SqlCheckPointImage>(cnn.Query<SqlCheckPointImage>(sql, this.sqlCheckpoint).ToList());
                }
            }
        }

        public static List<SqlCheckPointType> CheckPointTypes()
        {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                   string sql = "Select * from CheckPointTypes order by ItemOrder;";
                    return cnn.Query<SqlCheckPointType>(sql).ToList();
                }               

        }


    public static List<SqlCheckpointViewModel> GetCPFromSegment(int SegmentID)
        {
            string sql = $"Select * from CheckPoints where TargetICD10Segment == {SegmentID}";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                List<SqlCheckpoint> lcp = cnn.Query<SqlCheckpoint>(sql).ToList();
                List<SqlCheckpointViewModel> lcpvm = new List<SqlCheckpointViewModel>();
                foreach (SqlCheckpoint cp in lcp)
                {
                    lcpvm.Add(new SqlCheckpointViewModel(cp));
                }
                return lcpvm;
            }

        }





        public void RemoveTag(SqlTag st)
        {
            string sql = $"Delete From RelTagCheckPoint where CheckPointID = {sqlCheckpoint.CheckPointID} AND TagID = {st.TagID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
            OnPropertyChanged("Tags");
        }

        public string GetIndex()
        {
            string strReturn = "";
            strReturn += $"CheckPoint: '{sqlCheckpoint.CheckPointTitle}'" + Environment.NewLine;
            //strReturn += $"\tSignificance {ErrorSeverity}/10." + Environment.NewLine;
            strReturn += $"\tRecommended Remediation: {sqlCheckpoint.Action}" + Environment.NewLine;
            strReturn += $"\tExplanation: {sqlCheckpoint.Comment}" + Environment.NewLine;
            if (sqlCheckpoint.Link != "")
                strReturn += $"\tLink: {sqlCheckpoint.Link}" + Environment.NewLine;
            strReturn += Environment.NewLine;
            strReturn += Environment.NewLine;
            return strReturn;
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
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            SqlCheckpoint CurrentCheckpoint = parameter as SqlCheckpoint;
            SqlCheckpointViewModel vm = new SqlCheckpointViewModel(CurrentCheckpoint);
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
                SqlTag tg = SqlLiteDataAccess.GetTags(wat.ReturnValue).FirstOrDefault();
                if (tg == null) tg = new SqlTag(wat.ReturnValue);
                vm.AddTag(tg);

                //SqlTagRegEx srex = new SqlTagRegEx(tg.TagID, "Search Text", CurrentCheckpoint.TargetSection, 1);
            }
        }
        #endregion

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
            SqlCheckpoint CP = parameter as SqlCheckpoint;
            //CP.SaveToDB();
        }
        #endregion
    }
}
