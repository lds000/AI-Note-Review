using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AI_Note_Review
{
    public class SqlMissingICD10CodesM
    {
        public string StrCode { get; set; }
        public int Count { get; set; }

        public MasterReviewSummaryVM ParentMasterReviewSummary { get; set; }

        public string StringResult
        {
            get
            {
                return $"{StrCode} ({Count})";
            }
        }
        public SqlMissingICD10CodesM()
        {

        }

        //LinkToICD10Command
        private ICommand mLinkToICD10Command;
        public ICommand LinkToICD10Command
        {
            #region Command Def
            get
            {
                if (mLinkToICD10Command == null)
                    mLinkToICD10Command = new LinkToICD10();
                return mLinkToICD10Command;
            }
            set
            {
                mLinkToICD10Command = value;
            }
            #endregion
        }

        public void AddAlternativeICD10Code(string strCode)
        {
            WinChooseSegment wcs = new WinChooseSegment();
            wcs.DataContext = ParentMasterReviewSummary;
            wcs.ShowDialog();
            if (wcs.SelectedICD10Segment == null) return;
            wcs.SelectedICD10Segment.AddAlternativeICD10(StrCode);
        }

        //CreateAlternateICD10Command
        private ICommand mCreateAlternateICD10;
        public ICommand CreateAlternateICD10Command
        {
            #region Command Def
            get
            {
                if (mCreateAlternateICD10 == null)
                    mCreateAlternateICD10 = new CreateAlternateICD10();
                return mCreateAlternateICD10;
            }
            set
            {
                mCreateAlternateICD10 = value;
            }
            #endregion
        }
    }

    //
    class LinkToICD10 : ICommand
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
            SqlMissingICD10CodesM tmpCode = parameter as SqlMissingICD10CodesM;
            //z20.822
            System.Diagnostics.Process.Start($"https://www.icd10data.com/search?s={tmpCode.StrCode}");
        }
    }

    //CreateAlternateICD10
    class CreateAlternateICD10 : ICommand
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
            SqlMissingICD10CodesM tmpCode = parameter as SqlMissingICD10CodesM;
            tmpCode.AddAlternativeICD10Code(tmpCode.StrCode);
        }
    }
}
