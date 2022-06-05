using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AI_Note_Review
{
        /// <summary>
        /// Add Segment
        /// </summary>
        public class SegmentAdder : ICommand
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
                MasterReviewSummaryVM mr = parameter as MasterReviewSummaryVM; //may be null, if not null then add segment to MRS
            if (mr == null)
            {
                MessageBox.Show("Please select a Master Review from the list.");
                return;
            }
                SqlICD10SegmentVM seg = new SqlICD10SegmentVM("Enter Segment Title", mr);
                seg.icd10Chapter = 'S';
                WinEditSegment wes = new WinEditSegment(seg);
                wes.ShowDialog();
                seg.AssignToMasterReviewSummary(mr);
        }

        }
}
