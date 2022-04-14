using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                MasterReviewSummaryVM mr = parameter as MasterReviewSummaryVM;
                SqlICD10SegmentVM seg = new SqlICD10SegmentVM("Enter Segment Title", mr);
                WinEditSegment wes = new WinEditSegment(seg);
                wes.ShowDialog();
                mr.ICD10Segments = null; //reset segments.
            }

        }
}
