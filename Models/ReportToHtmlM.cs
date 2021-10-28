using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Note_Review
{
    public class ReportToHtmlM
    {
        private CPStates checkPointStatus;

        public ReportToHtmlM()
        {
        }
        public enum CPStates { Pass = 1, Fail = 2, Relevant = 3, Irrelevant = 4 }
        public int RelCPPRoviderID { get; set; }
        public int ProviderID { get; set; }
        public int CheckPointID { get; set; }
        public int PtID { get; set; }
        public string RelComment { get; set; }
        public string CheckPointTitle { get; set; }
        public int ErrorSeverity { get; set; }
        public int CheckPointType { get; set; }
        public int TargetSection { get; set; }
        public int TargetICD10Segment { get; set; }
        public string Comment { get; set; }
        public string Link { get; set; }
        public DateTime ReviewDate { get; set; }


        public CPStates CheckPointStatus
        {
            get => checkPointStatus; set
            {
                checkPointStatus = (CPStates)value;
            }
        }
    }
}
