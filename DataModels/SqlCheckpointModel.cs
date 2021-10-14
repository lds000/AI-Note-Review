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
    public class SqlCheckpoint : INotifyPropertyChanged
    {
        private bool includeCheckpoint = true;

        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public int CheckPointID { get; set; }
        public string CheckPointTitle { get; set; }


        public bool IncludeCheckpoint
        {
            get
            {
                return includeCheckpoint;
            }
            set
            {
                includeCheckpoint = value;
            }
        }
        public int ErrorSeverity { get; set; }
        public int CheckPointType { get; set; }
        public int TargetSection { get; set; }
        public string Comment { get; set; }
        public string Action { get; set; }
        public string Link { get; set; }

        private string customComment = "";
        public string CustomComment {
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
        public int Expiration { get; set; }
        public SqlCheckpoint()
        {
        }

        public List<SqlTag> Tags
        {
            get
            {
                string sql = $"select t.TagID, TagText from Tags t inner join RelTagCheckPoint relTC on t.TagID = relTC.TagID where CheckPointID = {CheckPointID};";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    var tmpList = cnn.Query<SqlTag>(sql, this).ToList();
                    foreach (SqlTag st in tmpList) 
                    {
                        st.ParentCheckPoint = this;
                    }
                    return tmpList;
                }
            }
        }

        public void AddTag(SqlTag tg)
        {
            string sql = "";
            sql = $"INSERT INTO RelTagCheckPoint (TagID, CheckPointID) VALUES ({tg.TagID},{CheckPointID});";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
            OnPropertyChanged("Tags");
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
            new SqlCheckPointImage(CheckPointID, bs);
            OnPropertyChanged("Images");
        }

        public ObservableCollection<SqlCheckPointImage> Images
        {
            get
            {
                string sql = $"select * from CheckPointImages where CheckPointID = @CheckPointID;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    return new ObservableCollection<SqlCheckPointImage>(cnn.Query<SqlCheckPointImage>(sql, this).ToList());
                }
            }
        }


        public string RichText { get; set; }
        public int TargetICD10Segment { get; set; }




        public SqlCheckpoint(string strCheckPointTitle, int iTargetICD10Segment)
        {
            strCheckPointTitle = strCheckPointTitle.Replace("'", "''"); //used to avoid errors in titles with ' character
            string sql = "";
            int targetType = 1;
            int targetSection = 1;

            //now guess the categories to save time.
            if (strCheckPointTitle.ToLower().Contains("history"))
            {
                targetType = 1;
                targetSection = 1;
            }

            if (strCheckPointTitle.ToLower().Contains("exam"))
            {
                targetType = 8;
                targetSection = 8;
            }

            if (strCheckPointTitle.ToLower().Contains("elderly"))
            {
                targetType = 10;
                targetSection = 9;
            }

            if (strCheckPointTitle.ToLower().Contains("lab"))
            {
                targetType = 3;
                targetSection = 11;
            }

            if (strCheckPointTitle.ToLower().Contains("educat"))
            {
                targetType = 7;
                targetSection = 10;
            }

            if (strCheckPointTitle.ToLower().Contains("xr"))
            {
                targetType = 4;
                targetSection = 12;
            }

            if (strCheckPointTitle.ToLower().Contains("imag"))
            {
                targetType = 4;
                targetSection = 12;
            }

            if (strCheckPointTitle.ToLower().Contains("query"))
            {
                targetSection = 1;
            }



            sql = $"INSERT INTO CheckPoints (CheckPointTitle, TargetICD10Segment, TargetSection, CheckPointType) VALUES ('{strCheckPointTitle}', {iTargetICD10Segment}, {targetSection}, {targetType});";
            sql += $"Select * from CheckPoints where CheckPointTitle = '{strCheckPointTitle}' AND TargetICD10Segment = {iTargetICD10Segment};"; //this part is to get the ID of the newly created phrase
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                SqlCheckpoint p = cnn.QueryFirstOrDefault<SqlCheckpoint>(sql);
                CheckPointID = p.CheckPointID;
                TargetICD10Segment = p.TargetICD10Segment;
                TargetSection = p.TargetSection;
                CheckPointType = p.CheckPointType;
                CheckPointTitle = p.CheckPointTitle;
            }
        }

        public void Commit(DocInfo di, SqlRelCPProvider.MyCheckPointStates cpState)
        {
            if (CustomComment == null) CustomComment = "";
            string sql = $"Replace INTO RelCPPRovider (ProviderID, CheckPointID, PtID, ReviewDate, VisitDate, CheckPointStatus, Comment) VALUES ({di.ProviderID}, {CheckPointID}, {di.PtID}, '{di.ReviewDate.ToString("yyyy-MM-dd")}', '{di.VisitDate.ToString("yyyy-MM-dd")}', {(int)cpState}, '{CustomComment}');";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }

        }

            public List<SqlTag> GetTags()
        {
            string sql = $"select t.TagID, TagText from Tags t inner join RelTagCheckPoint relTC on t.TagID = relTC.TagID where CheckPointID = {CheckPointID} order by RelTagCheckPointID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                return cnn.Query<SqlTag>(sql, this).ToList();
            }

        }

        public void RemoveTag(SqlTag st)
        {
            string sql = $"Delete From RelTagCheckPoint where CheckPointID = {CheckPointID} AND TagID = {st.TagID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
            OnPropertyChanged("Tags");
        }

        public string GetIndex()
        {
            string strReturn = "";
            strReturn += $"CheckPoint: '{CheckPointTitle}'" + Environment.NewLine;
            //strReturn += $"\tSignificance {ErrorSeverity}/10." + Environment.NewLine;
            strReturn += $"\tRecommended Remediation: {Action}" + Environment.NewLine;
            strReturn += $"\tExplanation: {Comment}" + Environment.NewLine;
            if (Link != "")
                strReturn += $"\tLink: {Link}" + Environment.NewLine;
            strReturn += Environment.NewLine;
            strReturn += Environment.NewLine;
            return strReturn;
        }
        public string GetReport()
        {
            string strReturn = ""; 
            strReturn += $"<li><dt><font size='+1'>{CheckPointTitle}</font><font size='-1'> (Score Weight<sup>**</sup>:{ErrorSeverity}/10)</font></dt><dd><i>{Comment}</i></dd></li>" + Environment.NewLine;
            if (CustomComment != "")
            {
                strReturn += $"<b>Comment: {CustomComment}</b><br>";
            }
            if (Link != "" && Link != null)
            {
                strReturn += $"<a href={Link}>Click here for reference.</a><br>";
            }
            strReturn += $"<a href='mailto:Lloyd.Stolworthy@PrimaryHealth.com?subject=Feedback on review of {CF.CurrentDoc.PtID} on {CF.CurrentDoc.VisitDate.ToShortDateString()}. (Ref:{CF.CurrentDoc.PtID}|{CF.CurrentDoc.VisitDate.ToShortDateString()}|{CheckPointID})'>Feedback</a>";
            /*
            strReturn += $"\tSignificance {ErrorSeverity}/10." + Environment.NewLine;
            strReturn += $"\tRecommended Remediation: {Action}" + Environment.NewLine;
            strReturn += $"\tExplanation: {Comment}" + Environment.NewLine;
            if (Link != "")
            strReturn += $"\tLink: {Link}" + Environment.NewLine;
            strReturn += Environment.NewLine;
            strReturn += Environment.NewLine;
            */

            //HPi, exam, Dx, Rx

            return strReturn;
        }

        public void SaveToDB()
        {
            string sql = "UPDATE CheckPoints SET " +
                    "CheckPointID=@CheckPointID, " +
                    "CheckPointTitle=@CheckPointTitle, " +
                    "CheckPointType=@CheckPointType, " +
                    "TargetSection=@TargetSection, " +
                    "TargetICD10Segment=@TargetICD10Segment, " +
                    "Comment=@Comment, " +
                    "Action=@Action, " +
                    "RichText=@RichText, " +
                    "ErrorSeverity=@ErrorSeverity, " +
                    "Link=@Link, " +
                    "Expiration=@Expiration " +
                    "WHERE CheckPointID=@CheckPointID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
        }

        public static SqlCheckpoint GetCP(int cpID)
        {
            string sql = $"Select * from CheckPoints WHERE CheckPointID={cpID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                return cnn.QueryFirstOrDefault<SqlCheckpoint>(sql);
            }
        }

        public bool DeleteFromDB()
        {
            MessageBoxResult mr = MessageBox.Show("Are you sure you want to remove this diagnosis? This is permenant and will delete all content.", "Confirm Delete", MessageBoxButton.YesNo);
            if (mr != MessageBoxResult.Yes)
            {
                return false;
            }

            string sql = "Delete from CheckPoints WHERE CheckPointID=@CheckPointID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
            return true;
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
                CurrentCheckpoint.AddTag(tg);

                //SqlTagRegEx srex = new SqlTagRegEx(tg.TagID, "Search Text", CurrentCheckpoint.TargetSection, 1);
            }
        }
        #endregion
    }

}
