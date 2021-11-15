using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Note_Review
{
    public class SqlRelAlternativeICD10M
    {
        public int RelAlternativeICD10ID { get; set; }
        public string AlternativeICD10Title { get; set; }
        public string AlternativeICD10 { get; set; }
        public int TargetICD10Segment { get; set; }

        public SqlRelAlternativeICD10M()
        {

        }
    }
}
