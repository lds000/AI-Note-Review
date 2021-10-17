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
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string action;
        private string checkPointTitle;
        private int checkPointType;
        private int targetSection;
        private string comment;
        private string link;
        private int targetICD10Segment;
        private int errorSeverity;


        /// <summary>
        /// not sure if this is correct for MVVM, but required for Dapper - a parameterless constructor
        /// </summary>
        public SqlCheckpoint()
        {
        }

        public int CheckPointID { get; set; } //ID is readonly
        public string CheckPointTitle
        {
            get => checkPointTitle;

            set
            {
                if (checkPointTitle != null)
                {
                    Console.WriteLine($"Update Title! to {value}");
                    checkPointTitle = value; //set this now for the update to work.
                    string sql = "UPDATE CheckPoints SET CheckPointTitle=@CheckPointTitle WHERE CheckPointID=@CheckPointID;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        cnn.Execute(sql, this);
                    }
                }
                checkPointTitle = value;
                OnPropertyChanged("CheckPointTitle");
            }
        }

        /// <summary>
        /// A score from 0-10 of how important the checlpoint is
        /// </summary>
        public int ErrorSeverity
        {
            get => errorSeverity;
            set
            {
                if (errorSeverity != 0)
                {
                    Console.WriteLine($"Update ErrorSeverity type to: {value}");
                    errorSeverity = value; //set this now for the update to work.
                    string sql = "UPDATE CheckPoints SET ErrorSeverity=@ErrorSeverity WHERE CheckPointID=@CheckPointID;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        cnn.Execute(sql, this);
                    }
                }
                errorSeverity = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int CheckPointType
        {
            get => checkPointType;
            set
            {
                if (checkPointType != 0)
                {
                    Console.WriteLine($"Update Checkpoint type to: {value}");
                    checkPointType = value; //set this now for the update to work.
                    string sql = "UPDATE CheckPoints SET CheckPointType=@CheckPointType WHERE CheckPointID=@CheckPointID;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        cnn.Execute(sql, this);
                    }
                }
                checkPointType = value;
                OnPropertyChanged("CheckPointType");
            }
        }

        public int TargetSection
        {
            get => targetSection; 
            set
            {
                if (targetSection != 0)
                {
                    targetSection = value; //set this now for the update to work.
                    string sql = "UPDATE CheckPoints SET TargetSection=@TargetSection WHERE CheckPointID=@CheckPointID;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        cnn.Execute(sql, this);
                    }
                }
                targetSection = value;
                OnPropertyChanged("TargetSection");
            }
        }
        public string Comment
        {
            get => comment;
            set
            {
                if (comment != null)
                {
                    Console.WriteLine($"Update Comment! to {value}");
                    comment = value; //set this now for the update to work.
                    string sql = "UPDATE CheckPoints SET Comment=@Comment WHERE CheckPointID=@CheckPointID;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        cnn.Execute(sql, this);
                    }
                }
                comment = value;
                OnPropertyChanged("Comment");
            }
        }
        public string Action
        {
            get => action;
            set
            {
                if (action != null)
                {
                    Console.WriteLine($"Update Action! to {value}");
                    action = value; //set this now for the update to work.
                    string sql = "UPDATE CheckPoints SET Action=@Action WHERE CheckPointID=@CheckPointID;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        cnn.Execute(sql, this);
                    }
                }
                action = value;
                OnPropertyChanged("Action");

            }
        }
        public string Link
        {
            get => link;
            set
            {
                if (link != null)
                {
                    Console.WriteLine($"Update Action! to {value}");
                    link = value; //set this now for the update to work.
                    string sql = "UPDATE CheckPoints SET Link=@Link WHERE CheckPointID=@CheckPointID;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        cnn.Execute(sql, this);
                    }
                }
                link = value;
                OnPropertyChanged("Link");

            }
        }

        /// <summary>
        /// A boolean indicating if the checkpoint will be included in the report.  This is true by default for all missed checkpoint, false for for all passed check point. It is not saved in the database.
        /// </summary>
        public bool IncludeCheckpoint
        {
            get; set;
        }

        /// <summary>
        /// A personal comment added to a checkpoint that is saved in the database under the commit (not checkpoint model).
        /// </summary>
        public string CustomComment { get; set; }

        /// <summary>
        /// A value of how long to have the checkpoint sleep between misses. Not implemented as of 10/17/2021
        /// </summary>
        public int Expiration { get; set; } //when ready to implement, do not set to zero, considered "null value" above


        public static SqlCheckpoint GetSqlCheckpoint(int cpID)
        {
            string sql = $"Select * from CheckPoints WHERE CheckPointID={cpID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                return cnn.QueryFirstOrDefault<SqlCheckpoint>(sql);
            }
        }



        /// <summary>
        /// The ICD10 segment this checkpoint is associated with, each checkpoint has only one segment, this is so I don't accidently change information between checkpoints and segments and give wrong information
        /// </summary>
        public int TargetICD10Segment
        {
            get => targetICD10Segment;
            set
            {
                if (targetICD10Segment != 0)
                {
                    Console.WriteLine($"Update TargetICD10Segment to: {value}");
                    targetICD10Segment = value; //set this now for the update to work.
                    string sql = "UPDATE CheckPoints SET TargetICD10Segment=@TargetICD10Segment WHERE CheckPointID=@CheckPointID;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        cnn.Execute(sql, this);
                    }
                }
                targetICD10Segment = value;
                OnPropertyChanged("TargetICD10Segment");
            }
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
                    "ErrorSeverity=@ErrorSeverity, " +
                    "Link=@Link, " +
                    "Expiration=@Expiration " +
                    "WHERE CheckPointID=@CheckPointID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
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

        /// <summary>
        /// Get the tags associated with the checkpoint
        /// </summary>
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

        public void RemoveTag(SqlTag st)
        {
            string sql = $"Delete From RelTagCheckPoint where CheckPointID = {CheckPointID} AND TagID = {st.TagID};";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql);
            }
            OnPropertyChanged("Tags");
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

        public static List<SqlCheckpoint> GetCPFromSegment(int SegmentID)
        {
            string sql = $"Select * from CheckPoints where TargetICD10Segment == {SegmentID}";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                return cnn.Query<SqlCheckpoint>(sql).ToList();
            }

        }



        /// <summary>
        /// Create a sql checkpoint given the title and targetsegment. Use clues from the title to suggest type and section the checkpoint belongs to.
        /// </summary>
        /// <param name="strCheckPointTitle"></param>
        /// <param name="iTargetICD10Segment"></param>
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

    }
    

}
