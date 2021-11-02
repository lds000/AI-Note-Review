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
        private SqlProvider sqlProvider;
        private MasterReviewSummaryVM biMonthlyReviews;

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

        public ObservableCollection<MasterReviewSummaryVM> MasterReviewSummaryList
        {
            get { return biMonthlyReviews.MasterReviewSummaryList; }
        }

        private MasterReviewSummaryVM selectedMasterReviewSummary;
        public MasterReviewSummaryVM SelectedMasterReviewSummary
        {
            get 
            {
                if (selectedMasterReviewSummary == null) return new MasterReviewSummaryVM();
                return selectedMasterReviewSummary; 
            }
            set
            {
                selectedMasterReviewSummary = value;
                OnPropertyChanged("MyPeeps");
            }
        }

        public BiMonthlyReviewVM()
        {
            biMonthlyReviewM = new BiMonthlyReviewM();
            biMonthlyReviews = new MasterReviewSummaryVM();
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

        public SqlProvider SelectedProviderForBiMonthlyReview
        {
            get
            {
                return sqlProvider;
            }
            set
            {
                sqlProvider = value;
                OnPropertyChanged("MyPeeps");
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
