using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AI_Note_Review
{
    /// 
    /// Code behind for report xaml
    /// </summary>
    public class ReportM : INotifyPropertyChanged
    {

        #region inotify

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Console.WriteLine($"iNotify property {propertyName}");
        }

        #endregion
        private DocumentM document;
        public ReportM(DocumentM tmpDoc)
        {
            document = tmpDoc;
        }

        public ReportM()
        {

        }

        public DateTime ReviewDate
        {
            get
            {
                return reviewDate;
            }
            set
            {
                reviewDate = value;
                NotifyPropertyChanged();
            }
        }

        private DateTime reviewDate;

        #region DocumentReport

        /// <summary>
        /// int CheckpointID, int CPstatus
        /// </summary>
        public Dictionary<SqlCheckpointVM, SqlRelCPProvider.MyCheckPointStates> CPStatusOverrides
        {
            get
            {
                return cPStatusOverrides;
            }
            set
            {
                cPStatusOverrides = value;
            }
        }

        public ObservableCollection<string> DocumentTags
        {
            get
            {
                return documentTags;
            }
            set
            {
                documentTags = value;
            }
        }

        public ObservableCollection<SqlCheckpointVM> DroppedCheckPoints
        {
            get
            {
                return droppedCheckPoints;
            }
            set
            {
                droppedCheckPoints = value;
                NotifyPropertyChanged();
            }
        }



        public ObservableCollection<SqlCheckpointVM> PassedCheckPoints
        {
            get
            {
                return passedCheckPoints;
            }
            set
            {
                passedCheckPoints = value;
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<SqlCheckpointVM> MissedCheckPoints
        {
            get
            {
                return missedCheckPoints;
            }
            set
            {
                missedCheckPoints = value;
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<SqlCheckpointVM> IrrelaventCP
        {
            get
            {
                return irrelaventCheckPoints;
            }
            set
            {
                irrelaventCheckPoints = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        private ObservableCollection<string> documentTags = new ObservableCollection<string>();
        private ObservableCollection<SqlCheckpointVM> irrelaventCheckPoints = new ObservableCollection<SqlCheckpointVM>();
        private ObservableCollection<SqlCheckpointVM> missedCheckPoints = new ObservableCollection<SqlCheckpointVM>();
        private ObservableCollection<SqlCheckpointVM> relevantCheckPoints = new ObservableCollection<SqlCheckpointVM>();
        private ObservableCollection<SqlCheckpointVM> passedCheckPoints = new ObservableCollection<SqlCheckpointVM>();
        private ObservableCollection<SqlCheckpointVM> droppedCheckPoints = new ObservableCollection<SqlCheckpointVM>();
        private Dictionary<SqlCheckpointVM, SqlRelCPProvider.MyCheckPointStates> cPStatusOverrides = new Dictionary<SqlCheckpointVM, SqlRelCPProvider.MyCheckPointStates>();

    }
}
