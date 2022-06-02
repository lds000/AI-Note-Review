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
    public class SqlCheckpointM : INotifyPropertyChanged
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
        private bool? doubleCheck;
        private bool? keyPoint;





        /// <summary>
        /// not sure if this is correct for MVVM, but required for Dapper - a parameterless constructor
        /// </summary>
        public SqlCheckpointM()
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
                    errorSeverity = value; //set this now for the update to work.
                    string sql = $"UPDATE CheckPoints SET ErrorSeverity={errorSeverity} WHERE CheckPointID=@CheckPointID;";
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

        /// <summary>
        /// 
        /// </summary>
        public bool DoubleCheck
        {
            get
            {
                if (doubleCheck == null)
                    doubleCheck = false;
                return (bool)doubleCheck;
            }
            set
            {
                doubleCheck = value; //set this now for the update to work.
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool KeyPoint
        {
            get
            {
                if (keyPoint == null)
                    keyPoint = false;
                return (bool)keyPoint;
            }
            set
            {
                keyPoint = value; //set this now for the update to work.
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
                if (comment == value)
                    return;
                if (comment != null)
                {
                    comment = value; //set this now for the update to work.
                    string sql = "UPDATE CheckPoints SET Comment=@Comment WHERE CheckPointID=@CheckPointID;";
                    Console.WriteLine($"writing to {CheckPointTitle}: {value}");
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
                if (link == value)
                    return;
                if (link != null)
                {
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
        /// A value of how long to have the checkpoint sleep between misses. Not implemented as of 10/17/2021
        /// </summary>
        public int Expiration { get; set; } //when ready to implement, do not set to zero, considered "null value" above

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
                    "DoubleCheck=@DoubleCheck, " +
                    "KeyPoint=@KeyPoint, " +
                    "Expiration=@Expiration " +
                    "WHERE CheckPointID=@CheckPointID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
            Console.WriteLine($"Saving entire checkpoint to {CheckPointTitle}: {Comment}");

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




        /// <summary>
        /// Create a sql checkpoint given the title and targetsegment. Use clues from the title to suggest type and section the checkpoint belongs to.
        /// </summary>
        /// <param name="strCheckPointTitle"></param>
        /// <param name="iTargetICD10Segment"></param>
        public SqlCheckpointM(string strCheckPointTitle, int iTargetICD10Segment)
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
                SqlCheckpointM p = cnn.QueryFirstOrDefault<SqlCheckpointM>(sql);
                CheckPointID = p.CheckPointID;
                TargetICD10Segment = p.TargetICD10Segment;
                TargetSection = p.TargetSection;
                CheckPointType = p.CheckPointType;
                CheckPointTitle = p.CheckPointTitle;
            }
        }

    }


}
