using Dapper;
using System;
using System.Collections.Generic;
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
    public class SqlCheckPointImage : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        // Declare the event
        public int ImageID { get; set; }
        public int CheckPointID { get; set; }
        public byte[] ImageData { get; set; }

        public SqlCheckPointImage()
        {
        }

        public SqlCheckPointImage(int iCheckPointID, BitmapSource bs)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bs));
            encoder.QualityLevel = 100;
            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(bs));
                encoder.Save(stream);
                ImageData = stream.ToArray();
                stream.Close();
            }
            string sql = "";
            CheckPointID = iCheckPointID;

            sql = $"INSERT INTO CheckPointImages (CheckPointID,ImageData) VALUES (@CheckPointID,@ImageData);SELECT last_insert_rowid()";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                int lastID = cnn.ExecuteScalar<int>(sql, this);
            }

        }

        /// <summary>
        /// Delete an image from the database. Todo: Is this correct? Could possibly lead to checkpoints with no image if this image is shared between two checkpoints.
        /// </summary>
        /// <returns></returns>
        public bool DeleteFromDB()
        {
            MessageBoxResult mr = MessageBox.Show("Are you sure you want to remove this image? This is permenant and will delete all content.", "Confirm Delete", MessageBoxButton.YesNo);
            if (mr != MessageBoxResult.Yes)
            {
                return false;
            }

            string sql = "Delete from CheckPointImages where ImageID=@ImageID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
            return true;
        }

        public SqlCheckpointVM ParentCheckPointVM
        {
            get { 
                return parentCheckPointVM;
            }
            set
            {
                parentCheckPointVM = value;
                OnPropertyChanged("ParentCheckPointVM");
            }
        }

        private ICommand mDeleteImage;
        private SqlCheckpointVM parentCheckPointVM;

        public ICommand DeleteImageCommand
        {
            get
            {
                if (mDeleteImage == null)
                    mDeleteImage = new CPImageDeleter();
                return mDeleteImage;
            }
            set
            {
                mDeleteImage = value;
            }
        }

    }

    class CPImageDeleter : ICommand
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
            SqlCheckPointImage CPI = parameter as SqlCheckPointImage;
            CPI.DeleteFromDB();
            CPI.ParentCheckPointVM.UpDateImages();
        }
        #endregion
    }

}

