using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AI_Note_Review
{
    public class SqlDocumentReviewSummaryVM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private SqlDocumentReviewSummaryM documentReviewSummary;
        public DateTime VisitDate 
        {
            get 
            { 
                return documentReviewSummary.VisitDate; 
            } 
            set 
            { 
                documentReviewSummary.VisitDate = value;
                OnPropertyChanged("CheckPointsSummaryHTML");
            }
        }
        public int PtID 
        { 
            get 
            {
                return documentReviewSummary.PtID; 
            } 
            set 
            { 
                documentReviewSummary.PtID = value;
                OnPropertyChanged("CheckPointsSummaryHTML");
            } 
        }
        public ProviderVM ParentProvider { get; set; }

        public SqlDocumentReviewSummaryVM(ProviderVM sp)
        {
            documentReviewSummary = new SqlDocumentReviewSummaryM();
            ParentProvider = sp;
        }

        //for dapper
        public SqlDocumentReviewSummaryVM()
        {
            documentReviewSummary = new SqlDocumentReviewSummaryM();
        }

        public string ReviewHTML
        {
            get
            {
                if (reportToHtmlVM == null)
                {
                    reportToHtmlVM = new ReportToHtmlVM(ParentProvider, VisitDate, PtID);
                }
                return reportToHtmlVM.CheckPointsSummaryHTML;
            }
        }

        public ReportToHtmlVM reportToHtmlVM { get; set; }
        public ReportToHtmlVM HtmlReport
            {
            get 
            {
                //figure this out
                if (ParentProvider == null) return null;
                if (VisitDate == null) return null;
                if (reportToHtmlVM == null)
                {
                    reportToHtmlVM = new ReportToHtmlVM(ParentProvider, VisitDate, PtID);
                }
                return reportToHtmlVM; 
            }
            set
            {
                reportToHtmlVM = value;
            }
            }

        private ICommand mDeleteThisReview;
        public ICommand DeleteThisReviewCommand
        {
            #region Command Def
            get
            {
                if (mDeleteThisReview == null)
                    mDeleteThisReview = new DeleteThisReview();
                return mDeleteThisReview;
            }
            set
            {
                mDeleteThisReview = value;
            }
            #endregion
        }
    }

    class DeleteThisReview : ICommand
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
            SqlDocumentReviewSummaryVM sdr = parameter as SqlDocumentReviewSummaryVM;
            MessageBoxResult mr = MessageBox.Show("Are you sure you want to remove this image? This is permenant and will delete all content.", "Confirm Delete", MessageBoxButton.YesNo);
            if (mr != MessageBoxResult.Yes)
            {
                return;
            }
            string strDelete = $"Delete from RelCPPRovider where PtID={sdr.PtID} AND VisitDate='{sdr.VisitDate.ToString("yyyy-MM-dd")}';";
            using (IDbConnection cnn1 = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn1.Execute(strDelete);
            }
            //now notify property changed
            sdr.ParentProvider.UpdateSqlDocumentReviewsSummaryProperty();
        }
    }

}
