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

        public BiMonthlyReviewM BiMonthlyReviewM
        {
            get
            {
                return biMonthlyReviewM;
            }
        }

        public BiMonthlyReviewVM()
        {
            biMonthlyReviewM = new BiMonthlyReviewM();
        }

        public List<SqlProvider> MyPeeps
        {
            get
            {
                return biMonthlyReviewM.MyPeeps;
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
