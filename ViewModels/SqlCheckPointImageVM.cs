using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AI_Note_Review
{
    public class SqlCheckPointImageVM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        SqlCheckPointImage checkPointImage;
        public SqlCheckpointVM ParentCheckPoint;


        public SqlCheckPointImage CheckPointImageVM
        {
            get { return checkPointImage; }
            set { checkPointImage = value; }
        }

        public SqlCheckPointImageVM()
        {
            checkPointImage = new SqlCheckPointImage();
        } //paramaterless constructor for dapper

        public SqlCheckPointImageVM(SqlCheckpointVM parentCheckpoint, int iCheckPointID, BitmapSource bs)
        {
            checkPointImage = new SqlCheckPointImage(iCheckPointID, bs);
            ParentCheckPoint = parentCheckpoint;
        }

        public SqlCheckPointImageVM(SqlCheckpointVM parentCheckpoint)
        {
            checkPointImage = new SqlCheckPointImage();
            ParentCheckPoint = parentCheckpoint;
        }

        public void DeleteFromDB()
        {
            checkPointImage.DeleteFromDB();
            ParentCheckPoint.UpdateImages();
        }

        public int ImageID { get { return checkPointImage.ImageID; } set { checkPointImage.ImageID = value; } }
        public int CheckPointID { get { return checkPointImage.CheckPointID; } set { checkPointImage.CheckPointID = value; } }
        public byte[] ImageData { get { return checkPointImage.ImageData; } set { checkPointImage.ImageData = value; } }


        #region DeleteImageCommand command
        private ICommand mDeleteImage;
        public ICommand DeleteImageCommand
        {
            get
            {
                if (mDeleteImage == null)
                    mDeleteImage = new DeleteThisImage();
                return mDeleteImage;
            }
            set
            {
                mDeleteImage = value;
            }
        }
        #endregion

    }

    //DeleteImage
    class DeleteThisImage : ICommand
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
            SqlCheckPointImageVM sc = parameter as SqlCheckPointImageVM;
            sc.DeleteFromDB();
        }
        #endregion
    }
    }
