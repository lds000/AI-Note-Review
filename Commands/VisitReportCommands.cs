using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AI_Note_Review
{
    class ShowReport : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion

        public void Execute(object parameter)
        {
            MasterReviewSummaryVM mrs = parameter as MasterReviewSummaryVM;
            mrs.VisitReport.NewEcWDocument(); //reset document
            mrs.VisitReport.PopulateCPStatuses();
            VisitReportV wp = new VisitReportV();
            wp.DataContext = mrs.VisitReport;
            wp.ShowDialog();
        }
    }

    class ShowReportGen : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion

        public void Execute(object parameter)
        {
            VisitReportVM rvm = parameter as VisitReportVM;
            rvm.NewEcWDocument(); //reset document
            VisitReportV wp = new VisitReportV();
            wp.DataContext = rvm;
            wp.ShowDialog();
        }
    }

    class CommitMyReport : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion

        public void Execute(object parameter)
        {
            VisitReportVM rvm = parameter as VisitReportVM;
            rvm.CommitReport();
        }
    }

}
