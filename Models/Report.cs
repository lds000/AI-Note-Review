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
    public class Report : INotifyPropertyChanged
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
        private Document document;
        public Report(Document tmpDoc)
        {
            document = tmpDoc;
        }

        public Report()
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
                NotifyPropertyChanged("ReviewDateStr");
            }
        }

        private DateTime reviewDate;

        #region DocumentReport

        /// <summary>
        /// int CheckpointID, int CPstatus
        /// </summary>
        public Dictionary<SqlCheckpointViewModel, SqlRelCPProvider.MyCheckPointStates> CPStatusOverrides
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

        private ObservableCollection<string> documentTags;
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

        public ObservableCollection<SqlCheckpointViewModel> DroppedCheckPoints
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



        public ObservableCollection<SqlCheckpointViewModel> PassedCheckPoints
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
        public ObservableCollection<SqlCheckpointViewModel> MissedCheckPoints
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
        public ObservableCollection<SqlCheckpointViewModel> IrrelaventCP
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

        private ObservableCollection<SqlCheckpointViewModel> irrelaventCheckPoints = new ObservableCollection<SqlCheckpointViewModel>();
        private ObservableCollection<SqlCheckpointViewModel> missedCheckPoints = new ObservableCollection<SqlCheckpointViewModel>();
        private ObservableCollection<SqlCheckpointViewModel> relevantCheckPoints = new ObservableCollection<SqlCheckpointViewModel>();
        private ObservableCollection<SqlCheckpointViewModel> passedCheckPoints = new ObservableCollection<SqlCheckpointViewModel>();
        private ObservableCollection<SqlCheckpointViewModel> droppedCheckPoints = new ObservableCollection<SqlCheckpointViewModel>();
        private Dictionary<SqlCheckpointViewModel, SqlRelCPProvider.MyCheckPointStates> cPStatusOverrides = new Dictionary<SqlCheckpointViewModel, SqlRelCPProvider.MyCheckPointStates>();

    }
}
