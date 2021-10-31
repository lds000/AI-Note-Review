using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AI_Note_Review
{
    public class SqlDocumentReviewSummaryM
    {
        public DateTime VisitDate { get; set; }
        public int PtID { get; set; }
        VisitReportM visitReport = new VisitReportM();
        public SqlProvider ParentProvider { get; set; }

        public SqlDocumentReviewSummaryM(SqlProvider sp)
        {
            ParentProvider = sp;
        }

        //for dapper
        public SqlDocumentReviewSummaryM()
        {
        }

        public string CheckPointsSummaryHTML
        {
            get
            {
                ReportToHtmlVM r = new ReportToHtmlVM(VisitDate, PtID);
                return r.CheckPointsSummaryHTML;
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
            SqlDocumentReviewSummaryM sdr = parameter as SqlDocumentReviewSummaryM;
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



