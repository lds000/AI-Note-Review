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

        private ObservableCollection<MasterReviewSummaryVM> masterReviewSummaryList;
        /// <summary>
        /// Contains a list of all the master reviews
        /// </summary>
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
        /// <summary>
        /// Binds to the SelectedValue of the Listbox containing the MasterReviewList, when initiated selects the review with the current date
        /// </summary>
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
                    //Now select the first ICD10 Segment
                    if (SelectedMasterReview != null)
                    {
                        SelectedICD10Segment = SelectedMasterReview.ICD10Segments.FirstOrDefault();
                    }
                    //then the 1st checkpoint
                    if (SelectedICD10Segment != null)
                    {
                        SelectedCheckPoint = SelectedICD10Segment.Checkpoints.FirstOrDefault();
                    }
                }
            }
        }

        private SqlICD10SegmentVM selectedICD10Segment;
        /// <summary>
        /// Binds to the Selected ICD10 Segment
        /// </summary>
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
                    if (selectedICD10Segment != null)
                    {
                        SelectedICD10Segment.PropertyChanged += SelectedICD10Segment_PropertyChanged;
                    }
                    //Select the 1st checkpoint
                    if (SelectedICD10Segment != null)
                    {
                        SelectedCheckPoint = SelectedICD10Segment.Checkpoints.FirstOrDefault();
                    }
                }
            }
        }

        private void SelectedICD10Segment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ///NOT SURE i NEED THIS
        }

        private SqlCheckpointVM selectedCheckPoint;
        /// <summary>
        /// Binds to the Selected Checkpoint
        /// </summary>
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
                    if (selectedCheckPoint != null)
                    {
                        SelectedCheckPoint.PropertyChanged += SelectedCheckPoint_PropertyChanged;
                    }
                }
            }
        }

        private void SelectedCheckPoint_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ReorderCheckPoints")
            {
                if (SelectedCheckPoint != null)
                {
                    int tmpID = SelectedCheckPoint.CheckPointID; //get the currently selected ID
                    SelectedICD10Segment.ReorderCheckPoints(); //reset ICD10Segments due to changes.
                    SelectedCheckPoint = (from c in SelectedICD10Segment.Checkpoints where c.CheckPointID == tmpID select c).FirstOrDefault(); //now load that ID.
                }
            }
            if (e.PropertyName == "ReloadICD10Segments")
            {
                int tmpID = SelectedICD10Segment.ICD10SegmentID; //get the currently selected ID
                SelectedMasterReview.ICD10Segments = null; //reset ICD10Segments due to changes.
                SelectedICD10Segment = (from c in SelectedMasterReview.ICD10Segments where c.ICD10SegmentID == tmpID select c).FirstOrDefault(); //now load that ID.
            }
        }

        private List<SqlCheckPointType> checkPointTypes;
        /// <summary>
        /// A list of the check point types from database
        /// </summary>
        public List<SqlCheckPointType> CheckPointTypes
        {
            get
            {
                if (checkPointTypes != null)
                {
                    return checkPointTypes;
                }
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from CheckPointTypes order by ItemOrder;";
                    return cnn.Query<SqlCheckPointType>(sql).ToList();
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
