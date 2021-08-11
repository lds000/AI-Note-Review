using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Note_Review
{
    public class SqlRelCPProvider
    {
        public int RelCPPRoviderID { get; set; }
        public int ProviderID { get; set; }

        public enum MyCheckPointStates  {Pass = 1, Fail = 2, Relevant = 3, Irrelevant = 4}
        public int CheckPointID { get; set; }

        public int PtID { get; set; }
        public DateTime ReviewDate { get; set; }
        public DateTime VisitDate { get; set; }
        public MyCheckPointStates CheckPointStatus { get; set; }

        public string CheckPointTitle { get; set; }

        public SqlRelCPProvider()
        {

        }

    }
}
