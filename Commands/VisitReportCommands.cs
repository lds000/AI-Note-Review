using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            //mrs.VisitReport.NewEcWDocument(); //reset document
            mrs.VisitReport.PopulateCPStatuses();
            VisitReportV wp = new VisitReportV(mrs.VisitReport);
            mrs.VisitReport.CurrentVisitReportV = wp;
            //wp.DataContext = mrs.VisitReport;
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
            rvm.CurrentVisitReportV.Close();
            rvm.MasterReviewSummary.DeleteParentNoteData();
            rvm.MasterReviewSummary.GetNextParentNote();
            VisitReportV wp = new VisitReportV(rvm);
            rvm.CurrentVisitReportV = wp;
            wp.DataContext = rvm;
            wp.ShowDialog();
        }
    }

    class ShowNoteEx : ICommand
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
            WinShowNote wsn = new WinShowNote();
            wsn.DataContext = rvm.Document;
            wsn.Show();
            ;
        }
    }

    class GrabNextNote : ICommand
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
            MasterReviewSummaryVM mrs = rvm.MasterReviewSummary;
            mrs.GetNextParentNote();
        }
    }


    class SkipNote : ICommand
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
            rvm.MasterReviewSummary.DeleteParentNoteData();
            rvm.MasterReviewSummary.GetNextParentNote();
        }
    }
}
