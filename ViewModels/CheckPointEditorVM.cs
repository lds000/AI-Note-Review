using Dapper;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AI_Note_Review
{
    public class CheckPointEditorVM : INotifyPropertyChanged
    {
        #region inotify
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Events
        public CheckPointEditorVM()
        {
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            Messenger.Default.Register<NotificationMessage>(this, NotifyMe);
        }

        private void NotifyMe(NotificationMessage obj)
        {
            if (obj.Notification == "ReloadCheckPoints")
            {
                if (selectedICD10Segment != null)
                    SelectedICD10Segment.Checkpoints = null; //reset checkpoints, forcing an update.
            }
            if (obj.Notification == "ReloadICD10Segments")
            {
                //todo:this is not working
                if (SelectedICD10Segment != null)
                {
                    int tmpID = SelectedICD10Segment.ICD10SegmentID; //get the currently selected ID
                    SelectedMasterReview.ICD10Segments = null; //reset ICD10Segments
                    SelectedICD10Segment = (from c in SelectedMasterReview.ICD10Segments where c.ICD10SegmentID == tmpID select c).FirstOrDefault(); //now load that ID.
                }
            }

            //ReorderICD10

            //ReorderCheckPoints
            if (obj.Notification == "ReorderCheckPoints")
            {
                if (selectedICD10Segment != null)
                SelectedICD10Segment.ReorderCheckPoints();
            }
        }

        #endregion

        private ObservableCollection<MasterReviewSummaryVM> masterReviewSummaryList;
        public ObservableCollection<MasterReviewSummaryVM> MasterReviewSummaryList
        {
            get
            {
                if (masterReviewSummaryList == null)
                {
                    string sql = $"Select * from MasterReviewSummary order by StartDate;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        masterReviewSummaryList = cnn.Query<MasterReviewSummaryVM>(sql).ToList().ToObservableCollection();
                    }
                }
                return masterReviewSummaryList;
            }
        }

        private MasterReviewSummaryVM selectedMasterReview;
        public MasterReviewSummaryVM SelectedMasterReview
        {
            get
            {
                if (selectedMasterReview == null)
                {
                    foreach (MasterReviewSummaryVM mrs in MasterReviewSummaryList)
                    {

                        if (DateTime.Now >= mrs.StartDate && DateTime.Now <= mrs.EndDate)
                        {
                            selectedMasterReview = mrs;
                            selectedICD10Segment = SelectedMasterReview.ICD10Segments.FirstOrDefault();
                            OnPropertyChanged("SelectedICD10Segment");
                        }
                    }
                }
                return selectedMasterReview;
            }
            set
            {
                if (selectedMasterReview != value)
                {
                    selectedMasterReview = value;
                    OnPropertyChanged();
                    if (SelectedMasterReview != null)
                    {
                        selectedICD10Segment = SelectedMasterReview.ICD10Segments.FirstOrDefault();
                        OnPropertyChanged("SelectedICD10Segment");
                    }
                    if (SelectedICD10Segment != null)
                    {
                        selectedCheckPoint = SelectedICD10Segment.Checkpoints.FirstOrDefault();
                        OnPropertyChanged("SelectedCheckPoint");
                    }
                }
            }
        }

        private SqlICD10SegmentVM selectedICD10Segment;
        public SqlICD10SegmentVM SelectedICD10Segment
        {
            get
            {
                return selectedICD10Segment;
            }
            set
            {
                if (selectedICD10Segment != value)
                {
                    selectedICD10Segment = value;
                    OnPropertyChanged();
                    if (SelectedICD10Segment != null)
                    {
                        selectedCheckPoint = SelectedICD10Segment.Checkpoints.FirstOrDefault();
                        OnPropertyChanged("SelectedCheckPoint");
                    }
                }
            }
        }

        private SqlCheckpointVM selectedCheckPoint;
        public SqlCheckpointVM SelectedCheckPoint
        {
            get
            {
                return selectedCheckPoint;
            }
            set
            {
                if (SelectedCheckPoint != value)
                {
                    selectedCheckPoint = value;
                    OnPropertyChanged();
                }
            }
        }

        #region commands
        private ICommand mAddSegment;
        public ICommand AddSegmentCommand
        {
            get
            {
                if (mAddSegment == null)
                    mAddSegment = new SegmentAdder();
                return mAddSegment;
            }
            set
            {
                mAddSegment = value;
            }
        }
        #endregion

    }
}
