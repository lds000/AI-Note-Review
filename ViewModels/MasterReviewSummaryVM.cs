﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AI_Note_Review
{

    /*
 * Great example I found online.
 class PersonM {
    public string Name { get; set; }
  }

class PersonVM {
    private PersonM Person { get; set;}
    public string Name { get { return this.Person.Name; } }
    public bool IsSelected { get; set; } // example of state exposed by view model

    public PersonVM(PersonM person) {
        this.Person = person;
    }
}
*/
    public class MasterReviewSummaryVM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MasterReviewSummaryVM()
        {
            masterReviewSummary = new SqlMasterReviewSummaryM();
            providerBiMonthlyReview = new SqlRelProviderMasterReviewSummaryM();
        }


        /// <summary>
        /// review for provider submitted every two months
        /// </summary>
        private SqlRelProviderMasterReviewSummaryM providerBiMonthlyReview { get; set; }
        public int RelProviderBiMonthlyID { get { return providerBiMonthlyReview.RelProviderMasterReviewSummaryID; } set { providerBiMonthlyReview.RelProviderMasterReviewSummaryID = value; OnPropertyChanged(); } }
        public int RelBiMonthlyReviewID { get { return providerBiMonthlyReview.RelMasterReviewSummaryID; } set { providerBiMonthlyReview.RelMasterReviewSummaryID = value; OnPropertyChanged(); } }
        public int RelProviderID { get { return providerBiMonthlyReview.RelProviderID; } set { providerBiMonthlyReview.RelProviderID = value; OnPropertyChanged(); } }
        public string ProviderBiMonthlyReviewMComment
        { 
            get 
            { 
                return providerBiMonthlyReview.RelComment; 
            } 
            set 
            {
                providerBiMonthlyReview.RelComment = value;
                OnPropertyChanged(); 
            } 
        }



/// <summary>
/// Record containing the review start, end date and topic information
/// </summary>
        private SqlMasterReviewSummaryM masterReviewSummary { get; set; }
        public int MasterReviewSummaryID { get { return masterReviewSummary.MasterReviewSummaryID; } set { masterReviewSummary.MasterReviewSummaryID = value; OnPropertyChanged(); } }
        public DateTime StartDate { get { return masterReviewSummary.StartDate; } set { masterReviewSummary.StartDate = value; OnPropertyChanged(); } }
        public DateTime EndDate { get { return masterReviewSummary.EndDate; } set { masterReviewSummary.EndDate = value; OnPropertyChanged(); } }
        public string MasterReviewSummaryTitle { get { return masterReviewSummary.MasterReviewSummaryTitle; } set { masterReviewSummary.MasterReviewSummaryTitle = value; OnPropertyChanged(); } }
        public string MasterReviewSummarySubject { get { return masterReviewSummary.MasterReviewSummarySubject; } set { masterReviewSummary.MasterReviewSummarySubject = value; OnPropertyChanged(); } }
        public string MasterReviewSummaryComment { get { return masterReviewSummary.MasterReviewSummaryComment; } set { masterReviewSummary.MasterReviewSummaryComment = value; OnPropertyChanged(); } }
        public string MasterReviewSummaryImpression { get { return masterReviewSummary.MasterReviewSummaryImpression; } set { masterReviewSummary.MasterReviewSummaryImpression = value; OnPropertyChanged(); } }

        public string MasterReviewSummaryToString
        {
            get
            {
                return $"{StartDate.ToString("yyyy/MM/dd")}-{ StartDate.ToString("yyyy/MM/dd")} {MasterReviewSummaryTitle}";
            }
        }

        public static MasterReviewSummaryVM CurrentMasterReview
        {
            get
            {
                string sql = $"Select * from MasterReviewSummary";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    var tmpCol = cnn.Query<MasterReviewSummaryVM>(sql).ToList();
                    foreach (MasterReviewSummaryVM mrs in tmpCol)
                    {

                        if (DateTime.Now >= mrs.StartDate && DateTime.Now <= mrs.EndDate)
                        {
                            return mrs;
                        }
                    }
                }
                return null;
            }
        }


        private List<SqlICD10SegmentVM> iCD10List;
        public List<SqlICD10SegmentVM> ICD10List
        {
            get
            {
                if (iCD10List == null)
                {
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        string sql = $"Select * from ICD10Segments icd inner join RelICD10SegmentMasterReviewSummary rel on icd.ICD10SegmentID == rel.ICD10SegmentID where rel.MasterReviewSummaryID == {MasterReviewSummaryID} order by icd10Chapter, icd10CategoryStart;";
                        var l = cnn.Query<SqlICD10SegmentM>(sql).ToList();
                        List<SqlICD10SegmentVM> lvm = new List<SqlICD10SegmentVM>();
                        foreach (SqlICD10SegmentM s in l)
                        {
                            SqlICD10SegmentVM scvm = new SqlICD10SegmentVM(s);
                            lvm.Add(scvm);
                        }
                        iCD10List = lvm;
                    }
                }
                return iCD10List;
            }
        }

        public bool ContainsDocument(DocumentVM d)
        {
            foreach (SqlICD10SegmentVM icd10 in d.ICD10Segments)
            {
                foreach (SqlICD10SegmentVM mrsICD10 in ICD10List)
                {
                    if (mrsICD10.ICD10SegmentID == icd10.ICD10SegmentID) return true;
                }
            }
            return false;
        }

        public ObservableCollection<MasterReviewSummaryVM> MasterReviewSummaries
        {
            get
            {
                    string sql = $"Select * from MasterReviewSummary";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        var tmpCol = cnn.Query<MasterReviewSummaryVM>(sql).ToList();
                        return tmpCol.ToObservableCollection();
                    }
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
                        }
                    }
                }
                return selectedMasterReview;
            }
            set
            {
                selectedMasterReview = value;
            }
        }

        private ObservableCollection<MasterReviewSummaryVM> masterReviewSummaryList;
        public ObservableCollection<MasterReviewSummaryVM> MasterReviewSummaryList
        {
            get 
            {
                if (masterReviewSummaryList == null)
                {
                    string sql = $"Select * from MasterReviewSummary;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        masterReviewSummaryList = cnn.Query<MasterReviewSummaryVM>(sql).ToList().ToObservableCollection();
                    }
                }
                return masterReviewSummaryList;
            }
        }

        private ObservableCollection<SqlICD10SegmentVM> iCD10Segments;
        /// <summary>
        /// A list of ICD10 Segments that belong to the current selected MasterReview
        /// </summary>
        public ObservableCollection<SqlICD10SegmentVM> ICD10Segments
        {
            get
            {
                if (iCD10Segments == null)
                {
                    iCD10Segments = SqlICD10SegmentVM.NoteICD10Segments;
                }
                if (SelectedMasterReview != null)
                {
                    if (SelectedMasterReview.MasterReviewSummaryID == 3)
                    {
                        iCD10Segments = SqlICD10SegmentVM.NoteICD10Segments; //All
                        return iCD10Segments;
                    }
                    if (SelectedMasterReview.MasterReviewSummaryID == 1) //general review with X
                    {
                        using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                        {
                            string sql = "Select * from ICD10Segments where icd10Chapter == 'X' order by icd10Chapter, icd10CategoryStart;";
                            var l = cnn.Query<SqlICD10SegmentM>(sql).ToList();
                            List<SqlICD10SegmentVM> lvm = new List<SqlICD10SegmentVM>();
                            foreach (SqlICD10SegmentM s in l)
                            {
                                SqlICD10SegmentVM scvm = new SqlICD10SegmentVM(s);
                                lvm.Add(scvm);
                            }
                            iCD10Segments = lvm.ToObservableCollection();
                            return iCD10Segments;
                        }
                    }
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        string sql = $"Select * from ICD10Segments icd inner join RelICD10SegmentMasterReviewSummary rel on icd.ICD10SegmentID == rel.ICD10SegmentID where rel.MasterReviewSummaryID == {SelectedMasterReview.MasterReviewSummaryID} order by icd10Chapter, icd10CategoryStart;";
                        var l = cnn.Query<SqlICD10SegmentM>(sql).ToList();
                        List<SqlICD10SegmentVM> lvm = new List<SqlICD10SegmentVM>();
                        foreach (SqlICD10SegmentM s in l)
                        {
                            SqlICD10SegmentVM scvm = new SqlICD10SegmentVM(s);
                            lvm.Add(scvm);
                        }
                        iCD10Segments = lvm.ToObservableCollection();
                        return iCD10Segments;
                    }


                }
                return iCD10Segments;
            }
            set
            {
                iCD10Segments = value;
            }
        }

    }
}
