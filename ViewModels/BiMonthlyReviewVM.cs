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
using System.Windows.Input;
using System.Windows.Media;

namespace AI_Note_Review
{
    public class BiMonthlyReviewVM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private BiMonthlyReviewM biMonthlyReviewM;
        private MasterReviewSummaryVM biMonthlyReviews;

        public BiMonthlyReviewVM()
        {
            biMonthlyReviewM = new BiMonthlyReviewM();
            biMonthlyReviews = new MasterReviewSummaryVM();
            masterReviewSummaryVM = biMonthlyReviews.MasterReviewSummaryList;
            //todo: set selected reviewmodel to one with today's date.
            SelectedMasterReviewSummary = masterReviewSummaryVM.First();
        }


        public BiMonthlyReviewM BiMonthlyReviewM
        {
            get
            {
                return biMonthlyReviewM;
            }
        }

        public MasterReviewSummaryVM BiMonthlyReviews
        {
            get
            {
                return biMonthlyReviews;
            }
        }

        private ObservableCollection<MasterReviewSummaryVM> masterReviewSummaryVM;
        public ObservableCollection<MasterReviewSummaryVM> MasterReviewSummaryList
        {
            get
            {
                return masterReviewSummaryVM;
            }
        }

        private MasterReviewSummaryVM selectedMasterReviewSummary;
        public MasterReviewSummaryVM SelectedMasterReviewSummary
        {
            get
            {
                //if (selectedMasterReviewSummary == null) return new MasterReviewSummaryVM();
                return selectedMasterReviewSummary;
            }
            set
            {
                selectedMasterReviewSummary = value;
                OnPropertyChanged("MyPeeps");
                SelectedProviderForBiMonthlyReview = MyPeeps.First();
            }
        }
        private SqlProvider selectedProviderForBiMonthlyReview;
        public SqlProvider SelectedProviderForBiMonthlyReview
        {
            get
            {
                return selectedProviderForBiMonthlyReview;
            }
            set
            {
                selectedProviderForBiMonthlyReview = value;
                OnPropertyChanged("ListOfDocumentReviews");
                //when the provider is changed, set the selected document for review to the first item in the list.
                SelectedDocumentReview = ListOfDocumentReviews.FirstOrDefault();
                OnPropertyChanged("SelectedDocumentReview");
            }
        }

        /// <summary>
        /// Populates a class containing PtID and Dates for a time period (usually two months)
        /// </summary>
        public ObservableCollection<SqlDocumentReviewSummaryVM> ListOfDocumentReviews
        {
            get
            {
                if (selectedProviderForBiMonthlyReview == null) return null;
                if (SelectedMasterReviewSummary == null) return null;
                string sql = "";
                sql += $"Select distinct VisitDate, PtID from RelCPPRovider where ProviderID={selectedProviderForBiMonthlyReview.ProviderID} and VisitDate Between '{SelectedMasterReviewSummary.StartDate.ToString("yyyy-MM-dd")}' and '{SelectedMasterReviewSummary.EndDate.ToString("yyyy-MM-dd")}';";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    ObservableCollection<SqlDocumentReviewSummaryVM> tmpL = new ObservableCollection<SqlDocumentReviewSummaryVM>(cnn.Query<SqlDocumentReviewSummaryVM>(sql).ToList());
                    foreach (var l in tmpL)
                    {
                        l.ParentProvider = selectedProviderForBiMonthlyReview;
                        
                    }
                    return tmpL;
                }
            }
        }


        private SqlDocumentReviewSummaryVM selectedDocumentReview;
        public SqlDocumentReviewSummaryVM SelectedDocumentReview
        {
            get
            {
                return selectedDocumentReview;
            }
            set
            {
                selectedDocumentReview = value;
                OnPropertyChanged("ReviewHTML");
            }
        }

        public string ReviewHTML
        {
            get
            {
                if (selectedDocumentReview == null) return "Select a review";
                return selectedDocumentReview.ReviewHTML;
            }
        }



        public ObservableCollection<SqlProvider> MyPeeps
        {
            get
            {
                Console.WriteLine($"getting west side pod and assigning {SelectedMasterReviewSummary.MasterReviewSummaryTitle} to provider.");
                string sql = "";
                sql += $"Select * from Providers where IsWestSidePod == '1' order by FullName;"; //this part is to get the ID of the newly created phrase
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    var tmpPeeps = cnn.Query<SqlProvider>(sql).ToList();
                    foreach(var tmpPeep in tmpPeeps)
                    {
                        tmpPeep.ParentMasterReviewSummary = selectedMasterReviewSummary;
                    }
                    return tmpPeeps.ToObservableCollection();
                }
            }
        }


        private ICommand mShowBiMonthlyReport;
        public ICommand ShowBiMonthlyReport
        {
            #region Command Def
            get
            {
                if (mShowBiMonthlyReport == null)
                    mShowBiMonthlyReport = new ShowBiMonthlyReport();
                return mShowBiMonthlyReport;
            }
            set
            {
                mShowBiMonthlyReport = value;
            }
            #endregion
        }
    }

    class ShowBiMonthlyReport : ICommand
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
        #endregion

        public void Execute(object parameter)
        {
            BiMonthlyReviewVM rvm = parameter as BiMonthlyReviewVM;
            BiMonthlyReviewV wp = new BiMonthlyReviewV(rvm);
            wp.ShowDialog();
        }
    }

}
